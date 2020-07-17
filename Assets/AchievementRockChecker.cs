using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementRockChecker : MonoBehaviour
{
  private static bool touchedRock;

  private void Start()
  {
    touchedRock = false;
  }

  private void OnCollisionEnter(Collision collision)
  {
    touchedRock = true;
  }

  public static bool HasTouchedRock()
  {
    return touchedRock;
  }
}
