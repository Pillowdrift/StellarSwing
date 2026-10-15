using UnityEngine;

// Keeps a quad's front face turned toward the main camera every frame.
// With an anchor set, the quad also sits `pushBack` units behind the anchor
// (away from the camera), so it reads as a halo behind the anchored object.
public class Billboard : MonoBehaviour
{
	public Transform anchor;
	public float pushBack = 0.0f;

	void LateUpdate()
	{
		Camera cam = Camera.main;
		if (cam == null)
			return;

		if (anchor != null)
		{
			Vector3 away = (anchor.position - cam.transform.position).normalized;
			transform.position = anchor.position + away * pushBack;
		}

		transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, cam.transform.up);
	}
}
