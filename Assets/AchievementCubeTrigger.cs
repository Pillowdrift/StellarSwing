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
      Debug.Log("Loaded achievement unlocker");
    }
  }
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnTriggerEnter(Collider other)
  {
    if (other != null && other.tag == "Player")
    {
      Unlocker.UnlockAchievement();
    }
  }
}
