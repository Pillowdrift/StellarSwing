using UnityEngine;

// Moves the splash mascot across the screen at a steady pace, in camera space,
// so the path is predictable regardless of camera orientation.
public class SplashDrift : MonoBehaviour
{
	// Units per second along the camera's right and up axes
	public Vector2 driftPerSecond = new Vector2(1.0f, -0.12f);

	void Update()
	{
		Camera cam = Camera.main;
		if (cam == null)
			return;

		Transform c = cam.transform;
		transform.position += (c.right * driftPerSecond.x + c.up * driftPerSecond.y) * Time.deltaTime;
	}
}
