using UnityEngine;
using UnityEngine.SceneManagement;

// Slowly tumbling asteroids drifting far out over the skybox, to make the background feel
// alive. Installs itself into every level (any scene with a Player), follows the camera's
// position but not its rotation so the rocks behave as if very distant, and reads its
// density from Settings > Graphics > Distant asteroids (0 = off, 1 = low, 2 = high).
public class DistantAsteroids : MonoBehaviour
{
	const float Radius = 2500.0f;
	static readonly int[] MaxRocks = { 0, 45, 130 };

	static DistantAsteroids instance;
	static int level = 2;

	ParticleSystem system;
	Transform target;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Install()
	{
		SceneManager.sceneLoaded += (scene, mode) => Spawn();
		Spawn();
	}

	// Called by Settings when the option changes
	public static void SetLevel(int newLevel)
	{
		level = Mathf.Clamp(newLevel, 0, MaxRocks.Length - 1);
		if (instance != null)
			instance.ApplyLevel(true);
	}

	static void Spawn()
	{
		if (instance != null || GameObject.Find("Player") == null || Camera.main == null)
			return;

		var mesh = Resources.Load<Mesh>("DistantAsteroids/Rock");
		var material = Resources.Load<Material>("DistantAsteroids/DistantRock");
		if (mesh == null || material == null)
			return;

		var go = new GameObject("DistantAsteroids");
		instance = go.AddComponent<DistantAsteroids>();
		instance.target = Camera.main.transform;
		instance.Build(mesh, material);
	}

	void Build(Mesh mesh, Material material)
	{
		system = gameObject.AddComponent<ParticleSystem>();
		system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		var main = system.main;
		main.loop = true;
		main.prewarm = true;
		main.duration = 60.0f;
		main.startLifetime = new ParticleSystem.MinMaxCurve(90.0f, 160.0f);
		main.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 14.0f);
		main.startSize3D = true;
		main.startSizeX = new ParticleSystem.MinMaxCurve(30.0f, 110.0f);
		main.startSizeY = new ParticleSystem.MinMaxCurve(30.0f, 110.0f);
		main.startSizeZ = new ParticleSystem.MinMaxCurve(30.0f, 110.0f);
		main.startRotation3D = true;
		main.startRotationX = new ParticleSystem.MinMaxCurve(0.0f, Mathf.PI * 2.0f);
		main.startRotationY = new ParticleSystem.MinMaxCurve(0.0f, Mathf.PI * 2.0f);
		main.startRotationZ = new ParticleSystem.MinMaxCurve(0.0f, Mathf.PI * 2.0f);
		main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.55f, 0.55f), Color.white);
		main.simulationSpace = ParticleSystemSimulationSpace.Local;
		main.scalingMode = ParticleSystemScalingMode.Local;
		main.playOnAwake = true;

		var emission = system.emission;
		emission.rateOverTime = 1.6f;

		var shape = system.shape;
		shape.shapeType = ParticleSystemShapeType.Sphere;
		shape.radius = Radius;
		shape.radiusThickness = 0.15f;
		shape.randomDirectionAmount = 1.0f;

		// Fade each rock in and out so none pop
		var colour = system.colorOverLifetime;
		colour.enabled = true;
		var fade = new Gradient();
		fade.SetKeys(
			new[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
			new[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.08f), new GradientAlphaKey(1.0f, 0.92f), new GradientAlphaKey(0.0f, 1.0f) });
		colour.color = fade;

		var rotation = system.rotationOverLifetime;
		rotation.enabled = true;
		rotation.separateAxes = true;
		rotation.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
		rotation.y = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
		rotation.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);

		var renderer = GetComponent<ParticleSystemRenderer>();
		renderer.renderMode = ParticleSystemRenderMode.Mesh;
		renderer.mesh = mesh;
		renderer.sharedMaterial = material;
		renderer.alignment = ParticleSystemRenderSpace.Local;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		renderer.enableGPUInstancing = true;

		ApplyLevel(false);
		system.Play();
	}

	void ApplyLevel(bool restart)
	{
		var main = system.main;
		main.maxParticles = MaxRocks[level];
		gameObject.SetActive(level > 0);
		if (restart && level > 0)
		{
			system.Clear();
			system.Simulate(60.0f, true, true);
			system.Play();
		}
	}

	void LateUpdate()
	{
		if (target == null)
		{
			if (Camera.main == null)
				return;
			target = Camera.main.transform;
		}
		// Follow position only: distant objects show no parallax as the camera moves
		transform.position = target.position;
	}

	void OnDestroy()
	{
		if (instance == this)
			instance = null;
	}
}
