using UnityEngine;
using System.Collections;

public class VisibleOnCollide
	: MonoBehaviour
{
	public GameObject[] objs;

	public void Start()
	{
		Reload();
	}

	public void Reload()
	{
		for (int i = 0; i < objs.Length; ++i)
			objs[i].GetComponent<Renderer>().enabled = false;
	}
	
	public void OnCollisionEnter()
	{
		for (int i = 0; i < objs.Length; ++i)
			objs[i].GetComponent<Renderer>().enabled = true;
	}
	
	public void OnTriggerEnter()
	{
		for (int i = 0; i < objs.Length; ++i)
			objs[i].GetComponent<Renderer>().enabled = true;
	}
}
