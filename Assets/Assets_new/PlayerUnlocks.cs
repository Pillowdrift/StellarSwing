using UnityEngine;
using UnityEngine.UI;

// Hats and powerups bought from the Unlocks screen.
// Powerups are held on right click and run off a charge that only refills when the
// level is restarted or another level is started.
public class PlayerUnlocks : MonoBehaviour
{
  public enum Powerup
  {
    None,
    Jump,
    Jetpack,
    Boost,
    BulletTime
  }

  [Header("Jump: a few hops, only from a platform")]
  public int JumpCharges = 3;
  public float JumpSpeed = 24.0f;

  [Header("Jetpack: thrust upwards")]
  public float JetpackSeconds = 2.5f;
  public float JetpackAcceleration = 70.0f;
  public float JetpackMaxRise = 18.0f;

  [Header("Boost: speed up along your heading")]
  public float BoostSeconds = 2.0f;
  public float BoostAcceleration = 45.0f;
  public float BoostMaxSpeed = 90.0f;

  [Header("Bullet time: slow everything down (real seconds)")]
  public float BulletTimeSeconds = 5.0f;
  public float BulletTimeScale = 0.4f;

  [Header("Effects")]
  public Material ThrustMaterial;
  public Font HudFont;
  public AudioClip JumpClip;
  public AudioClip ThrustClip;
  public AudioClip SlowInClip;
  public AudioClip SlowOutClip;
  public AudioClip EmptyClip;

#if UNITY_EDITOR
  // Lets editor scripts hold the powerup button, since they can't send real mouse input
  public static bool DebugHeld;
#endif

  // Whether a powerup was used this attempt; the "No powerups" bonus depends on it
  public static bool UsedThisAttempt { get; private set; }

  static readonly Color Amber = new Color(0.878f, 0.592f, 0.235f);

  private Powerup currentPowerup = Powerup.None;
  private Rigidbody _rigidbody;
  private float hatHeight = 0.0f;

  private float charge = 1.0f;
  private int jumpsLeft;
  private float lastGrounded = -1.0f;
  private bool held;
  private bool active;
  private bool slowed;
  private float baseFixedDeltaTime;

  private AudioSource oneShots;
  private AudioSource thrustLoop;
  private ParticleSystem thrust;
  private CanvasGroup hud;
  private Text hudLabel;
  private Text hudHint;
  private Image barFill;
  private Image[] pips;
  private Image slowTint;
  private RectTransform hudPanel;
  private RectTransform counterText;

  private void Start()
  {
    _rigidbody = GetComponent<Rigidbody>();
    UsedThisAttempt = false;
    baseFixedDeltaTime = Time.fixedDeltaTime;

    if (SaveManager.save != null)
    {
      foreach (var hat in SaveManager.save.currentHats)
        EnableHat(hat);
      EnablePowerup(SaveManager.save.currentUpgrade);
    }

    if (currentPowerup != Powerup.None)
    {
      jumpsLeft = JumpCharges;
      BuildEffects();
      BuildHud();
    }
  }

  // Powerups only work while actually playing: not before the start click, while paused,
  // dead, finished, in a tutorial or watching a replay
  private bool CanUse()
  {
    return GUIController.LevelStarted && !LevelState.Dead && !LevelState.HasFinished &&
           !GameRecorder.playingBack && !PauseButton.paused && Time.timeScale > 0.0f && !ControlManager.Disabled &&
           !TutorialCamera.Enabled();
  }

  private void Update()
  {
    if (currentPowerup == Powerup.None)
      return;

    bool usable = CanUse();
    bool down = Input.GetMouseButtonDown(1);
    bool hold = Input.GetMouseButton(1);
#if UNITY_EDITOR
    down |= DebugHeld && !held;
    hold |= DebugHeld;
#endif
    bool pressed = usable && down;
    held = usable && hold;

    switch (currentPowerup)
    {
      case Powerup.Jump:
        if (pressed)
          TryJump();
        break;

      case Powerup.Jetpack:
      case Powerup.Boost:
        if (pressed && charge <= 0)
          PlayOneShot(EmptyClip, 0.6f);
        SetActive(held && charge > 0);
        if (active)
          Drain(Time.deltaTime / (currentPowerup == Powerup.Jetpack ? JetpackSeconds : BoostSeconds));
        break;

      case Powerup.BulletTime:
        if (pressed && charge <= 0)
          PlayOneShot(EmptyClip, 0.6f);
        SetSlow(held && charge > 0);
        if (slowed)
          Drain(Time.unscaledDeltaTime / BulletTimeSeconds);
        break;
    }

    UpdateHud(usable);
  }

  private void FixedUpdate()
  {
    if (!active)
      return;

    var vel = _rigidbody.linearVelocity;
    if (currentPowerup == Powerup.Jetpack)
    {
      if (vel.y < JetpackMaxRise)
        _rigidbody.AddForce(Vector3.up * JetpackAcceleration, ForceMode.Acceleration);
    }
    else if (currentPowerup == Powerup.Boost)
    {
      var dir = vel.sqrMagnitude > 1.0f ? vel.normalized : transform.forward;
      if (vel.magnitude < BoostMaxSpeed)
        _rigidbody.AddForce(dir * BoostAcceleration, ForceMode.Acceleration);
    }
  }

  private void OnCollisionStay(Collision collision)
  {
    for (int i = 0; i < collision.contactCount; i++)
    {
      if (collision.GetContact(i).normal.y > 0.5f)
      {
        lastGrounded = Time.time;
        return;
      }
    }
  }

  private void TryJump()
  {
    if (jumpsLeft <= 0)
    {
      PlayOneShot(EmptyClip, 0.6f);
      return;
    }
    // Only off a platform, so it can't be used to fly
    if (Time.time - lastGrounded > 0.15f)
      return;

    jumpsLeft--;
    UsedThisAttempt = true;
    var vel = _rigidbody.linearVelocity;
    vel.y = Mathf.Max(vel.y, 0.0f) + JumpSpeed;
    _rigidbody.linearVelocity = vel;
    lastGrounded = -1.0f;
    PlayOneShot(JumpClip, 0.8f);
    if (thrust != null)
      thrust.Emit(25);
  }

  private void Drain(float amount)
  {
    charge = Mathf.Max(0.0f, charge - amount);
    UsedThisAttempt = true;
    if (charge <= 0)
      PlayOneShot(EmptyClip, 0.6f);
  }

  private void SetActive(bool on)
  {
    if (on == active)
      return;
    active = on;

    var emission = thrust.emission;
    emission.enabled = on;
    if (on)
    {
      thrustLoop.volume = Volume() * 0.5f;
      thrustLoop.Play();
    }
    else
    {
      thrustLoop.Stop();
    }
  }

  private void SetSlow(bool on)
  {
    if (on == slowed)
      return;
    slowed = on;

    if (on)
    {
      Time.timeScale = BulletTimeScale;
      Time.fixedDeltaTime = baseFixedDeltaTime * BulletTimeScale;
      PlayOneShot(SlowInClip, 0.7f);
    }
    else
    {
      // Pausing sets its own time scale; only hand back normal speed if we're still in charge of it
      if (!PauseButton.paused && Time.timeScale > 0.0f)
        Time.timeScale = 1.0f;
      Time.fixedDeltaTime = baseFixedDeltaTime;
      PlayOneShot(SlowOutClip, 0.6f);
    }
  }

  private void OnDisable()
  {
    if (slowed)
    {
      slowed = false;
      if (Time.timeScale > 0.0f)
        Time.timeScale = 1.0f;
      Time.fixedDeltaTime = baseFixedDeltaTime;
    }
  }

  private float Volume()
  {
    return Settings.Current.MasterVolume * Settings.Current.SoundVolume;
  }

  private void PlayOneShot(AudioClip clip, float volume)
  {
    if (clip != null && oneShots != null)
      oneShots.PlayOneShot(clip, volume * Volume());
  }

  private void BuildEffects()
  {
    oneShots = gameObject.AddComponent<AudioSource>();
    oneShots.playOnAwake = false;
    oneShots.ignoreListenerPause = true;

    thrustLoop = gameObject.AddComponent<AudioSource>();
    thrustLoop.playOnAwake = false;
    thrustLoop.loop = true;
    thrustLoop.clip = ThrustClip;

    // Sparks out the back for the jetpack and boost, and a puff for each jump
    var go = new GameObject("Powerup thrust");
    go.transform.SetParent(transform, false);
    thrust = go.AddComponent<ParticleSystem>();
    thrust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    var main = thrust.main;
    main.duration = 1.0f;
    main.loop = true;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(6.0f, 12.0f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
    main.startColor = new ParticleSystem.MinMaxGradient(Amber, new Color(1.0f, 0.45f, 0.15f));
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.maxParticles = 300;
    var emission = thrust.emission;
    emission.rateOverTime = 70.0f;
    emission.enabled = false;
    var shape = thrust.shape;
    shape.shapeType = ParticleSystemShapeType.Cone;
    shape.angle = 18.0f;
    shape.radius = 0.2f;
    var col = thrust.colorOverLifetime;
    col.enabled = true;
    var grad = new Gradient();
    grad.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                 new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
    col.color = grad;
    var size = thrust.sizeOverLifetime;
    size.enabled = true;
    size.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.Linear(0, 1, 1, 0.2f));
    var renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.sharedMaterial = ThrustMaterial;
    thrust.Play();
  }

  private void LateUpdate()
  {
    // Point the exhaust opposite to the push
    if (thrust == null)
      return;
    Vector3 push = Vector3.up;
    if (currentPowerup == Powerup.Boost && _rigidbody.linearVelocity.sqrMagnitude > 1.0f)
      push = _rigidbody.linearVelocity.normalized;
    thrust.transform.rotation = Quaternion.LookRotation(-push);
  }

  private void BuildHud()
  {
    var root = new GameObject("Powerup HUD", typeof(RectTransform));
    var canvas = root.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 20;
    var scaler = root.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);
    scaler.matchWidthOrHeight = 1.0f;

    // Cool tint round the edges while time is slowed
    if (currentPowerup == Powerup.BulletTime)
    {
      slowTint = UiChild(root.transform, "Slow tint", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
      slowTint.sprite = Vignette();
      slowTint.color = new Color(0.45f, 0.75f, 1.0f, 0.0f);
      slowTint.raycastTarget = false;
    }

    // The power bar sits under the picolinium counter in the level HUD, styled to match
    var rightSide = GameObject.Find("UIs/MainGUI/Content/General/Resources/Right-Side");
    var counter = rightSide != null ? rightSide.transform.Find("Picolinium/Value/Content/Text") : null;
    RectTransform panel;
    if (counter != null)
    {
      counterText = (RectTransform)counter;
      panel = UiChild(rightSide.transform, "Powerup", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(128, 44));
      panel.pivot = new Vector2(1, 1);
    }
    else
    {
      panel = UiChild(root.transform, "Powerup", Vector2.one, Vector2.one, new Vector2(-40, -110), new Vector2(280, 82));
      panel.pivot = new Vector2(1, 1);
    }
    hudPanel = panel;
    float w = panel.sizeDelta.x, h = panel.sizeDelta.y, u = h / 44.0f;
    hud = panel.gameObject.AddComponent<CanvasGroup>();
    hud.alpha = 0;
    hud.blocksRaycasts = false;

    hudLabel = Label(panel, "Label", TextAnchor.LowerRight, Mathf.RoundToInt(18 * u), new Vector2(0, 18 * u), new Vector2(w, 26 * u));
    hudLabel.text = PowerupName(currentPowerup);
    hudHint = Label(panel, "Hint", TextAnchor.UpperRight, Mathf.RoundToInt(12 * u), new Vector2(0, -16 * u), new Vector2(w, 16 * u));
    hudHint.text = "Hold right click";
    hudHint.color = new Color(1, 1, 1, 0.7f);

    var backing = new Color(0, 0, 0, 0.45f);
    if (currentPowerup == Powerup.Jump)
    {
      pips = new Image[JumpCharges];
      float gap = 4 * u, pw = 22 * u;
      for (int i = 0; i < JumpCharges; i++)
      {
        var back = Box(panel, "Pip", new Vector2(w - (JumpCharges - i) * (pw + gap) + gap, 6 * u), new Vector2(pw, 9 * u), backing);
        pips[i] = Box(back, "Fill", new Vector2(1.5f * u, 1.5f * u), new Vector2(pw - 3 * u, 6 * u), Amber);
      }
      hudHint.text = "Right click on a platform";
    }
    else
    {
      var back = Box(panel, "Bar", new Vector2(0, 6 * u), new Vector2(w, 9 * u), backing);
      barFill = Box(back, "Fill", new Vector2(1.5f * u, 1.5f * u), new Vector2(w - 3 * u, 6 * u), Amber);
      barFill.type = Image.Type.Filled;
      barFill.fillMethod = Image.FillMethod.Horizontal;
      barFill.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
    }
  }

  private void UpdateHud(bool usable)
  {
    if (hud == null)
      return;

    // Right-aligned with the counter's number, just below it
    if (counterText != null)
    {
      var corners = new Vector3[4];
      counterText.GetWorldCorners(corners);
      hudPanel.position = new Vector3(corners[2].x, corners[0].y - 2.0f * hudPanel.lossyScale.y, 0);
    }

    bool show = usable || (GUIController.LevelStarted && PauseButton.paused);
    hud.alpha = Mathf.MoveTowards(hud.alpha, show ? 1.0f : 0.0f, Time.unscaledDeltaTime * 4.0f);

    // The hint fades once the powerup has been tried
    var hint = hudHint.color;
    hint.a = Mathf.MoveTowards(hint.a, UsedThisAttempt ? 0.0f : 0.7f, Time.unscaledDeltaTime * 2.0f);
    hudHint.color = hint;

    bool on = active || slowed;
    var lit = on ? Color.Lerp(Amber, Color.white, 0.35f) : Amber;
    if (barFill != null)
    {
      barFill.fillAmount = charge;
      barFill.color = charge > 0 ? lit : Amber;
    }
    if (pips != null)
    {
      for (int i = 0; i < pips.Length; i++)
        pips[i].color = i < jumpsLeft ? Amber : new Color(1, 1, 1, 0.12f);
    }
    hudLabel.color = (currentPowerup == Powerup.Jump ? jumpsLeft > 0 : charge > 0) ? Color.white : new Color(1, 1, 1, 0.45f);

    if (slowTint != null)
    {
      var c = slowTint.color;
      c.a = Mathf.MoveTowards(c.a, slowed ? 0.45f : 0.0f, Time.unscaledDeltaTime * 3.0f);
      slowTint.color = c;
    }
  }

  public static string PowerupName(Powerup powerup)
  {
    return powerup == Powerup.BulletTime ? "Bullet time" : powerup.ToString();
  }

  private RectTransform UiChild(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
  {
    var go = new GameObject(name, typeof(RectTransform));
    var rt = (RectTransform)go.transform;
    rt.SetParent(parent, false);
    rt.anchorMin = anchorMin;
    rt.anchorMax = anchorMax;
    rt.pivot = new Vector2(0.5f, 0);
    rt.anchoredPosition = pos;
    rt.sizeDelta = size;
    return rt;
  }

  private Image Box(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
  {
    var go = new GameObject(name, typeof(RectTransform));
    var rt = (RectTransform)go.transform;
    rt.SetParent(parent, false);
    rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
    rt.anchoredPosition = pos;
    rt.sizeDelta = size;
    var img = go.AddComponent<Image>();
    img.color = color;
    img.raycastTarget = false;
    return img;
  }

  private Image Box(Image parent, string name, Vector2 pos, Vector2 size, Color color)
  {
    return Box(parent.transform, name, pos, size, color);
  }

  private Text Label(Transform parent, string name, TextAnchor align, int size, Vector2 pos, Vector2 box)
  {
    var go = new GameObject(name, typeof(RectTransform));
    var rt = (RectTransform)go.transform;
    rt.SetParent(parent, false);
    rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
    rt.anchoredPosition = pos;
    rt.sizeDelta = box;
    var text = go.AddComponent<Text>();
    text.font = HudFont;
    text.fontSize = size;
    text.alignment = align;
    text.raycastTarget = false;
    var shadow = go.AddComponent<Shadow>();
    // Same shadow as the picolinium counter
    shadow.effectColor = new Color(0, 0, 0, 0.5f);
    shadow.effectDistance = new Vector2(2, -2);
    return text;
  }

  // Soft radial vignette: clear in the middle, solid at the corners
  private static Sprite Vignette()
  {
    const int n = 128;
    var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
    tex.wrapMode = TextureWrapMode.Clamp;
    var px = new Color32[n * n];
    for (int y = 0; y < n; y++)
    {
      for (int x = 0; x < n; x++)
      {
        float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
        float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.414f;
        float a = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.35f, 1.0f, d));
        px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
      }
    }
    tex.SetPixels32(px);
    tex.Apply();
    return Sprite.Create(tex, new Rect(0, 0, n, n), Vector2.one * 0.5f);
  }

  private void EnableHat(string hatToEnable)
  {
    var hats = transform.Find("Hats");

    for (int i = 0; i < hats.childCount; ++i)
    {
      var hat = hats.GetChild(i).gameObject;
      if (hatToEnable == hat.name)
      {
        hat.SetActive(true);

        // Stack em up except the mustache
        if (hat.name != "Mustache")
        {
          float height = hat.GetComponent<MeshFilter>().mesh.bounds.size.y * hat.transform.lossyScale.y;
          var pos = hat.transform.position;
          pos.y += hatHeight;
          hat.transform.position = pos;
          hatHeight += height * 0.5f * 0.7f;
        }
      }
    }
  }

  private void EnablePowerup(string powerupToEnable)
  {
    switch (powerupToEnable)
    {
      case "Jump":
        currentPowerup = Powerup.Jump;
        break;
      case "Jetpack":
        currentPowerup = Powerup.Jetpack;
        break;
      case "Boost":
        currentPowerup = Powerup.Boost;
        break;
      case "BulletTime":
        currentPowerup = Powerup.BulletTime;
        break;
      default:
        currentPowerup = Powerup.None;
        break;
    }
  }
}
