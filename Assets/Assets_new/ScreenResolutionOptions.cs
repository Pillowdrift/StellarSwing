using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenResolutionOptions : MonoBehaviour
{
  private Dropdown dropdown;

  public void Awake()
  {
    dropdown = GetComponent<Dropdown>();

  }
}
