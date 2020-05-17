using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleFixer : MonoBehaviour
{
  public Vector3 TargetScale = Vector3.zero;

  void Awake()
  {
    transform.localScale = TargetScale;
  }

  void Update()
  {
    if (transform.localScale != TargetScale)
    {
      transform.localScale = TargetScale;
    }
  }
}
