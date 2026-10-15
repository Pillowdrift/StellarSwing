using UnityEngine;
using UnityEngine.UI;

// Shows the baked 3D level models (Assets/LevelPreviews) on the level-select cards.
// Each card slot (current / next) owns a small camera that renders its model into a
// RenderTexture, which is displayed by a RawImage laid over the card's flat preview image.
// The rig sits far from the playable title scene so nothing else is ever in frame.
public class LevelPreviewRig : MonoBehaviour
{
	[System.Serializable]
	public class Slot
	{
		public RawImage target;        // shown when a model exists for the level
		public Image fallback;         // the old flat preview, shown otherwise
		[HideInInspector] public Camera camera;
		[HideInInspector] public Transform pivot;
		[HideInInspector] public GameObject current;
		[HideInInspector] public RenderTexture texture;
		[HideInInspector] public Light light;
	}

	public GameObject[] models;
	public Slot currentSlot;
	public Slot nextSlot;
	public float spinDegreesPerSecond = 18.0f;
	public Vector2Int textureSize = new Vector2Int(768, 480);

	void Awake()
	{
		Build(currentSlot, 0);
		Build(nextSlot, 1);
	}

	void OnDestroy()
	{
		Release(currentSlot);
		Release(nextSlot);
	}

	private void Build(Slot slot, int index)
	{
		var root = new GameObject(index == 0 ? "PreviewCurrent" : "PreviewNext");
		root.transform.SetParent(transform, false);
		root.transform.localPosition = new Vector3(index * 60.0f, 0.0f, 0.0f);

		slot.pivot = new GameObject("Pivot").transform;
		slot.pivot.SetParent(root.transform, false);

		var camObject = new GameObject("Camera");
		camObject.transform.SetParent(root.transform, false);
		camObject.transform.localPosition = new Vector3(0.0f, 4.6f, -10.5f);
		camObject.transform.LookAt(root.transform.position + Vector3.up * 0.3f);
		slot.camera = camObject.AddComponent<Camera>();
		slot.camera.clearFlags = CameraClearFlags.SolidColor;
		slot.camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		slot.camera.fieldOfView = 32.0f;
		slot.camera.nearClipPlane = 0.5f;
		slot.camera.farClipPlane = 80.0f;
		slot.camera.allowHDR = false;
		slot.camera.allowMSAA = true;

		var lightObject = new GameObject("Light");
		lightObject.transform.SetParent(root.transform, false);
		lightObject.transform.rotation = Quaternion.Euler(38.0f, -35.0f, 0.0f);
		var light = lightObject.AddComponent<Light>();
		light.type = LightType.Directional;
		light.intensity = 1.0f;
		light.color = new Color(1.0f, 0.93f, 0.85f);
		// Directional lights affect the whole scene, so this one is only on while the slot renders
		light.enabled = false;
		slot.light = light;

		slot.texture = new RenderTexture(textureSize.x, textureSize.y, 16, RenderTextureFormat.ARGB32);
		slot.texture.antiAliasing = 4;
		slot.camera.targetTexture = slot.texture;
		slot.camera.enabled = false; // rendered on demand while a model is showing

		if (slot.target != null)
		{
			slot.target.texture = slot.texture;
			slot.target.enabled = false;
		}
	}

	private void Release(Slot slot)
	{
		if (slot.texture != null)
			slot.texture.Release();
	}

	void Update()
	{
		Spin(currentSlot);
		Spin(nextSlot);
	}

	private void Spin(Slot slot)
	{
		if (slot.current == null)
			return;

		slot.pivot.Rotate(0.0f, spinDegreesPerSecond * Time.unscaledDeltaTime, 0.0f, Space.World);
		slot.light.enabled = true;
		slot.camera.Render();
		slot.light.enabled = false;
	}

	// Pull the camera back just far enough that the whole model stays in frame as it spins
	private void Frame(Slot slot)
	{
		var filter = slot.current.GetComponentInChildren<MeshFilter>();
		if (filter == null || filter.sharedMesh == null)
			return;

		Vector3 extents = filter.sharedMesh.bounds.extents;
		float horizontalRadius = Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);
		float vFov = slot.camera.fieldOfView * Mathf.Deg2Rad;
		float hFov = 2.0f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * (float)textureSize.x / textureSize.y);

		float viewDirY = 0.38f;
		float needed = Mathf.Max(
			(extents.y * 1.05f + horizontalRadius * viewDirY) / Mathf.Tan(vFov * 0.5f),
			horizontalRadius * 1.05f / Mathf.Tan(hFov * 0.5f));

		var cam = slot.camera.transform;
		var direction = new Vector3(0.0f, viewDirY, -1.0f).normalized;
		cam.position = slot.pivot.parent.position + direction * (needed * 0.82f);
		cam.LookAt(slot.pivot.parent.position);
	}

	// Called by the menu whenever a card's level changes
	public void Show(bool next, string levelName)
	{
		var slot = next ? nextSlot : currentSlot;

		if (slot.current != null)
			Destroy(slot.current);
		slot.current = null;

		GameObject model = null;
		if (models != null)
		{
			foreach (var candidate in models)
			{
				if (candidate != null && candidate.name == levelName)
				{
					model = candidate;
					break;
				}
			}
		}

		bool haveModel = model != null;
		if (haveModel)
		{
			slot.current = Instantiate(model, slot.pivot);
			slot.current.transform.localPosition = Vector3.zero;
			slot.current.transform.localRotation = Quaternion.identity;
			slot.pivot.localRotation = Quaternion.Euler(0.0f, 25.0f, 0.0f);
			Frame(slot);
		}

		if (slot.target != null)
			slot.target.enabled = haveModel;
		if (slot.fallback != null)
			slot.fallback.enabled = !haveModel;
	}
}
