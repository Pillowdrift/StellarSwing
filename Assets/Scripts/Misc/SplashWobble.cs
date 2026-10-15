using UnityEngine;

// Gentle idle motion for the splash mascot: a slow bob and a lazy tilt,
// optionally keeping the mascot turned toward the camera as it drifts.
public class SplashWobble : MonoBehaviour
{
	public float bobAmplitude = 0.25f;
	public float bobSpeed = 0.8f;
	public float tiltDegrees = 6.0f;
	public float tiltSpeed = 0.5f;
	public bool faceCamera = true;

	private Vector3 startLocalPosition;
	private Quaternion startRotation;

	void Start()
	{
		startLocalPosition = transform.localPosition;
		startRotation = transform.rotation;
	}

	void Update()
	{
		float t = Time.time;
		transform.localPosition = startLocalPosition + Vector3.up * (Mathf.Sin(t * bobSpeed) * bobAmplitude);

		Quaternion baseRotation = startRotation;
		Camera cam = Camera.main;
		if (faceCamera && cam != null)
		{
			// Turn the model's front (+Z) toward the camera, yaw only so it stays upright
			Vector3 toCamera = cam.transform.position - transform.position;
			toCamera.y = 0.0f;
			if (toCamera.sqrMagnitude > 0.0001f)
				baseRotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
		}

		transform.rotation = baseRotation * Quaternion.Euler(
			Mathf.Sin(t * tiltSpeed * 0.7f) * tiltDegrees * 0.4f,
			Mathf.Sin(t * tiltSpeed) * tiltDegrees,
			Mathf.Sin(t * tiltSpeed * 1.3f) * tiltDegrees * 0.5f);
	}
}
