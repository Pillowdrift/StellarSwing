// #define ENABLE_BUG_BUTTON

using UnityEngine;
using System.Collections;

public class EndDeathScript : MonoBehaviour
{
  private const float LABEL_WIDTH = 180.0f;
  private const float LABEL_HEIGHT = 100.0f;

  private const float BUTTON_WIDTH = 200.0f;
  private const float BUTTON_HEIGHT = 80.0f;

  private const float RESET_TIME = 1.0f;

  public float minDistanceForWarning = 10.0f;

  public bool explode = true;

  private Vector2 screenSpacePos;
  private float distance;

  public string WorldMapSceneName;

  public ThirdPersonCamera aCam;

  public static bool hasFinished = false;

  public GameObject playerWarningPrefab;

  private GameObject player;
  private GameObject playerWarning;

  private GrapplingHook grapplingHook;

  private Animator MainGUI;

  // Kill immediately, even if the player is grappled etc, for grids and planets
  public bool Immediate = false;

  // The player currently in the death trigger.
  // If they stop grappling or moving up, kill em.
  private GameObject playerCurrentlyInArea;

  private AchievementUnlocker crashUnlocker;

  void Start()
  {
    MainGUI = GameObject.Find("MainGUI").GetComponent<Animator>();

    hasFinished = false;

    player = GameObject.Find("Player");
    grapplingHook = player.GetComponent<GrapplingHook>();

    if (playerWarningPrefab != null)
      playerWarning = GameObject.Instantiate(playerWarningPrefab) as GameObject;

    playerCurrentlyInArea = null;
  }

  void Update()
  {
    if (playerWarning != null)
    {
      // Move death warning under player
      Vector3 position = player.transform.position;
      position.y = transform.position.y + GetComponent<Collider>().bounds.extents.y;
      playerWarning.transform.position = position;

      // Fade its transparency on player distance
      Color colour = playerWarning.GetComponent<Renderer>().material.color;
      float dist = (player.transform.position - playerWarning.transform.position).magnitude;
      colour.a = minDistanceForWarning / dist;
      playerWarning.GetComponent<Renderer>().material.color = colour;
    }

    if (playerCurrentlyInArea != null)
    {
      Debug.Log("Player in death area");

      var rigidbody = playerCurrentlyInArea.GetComponent<Rigidbody>();
      var grapplingHook = playerCurrentlyInArea.GetComponent<GrapplingHook>();

      // If we aren't grappled and we aren't moving up, kill them
      if (!grapplingHook.IsGrappled)
      {
        float dot = Vector3.Dot(Vector3.up, rigidbody.velocity.normalized);
        if (dot < -0.25f)
        {
          KillPlayer(playerCurrentlyInArea);
          playerCurrentlyInArea = null;
        }
      }
    }
  }

  void OnTriggerEnter(Collider collider)
  {
    // Old style
    //if (collider.tag == "Player")

    if (collider.tag == "Player")
    {
      if (Immediate)
      {
        KillPlayer(collider.gameObject);
      }
      else
      {
        playerCurrentlyInArea = collider.gameObject;
      }
    }
  }

  void OnTriggerExit(Collider collider)
  {
    if (collider.gameObject == playerCurrentlyInArea)
      playerCurrentlyInArea = null;
  }


  void KillPlayer(GameObject player)
  {
    if (TutorialCamera.Enabled())
    {
      GameRecorder.StopPlayback();
      if (explode)
      {
        player.SendMessage("Explode");
        SoundManager.Play("crash");

        player.SendMessage("Reload");
        IncrementCrashCount(true);
      }

      return;
    }

    if (!LevelState.Dead)
    {
      MainGUI.Play("RestartLevel");

      // Disable text
      GUIController.DisableTexts();

      // Update drone count
      if (SaveManager.save != null)
      {
        if (explode) IncrementCrashCount(false);
        SaveManager.save.droneCount++;
        SaveManager.Write();

        // Display drone coun
        if (false)
        {
          GameObject thingy = Tutorial.ShowText("DroneText", "Drones lost: " + SaveManager.save.droneCount, 0, TextAlignment.Center, TextAnchor.MiddleCenter, 0.5f, 0.5f);
          Mover mover = thingy.AddComponent<Mover>();
          mover.direction = Vector3.up;
          mover.speed = 0.2f;
          TextFader fader = thingy.AddComponent<TextFader>();
          fader.fadeRate = 1.0f;
          fader.FadeOut();
        }
      }

      // Update level state
      GUIController.LevelStarted = false;
      LevelStart.started = false;
      LevelState.Dead = true;

      if (explode)
      {
        // Create explosion effect
        player.SendMessage("Explode");

        // Crash sound
        SoundManager.Play("crash");

        // Disable renderers if exploding
        player.GetComponent<Renderer>().enabled = false;
        player.transform.Find("Shield").GetComponent<Renderer>().enabled = false;

      }

      // Disable default camera controller until level restarts
      ThirdPersonCamera cameraController = Camera.main.GetComponent<ThirdPersonCamera>();
      if (cameraController != null)
        cameraController.enabled = false;

      // Detach grappling hook so we don't keep swinging
      grapplingHook.Detach();

      // I don't really remember why
      ControlManager.DisabledFrame = true;

      // Pause and then restart the level
      StartCoroutine(PauseBeforeReset());
    }
  }

  void IncrementCrashCount(bool write)
  {
    if (SaveManager.save != null)
    {
      SaveManager.save.crashCount += 1;
      if (write)
      {
        SaveManager.Write();
      }
      if (SaveManager.save.crashCount >= 3)
      {
        if (crashUnlocker == null)
        {
          crashUnlocker = Resources.Load<GameObject>("AchievementUnlocker").GetComponent<AchievementUnlocker>();
          if (crashUnlocker != null)
          {
            crashUnlocker.AchievementIDStr = "dronecrash";
            crashUnlocker.UnlockAchievement();
          }
        }
      }
    }
  }

  IEnumerator PauseBeforeReset()
  {
    float time = 0.0f;
    while (time < RESET_TIME && !Input.GetMouseButtonDown(0))
    {
      time += Time.deltaTime;
      yield return null;
    }

    // Stop the recording, and also the line it generates
    GameRecorder.StopRecording();

    // Reset player
    player.BroadcastMessage("Reload");
    player.GetComponent<Renderer>().enabled = true;

    // Lerp camera back to start transform
    Camera.main.SendMessage("LerpToStartPos");
  }
}
