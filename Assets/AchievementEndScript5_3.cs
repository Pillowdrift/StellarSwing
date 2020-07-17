using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementEndScript5_3 : MonoBehaviour
{
  static bool triedUnlock = false;
  private void OnCollisionEnter(Collision collision)
  {
    if (!triedUnlock && collision.gameObject.name == "Player" && AchievementGrateChecker.GetCount() == 0)
    {
      AchievementUnlocker.MakeUnlocker("world5_3").UnlockAchievement();
      triedUnlock = true;
    }
  }
}
