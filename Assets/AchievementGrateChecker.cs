using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementGrateChecker : MonoBehaviour
{
  private static HashSet<int> grateSet = new HashSet<int>();

  void Start()
  {
    grateSet = new HashSet<int>();
  }
  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.name == "Player")
    {
      grateSet.Add(GetInstanceID());
    }
  }

  public static int GetCount()
  {
    return grateSet.Count;
  }
}
