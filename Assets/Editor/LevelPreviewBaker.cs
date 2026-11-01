using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Bakes a simplified 3D model of every campaign level for the level-select preview.
// For each "World N Level M" scene in the build list it merges the solid geometry into
// one mesh (a submesh per material), centres it, and saves a prefab under
// Assets/LevelPreviews. Run from Tools > Level Previews > Bake All, or in batch mode with
// -executeMethod LevelPreviewBaker.BakeAll.
public static class LevelPreviewBaker
{
	// Max spread of a level along one axis, in multiples of its typical piece size
	public const float SpreadInSizes = 5.0f;

	public const string OutputFolder = "Assets/LevelPreviews";

	// Root object names that never belong in a preview
	static readonly string[] SkipRootPrefixes = { "Planet", "DebugObject", "GUI", "UIs", "Tutorial", "Preview", "CamTrigger", "TheCamera", "Rope", "Player", "DirLight", "Directional", "StellarFlame" };
	// Shaders that are effects, not solid geometry
	static readonly string[] SkipShaderParts = { "Particles/", "Additive", "Alpha Blended", "Skybox", "Sprites/", "Unlit/Transparent" };

	[MenuItem("Tools/Level Previews/Bake All")]
	public static void BakeAll()
	{
		if (!AssetDatabase.IsValidFolder(OutputFolder))
			AssetDatabase.CreateFolder("Assets", "LevelPreviews");

		var levelScenes = EditorBuildSettings.scenes
			.Select(s => s.path)
			.Where(p => Regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(p), @"^World \d+ Level \d+$"))
			.ToList();

		int ok = 0;
		foreach (var path in levelScenes)
		{
			if (Bake(path)) ok++;
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("LevelPreviewBaker: baked " + ok + " of " + levelScenes.Count + " levels");
	}

	[MenuItem("Tools/Level Previews/Bake Open Scene")]
	public static void BakeOpenScene()
	{
		Bake(EditorSceneManager.GetActiveScene().path);
		AssetDatabase.SaveAssets();
	}

	public static bool Bake(string scenePath)
	{
		if (!AssetDatabase.IsValidFolder(OutputFolder))
			AssetDatabase.CreateFolder("Assets", "LevelPreviews");

		string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
		var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

		var solids = CollectSolids(out var spreadCentre, out var spreadScale);

		// Squashing the spacing can push copies of the same object on top of each other
		// (World 5 Level 5's spinning platforms), which reads as a glitchy tangle: keep only one
		var dropped = Declutter(solids, spreadCentre, spreadScale);
		if (dropped > 0)
			Debug.Log("LevelPreviewBaker: " + name + " -> dropped " + dropped + " overlapping copies");
		LastDropped = dropped;

		var byMaterial = new Dictionary<Material, List<CombineInstance>>();
		foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (!solids.Contains(renderer))
				continue;

			var filter = renderer.GetComponent<MeshFilter>();
			if (filter == null || filter.sharedMesh == null)
				continue;

			var centre = renderer.bounds.center;
			var moved = spreadCentre + Vector3.Scale(centre - spreadCentre, spreadScale);
			var matrix = Matrix4x4.Translate(moved - centre) * renderer.transform.localToWorldMatrix;

			var mats = renderer.sharedMaterials;
			for (int sub = 0; sub < filter.sharedMesh.subMeshCount; sub++)
			{
				var mat = mats[Mathf.Min(sub, mats.Length - 1)];
				if (mat == null) continue;
				if (!byMaterial.TryGetValue(mat, out var list))
					byMaterial[mat] = list = new List<CombineInstance>();
				list.Add(new CombineInstance { mesh = filter.sharedMesh, subMeshIndex = sub, transform = matrix });
			}
		}

		if (byMaterial.Count == 0)
		{
			Debug.LogWarning("LevelPreviewBaker: nothing to bake in " + name);
			return false;
		}

		// One merged mesh per material, then stack those as submeshes of one final mesh
		var materials = new List<Material>();
		var perMaterial = new List<CombineInstance>();
		foreach (var pair in byMaterial)
		{
			var part = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
			part.CombineMeshes(pair.Value.ToArray(), true, true);
			perMaterial.Add(new CombineInstance { mesh = part, subMeshIndex = 0, transform = Matrix4x4.identity });
			materials.Add(PreviewMaterial(pair.Key));
		}

		var combined = new Mesh { name = name + " Preview", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
		combined.CombineMeshes(perMaterial.ToArray(), false, false);

		// Centre on the bounds, and scale so the longest side is 10 units
		var bounds = combined.bounds;
		float scale = 10.0f / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
		var verts = combined.vertices;
		for (int i = 0; i < verts.Length; i++)
			verts[i] = (verts[i] - bounds.center) * scale;
		combined.vertices = verts;
		combined.RecalculateBounds();
		combined.RecalculateNormals();
		combined.Optimize();

		string meshPath = OutputFolder + "/" + name + ".asset";
		AssetDatabase.DeleteAsset(meshPath);
		AssetDatabase.CreateAsset(combined, meshPath);

		var go = new GameObject(name + " Preview");
		go.AddComponent<MeshFilter>().sharedMesh = combined;
		go.AddComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
		string prefabPath = OutputFolder + "/" + name + ".prefab";
		PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
		Object.DestroyImmediate(go);

		Debug.Log("LevelPreviewBaker: " + name + " -> " + combined.vertexCount + " verts, " + materials.Count + " materials, source size " + bounds.size);
		return true;
	}

	// The open scene's solid renderers, and how to squash their spacing (see Bake)
	static List<MeshRenderer> CollectSolids(out Vector3 spreadCentre, out Vector3 spreadScale)
	{
		// Collect the solid renderers first so we can compress over-long levels
		var solids = new List<MeshRenderer>();
		foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			var f = renderer.GetComponent<MeshFilter>();
			if (renderer.enabled && f != null && f.sharedMesh != null && IsSolid(renderer))
				solids.Add(renderer);
		}

		// Spacing compression: levels are very long and thin, so their pieces would be specks on a card.
		// Pull the pieces closer along any axis that spreads further than ~SpreadInSizes typical piece sizes.
		spreadScale = Vector3.one;
		spreadCentre = Vector3.zero;
		if (solids.Count > 1)
		{
			var centres = new Bounds(solids[0].bounds.center, Vector3.zero);
			var sizes = new List<float>();
			foreach (var r in solids)
			{
				centres.Encapsulate(r.bounds.center);
				sizes.Add(Mathf.Max(r.bounds.size.x, r.bounds.size.y, r.bounds.size.z));
			}
			sizes.Sort();
			float typical = Mathf.Max(sizes[sizes.Count / 2], 1.0f);
			float cap = typical * SpreadInSizes;
			spreadCentre = centres.center;
			for (int axis = 0; axis < 3; axis++)
			{
				float extent = centres.size[axis];
				spreadScale[axis] = extent > cap ? cap / extent : 1.0f;
			}
		}
		return solids;
	}

	// Lists how many overlapping copies each level's preview would drop, without baking
	public static string CountCopies()
	{
		var report = new System.Text.StringBuilder();
		foreach (var path in EditorBuildSettings.scenes.Select(s => s.path)
			.Where(p => Regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(p), @"^World \d+ Level \d+$")))
		{
			EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			var solids = CollectSolids(out var centre, out var scale);
			int dropped = Declutter(solids, centre, scale);
			if (dropped > 0)
				report.AppendLine(System.IO.Path.GetFileNameWithoutExtension(path) + ": " + dropped);
		}
		return report.ToString();
	}

	// How many copies the last Bake dropped
	public static int LastDropped;

	// How much of the smaller of two boxes must be covered by the other to count as on top of it
	const float CopyOverlap = 0.5f;
	// Only pieces at least this big in the finished 10-unit preview are dropped, as small
	// overlapping copies don't confuse anything (same rule as the Rust port's preview)
	const float BigPiece = 2.0f;

	// Removes renderers that draw the same mesh with the same materials as an earlier one in
	// nearly the same place once the spacing is squashed (a copy in another material, like a
	// tinted rock, stays, or the preview's colours would change). Returns how many went.
	static int Declutter(List<MeshRenderer> solids, Vector3 spreadCentre, Vector3 spreadScale)
	{
		var moved = new List<Bounds>();
		foreach (var r in solids)
		{
			var b = r.bounds;
			b.center = spreadCentre + Vector3.Scale(b.center - spreadCentre, spreadScale);
			moved.Add(b);
		}
		if (moved.Count == 0)
			return 0;
		var all = moved[0];
		foreach (var b in moved)
			all.Encapsulate(b);
		float previewScale = 10.0f / Mathf.Max(all.size.x, all.size.y, all.size.z, 1e-6f);

		var kept = new List<(MeshRenderer renderer, Mesh mesh, Bounds bounds)>();
		var survivors = new List<MeshRenderer>();
		for (int i = 0; i < solids.Count; i++)
		{
			var mesh = solids[i].GetComponent<MeshFilter>().sharedMesh;
			var bounds = moved[i];
			bool big = bounds.size.magnitude * previewScale > BigPiece;
			bool copy = false;
			if (big)
			{
				foreach (var other in kept)
				{
					if (other.mesh == mesh && SameMaterials(other.renderer, solids[i]) && Overlap(bounds, other.bounds) > CopyOverlap)
					{
						copy = true;
						break;
					}
				}
			}

			if (!copy)
			{
				kept.Add((solids[i], mesh, bounds));
				survivors.Add(solids[i]);
			}
		}

		int dropped = solids.Count - survivors.Count;
		solids.Clear();
		solids.AddRange(survivors);
		return dropped;
	}

	static bool SameMaterials(MeshRenderer a, MeshRenderer b)
	{
		var ma = a.sharedMaterials;
		var mb = b.sharedMaterials;
		if (ma.Length != mb.Length)
			return false;
		for (int i = 0; i < ma.Length; i++)
		{
			if (ma[i] != mb[i])
				return false;
		}
		return true;
	}

	// How much of the smaller of two boxes the other covers, 0 to 1
	static float Overlap(Bounds a, Bounds b)
	{
		var shared = Vector3.Min(a.max, b.max) - Vector3.Max(a.min, b.min);
		if (shared.x < 0 || shared.y < 0 || shared.z < 0)
			return 0.0f;
		return Volume(shared) / Mathf.Min(Volume(a.size), Volume(b.size));
	}

	static float Volume(Vector3 size)
	{
		// Flat pieces (planes, decals) still count, by treating a zero side as a sliver
		return Mathf.Max(size.x, 1e-3f) * Mathf.Max(size.y, 1e-3f) * Mathf.Max(size.z, 1e-3f);
	}

	// Outline shaders push the outline out in object units, which are world units once the
	// level is merged into one mesh, so give the preview a copy with the outline turned off
	static Material PreviewMaterial(Material mat)
	{
		if (!mat.HasProperty("_Outline") || mat.GetFloat("_Outline") == 0)
			return mat;

		string folder = OutputFolder + "/Materials";
		if (!AssetDatabase.IsValidFolder(folder))
			AssetDatabase.CreateFolder(OutputFolder, "Materials");
		string path = folder + "/" + mat.name + " NoOutline.mat";
		var copy = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (copy == null)
		{
			copy = new Material(mat);
			AssetDatabase.CreateAsset(copy, path);
		}
		copy.CopyPropertiesFromMaterial(mat);
		copy.SetFloat("_Outline", 0);
		EditorUtility.SetDirty(copy);
		AssetDatabase.SaveAssetIfDirty(copy);
		return copy;
	}

	static bool IsSolid(MeshRenderer renderer)
	{
		var root = renderer.transform.root.name;
		if (SkipRootPrefixes.Any(p => root.StartsWith(p)))
			return false;

		var mat = renderer.sharedMaterial;
		if (mat == null || mat.shader == null)
			return false;

		string shader = mat.shader.name;
		if (SkipShaderParts.Any(p => shader.Contains(p)))
			return false;

		// Ignore anything absurdly large (skydomes and the like)
		return renderer.bounds.size.magnitude < 3500.0f;
	}
}
