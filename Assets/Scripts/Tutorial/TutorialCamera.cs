using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class TutorialCamera : MonoBehaviour
{
	public enum TutorialEvent
	{
		NONE,
		LOAD_PRESET_LEVEL,
		LOAD_NEXT_LEVEL,
		START_PLAYER,
		PLAY_RECORDING,
		RESET_PLAYER
	}
	
	public string level;

	public Text TutorialHeading;
	public Text TutorialTextbox;

	internal static void ResetShownTutorials()
	{
		tutorialsShown.Clear();
	}

	private List<TutorialPoint> points;
	
	private Vector3 lastPosition;
	private Quaternion lastRotation;
	
	private Vector3 nextPosition;
	private Quaternion nextRotation;
	
	private float currentLerpCoeff = 1.0f;
	
	private int index = 0;
	
	public Button nextButton;
	public Button lastButton;
	public Button skipButton;

	public Animation animation;
	
	private GameObject theCamera;
	
	private static bool tutorialEnabled = false;
	
	private bool moving = false;
	
	private bool init = false;

	private static HashSet<string> tutorialsShown = new HashSet<string>();

	public bool tutorialDialogUp = false;

	public void ShowTutorialText(string text, bool withButtons)
	{
		ShowTutorialText(text, withButtons, "Tutorial");
	}

	public void ShowTutorialText(string text, bool withButtons, string titleText)
	{
		if (text == "")
		{
			HideTutorial();
			return;
		}

		if (!tutorialDialogUp)
      StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowTutorial", false, () => { }));
		tutorialDialogUp = true;
		TutorialHeading.text = titleText;
    TutorialTextbox.text = text;

		nextButton.transform.parent.gameObject.active = withButtons;
		lastButton.transform.parent.gameObject.active = withButtons;
		skipButton.gameObject.active = withButtons;
	}

	public void HideTutorial()
	{
		if (tutorialDialogUp)
      StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowTutorial", true, () => { }));
		tutorialDialogUp = false;
	}

	private bool autoStartTutorial = true;

	public bool TutorialsEnabled() { return Settings.Current.TutorialEnabled; }

	private bool HasPoints()
	{
		return FindObjectsOfType<TutorialPoint>().Length > 0;
		//return transform.parent?.gameObject.GetComponentsInChildren<TutorialPoint>().Length > 0;
	}
		
	public IEnumerator Start()
	{
		if (SceneManager.GetActiveScene().name == "Title_new")
		{
			gameObject.SetActive(false);
			yield break;
		}

		if (!TutorialsEnabled() || !autoStartTutorial || !HasPoints())
		{
			autoStartTutorial = false;
			this.enabled = false;
			yield break;
		}

		var levelName = Application.loadedLevelName;
		if (tutorialsShown.Contains(levelName))
			yield break;
		tutorialsShown.Add(levelName);

		tutorialEnabled = true;
		
		points = new List<TutorialPoint>();
		
		//nextButton = GameObject.Find("TutorialNext").GetComponent<GUIButton>();
		//lastButton = GameObject.Find("TutorialBack").GetComponent<GUIButton>();
		//skipButton = GameObject.Find("TutorialSkip").GetComponent<GUIButton>();
		
		lastButton.interactable = false;
		nextButton.interactable = false;
		skipButton.interactable = false;
		
		// Disable camera
		theCamera = GameObject.Find("TheCamera");		

		if (level != "")
		{
			//Application.LoadLevelAdditive(level);
			//StartCoroutine(LoadDummyLevel(level));
			AsyncOperation async = Application.LoadLevelAdditiveAsync(level);
			yield return async;

      if (theCamera != null)		
      {
        theCamera.active = false;
      }
		}
		
		nextButton.interactable = true;
		skipButton.interactable = true;
		
		// Freeze player
		GameObject player = GameObject.Find("Player");
		if (player != null)
		{
			player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		}
		
		if (transform.parent?.gameObject != null)
		{
			LoadPoints(transform.parent?.gameObject);
		}
		
		init = true;
		
		//Tutorial.ShowText("Click", "Click to continue", 0, TextAlignment.Center, TextAnchor.LowerLeft, 0.0f, 0.0f);
	}

	IEnumerator LoadDummyLevel(string level)
	{
		// Wait a frame
		yield return null;
		
		//GameObject.Find("Player").AddComponent<TutorialPlayer>();
	}
	
	public void Update()
	{
		if (!init)
			return;
		
		// Show or hide buttons
		if (index > 0)
			lastButton.interactable = true;
		else
			lastButton.interactable = false;
		
		if (index < points.Count-1)
			nextButton.interactable = true;
		else
			nextButton.interactable = false;
		
		// Update lerp coefficient
		currentLerpCoeff += Time.deltaTime;
		
		if (currentLerpCoeff > 1.0f)
			currentLerpCoeff = 1.0f;
		
		if (moving && currentLerpCoeff >= 1.0f)
		{
			moving = false;
			
			HandleEvent(points[index].endEvent);
			
			if (index < points.Count)
			{
				ShowTutorialText(points[index].text.Trim(), true);
        //TutorialTextbox.text = points[index].text.Trim();
				//Tutorial.ShowText(points[index].textName, points[index].text, 0, TextAlignment.Center, TextAnchor.MiddleCenter, points[index].textX, points[index].textY);
			}
		}
		
		Vector3 newPos = Vector3.Lerp(lastPosition, nextPosition, Mathf.SmoothStep(0, 1, currentLerpCoeff));
		Quaternion newRot = Quaternion.Lerp(lastRotation, nextRotation, Mathf.SmoothStep(0, 1, currentLerpCoeff));
		
		if (!float.IsNaN(newRot.x))
		{
			theCamera.transform.position = newPos;
			theCamera.transform.rotation = newRot;
		}
	}
	
	public bool Stopped
	{
		get
		{
			return (currentLerpCoeff >= 1.0f);
		}
	}
	
	public void Next()
	{
		if (index < points.Count-1)
		{
			NextPoint();
			Tutorial.HideText(points[index].textName);
		}
	}
	
	public void Prev()
	{
		if (index > 0)
		{
			PreviousPoint();
			Tutorial.HideText(points[index].textName);
		}
	}
	
	public void Skip()
	{
    GameRecorder.StopPlayback();
    GameObject.Find("Player").SendMessage("Reload");

		HideTutorial();
    //StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowTutorial", true, () => { }));

		// Disable buttons
		lastButton.interactable = false;
		nextButton.interactable = false;
		skipButton.interactable = false;
		
		// Load final point
		EndPoint();
		if (index < points.Count)
		{
			Tutorial.HideText(points[index].textName);
		}
	}
	
	public void LoadPoints(GameObject list)
	{
		points = new List<TutorialPoint>(FindObjectsOfType<TutorialPoint>());
		points.Sort();
			
		if (points.Count > 0)
		{
			moving = true;
			
			currentLerpCoeff = 1.0f;
			
			// Just sit at the first point until a button is pressed
			lastPosition = points[0].transform.position;
			lastRotation = points[0].transform.rotation;
			
			nextPosition = lastPosition;
			nextRotation = lastRotation;
			
			theCamera.transform.position = nextPosition;
			theCamera.transform.rotation = nextRotation;
		}
	}
	
	private void NextPoint()
	{
		theCamera.transform.position = nextPosition;
		theCamera.transform.rotation = nextRotation;
		
		lastPosition = nextPosition;
		lastRotation = nextRotation;
		
		index++;
		if (index < points.Count)
		{
			currentLerpCoeff = 0.0f;
			
			nextPosition = points[index].transform.position;
			nextRotation = points[index].transform.rotation;
			
			StartEvent();
		}
		
		moving = true;
	}
	
	private void PreviousPoint()
	{
		theCamera.transform.position = nextPosition;
		theCamera.transform.rotation = nextRotation;
				
		lastPosition = nextPosition;
		lastRotation = nextRotation;
		
		index--;
		if (index >= 0 && index < points.Count)
		{
			currentLerpCoeff = 0.0f;
			
			nextPosition = points[index].transform.position;
			nextRotation = points[index].transform.rotation;
			
			StartEvent();
		}
		
		moving = true;
	}
	
	private void EndPoint()
	{
		// Final point
		index = points.Count-1;
		
		// Set up interpolation
		currentLerpCoeff = 0.0f;
		
		lastPosition = theCamera.transform.position;
		lastRotation = theCamera.transform.rotation;
		
		nextPosition = points[index].transform.position;
		nextRotation = points[index].transform.rotation;
		
		moving = true;
	}
	
	public static bool Enabled()
	{
		return tutorialEnabled;
		//return GameObject.FindObjectOfType(typeof(TutorialCamera)) != null;
	}
	
	private void StartEvent()
	{
		HandleEvent(points[index].startEvent);
    ShowTutorialText(points[index].text.Trim(), true);
		//TutorialTextbox.text = points[index].text.Trim();
		if (index == points.Count - 1)
			HideTutorial();
      //StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowTutorial", true, () => { }));
	}
	
	private void HandleEvent(TutorialEvent tutorialEvent)
	{
		switch (tutorialEvent)
		{
		case TutorialEvent.NONE:
			break;
		case TutorialEvent.LOAD_PRESET_LEVEL:
			LevelSelectGUI.currentLevel = Levels.GetLevel(level);
			Loading.Load(level);
			break;
		case TutorialEvent.LOAD_NEXT_LEVEL:
			LoadLevel.LoadALevel("next");
			break;
		case TutorialEvent.START_PLAYER:
			tutorialEnabled = false;
			
			// Disable buttons
			lastButton.interactable = false;
			nextButton.interactable = false;
			skipButton.interactable = false;
			
			// Get rid of mainCamera tag
			//tag = "Untagged";
			
			// Unfreeze camera
			{
				//theCamera.active = true;
				theCamera.GetComponent<LevelStart>().Start();
			}
			
			// Unfreeze player
			GameObject player = GameObject.Find("Player");
			if (player != null)
			{
				player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
			}

      // Disable tutorial camera
      //gameObject.active = false;
      this.enabled = false;
			break;
		case TutorialEvent.PLAY_RECORDING:
			string test = "Recordings/" + points[index].recordingName;
			TextAsset ta = (TextAsset)Resources.Load(test);
			Stream s = new MemoryStream(ta.bytes);
			BinaryReader br = new BinaryReader(s);
			GameRecorder.StartPlaybackTutorial(Recording.Read(br));
			break;
		case TutorialEvent.RESET_PLAYER:
			GameRecorder.StopPlayback();
			GameObject.Find("Player").BroadcastMessage("Reload");
			break;
		}
		
	}
}
