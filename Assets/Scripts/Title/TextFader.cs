using UnityEngine;
using System.Collections;

public class TextFader : MonoBehaviour
{
	public float fadeRate = 1.0f;
	public float delay = 0.0f;
	
	public bool invisibleOnStart = false;
	
	bool fadeout = false;
	bool fadein = false;

	private float startTime;

	void Start()
	{
		startTime = Time.time;
	}
	
	void Update()
	{
		if (Time.time < startTime + delay)
			return;

		if (fadeout || fadein)
		{
			Color c = GetComponent<GUIText>().material.color;
			c.a = Mathf.Lerp(c.a, (fadeout ? 0 : 1), Time.deltaTime * fadeRate);
			GetComponent<GUIText>().material.color = c;
		}
	}
	
	public void FadeOut()
	{
		fadeout = true;
		fadein = false;
		if (GetComponent<Collider>() != null)
			GetComponent<Collider>().enabled = false;
	}
	
	public void FadeIn()
	{
		fadein = true;
		fadeout = false;
		if (GetComponent<Collider>() != null)
			GetComponent<Collider>().enabled = true;
	}
}
