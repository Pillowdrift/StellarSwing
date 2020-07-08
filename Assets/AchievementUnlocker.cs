using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AchievementUnlocker : MonoBehaviour
{
  public string AchievementIDStr;

  void Awake() {
    if (!SteamManager.Initialized) {
      gameObject.SetActive(false);
    }
  }

  // Start is called before the first frame update
  void Start() {
    
  }

  // Update is called once per frame
  void Update() {

  }
}
