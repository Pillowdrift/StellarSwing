using UnityEngine;
using System.Collections;

public class GUIButtonFade : MonoBehaviour
{
	public const float normalAlpha = 0.5f;
	public const float pressedAlpha = 1.0f;
	
	private const float ALPHA_MIN = 0.01f;
	
	public float fadeRate = 1.0f;
	
	private GUIButton button;
	
	void Start()
	{
		// Enable guiTexture (but keep it faded out if button disabled)
		GetComponent<GUITexture>().enabled = true;
		
		// Set initial alpha to minimum
		SetAlpha(normalAlpha);
		
		// Get attached button
		button = GetComponent<GUIButton>();
		if (button != null && !button.enabled)
		{
			SetAlpha(0);
		}
	}
	
	void Update()
	{
		if (GetComponent<GUITexture>() != null && button != null)
		{
			// Fade in if pressed
			if (button.enabled && InputManager.held && GetComponent<GUITexture>().HitTest(InputManager.currentPosition))
			{
				Color col = GetComponent<GUITexture>().color;
				col.a = Mathf.Lerp(col.a, pressedAlpha, RealTime.realDeltaTime * fadeRate);
				GetComponent<GUITexture>().color = col;
			}
			// Fade in if enabled
			else if (button.enabled && GetAlpha() < normalAlpha)
			{
				Color col = GetComponent<GUITexture>().color;
				col.a = Mathf.Lerp(col.a, normalAlpha, RealTime.realDeltaTime * fadeRate);
				GetComponent<GUITexture>().color = col;
				
			}
			// Fade out if disabled
			else if (!button.enabled)
			{
				Color col = GetComponent<GUITexture>().color;
				col.a = Mathf.Lerp(col.a, 0.0f, RealTime.realDeltaTime * fadeRate);
				GetComponent<GUITexture>().color = col;
			}
			
			if (GetComponent<GUITexture>().color.a < ALPHA_MIN)
				GetComponent<GUITexture>().enabled = false;
			else
				GetComponent<GUITexture>().enabled = true;
		}
		
		/*bool fadeToMin = true;
		
		if ((button == null || button.enabled) && guiTexture.HitTest(InputManager.currentPosition))
		{
			if (InputManager.held)
			{
				SetAlpha(Mathf.Lerp(GetAlpha(), maxAlpha, Time.fixedDeltaTime * fadeRate));
				
				fadeToMin = false;
			}
		}
		
		if (button != null && !button.enabled)
		{
			SetAlpha(Mathf.Lerp(GetAlpha(), 0, Time.fixedDeltaTime * fadeRate * 2.0f));
		}
		else if (fadeToMin)
		{
			SetAlpha(Mathf.Lerp(GetAlpha(), minAlpha, Time.fixedDeltaTime * fadeRate));
		}*/
	}
	
	void SetAlpha(float a)
	{
		Color col = GetComponent<GUITexture>().color;
		col.a = a;
		GetComponent<GUITexture>().color = col;
	}
	
	float GetAlpha()
	{
		return GetComponent<GUITexture>().color.a;
	}
}
