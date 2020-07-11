using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
	public static bool paused = false;

	private Canvas sharedUI;

	private bool animating = false;

	public Animator MainGUI;

	public void Awake()
	{
		sharedUI = GameObject.Find("Shared UI").GetComponent<Canvas>();
	}

	public void Pause()
	{
		if (Time.timeScale == 0.0f)
			return;

		if (MainGUI.GetCurrentAnimatorStateInfo(0).IsName("Playing"))
		{
			MainGUI.Play("Pause");
      Time.timeScale = 0.0f;
		}
	}
	public void Unpause()
	{
		if (MainGUI.GetCurrentAnimatorStateInfo(0).IsName("Pause"))
		{
			MainGUI.Play("Unpause");
      Time.timeScale = 1.0f;
		}
	}

	
	public void ButtonPressed()
	{
		if (animating)
			return;

		Debug.Log("pausing");

		var currentState = MainGUI.GetCurrentAnimatorStateInfo(0);

		if (GUIController.LevelStarted && !LevelState.Dead && !LevelState.HasFinished && !GameRecorder.playingBack)
		{
			if (Time.timeScale > 0.0f && !paused && currentState.IsName("Unpaused"))
			{
				Time.timeScale = 1.0f - Time.timeScale;
				GUIController.GUILevelPause();
				paused = true;
				sharedUI.enabled = true;
				//animating = true;
				//MainMenuController.PlayAnimation(MainGUI, "PauseAnim", false, () => { animating = false; });
				MainGUI.Play("Pause");
			}
			else if (paused && currentState.IsName("Pause"))
			{
				Time.timeScale = 1.0f - Time.timeScale;
				GUIController.GUILevelPlay();
				paused = false;
				sharedUI.enabled = false;
				//animating = true;
				//MainMenuController.PlayAnimation(MainGUI, "PauseAnim", true, () => { animating = false; });
				MainGUI.Play("Unpause");
			}
		}
	}
}
