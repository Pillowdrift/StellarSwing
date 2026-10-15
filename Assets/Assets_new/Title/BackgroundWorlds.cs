using UnityEngine;

// The five world planets sit on a ring around the title camera. Rotating the
// ring brings the selected world into the "hero" spot behind the menu, so
// changing world on the select screen flies the backdrop over to it.
public class BackgroundWorlds : MonoBehaviour
{
	[Tooltip("Planet roots in world order (index 0 = World 1)")]
	public Transform[] worlds;

	[Tooltip("Degrees between neighbouring planets on the ring")]
	public float spacingDegrees = 48.0f;

	[Tooltip("Seconds for a one-world turn; longer turns scale up a little")]
	public float turnSeconds = 1.4f;

	private Quaternion fromRotation;
	private Quaternion toRotation;
	private float progress = 1.0f;
	private float duration = 1.0f;
	private int currentWorld = -1;

	public int CurrentWorld { get { return currentWorld; } }

	// world is 1-based
	public void ShowWorld(int world, bool instant)
	{
		world = Mathf.Clamp(world, 1, worlds.Length);
		int steps = currentWorld < 0 ? 0 : Mathf.Abs(world - currentWorld);
		currentWorld = world;

		// Planet i was placed at yaw i*spacing; turning the ring by -yaw brings it to the front
		toRotation = Quaternion.Euler(0.0f, -(world - 1) * spacingDegrees, 0.0f);

		if (instant)
		{
			transform.localRotation = toRotation;
			progress = 1.0f;
			return;
		}

		fromRotation = transform.localRotation;
		duration = turnSeconds * (1.0f + 0.25f * Mathf.Max(steps - 1, 0));
		progress = 0.0f;
	}

	void Update()
	{
		if (progress >= 1.0f)
			return;

		progress = Mathf.Min(1.0f, progress + Time.deltaTime / duration);
		float eased = progress * progress * (3.0f - 2.0f * progress);
		transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, eased);
	}
}
