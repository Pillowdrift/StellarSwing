using UnityEngine;

// Gentle idle motion for the splash mascot: a slow bob and a lazy tilt.
public class SplashWobble : MonoBehaviour
{
	public float bobAmplitude = 0.25f;
	public float bobSpeed = 0.8f;
	public float tiltDegrees = 6.0f;
	public float tiltSpeed = 0.5f;

	private Vector3 startLocalPosition;
	private Quaternion startLocalRotation;

	void Start()
	{
		startLocalPosition = transform.localPosition;
		startLocalRotation = transform.localRotation;
	}

	void Update()
	{
		float t = Time.time;
		transform.localPosition = startLocalPosition + Vector3.up * (Mathf.Sin(t * bobSpeed) * bobAmplitude);
		transform.localRotation = startLocalRotation * Quaternion.Euler(
			Mathf.Sin(t * tiltSpeed * 0.7f) * tiltDegrees * 0.4f,
			Mathf.Sin(t * tiltSpeed) * tiltDegrees,
			Mathf.Sin(t * tiltSpeed * 1.3f) * tiltDegrees * 0.5f);
	}
}
