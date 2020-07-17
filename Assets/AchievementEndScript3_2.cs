using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementEndScript3_2 : MonoBehaviour
{
  private static bool triedUnlock = false;

  void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.name != "Player")
      return;

    if (!triedUnlock && AchievementDashChecker.GetCount() == 0)
    {
      AchievementUnlocker.MakeUnlocker("world3_2").UnlockAchievement();
      triedUnlock = true;
    }
  }

}
