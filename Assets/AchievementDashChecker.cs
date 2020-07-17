using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementDashChecker : MonoBehaviour
{
  private static HashSet<int> dashSet = new HashSet<int>();

  void Start()
  {
    dashSet = new HashSet<int>();
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.name == "Player")
    {
      dashSet.Add(GetInstanceID());
    }
  }

  public static int GetCount()
  {
    return dashSet.Count;
  }
}
