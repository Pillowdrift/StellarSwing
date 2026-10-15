using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsUnlocker : MonoBehaviour
{

  private AchievementUnlocker speedyUnlocker;
  public GameObject SpeedrunUnlockerPrefab;

  private AchievementUnlocker noDeathUnlocker;
  public GameObject NoDeathUnlockerPrefab;

  void Start()
  {
    speedyUnlocker = GameObject.Instantiate(SpeedrunUnlockerPrefab).GetComponent<AchievementUnlocker>();

    noDeathUnlocker = GameObject.Instantiate(NoDeathUnlockerPrefab).GetComponent<AchievementUnlocker>();

    var minutes = SaveManager.save.playTime / 60;
    if (minutes <= 20)
    {
      speedyUnlocker.UnlockAchievement();
    }
    if (SaveManager.save.droneCount == 0)
    {
      noDeathUnlocker.UnlockAchievement();
    }
    Debug.Log("Beat game in " + minutes + " minutes");
    Debug.Log("Used " + SaveManager.save.droneCount + " drones");
  }

  void Update()
  {

  }
}
