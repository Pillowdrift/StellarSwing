using UnityEngine;
using System.Collections;

public class GUIButton : MonoBehaviour
{
	void Update()
	{
		if (GetComponent<GUITexture>().HitTest(InputManager.currentPosition))
		{
			if (InputManager.released)
			{
				SendMessage("ButtonPressed");
				
				// Consume
				InputManager.released = false;
			}
		}
	}
}
