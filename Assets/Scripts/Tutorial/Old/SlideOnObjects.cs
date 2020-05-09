using UnityEngine;
using System.Collections;

public class SlideOnObjects : MonoBehaviour
{
	TutorialCamera tutorialCamera;

	void Awake()
	{
		tutorialCamera = FindObjectOfType<TutorialCamera>();
	}

	void EnableTutorial()
	{
		if (!tutorialCamera.TutorialsEnabled())
			return;

		tutorialCamera.ShowTutorialText("Land on objects to slide across them", false);

		//GUIController.ShowText("Tutorial", "Land on objects to slide across them");
	}
	
	void DisableTutorial()
	{
		//GUIController.HideText("TutorialText");
		tutorialCamera.HideTutorial();
		
		Time.timeScale = 1.0f;
	}
	
	void OnTriggerEnter(Collider collider)
	{
		if (collider.tag == "Player" && !GameRecorder.playingBack)
		{
			EnableTutorial();
		}
	}
}
