using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class JukeBox : MonoBehaviour {
	
	// The static instance
	public static JukeBox Instance = null;
	
	// The AudioSource to play.
	public AudioClip clip;

	public static bool DontChangeMusic = false;
	
	// Create a static instance.
	void Awake()
	{	
		if (Instance == null)
		{
			Instance = this;
			
			// Make sure this object isn't destroyed when a new scene is loaded.
			DontDestroyOnLoad(this.gameObject);	
			
			// Play the music.
			OnLevelWasLoaded(0);		
		}
		else
		{
			// Play the music.
			Instance.Play(this.clip);
			
			DestroyImmediate(gameObject);
		}
	}
	
	void Update()
	{
		GetComponent<AudioSource>().volume = Settings.Current.MasterVolume * Settings.Current.MusicVolume * GUIController.MusicVolumeModifier;
	}

	void OnLevelWasLoaded(int levelNum)
	{
		StartCoroutine(SwitchMusic());
	}

	IEnumerator SwitchMusic()
	{
		yield return new WaitForEndOfFrame();
		switch (Application.loadedLevelName)
		{
    case "Title_new":
			if (!DontChangeMusic)
        Play(SoundManager.sounds["Title"]);
      break;
		case "Tutorial 1":
			Play(SoundManager.sounds["World 1"]);
			break;
		case "Credits":
			Play(SoundManager.sounds["Credits"]);
			break;
		case "Title":
			Play(SoundManager.sounds["Title"]);
			break;
		default:
			if (LevelSelectGUI.currentWorld == "World6")
			{
				Play(SoundManager.sounds["World 6"]);
			}
			else
			{
				Regex exp = new Regex("World (?<world>.*) Level (?<level>.*)");
				Match match = exp.Match(Application.loadedLevelName);
				
				if (match.Success)
				{
					Play(SoundManager.sounds["World " + match.Groups["world"].ToString()]);
				}
			}
			break;
		}
	}
	
	// Force plays some music.
	public void ForcePlay(AudioClip music)
	{
		AudioSource source = GetComponent<AudioSource>();
		
		if (source != null)		
		{
			clip = music;
			source.loop = true;
			source.GetComponent<AudioSource>().clip = clip; 
			source.volume = Settings.Current.MasterVolume * Settings.Current.MusicVolume;
			source.Play();
		}
	}
	
	// Play some music.
	public void Play(AudioClip music)
	{
		if (clip != null && clip == music)
			return;
		if (music == null)
			return;

		Debug.Log("Playing " + music.name);
		
		if (music != null && music.name.Contains("I am, therefore I shall be"))
		{
			GameObject thingy = Tutorial.ShowText("BGM", "BGM - Bwarch - I am, therefore I shall be",
			                                      0, TextAlignment.Right, TextAnchor.UpperRight, 0.95f, 0.95f);
			TextFader fader = thingy.AddComponent<TextFader>();
			fader.fadeRate = 1.0f;
			fader.delay = 2.0f;
			fader.FadeOut();

		}
			
		ForcePlay(music);
	}
}
