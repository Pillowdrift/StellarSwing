using UnityEngine;
using System.Collections.Generic;

public class PlayerExplode : MonoBehaviour
{
	public GameObject ExplosionPrefab;

	// Renderers hidden by the explosion, shown again when the drone is reset
	private readonly List<Renderer> hidden = new List<Renderer>();

	// Explode
	void Explode()
	{
		Instantiate(ExplosionPrefab, transform.position, transform.rotation);

		SoundManager.Play("bang");

		// Stop the drone where it blew up. Grids and planets are only triggers, so
		// otherwise it carries on flying through them after the explosion
		var body = GetComponent<Rigidbody>();
		if (body != null && !body.isKinematic)
		{
			body.linearVelocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
			body.constraints = RigidbodyConstraints.FreezeAll;
		}

		// Hide everything on it, not just the body: hats, glow, particles
		foreach (var r in GetComponentsInChildren<Renderer>())
		{
			if (r.enabled)
			{
				r.enabled = false;
				hidden.Add(r);
			}
		}
	}

	// Sent when the player is reset after dying
	void Reload()
	{
		foreach (var r in hidden)
		{
			if (r != null)
				r.enabled = true;
		}
		hidden.Clear();
	}
}
