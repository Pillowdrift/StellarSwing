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

  static readonly Dictionary<string, AchievementUnlocker> made = new Dictionary<string, AchievementUnlocker>();

  // An unlocker for code to call. This used to hand out the Resources prefab itself, which
  // wrote the ID into the prefab asset and, with Steam not running in the editor, switched the
  // asset off for good: every unlocker placed in a scene (the finish-world ones) then never ran.
  // Now each ID gets its own hidden object that's never saved.
  public static AchievementUnlocker MakeUnlocker(string AchievementIDStr)
  {
    if (made.TryGetValue(AchievementIDStr, out var existing) && existing != null)
      return existing;

    // Made inactive so Awake doesn't check it before the ID is set; unlocking doesn't need it active
    var gameobj = new GameObject("AchievementUnlocker " + AchievementIDStr);
    gameobj.SetActive(false);
    gameobj.hideFlags = HideFlags.HideAndDontSave;
    DontDestroyOnLoad(gameobj);
    var result = gameobj.AddComponent<AchievementUnlocker>();
    result.AchievementIDStr = AchievementIDStr;
    made[AchievementIDStr] = result;
    return result;
  }

  void Awake()
  {
    CheckActive();
  }

  // Whether this can unlock anything. It doesn't switch the object off when it can't: that
  // got saved into the prefab and broke every scene unlocker.
  bool CheckActive()
  {
    if (string.IsNullOrEmpty(AchievementIDStr))
    {
      Debug.LogError("AchievementIDStr not set in AchievementUnlocker!");
      return false;
    }
    return SteamManager.Initialized;
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
