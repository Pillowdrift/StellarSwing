using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using Steamworks;

public class AchievementUnlocker : MonoBehaviour
{
  public string AchievementIDStr;
  public bool UnlockOnLoad = false;
  private bool unlocked;

  public static AchievementUnlocker MakeUnlocker(string AchievementIDStr)
  {
    var gameobj = Resources.Load<GameObject>("AchievementUnlocker");
    if (gameobj == null)
    {
      Debug.LogError("Error -- could not fabricate AchievementUnlocker");
    }
    AchievementUnlocker result = gameobj.GetComponent<AchievementUnlocker>();
    result.AchievementIDStr = AchievementIDStr;
    return result;
  }

  void Awake()
  {
    CheckActive();
  }

  bool CheckActive()
  {
    if (!SteamManager.Initialized || AchievementIDStr == null)
    {
      gameObject.SetActive(false);
      if (AchievementIDStr == null) Debug.LogError("AchievementIDStr not set in AchievementUnlocker!");
      return false;
    }
    return true;
  }


  public void UnlockAchievement()
  {
    if (!CheckActive()) return;
    SteamUserStats.GetAchievement(AchievementIDStr, out unlocked);
    if (!unlocked)
    {
      SteamUserStats.SetAchievement(AchievementIDStr);
      SteamUserStats.StoreStats();
    }
    else
    {
      Debug.Log("Already unlocked " + AchievementIDStr);
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    if (UnlockOnLoad)
    {
      UnlockAchievement();
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
