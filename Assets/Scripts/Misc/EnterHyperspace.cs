#define GO_BACK_TO_TITLE

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class EnterHyperspace : MonoBehaviour
{
	// Public vars
	public float ShrinkSpeed = 0.99f;
	public float PushPower = 100.0f;
	public float ShrinkPushPower = 20.0f;
	public float BeforeShrinkTime = 2.0f;
	public float AfterShrinkTime = 1.0f;
	public string WorldToLoad = "";
	
	// Private vars
	private float beforeShrinkTimer;
	private float afterShrinkTimer;
	
	private int nextWorld;

	public bool prelude = false;

	// Gentle drift while the story is being read, before the warp starts
	public float IdleBob = 0.04f;
	public float IdleRoll = 1.5f;
	
	// Use this for initialization
	void Start() 
	{
		// Set the timer.
		beforeShrinkTimer = BeforeShrinkTime;
		afterShrinkTimer = AfterShrinkTime;
		
		// Parse level name
		Regex exp = new Regex("World (?<world>.*) Level (?<level>.*)");
		MatchCollection matchList = exp.Matches(WorldToLoad);
		
		if (matchList.Count > 0)
		{
			// Parse level number
			int levelToShow = 0;
			int.TryParse(matchList[0].Groups["level"].ToString(), out levelToShow);
			int.TryParse(matchList[0].Groups["world"].ToString(), out nextWorld);
		}
		
		StartCoroutine("DoWarp");
	}
	
	IEnumerator DoWarp()
	{
		// Hold the warp until the story is nearly done, so the jump lands with its last line
		StoryNarrator story = StoryNarrator.Instance;
		if (story != null)
		{
			Vector3 startPos = transform.position;
			Quaternion startRot = transform.rotation;
			float warpTime = BeforeShrinkTime + AfterShrinkTime;
			float t = 0;
			while (!story.Finished && story.EstimatedRemaining > warpTime)
			{
				t += Time.deltaTime;
				float bob = Mathf.Sin(t * 0.7f) * IdleBob;
				transform.position = startPos + transform.up * bob * transform.localScale.y;
				transform.rotation = startRot * Quaternion.AngleAxis(Mathf.Sin(t * 0.45f) * IdleRoll, Vector3.forward);
				yield return null;
			}
		}

		// Give a short push, then warp
		GetComponent<Rigidbody>().AddForce(Vector3.forward * PushPower);
		SoundManager.Play("warp");
		
		yield return new WaitForSeconds(beforeShrinkTimer);
		
		float shrinkTimer = afterShrinkTimer;
		
		while (shrinkTimer > 0)
		{
			// Update timer
			shrinkTimer -= Time.deltaTime;
			
			// Now shrink the ship really quickly and push it forward more.
			if (transform.localScale.magnitude > 0.1f)
				transform.localScale *= ShrinkSpeed;
			else
				GetComponent<Renderer>().enabled = false;
			
			GetComponent<Rigidbody>().AddForce(transform.forward * ShrinkPushPower);
			
			// Wait a frame
			yield return 0;
		}

		// Then wait for the player to finish reading and click to continue
		if (story != null)
		{
			story.ReadyToContinue = true;
			while (!story.Continued)
				yield return null;
		}

		if (prelude)
		{
			Application.LoadLevel("World 1 Level 1");
		}
		else
		{
#if GO_BACK_TO_TITLE
			LevelSelectGUI.worldToShow = "World" + (nextWorld - 1);
			LevelSelectGUI.levelToShow = 0;
			LevelSelectGUI.worldTransition = true;
			MainMenuController.UnlockNextWorld = true;
			Application.LoadLevel("Title_new");
#else
      LevelSelectGUI.worldToShow = "World" + (nextWorld);
      LevelSelectGUI.levelToShow = 0;
      LevelSelectGUI.worldTransition = false;
      Loading.Load(WorldToLoad);
#endif
		}
	}
}
