using UnityEngine;
using System.Collections;

public class Dasher : MonoBehaviour
{
  public float Power = 1.0f;
  public float MaxSpeed = 100.0f;

  public Transform RiserStart;

  private GameObject boostingObject;
	
	// The prefab of the object to destroy.
	public GameObject Riser;
	
	// The time interval between spawning.
	public float SpawnInterval = 0.3f;
	
	// A timer for creating the rising objects.
	private float timer = 0;
	
	// Generate the rising objects that come out of the flipper.
	void Update()
	{
		timer += Time.deltaTime;
		if (timer > SpawnInterval)
		{
			// Spawn a Riser
			GameObject riser = (GameObject)Instantiate(Riser);
      riser.transform.position = RiserStart.position;
      riser.transform.rotation = RiserStart.rotation;
      riser.transform.localScale = RiserStart.lossyScale;
			//riser.transform.localScale = new Vector3 (transform.lossyScale.x * 1000,
			//										  transform.lossyScale.y * 1000 * 2.5f,
			//										  transform.lossyScale.z * 1000 * 0.5f);
			//riser.transform.Rotate(new Vector3(-90, 0, 0));
			//riser.transform.position += riser.transform.forward * 1.0f;
			//riser.transform.position -= riser.transform.up * 2.3f;
			timer = 0;
		}

    if (boostingObject != null)
    {
      return;
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
      var direction = boostingObject.transform.forward;
      var rigidbody = boostingObject.GetComponent<Rigidbody>();
      rigidbody.AddForce(direction * Power);
      if (rigidbody.velocity.sqrMagnitude > MaxSpeed * MaxSpeed)
        rigidbody.velocity = rigidbody.velocity.normalized * MaxSpeed;

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
