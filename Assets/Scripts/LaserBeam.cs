using UnityEngine;

// Makes the grapple line read as a laser: scrolls the beam texture along its length so
// the energy bands travel from the drone to the anchor, and adds a gentle flicker.
[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
	public float scrollSpeed = -3.0f;   // texture repeats per second
	public float flicker = 0.08f;       // fractional width wobble

	LineRenderer line;
	Material material;
	float baseWidth;

	void Awake()
	{
		line = GetComponent<LineRenderer>();
		material = line.material;          // instance, so the shared asset is untouched
		baseWidth = line.widthMultiplier;
	}

	void Update()
	{
		material.mainTextureOffset = new Vector2(Time.time * scrollSpeed, 0.0f);
		float wobble = 1.0f + flicker * (Mathf.PerlinNoise(Time.time * 18.0f, 0.0f) - 0.5f) * 2.0f;
		line.widthMultiplier = baseWidth * wobble;
	}
}
