using UnityEngine;
using System.Collections;

public class AmbientNoise : MonoBehaviour
{
	private static AmbientNoise instance;
	
	void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
	
	void Update()
	{
		GetComponent<AudioSource>().volume = Options.SFXVolume;
		transform.position = Camera.main.transform.position;
	}
	
	public static void Play()
	{
		if (instance != null && !instance.GetComponent<AudioSource>().isPlaying)
			instance.GetComponent<AudioSource>().Play();
	}
	
	public static void Stop()
	{
		if (instance != null)
			instance.GetComponent<AudioSource>().Stop();
	}
}
