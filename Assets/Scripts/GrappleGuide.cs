using UnityEngine;
using System.Collections;

public class GrappleGuide : MonoBehaviour
{
	public Transform grappleTo;
	
	public void Start()
	{
		Physics.IgnoreCollision(GetComponent<Collider>(), GameObject.Find("Player").GetComponent<Collider>());
	}
	
	public Vector3 GrappleTo()
	{
		return grappleTo.position;
	}
}
