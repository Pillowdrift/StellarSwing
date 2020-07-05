using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
  public float Power = 1.0f;
  public float MaxSpeed = 100.0f;

  private GameObject boostingObject;

  void FixedUpdate()
  {
    if (boostingObject != null)
    {
      var direction = boostingObject.transform.forward;
      var rigidbody = boostingObject.GetComponent<Rigidbody>();
      rigidbody.AddForce(direction * Power);
      if (rigidbody.velocity.sqrMagnitude > MaxSpeed * MaxSpeed)
        rigidbody.velocity = rigidbody.velocity.normalized * MaxSpeed;
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
