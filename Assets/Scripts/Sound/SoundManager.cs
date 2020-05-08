using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
	[Serializable]
	public class SoundFile
	{
		public string name;
		public AudioClip sound;
	}
	
	public static Dictionary<string, AudioClip> sounds;
	
	public SoundFile[] soundfiles;
	
	private static AudioSource source;
	
	void Awake()
	{
		if (sounds == null)
		{
			// Move all sound files to dictionary
			sounds = new Dictionary<string, AudioClip>();
			foreach (SoundFile soundfile in soundfiles)
			{
				sounds.Add(soundfile.name, soundfile.sound);
			}
		}
	}
	
	void Start()
	{
		
	}
	
	public static void Play(string name)
	{
		// Play at volume set in Options
		Play(name, Settings.Current.MasterVolume * Settings.Current.SoundVolume);
	}
	
	public static void Play(string name, float volume)
	{
		if (source == null)
		{
			source = Camera.main.GetComponent<AudioSource>();
		}
		
		if (source != null)
		{
			AudioClip clip;
			
			if (sounds.TryGetValue(name, out clip))
			{
				source.PlayOneShot(clip, volume);
			}
		}
	}
	
	public static void StopAll() 
	{
		source.Stop();
	}
}
