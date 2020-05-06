using UnityEngine;
using System.Collections;

public class GUIImageScale : MonoBehaviour
{
	private float width;
	private float height;
	
	private float x;
	private float y;
	
	void Start()
	{
		// Get their original size.
		if (GetComponent<GUITexture>() != null)
		{
			width = GetComponent<GUITexture>().pixelInset.width;
			height = GetComponent<GUITexture>().pixelInset.height;
			x = GetComponent<GUITexture>().pixelInset.x;
			y = GetComponent<GUITexture>().pixelInset.y;
		}
	}
	
	void Update()
	{
		if (GetComponent<GUITexture>() != null)
		{
			float scale = (float)(Screen.width / (float)GUIScale.WIDTH);
			Rect rect = GetComponent<GUITexture>().pixelInset;
			rect.x = x * scale;
			rect.y = y * scale;
			rect.width = width * scale;
			rect.height = height * scale;
			GetComponent<GUITexture>().pixelInset = rect;
		}
	}
}
