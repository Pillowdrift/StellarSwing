using UnityEngine;

// Keeps a quad's front face turned toward the main camera every frame,
// regardless of how its parent moves or rotates.
public class Billboard : MonoBehaviour
{
	void LateUpdate()
	{
		Camera cam = Camera.main;
		if (cam == null)
			return;
		transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, cam.transform.up);
	}
}
