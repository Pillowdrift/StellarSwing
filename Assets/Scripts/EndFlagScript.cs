using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EndFlagScript : MonoBehaviour
{
  public const float START_WAIT_TIME = 1.4f;
  public const float WAIT_TIME = 1.4f;

  private const float LABEL_WIDTH = 120.0f;
  private const float LABEL_HEIGHT = 100.0f;

  private const float BUTTON_WIDTH = 200.0f;
  private const float BUTTON_HEIGHT = 80.0f;

  private Vector2 screenSpacePos;
  private float distance;

  public float ZoomOut = 1.0f;
  public float ZoomOutTimer = 0.5f;

  public float SaveRecordDelay = 1.0f;

  public string WorldMapSceneName;

  public ThirdPersonCamera aCam;

  public static bool hasFinished = false;
  public static bool savedRecording = false;

  private GameObject player;

  private bool tapped = false;
  private bool uploading = false;
  public bool endingDone = false;

  private Save.LevelHighScore score;

  private TutorialCamera tutorialCam;

  private float speedWaitStart = 0;
  private float timeWaitStart = 0;

  void Awake()
  {
    player = GameObject.Find("Player");
    tutorialCam = GameObject.FindObjectOfType<TutorialCamera>();
  }

  void Start()
  {
    hasFinished = false;
    savedRecording = false;
  }

  void Update()
  {
    if (InputManager.pressed)
      tapped = true;

    // If the speed text is enabled, display the speed counting up
    if (speedWaitStart > 0)
    {
      float speedCount = Mathf.Max(0.0f, score.Speed);
      float normalisedSpeed = 0;

      if (LevelSelectGUI.currentLevel != null)
        normalisedSpeed = (100.0f * speedCount) / LevelSelectGUI.currentLevel.ranks.SpeedThreshold;

      // Clamp normalised speed
      normalisedSpeed = Math.Min(normalisedSpeed, 100.0f);

      float waitElapsed = Mathf.Clamp(Mathf.Lerp(0.0f, 1.0f, (Time.time - speedWaitStart) / WAIT_TIME), 0.0f, 1.0f);

      float normalisedSpeedCounter = normalisedSpeed * waitElapsed;

      GUIController.ShowText("Speed", "Energy: " + Math.Round(normalisedSpeedCounter).ToString() + "%");
    }

    // If the time text is enabled, display the time counting up
    if (timeWaitStart > 0)
    {
      float timeCount = score.Time;

      float waitElapsed = Mathf.Clamp(Mathf.Lerp(0.0f, 1.0f, (Time.time - timeWaitStart) / WAIT_TIME), 0.0f, 1.0f);

      float timeCounter = timeCount * waitElapsed;

      GUIController.ShowText("Time", "Time: " + timeCounter.ToString(".#") + "s");
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    if (TutorialCamera.Enabled())
      return;

    Collider collider = collision.collider;
    if (collider.gameObject.tag == "Player" && !LevelState.Dead && !LevelState.HasFinished)
    {
      GUIController.HideText("Tutorial");

      // Set flag so this doesn't run more than once
      LevelState.HasFinished = true;

      // Freeze score values
      ScoreCalculator.End();

      //freeze the player
      LevelState.Dead = true;
      collider.GetComponent<Rigidbody>().velocity = Vector3.zero;
      collider.GetComponent<Rigidbody>().isKinematic = true;

      //make the player face the thing it hit
      collider.transform.LookAt(collider.transform.position + collision.contacts[0].normal, collider.transform.up);

      // Send a message to the player to start the mining particles.
      player.SendMessage("StartMining");

      // Switch to a camera that looks at the end thingy.
      ZoomOuterizer zout = Camera.main.GetComponent<ZoomOuterizer>();
      if (zout != null)
        zout.ZoomOut(ZoomOut, ZoomOutTimer);

      // Do level finish routine (scores, stars and audio)
      StartCoroutine("FinishLevel");
    }
  }

  private IEnumerator WaitButAllowSkip(float seconds)
  {
    float endTime = Time.time + seconds;
    while (Time.time < endTime && !tapped)
    {
      if (!tapped)
        yield return new WaitForEndOfFrame();
    }
  }

  IEnumerator FinishLevel()
  {
    Debug.Log("Level finished!");

    // Wait time between stars unless tapped to skip
    const bool playting = true;
    const bool playfail = false;

    // Which stars are unlocked
    bool scoreStar = false;
    bool timeStar = false;

    Debug.Log("Updating next level");

    hasFinished = true;

    // Calculate scores
    Debug.Log("Calculating scores");

    score = new Save.LevelHighScore();

    if (!GameRecorder.playingBack)
    {
      score.Score = ScoreCalculator.finalScore;
      score.Speed = ScoreCalculator.finalSpeed;
      score.Time = ScoreCalculator.finalTime;
    }
    else
    {
      score.Score = GameRecorder.current.score.score;
      score.Speed = GameRecorder.current.score.speed;
      score.Time = GameRecorder.current.score.time;
    }

    // Save recording
    Debug.Log("Save recording!");

    if (!GameRecorder.playingBack)
    {
      savedRecording = true;
      GameRecorder.StopRecording();
      //Recording rec = GameRecorder.Save(true, false);

      // Upload score to online thing
      if (!GameRecorder.playingBack)
      {
        //HighScores.PostScore(LevelSelectGUI.currentLevel, rec);

      }
      // Disable buttons so user doesn't disrupt score uploading
      GUIController.DisableButtons();
    }

    // Check which stars have been earnt
    Debug.Log("Checking speed star!");

    scoreStar = SaveManager.HasSurpassedSpeed(score, LevelSelectGUI.currentLevel);
    timeStar = SaveManager.HasSurpassedTime(score, LevelSelectGUI.currentLevel);

    // Check rewards
    int passedReward = 500;
    int noPowerupsReward = 500;
    int scoreReward = scoreStar ? 500 : 0;
    int timeReward = timeStar ? 500 : 0;

    // Get picolinium counter and set target to current value so we can manually increment it
    var picoliniumCounter = GameObject.Find("Game End/Content/Picolinium/Value/Content/Text")?.GetComponent<PicoliniumCounter>();
    picoliniumCounter.Target = SaveManager.save?.picolinium ?? 0;

    if (SaveManager.save != null)
    {
      SaveManager.save.levelCompletions++;
      SaveManager.save.IncrementPicolinium(passedReward + noPowerupsReward + scoreReward + timeReward);
    }

    // Update save
    // Don't save if we are replaying a recording.
    if (!GameRecorder.playingBack)
    {
      if (LevelSelectGUI.currentLevel != null)
      {
        SaveManager.Beaten(LevelSelectGUI.currentLevel);
        SaveManager.UpdateScore(LevelSelectGUI.currentLevel);
        SaveManager.Write();
      }
    }

    // Let the camera pan back a bit
    tapped = false;
    yield return WaitButAllowSkip(START_WAIT_TIME);

    // Show the gui
    // Do everything important before this because they can exit the level or reset after this
    GameObject.Find("Game End").BroadcastMessage("Reset");
    GameObject.Find("MainGUI").GetComponent<Animator>().Play("EndLevel");

    tapped = false;
    yield return new WaitForSeconds(0.5f);

    string formattedTime = ReadableTime((int)(score.Time * 1000.0f));

    // Get reward text template
    var bonusTemplate = GameObject.Find("RewardText");

    SoundManager.Play("ting");
    picoliniumCounter.Target += passedReward;

    // Show level time
    var levelTime = GameObject.Find("LevelTime/Text").GetComponent<TextRevealer>();
    levelTime.Text = $"{formattedTime}";
    levelTime.Run = true;
    levelTime.Counter = 0.0f;

    TextFadeout newBonusText;
    if (!tapped)
    {
      newBonusText = GameObject.Instantiate(bonusTemplate, bonusTemplate.transform.parent).GetComponent<TextFadeout>();
      newBonusText.Text = "Level complete!";
      newBonusText.Go = true;
    }

    yield return WaitButAllowSkip(WAIT_TIME);

    // Show powerups bonus
    if (noPowerupsReward > 0)
    {
      picoliniumCounter.Target += noPowerupsReward;

      if (!tapped)
      {
        newBonusText = GameObject.Instantiate(bonusTemplate, bonusTemplate.transform.parent).GetComponent<TextFadeout>();
        newBonusText.Text = "No powerups";
        newBonusText.Go = true;

        SoundManager.Play("ting");
      }

      yield return WaitButAllowSkip(WAIT_TIME);
    }

    // Show time bonus
    if (timeReward > 0)
    {
      picoliniumCounter.Target += timeReward;

      if (!tapped)
      {
        SoundManager.Play("ting");

        newBonusText = GameObject.Instantiate(bonusTemplate, bonusTemplate.transform.parent).GetComponent<TextFadeout>();
        newBonusText.Text = "Super fast time!";
        newBonusText.Go = true;
      }

      yield return WaitButAllowSkip(WAIT_TIME);
    }

    // Show speed bonus
    if (scoreReward > 0)
    {
      picoliniumCounter.Target += scoreReward;

      if (!tapped)
      {
        SoundManager.Play("ting");

        newBonusText = GameObject.Instantiate(bonusTemplate, bonusTemplate.transform.parent).GetComponent<TextFadeout>();
        newBonusText.Text = "Super fast speed!";
        newBonusText.Go = true;
      }

      yield return WaitButAllowSkip(WAIT_TIME);
    }

    // End
    player.SendMessage("StopCounting");

    player.SendMessage("EndMining");

    //GameObject.Find("Game End/Content/Story/Text").GetComponent<TextRevealer>().Run = true;
    GameObject.Find("MainGUI").GetComponent<Animator>().Play("ExpandEndLevel");

    if (!uploading)
      GUIController.EndLevel(true);

    endingDone = true;
  }

  private static List<double> _intervals = new List<double>
  {
      1.0 / 1000 / 1000,
      1.0 / 1000,
      1,
      1000,
      60 * 1000,
      60 * 60 * 1000
  };

  private static List<string> _units = new List<string>
  {
      "ns",
      "µs",
      "ms",
      "s",
      "min",
      "h"
  };

  public string FormatTime(double milliseconds, string format = "#.#")
  {
    var interval = _intervals.Last(i => i <= milliseconds);
    var index = _intervals.IndexOf(interval);

    return string.Concat((milliseconds / interval).ToString(format), " ", _units[index]);
  }

  public static string ReadableTime(int milliseconds)
  {
    TimeSpan t = TimeSpan.FromMilliseconds(milliseconds);
    return string.Format(
      "{0:D2}:{1:D2}:{2:D2}",
      t.Minutes,
      t.Seconds,
      t.Milliseconds);
  }
}
