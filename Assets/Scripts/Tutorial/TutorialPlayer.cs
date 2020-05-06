using UnityEngine;
using System.Collections;

public class TutorialPlayer : MonoBehaviour
{
	void Start()
	{
		GetComponent<Rigidbody>().isKinematic = true;
	}
}
