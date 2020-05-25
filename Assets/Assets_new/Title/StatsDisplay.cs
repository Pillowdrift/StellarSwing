using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsDisplay : MonoBehaviour
{
  public enum Stat
  {
    PlayTime,
    HighestSpeed,
    LevelCompletions,
    PicoliniumFound,
    DronesLost
  }

  public Stat TheStat = Stat.PlayTime;

  private int currentPlayTime = -1;

  private Text text;

  void Start()
  {
    text = GetComponent<Text>();

    if (SaveManager.save == null)
      return;

    var save = SaveManager.save;

    if (TheStat == Stat.HighestSpeed)
    {
      var highestSpeed = string.Format("{0:0.0}", save.highestSpeed);
      text.text = $"{highestSpeed}m/s";
    }
    else if (TheStat == Stat.LevelCompletions)
    {
      text.text = $"{save.levelCompletions}";
    }
    else if (TheStat == Stat.PicoliniumFound)
    {
      text.text = $"{save.picoliniumFoundTotal}";
    }
    else if (TheStat == Stat.DronesLost)
    {
      text.text = $"{save.droneCount}";
    }
  }

  void LateUpdate()
  {
    if (SaveManager.save == null)
      return;

    if (TheStat == Stat.PlayTime)
    {
      int curValue = (int)SaveManager.save.playTime;
      if (curValue != currentPlayTime)
      {
        currentPlayTime = curValue;
        text.text = FormatPlaytime(currentPlayTime);
      }
    }
  }
  public static string FormatPlaytime(int seconds)
  {
    TimeSpan t = TimeSpan.FromSeconds(seconds);
    return string.Format(
      "{0:D2}:{1:D2}:{2:D2}",
      (int)t.TotalHours,
      t.Minutes,
      t.Seconds);
  }

}
