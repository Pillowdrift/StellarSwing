using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
	private Quaternion initialRotation;
	
	void Start()
	{
		initialRotation = transform.rotation;
	}
	
	void Update()
	{
		transform.LookAt(transform.position - Camera.main.transform.forward, Camera.main.transform.up);
		transform.rotation *= initialRotation;
	}
}
