using UnityEngine;
using System.Collections;

public class GUIButton : MonoBehaviour
{
	void Update()
	{
		GUITexture texture = GetComponent<GUITexture>();
		if (texture != null && texture.HitTest(InputManager.currentPosition))
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
