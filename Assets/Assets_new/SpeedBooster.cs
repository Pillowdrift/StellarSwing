using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
  public float Power = 1.0f;

  private GameObject boostingObject;

  void FixedUpdate()
  {
    if (boostingObject != null)
    {
      var direction = boostingObject.transform.forward;
      boostingObject.GetComponent<Rigidbody>().AddForce(direction * Power);
    }
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.name == "Player")
    {
      boostingObject = other.gameObject;
    }
  }

  void OnTriggerExit(Collider other)
  {
    if (other.gameObject == boostingObject)
    {
      boostingObject = null;
    }
  }
}
