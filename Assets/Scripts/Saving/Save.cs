using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

/// <summary>
/// Recording
/// Serialized to save game
/// Fields can be added (and will assume their default value if not available in the file)
/// but removing or renaming a field will result in an exception when a file is next loaded
/// </summary> 
[Serializable]
public class Save
{
  public const string dir = "";
  public const int VERSION = 5;

  [NonSerialized]
  public string filename;

  public int worldUnlocked;
  public int levelUnlocked;
  public string playerName = "Name";
  public int droneCount = 0;
  public int picolinium = 0;

  // stats
  public float playTime = 0.0f;
  public int picoliniumFoundTotal = 0;
  public float highestSpeed = 0.0f;
  public int levelCompletions = 0;

  public HashSet<string> unlocks = new HashSet<string>();
  public string currentUpgrade = "";
  public string currentHat = "";

  public Save()
  {
    worldUnlocked = 1;
    levelUnlocked = 1;
  }

  // The high scores.
  [Serializable]
  public struct LevelHighScore
  {
    public float Score;
    public float Speed;
    public float Time;

    public int Stars;
  }
  //public LevelHighScore[] highScores = new LevelHighScore[255];
  public Dictionary<(int, int), LevelHighScore> levelHighscores = new Dictionary<(int, int), LevelHighScore>();

  // Options
  public bool OnlineEnabled = true;
  public float BGMSound = 1.0f;

  // Get a high score for a level.
  public LevelHighScore GetHighScore(int world, int number)
  {
    if (levelHighscores.TryGetValue((world, number), out LevelHighScore value))
      return value;
    else
      return new LevelHighScore
      {
        Score = -1.0f,
        Speed = -1.0f,
        Time = -1.0f,
        Stars = -1
      };
  }

  // Update the high score for a level.
  public void UpdateHighScore(LevelHighScore _score, int world, int number)
  {
    // Get the current scores
    LevelHighScore? hs = GetHighScore(world, number);
    if (hs == null)
    {
      Debug.Log("Unknown level " + world + ", " + number);
      return;
    }
    LevelHighScore current = hs.Value;

    // Compare them and update them.
    if (_score.Score > current.Score)
      current.Score = _score.Score;
    if (_score.Speed > current.Speed)
      current.Speed = _score.Speed;
    if (_score.Time < current.Time || current.Time == 0)
      current.Time = _score.Time;
    if (_score.Stars > current.Stars)
      current.Stars = _score.Stars;

    // Set the score.
    levelHighscores[(world, number)] = current;
  }

  public string Write()
  {
    return WriteAsXml(filename);
  }

  private void WriteValue<T>(StreamWriter writer, string name, T val)
  {
    writer.WriteLine($"{name}={val}");
  }

  private void WriteValues(StreamWriter writer, object values)
  {
    foreach (var prop in values.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
    {
      var name = prop.Name;
      var value = prop.GetValue(values);
      writer.WriteLine($"{name}={value}");
    }
  }

  private static void ReadValue<T>(string line, string name, ref T outVal)
  {
    try
    {
      string[] split = line.Split('=');
      if (split.Length == 2)
      {
        if (split[0] == name)
        {
          outVal = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(split[1]);
        }
      }
    }
    catch
    { }
  }

  private static void ReadHighscore(string line, ref Save save)
  {
    try
    {
      string[] split = line.Split('=');
      if (split.Length == 2)
      {
        if (split[0].StartsWith("highscores."))
        {
          string[] parts = split[0].Split('.');
          if (parts.Length == 4)
            if (int.TryParse(parts[1], out int world))
              if (int.TryParse(parts[2], out int level))
              {
                var hs = save.GetHighScore(world, level);
                if (parts[3] == "time")
                  float.TryParse(split[1], out hs.Time);
                else if (parts[3] == "stars")
                  int.TryParse(split[1], out hs.Stars);
                else if (parts[3] == "score")
                  float.TryParse(split[1], out hs.Score);
                else if (parts[3] == "speed")
                  float.TryParse(split[1], out hs.Speed);
                save.UpdateHighScore(hs, world, level);
              }
        }
      }
    }
    catch
    { }
  }

  private static void ReadUpgrades(string line, ref Save save)
  {
    try
    {
      string[] split = line.Split('=');
      if (split.Length == 2)
      {
        if (split[0] == "unlocks")
        {
          string[] unlockNames = split[1].Split(',');
          foreach (string unlockName in unlockNames)
          {
            save.unlocks.Add(unlockName);
          }
        }
      }
    }
    catch
    { }
  }

  public string WriteAsXml(string filename)
  {
    // Get the full path.
    string fullfilename = Application.persistentDataPath + dir + "/" + filename;
    Directory.CreateDirectory(Application.persistentDataPath + dir);

    // Open the file.
    FileStream file = new FileStream(fullfilename, FileMode.Create, FileAccess.Write);
    var writer = new StreamWriter(file);

    // Write the rest of the data.
    WriteValues(writer, new { worldUnlocked, levelUnlocked, playerName, droneCount, picolinium, playTime, picoliniumFoundTotal, highestSpeed, levelCompletions });

    // Write the high scores
    foreach (KeyValuePair<(int, int), LevelHighScore> highscore in levelHighscores)
    {
      // Write each element.
      var world = highscore.Key.Item1;
      var level = highscore.Key.Item2;
      WriteValue(writer, $"highscores.{world}.{level}.score", highscore.Value.Score);
      WriteValue(writer, $"highscores.{world}.{level}.speed", highscore.Value.Speed);
      WriteValue(writer, $"highscores.{world}.{level}.time", highscore.Value.Time);
      WriteValue(writer, $"highscores.{world}.{level}.stars", highscore.Value.Stars);
    }

    // Write unlocks
    string unlockString = "";
    foreach (string unlock in unlocks)
    {
      unlockString += (unlockString.Length == 0 ? "" : ",") + unlock;
    }
    WriteValue(writer, "unlocks", unlockString);
    WriteValue(writer, "currentUpgrade", currentUpgrade);
    WriteValue(writer, "currentHat", currentHat);

    // Cool we're done.
    writer.Close();
    file.Close();

    // Return the full filename just in case something needs it.
    return fullfilename;
  }

  private static Save ReadAsXml(string filename)
  {
    Save save = new Save();

    try
    {
      var lines = File.ReadAllLines(filename);

      foreach (var line in lines)
      {
        ReadValue(line, nameof(worldUnlocked), ref save.worldUnlocked);
        ReadValue(line, nameof(levelUnlocked), ref save.levelUnlocked);
        ReadValue(line, nameof(playerName), ref save.playerName);
        ReadValue(line, nameof(droneCount), ref save.droneCount);
        ReadValue(line, nameof(picolinium), ref save.picolinium);
        ReadValue(line, nameof(playTime), ref save.playTime);
        ReadValue(line, nameof(highestSpeed), ref save.highestSpeed);
        ReadValue(line, nameof(levelCompletions), ref save.levelCompletions);
        ReadValue(line, nameof(picoliniumFoundTotal), ref save.picoliniumFoundTotal);
        ReadValue(line, nameof(currentHat), ref save.currentHat);
        ReadValue(line, nameof(currentUpgrade), ref save.currentUpgrade);
        ReadHighscore(line, ref save);
        ReadUpgrades(line, ref save);
      }
    }
    catch
    { }

    return save;
  }

  public static Save Read(string filename)
  {
    string fullfilename = Application.persistentDataPath + dir + "/" + filename;

    if (!File.Exists(fullfilename))
      return null;

    Save read = ReadAsXml(fullfilename);
    read.filename = filename;

    return read;
  }
  public void IncrementPicolinium(int count)
  {
    picolinium += count;
    picoliniumFoundTotal += count;
  }
}
