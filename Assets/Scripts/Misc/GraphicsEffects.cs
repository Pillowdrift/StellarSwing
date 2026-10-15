using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

// Post-processing for every scene: HDR + neutral tone mapping, bloom, ambient occlusion
// and a subtle vignette, driven by Settings > Graphics. A rig prefab in
// Resources/Graphics holds the global volume and the package's shader resources; it
// lives across scenes and attaches a PostProcessLayer to each scene's main camera.
public class GraphicsEffects : MonoBehaviour
{
	public const int VolumeLayer = 30;

	public PostProcessResources resources;

	static GraphicsEffects instance;
	static bool hdr = true;
	static int bloomLevel = 1;      // 0 off, 1 low, 2 high
	static bool ambientOcclusion = true;

	PostProcessVolume volume;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Install()
	{
		var prefab = Resources.Load<GameObject>("Graphics/PostProcessRig");
		if (prefab == null)
			return;

		var go = Instantiate(prefab);
		go.name = "PostProcessRig";
		DontDestroyOnLoad(go);
		instance = go.GetComponent<GraphicsEffects>();
		instance.volume = go.GetComponent<PostProcessVolume>();

		SceneManager.sceneLoaded += (scene, mode) => instance.AttachToCamera();
		instance.AttachToCamera();
		instance.ApplySettings();
	}

	// Called by Settings whenever graphics options change
	public static void Configure(bool useHdr, int bloom, bool ao)
	{
		hdr = useHdr;
		bloomLevel = Mathf.Clamp(bloom, 0, 2);
		ambientOcclusion = ao;
		if (instance != null)
		{
			instance.AttachToCamera();
			instance.ApplySettings();
		}
	}

	void AttachToCamera()
	{
		var cam = Camera.main;
		if (cam == null)
			return;

		cam.allowHDR = hdr;

		var layer = cam.GetComponent<PostProcessLayer>();
		if (layer == null)
		{
			layer = cam.gameObject.AddComponent<PostProcessLayer>();
			layer.Init(resources);
			layer.volumeTrigger = cam.transform;
			layer.volumeLayer = 1 << VolumeLayer;
			layer.antialiasingMode = PostProcessLayer.Antialiasing.None; // MSAA from QualitySettings is kept
		}
	}

	void ApplySettings()
	{
		var profile = volume.profile; // instance copy, so the asset is never modified at runtime

		if (profile.TryGetSettings(out Bloom bloom))
		{
			bloom.enabled.Override(bloomLevel > 0);
			bloom.intensity.Override(bloomLevel == 2 ? 1.6f : 0.8f);
			bloom.threshold.Override(hdr ? 1.0f : 0.92f);
			bloom.softKnee.Override(0.55f);
			bloom.diffusion.Override(bloomLevel == 2 ? 7.5f : 6.0f);
		}

		if (profile.TryGetSettings(out AmbientOcclusion ao))
		{
			ao.enabled.Override(ambientOcclusion);
			ao.mode.Override(AmbientOcclusionMode.MultiScaleVolumetricObscurance);
			ao.intensity.Override(0.6f);
			ao.thicknessModifier.Override(1.2f);
		}

		if (profile.TryGetSettings(out ColorGrading grading))
		{
			grading.enabled.Override(hdr);
			grading.gradingMode.Override(GradingMode.HighDefinitionRange);
			grading.tonemapper.Override(Tonemapper.Neutral);
		}

		if (profile.TryGetSettings(out Vignette vignette))
		{
			vignette.enabled.Override(true);
			vignette.intensity.Override(0.22f);
			vignette.smoothness.Override(0.45f);
		}

		var cam = Camera.main;
		if (cam != null)
			cam.allowHDR = hdr;
	}
}
