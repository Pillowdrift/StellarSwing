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
		Vector3 spreadScale = Vector3.one;
		Vector3 spreadCentre = Vector3.zero;
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
