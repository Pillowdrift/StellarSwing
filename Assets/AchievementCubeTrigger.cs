using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementCubeTrigger : MonoBehaviour
{

  private AchievementUnlocker Unlocker;

  private void Awake()
  {
    Unlocker = GameObject.Find("AchievementUnlocker").GetComponent<AchievementUnlocker>();
    if (Unlocker != null)
    {
      Debug.Log("FOUND UNLOCKER1");
    }
    else
    {
      Debug.LogError("DID NOT FIND UNLOCKER1");
    }
  }
  // Start is called before the first frame update
  void Start()
  {
    if (Unlocker != null)
    {
      Debug.Log("FOUND UNLOCKER2");
    }
    else
    {
      Debug.LogError("DID NOT FIND UNLOCKER2");
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnTriggerEnter(Collider other)
  {
    if (other != null && other.tag == "Player")
    {
      Debug.LogError("Trying to unlock achievement");
      Unlocker.UnlockAchievement();
    }
  }
}
