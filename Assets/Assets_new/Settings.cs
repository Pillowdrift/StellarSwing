using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
  public struct GameSettings
  {
    public enum DisplayModes
    {
      Fullscreen,
      Windowed
    }

    // General settings
    public bool TutorialEnabled;
    public float TurningSensitivity;

    // Graphics settings
    public DisplayModes DisplayMode;
    public (int, int) Resolution;
    public int AALevel;
    public int AnisoLevel;
    public bool VSync;

    // Sound settings
    public float MasterVolume;
    public float MusicVolume;
    public float SoundVolume;

    public static GameSettings Default {
      get {
        return new GameSettings
        {
          // General
          TutorialEnabled = true,
          TurningSensitivity = 1.0f,
          // Graphics
          DisplayMode = DisplayModes.Fullscreen,
          Resolution = (Screen.currentResolution.width, Screen.currentResolution.height),
          AALevel = 4,
          AnisoLevel = 1,
          VSync = true,
          // Sound
          MasterVolume = 1.0f,
          MusicVolume = 1.0f,
          SoundVolume = 1.0f
        };
      }
    }
  }

  public static bool Current_InitiallyLoaded = false;

  public static GameSettings Current { get; private set; }

  public static GameSettings New = Current;

  // Gui stuff
  public Toggle tutorialToggle;
  public Slider sensitivitySlider;
  public Dropdown displayModeDropdown;
  public Dropdown resolutionDropdown;
  public Dropdown antialiasingDropdown;
  public Dropdown filteringDropdown;
  public Dropdown vsyncDropdown;
  public Slider masterVolumeSlider;
  public Slider musicVolumeSlider;
  public Slider soundVolumeSlider;

  public Button optionsConfirmButton;

  private const string optionsfile = "new_settings.txt";

  public void Flush()
  {
    Current = New;
    UpdateConfirmButtonState();
    Save(Current);
    Apply(Current);
  }

  private void Apply(GameSettings values)
  {
    // Apply current settings to the actual game

    // DisplayMode and Resolution
    bool enableFullscreen = values.DisplayMode == GameSettings.DisplayModes.Fullscreen;
    if (Current.Resolution.Item1 != Screen.width
      || Current.Resolution.Item2 != Screen.height
      || enableFullscreen != Screen.fullScreen)
    {
      Screen.SetResolution(values.Resolution.Item1, values.Resolution.Item2, enableFullscreen);
    }

    // AA, Filtering and vsync
    QualitySettings.antiAliasing = values.AALevel;
    QualitySettings.anisotropicFiltering = values.AnisoLevel == 1 ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
    QualitySettings.vSyncCount = values.VSync ? 1 : 0;
  }

  public void Save(GameSettings values)
  {
    // Save settings to file
		using (StreamWriter w = File.CreateText(Application.persistentDataPath + "/" + optionsfile))
		{
			w.WriteLine("TutorialEnabled=" + values.TutorialEnabled);
			w.WriteLine("TurningSensitivity=" + values.TurningSensitivity);
			w.WriteLine("DisplayMode=" + values.DisplayMode);
			w.WriteLine("Resolution=" + values.Resolution);
			w.WriteLine("AALevel=" + values.AALevel);
			w.WriteLine("AnisoLevel=" + values.AnisoLevel);
			w.WriteLine("VSync=" + values.VSync);
			w.WriteLine("MasterVolume=" + values.MasterVolume);
			w.WriteLine("MusicVolume=" + values.MusicVolume);
			w.WriteLine("SoundVolume=" + values.SoundVolume);
		}
  }

  public GameSettings Load()
  {
    try
    {
      var lines = File.ReadAllLines(Application.persistentDataPath + "/" + optionsfile);

      GameSettings values = GameSettings.Default;

      foreach (var line in lines)
      {
        var split = line.Split('=');
        if (split == null || split.Length < 2)
        {
          Debug.LogWarning("Failed to parse settings line: " + line);
          continue;
        }

        var key = split[0];
        var value = split[1];

        switch (key)
        {
          case "TutorialEnabled":
            if (!bool.TryParse(value, out bool tutorialEnabled))
              Debug.LogWarning("Failed to parse tutorialEnabled " + line);
            else
              values.TutorialEnabled = tutorialEnabled;
            break;
          case "TurningSensitivity":
            if (!float.TryParse(value, out float turningSensitivity))
              Debug.LogWarning("Failed to parse turningSensitivity " + line);
            else
              values.TurningSensitivity = turningSensitivity;
            break;
          case "DisplayMode":
            if (!GameSettings.DisplayModes.TryParse(value, out GameSettings.DisplayModes displayMode))
              Debug.LogWarning("Failed to parse displayMode " + line);
            else
              values.DisplayMode = displayMode;
            break;
          case "Resolution":
            var res = ParseResolutionTuple(value);
            if (res.HasValue)
              values.Resolution = res.Value;
            else
              Debug.LogWarning("Failed to parse resolution " + line);
            break;
          case "AALevel":
            if (!int.TryParse(value, out int aaLevel))
              Debug.LogWarning("Failed to parse aaLevel " + line);
            else
              values.AALevel = aaLevel;
            break;
          case "AnisoLevel":
            if (!int.TryParse(value, out int anisoLevel))
              Debug.LogWarning("Failed to parse anisoLevel " + line);
            else
              values.AnisoLevel = anisoLevel;
            break;
          case "VSync":
            if (!bool.TryParse(value, out bool vsync))
              Debug.LogWarning("Failed to parse vsync " + line);
            else
              values.VSync = vsync;
            break;
          case "MasterVolume":
            if (!float.TryParse(value, out float masterVolume))
              Debug.LogWarning("Failed to parse masterVolume " + line);
            else
              values.MasterVolume = masterVolume;
            break;
          case "MusicVolume":
            if (!float.TryParse(value, out float musicVolume))
              Debug.LogWarning("Failed to parse musicVolume " + line);
            else
              values.MusicVolume = musicVolume;
            break;
          case "SoundVolume":
            if (!float.TryParse(value, out float soundVolume))
              Debug.LogWarning("Failed to parse soundVolume " + line);
            else
              values.SoundVolume = soundVolume;
            break;
          default:
            Debug.LogWarning("Unrecognised settings key for line " + line);
            break;
        }
      }

      return values;
    }
    catch (Exception)
    {
      return GameSettings.Default;
    }
  }

  private (int, int)? ParseResolutionTuple(string resolution)
  {
    var split = resolution.Replace("(", "").Replace(")", "").Replace(",", "").Split(' ');

    if (split != null && split.Length == 2 && int.TryParse(split[0], out int width) && int.TryParse(split[1], out int height))
      return (width, height);
    else
      return null;

  }

  public void Populate(GameSettings values)
  {
    Debug.Log("populating options gui");

    // Populate the gui
    // General
    tutorialToggle.isOn = values.TutorialEnabled;
    sensitivitySlider.value = values.TurningSensitivity;

    // Graphics
    PopulateDisplayMode(values, displayModeDropdown);
    PopulateResolution(values, resolutionDropdown);
    PopulateAA(values, antialiasingDropdown);
    PopulateFiltering(values, filteringDropdown);
    PopulateVSync(values, vsyncDropdown);

    // Sounds
    masterVolumeSlider.value = values.MasterVolume;
    musicVolumeSlider.value = values.MusicVolume;
    soundVolumeSlider.value = values.SoundVolume;
  }

  private void PopulateDisplayMode(GameSettings values, Dropdown dropdown)
  {
    dropdown.value = values.DisplayMode == GameSettings.DisplayModes.Fullscreen ? 0 : 1;
  }

  private void PopulateResolution(GameSettings values, Dropdown dropdown)
  {
    // Configure options
    List<string> options = new List<string>();

    foreach (var resolution in Screen.resolutions)
    {
      string text = resolution.width + " x " + resolution.height;
      options.Add(text);
    }

    options.Reverse();
    dropdown.ClearOptions();
    dropdown.AddOptions(options);

    // Now find the one that fits
    string correctResolution = $"{values.Resolution.Item1} x {values.Resolution.Item2}";
    Debug.Log("Populating resolution gui, (" + dropdown.options.Count + $" options - looking for {correctResolution})");

    int rightOption = -1;

    for (int i = 0; i < dropdown.options.Count; ++i)
    {
      Debug.Log("Resolution option: " + dropdown.options[i].text);
      if (dropdown.options[i].text == correctResolution)
      {
        rightOption = i;
        break;
      }
    }

    if (rightOption == -1)
    {
      Debug.LogWarning("Failed to find right resolution option");
    }
    else
    {
      Debug.Log("Setting resolution dropdown value " + rightOption);
      dropdown.value = rightOption;
    }
  }

  private void PopulateAA(GameSettings values, Dropdown dropdown)
  {
    // Now find the one that fits
    string correctAAMode = values.AALevel == 0 ? "None" : $"MSAA {values.AALevel}x";
    Debug.Log("Populating AA gui, (" + dropdown.options.Count + $" options - looking for {correctAAMode})");

    int rightOption = -1;

    for (int i = 0; i < dropdown.options.Count; ++i)
    {
      Debug.Log("AA option: " + dropdown.options[i].text);
      if (dropdown.options[i].text == correctAAMode)
      {
        rightOption = i;
        break;
      }
    }

    if (rightOption == -1)
    {
      Debug.LogWarning("Failed to find right AA option");
    }
    else
    {
      Debug.Log("Setting AA dropdown value " + rightOption);
      dropdown.value = rightOption;
    }

  }

  private void PopulateFiltering(GameSettings values, Dropdown dropdown)
  {
    dropdown.value = values.AnisoLevel;
  }

  private void PopulateVSync(GameSettings values, Dropdown dropdown)
  {
    dropdown.value = values.VSync ? 1 : 0;
  }

  public void Start()
  {
    // If we didn't initialise Current yet do it here
    // We do this because we can't access the resolution in a static constructor
    if (!Current_InitiallyLoaded)
    {
      Current_InitiallyLoaded = true;
      Current = GameSettings.Default;
    }

    Debug.Log("Loading settings");
    Current = Load();
    New = Current;
    Apply(Current);
    Populate(Current);
  }

  public void InitPage()
  {
    New = Current;
    Populate(Current);
    UpdateConfirmButtonState();
  }

  public void UpdateConfirmButtonState()
  {
    Debug.Log("Updating confirm button state: " + New.Equals(Current));
    optionsConfirmButton.interactable = !New.Equals(Current);
  }

  public void SetTutorialEnabled(bool enabled)
  {
    Debug.Log("Setting tutorial enabled: " + enabled);
    New.TutorialEnabled = enabled;
    UpdateConfirmButtonState();
  }

  public void SetTurningSensitivity(float sensitivity)
  {
    Debug.Log("Setting turning sensitivity: " + sensitivity);
    New.TurningSensitivity = sensitivity;
    UpdateConfirmButtonState();
  }

  public void SetDisplayMode(int option)
  {
    New.DisplayMode = option == 0 ? GameSettings.DisplayModes.Fullscreen : GameSettings.DisplayModes.Windowed;
    UpdateConfirmButtonState();
    Debug.Log("Setting display mode to: " + New.DisplayMode);
  }

  public void SetScreenResolution(int option)
  {
    string optionText = resolutionDropdown.options[option].text;
    Debug.Log("Setting resolution to: " + optionText);

    var split = optionText.Split(' ');

    if (split != null && int.TryParse(split[0], out int width) && int.TryParse(split[2], out int height))
    {
      New.Resolution = (width, height);
      UpdateConfirmButtonState();
      Debug.Log("Set resolution to " + New.Resolution);
    }
  }

  public void SetAAOption(int option)
  {
    string optionText = antialiasingDropdown.options[option].text;
    Debug.Log("Setting aa to: " + optionText);

    int level = 0;

    switch (optionText)
    {
      case "MSAA 1x":
        level = 1;
        break;
      case "MSAA 2x":
        level = 2;
        break;
      case "MSAA 4x":
        level = 4;
        break;
      case "MSAA 8x":
        level = 8;
        break;
      default:
        level = 0;
        break;
    }

    New.AALevel = level;
    UpdateConfirmButtonState();
    Debug.Log("Set aa level to " + New.AALevel);
  }

  public void SetTextureFiltering(int option)
  {
    New.AnisoLevel = option;
    UpdateConfirmButtonState();
    Debug.Log("Set aniso to " + New.AnisoLevel);
  }

  public void SetVSync(int option)
  {
    New.VSync = option == 1;
    UpdateConfirmButtonState();
    Debug.Log("Set vsync to " + New.VSync);
  }

  public void SetMasterVolume(float value)
  {
    New.MasterVolume = value;
    UpdateConfirmButtonState();
    Debug.Log("Set master volume to " + New.MasterVolume);
  }

  public void SetMusicVolume(float value)
  {
    New.MusicVolume = value;
    UpdateConfirmButtonState();
    Debug.Log("Set music volume to " + New.MusicVolume);
  }

  public void SetSoundVolume(float value)
  {
    New.SoundVolume = value;
    UpdateConfirmButtonState();
    Debug.Log("Set sound volume to " + New.SoundVolume);
  }
}
