using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomHatSelector : MonoBehaviour
{
  void Start()
  {
    int hatToEnable = Random.Range(0, transform.childCount);
    transform.GetChild(hatToEnable).gameObject.SetActive(true);
  }
}
