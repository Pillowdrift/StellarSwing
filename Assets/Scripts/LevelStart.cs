using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelStart : MonoBehaviour
{
	public static bool started = false;
	public static Vector3 clickpos;
	
	public bool enablePreviews = true;
	public bool reload = true;
	
	private const float previewTime = 3.0f;
	
	private GameObject player;
	//private List<PreviewNode> previews;
	private ScrollingCamera scrollingCamera;
	private GrapplingHook grapplingHook;
	private PlayerMovements playerMovements;
	
	private Vector3 initialCameraPosition;
	private Quaternion initialCameraRotation;
	
	private static bool start = false;
	
	private static bool persistClick = false;

	private TutorialCamera tutorialCamera;

	private Animator MainGUI;

	private IEnumerator ShowStartText()
	{
		yield return new WaitForEndOfFrame();
		if (!LevelStart.started)
      tutorialCamera.ShowTutorialText("Click anywhere to begin", false, "Level Start");
	}
	
	public void Start()
	{
		tutorialCamera = FindObjectOfType<TutorialCamera>();
		MainGUI = GameObject.Find("MainGUI").GetComponent<Animator>();
		if (TutorialCamera.Enabled())
		{
			enabled = false;
			return;
		}
		else
		{
			enabled = true;
			
			initialCameraPosition = transform.position;
			initialCameraRotation = transform.rotation;
			
			AmbientNoise.Play();
			
			PauseButton.paused = false;
			
			player = GameObject.Find("Player");
			grapplingHook = player.GetComponent<GrapplingHook>();
			playerMovements = player.GetComponent<PlayerMovements>();
			
			//ControlManager.Disabled = true;
			if (!GameRecorder.playingBack)
			{
				GameRecorder.StopPlayback();
			
				if (LevelSelectGUI.currentLevel != null)
					GUIController.ShowText("LevelName", LevelSelectGUI.currentLevel.name);

#if UNITY_ANDROID || UNITY_IPHONE
				GUIController.ShowText("Text", "Tap to begin");
#else
				//GUIController.ShowText("Text", "Click to begin");
				//tutorialCamera.ShowTutorialText("Click to begin", false);
				StartCoroutine(ShowStartText());
#endif
				
				GUIController.GUILevelPause();
				GUIController.HideText("Paused");
			}
			
			EnableCameraController();
		}
	}
	
	// Resets after death
	void RestartLevel()
	{
		started = false;
		LevelState.Dead = false;
		Start();
	}
	
	void Update()
	{
		if (!TutorialCamera.Enabled() && !LevelState.Dead)
		{
			if (!grapplingHook.enabled)
				grapplingHook.Update();

			// Handle back button
			if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("SkipTutorial"))
			{
				GameObject pauseButton = GameObject.Find("Pause");
				if (pauseButton != null)
					pauseButton.SendMessage("ButtonPressed", SendMessageOptions.DontRequireReceiver);
			}
			
			if (!started && !GameRecorder.playingBack)
			{
	//			if (enablePreviews && useStaticPreviews && !GameRecorder.playingBack)
	//			{
	//				// Handle previews
	//				timePassed += Time.deltaTime;
	//				if (timePassed >= previewTime)
	//				{
	//					timePassed = 0.0f;
	//					NextPreview();
	//				}
	//			}
				
				// Start level if player tapped last frame or if reload is disabled
				if (start ||
					(!reload && !started && (InputManager.pressed || InputManager.released) && !ControlManager.MouseOnGUI && !GameRecorder.playingBack))
				{
					if (!reload)
					{
						Debug.Log("Reloading");
						foreach (GameObject gameObject in GameObject.FindObjectsOfType(typeof(GameObject)))
						{
							Debug.Log(gameObject.name);
							gameObject.BroadcastMessage("Reload", SendMessageOptions.DontRequireReceiver);
						}
						
						//persistClick = true;
						//clickpos = InputManager.currentPosition;
					}
					
					playerMovements.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
					
					// Apply initial velocity
					playerMovements.Go();
					
					// Set state
					start = false;
					started = true;
					
					// Reset camera
					transform.position = initialCameraPosition;
					transform.rotation = initialCameraRotation;
					
					// Enable camera controller
					EnableCameraController();
					
					// Initialise systems
					GameRecorder.playingBack = false;
					GameRecorder.StartRecording();
					GUIController.HideText("Text");
					tutorialCamera.HideTutorial();
					GUIController.GUILevelPlay();
					GUIController.LevelStarted = true;
					ControlManager.Disabled = false;
					ScoreCalculator.Reset();
					Debug.Log("Playing start level");
          MainGUI.Play("StartLevel");
					
					// Try to grapple
					if (persistClick)
					{
						persistClick = false;
						InputManager.currentPosition = clickpos;
						
						grapplingHook.TryGrapple(InputManager.currentPosition);
					}
				}
				else if (!LevelState.Dead)
				{
					playerMovements.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				}
				
				// Reload and start level if reload is enabled
				if (reload && !started && InputManager.pressed && !ControlManager.MouseOnGUI && !GameRecorder.playingBack)
				{
					start = true;
					persistClick = true;
					clickpos = InputManager.currentPosition;
					PauseButton.paused = false;
					Application.LoadLevel(Application.loadedLevelName);
					InputManager.pressed = true;
					InputManager.released = true;
				}
			}
		}
	}
	
//	void NextPreview()
//	{
//		currentPreview++;
//		if (currentPreview >= previews.Count)
//			currentPreview = 0;
//		
//		SetPreview(currentPreview);
//	}
//	
//	void SetPreview(int preview)
//	{
//		PreviewNode current = previews[preview];
//		transform.position = current.transform.position;
//		transform.rotation = current.transform.rotation;	
//	}
	
	void EnableCameraController()
	{
		ThirdPersonCamera tpc = GetComponent<Camera>().GetComponent<ThirdPersonCamera>();
		tpc.enabled = true;	
	}
}
