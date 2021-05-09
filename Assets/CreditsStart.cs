using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsStart : MonoBehaviour
{
  void Start()
  {
    Animation anim = GetComponent<Animation>();
    foreach (AnimationState state in anim)
    {
      state.time = 10.0f;
    }
  }
}
