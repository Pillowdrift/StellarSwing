using UnityEngine;
using System.Collections;

public class PlayButtonFader : MonoBehaviour {
	
	static float fadeStartTime;
	static float fadeTo = 1;
	
	const float DELAY = 1.0f;
	const float RATE = 10.0f;
	
	// Use this for initialization
	void Start () 
	{
		Reset();
	}
	
	void Reset()
	{
		fadeStartTime = Time.time + DELAY;
		fadeTo = 1;
		
		Color c = GetComponent<Renderer>().material.color;
		c.a = 0;
		GetComponent<Renderer>().material.color = c;
	}
	
	// Update is called once per frame
	void Update () 	
	{
		if(Time.time > fadeStartTime)
		{
			Color c = GetComponent<Renderer>().material.color;
			c.a = Mathf.Lerp(c.a, fadeTo, Time.deltaTime * RATE);
			GetComponent<Renderer>().material.color = c;
		}		
	}
	
	public static void FadeOut()
	{
		fadeTo = 0;
	}
}
