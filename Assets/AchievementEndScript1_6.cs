using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementEndScript1_6 : MonoBehaviour
{
  private AchievementUnlocker unlocker;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.name != "Player")
      return;

    if (unlocker == null && AchievementCube1_6.GetCount() == 0)
    {
      unlocker = AchievementUnlocker.MakeUnlocker("world1_6");
      unlocker.UnlockAchievement();
    }
  }

}
