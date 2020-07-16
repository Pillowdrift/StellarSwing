using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementCube1_6 : MonoBehaviour
{
  private static HashSet<int> collidedCubes;
  // Start is called before the first frame update
  void Start()
  {
    collidedCubes = new HashSet<int>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.tag != "Player")
      return;

    collidedCubes.Add(GetInstanceID());
  }
  

  public static int GetCount()
  {
    return collidedCubes.Count;
  }
}
