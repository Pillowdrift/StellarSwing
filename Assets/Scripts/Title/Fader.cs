using UnityEngine;
using System.Collections;

public class Fader : MonoBehaviour
{
	public float fadeRate = 1.0f;
	
	public bool invisibleOnStart = false;
	
	bool fadeout = false;
	bool fadein = false;
	
	void Update()
	{
		if (fadeout || fadein)
		{
			Color c = GetComponent<Renderer>().material.color;
			c.a = Mathf.Lerp(c.a, (fadeout ? 0 : 1), Time.deltaTime * fadeRate);
			GetComponent<Renderer>().material.color = c;
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
