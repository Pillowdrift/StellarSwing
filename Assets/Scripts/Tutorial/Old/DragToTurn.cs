using UnityEngine;
using System.Collections;

public class DragToTurn : MonoBehaviour
{
	private bool tutorialEnabled = false;
	// Legacy animated finger; its GUITexture no longer exists in Unity 6, so this is normally null
	public GUITexture finger;
	
	const float SWIPE_DISTANCE = 0.25f;
	const float SWIPE_DURATION = 3.0f;
	float time = 0;

	TutorialCamera tutorialCamera;
	void Awake()
	{
		tutorialCamera = FindObjectOfType<TutorialCamera>();
	}
	
	void EnableTutorial()
	{
		GUIController.DisableButtons();
		tutorialCamera.ShowTutorialText("Drag to alter your trajectory", false);
		//GUIController.ShowText("TutorialText");
		
		if (finger != null)
			finger.enabled = true;
		
		//Time.timeScale = 0.0f;
		tutorialEnabled = true;

		SendMessage("ActivateSlowDown");
	}
	
	void DisableTutorial()
	{
		GUIController.GUILevelPlay();
		//GUIController.HideText("TutorialText");
		tutorialCamera.HideTutorial();
		GUIController.DisableImage("DragIcon");
		
		//Time.timeScale = 1.0f;
		tutorialEnabled = false;

		SendMessage("DeactivateSlowDown");
	}
	
	void HideDrag()
	{
		if (finger != null)
			finger.enabled = false;
	}
	
	void OnTriggerEnter(Collider collider)
	{
		if (collider.tag == "Player" && !GameRecorder.playingBack)
		{
			EnableTutorial();
		}
	}
	
	void OnTriggerLeave(Collider collider)
	{
		if (collider.tag == "Player" && !GameRecorder.playingBack)
		{
			HideDrag();
		}
	}
	
	
	void Update()
	{
		if (tutorialEnabled)
		{
			// Dismiss on a click or as soon as the player drags; they are often already
			// holding the mouse when they reach the trigger, so a fresh press may never come
			if (InputManager.pressed || InputManager.dragging)
			{
				DisableTutorial();
			}
		}
		
		if (finger == null)
			return;

		if(finger.enabled || LevelState.HasFinished)
		{				
			finger.transform.position = new Vector3(Mathf.SmoothStep(0.5f - SWIPE_DISTANCE, 0.5f + SWIPE_DISTANCE, Mathf.SmoothStep(0,1,time)), finger.transform.position.y, 0);			
			
			time += 1.0f/SWIPE_DURATION * RealTime.realDeltaTime;
			
			if(time > 1)
				time -= 1;
		}
		else
			time = 0;
	}
}
