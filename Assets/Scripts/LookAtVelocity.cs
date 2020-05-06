using UnityEngine;
using System.Collections;

public class LookAtVelocity : MonoBehaviour
{
	const float UP_CORRECT_RATE = 2.0f;
	const float CORRECT_RATE = 0.01f;
	
	void Update()
	{
		if(GetComponent<Rigidbody>().velocity.sqrMagnitude < 0.01f)
			return;
		
		Quaternion fromUpAngle = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, transform.up);			
		Quaternion toUpAngle = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
		
		Quaternion targetAngle = Quaternion.Slerp(fromUpAngle, toUpAngle, UP_CORRECT_RATE * Time.deltaTime);
		
		transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, CORRECT_RATE * Time.deltaTime * GetComponent<Rigidbody>().velocity.sqrMagnitude);
	}
}
