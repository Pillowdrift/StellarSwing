using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Camera.main.transform.position;
	}
}
