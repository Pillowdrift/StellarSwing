using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementEndScript4_3 : MonoBehaviour
{
  static bool triedUnlock = false;
  private void OnCollisionEnter(Collision collision)
  {
    if (!triedUnlock && collision.gameObject.name == "Player" && !AchievementRockChecker.HasTouchedRock())
    {
      AchievementUnlocker.MakeUnlocker("world4_3").UnlockAchievement();
      triedUnlock = true;
    }
  }
}
