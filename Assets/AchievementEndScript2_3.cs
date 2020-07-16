using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementEndScript2_3 : MonoBehaviour
{
  static bool triedUnlock = false;
  private void OnCollisionEnter(Collision collision)
  {
    if (!triedUnlock && collision.gameObject.name == "Player" && !AchievementRock2_3.HasTouchedRock())
    {
      AchievementUnlocker.MakeUnlocker("world2_3").UnlockAchievement();
      triedUnlock = true;
    }
  }
}
