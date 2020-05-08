using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TitleBlurScript : MonoBehaviour
{
  public Color AdditiveColor = Color.black;
  public Color MultiplyColor = Color.white;

  private Material material;

  public void Start()
  {
    material = GetComponent<Image>().material;
  }

  public void Update()
  {
    material.SetColor("_AdditiveColor", AdditiveColor);
    material.SetColor("_MultiplyColor", MultiplyColor);
  }
}
