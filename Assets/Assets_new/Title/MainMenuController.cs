using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class MainMenuController : MonoBehaviour
{
  public enum MainMenuState
  {
    Between,
    Title,
    Menu,
    Options,
    WorldSelect,
    LevelSelect,
    Stats,
    Unlocks,
  }

  public static MainMenuState NextStateToLoad = MainMenuState.Between;

  public GameObject LevelCompletePrefab;
  public AudioClip LevelCompleteSound;

  // Used if the screen to show is the level select, shows the next level with an unlock animation
  public static bool UnlockNextWorld = false;
  public static bool UnlockNextLevel = false;

  private MainMenuState menuState = MainMenuState.Between;

  private Animation animation;
  private Animator worldAnimator;
  private Animation levelAnimation;
  private Image modalBlocker;
  private AudioSource source;

  private static int worldSelected = 1;
  private static bool worldCurrentlySelected = false;
  private static int levelSelected = 1;

  protected void Awake()
  {
    animation = GetComponent<Animation>();
    source = GetComponent<AudioSource>();
    worldAnimator = GameObject.Find("Worlds").GetComponent<Animator>();
    modalBlocker = GameObject.Find("ModalBlocker").GetComponent<Image>();
    levelAnimation = GameObject.Find("FullLevelSelect/Levels").GetComponent<Animation>();

    // World select stuff
    worldSelected = MaxWorld();
    Debug.Log("Showing world " + worldSelected);

    UpdateWorldSelectButtons();
    FlushWorldSelectAnimation(true);
    FadeWorlds();

    if (worldCurrentlySelected)
      NextStateToLoad = UnlockNextWorld ? MainMenuState.WorldSelect : MainMenuState.LevelSelect;

    if (UnlockNextWorld)
    {
      StartCoroutine(UnlockNextWorldNow());
    }
    else if (UnlockNextLevel)
    {
      StartCoroutine(UnlockNextLevelNow());
    }

    UnlockNextLevel = false;
    UnlockNextWorld = false;
  }

  protected void Start()
  {
    switch (NextStateToLoad)
    {
      case MainMenuState.LevelSelect:
        ShowPageInstant("WorldToLevelSelect", NextStateToLoad);
        LoadPreview(false, worldSelected, levelSelected);
        break;
      case MainMenuState.WorldSelect:
        ShowPageInstant("MenuToWorldSelect", NextStateToLoad);
        break;
      case MainMenuState.Menu:
        ShowPageInstant("TitleToMenu", NextStateToLoad);
        break;
      case MainMenuState.Options:
        ShowPageInstant("MenuToOptions", NextStateToLoad);
        break;
      default:
        StartCoroutine(PlayAnimation(animation, "Title_Intro", false, () =>
        {
          menuState = MainMenuState.Title;
        }));
        break;
    }
  }

  private void ShowPageInstant(string anim, MainMenuState state)
  {
    Debug.Log("Showing " + state + " instantly");

    var fadeObject = GameObject.Find("FadeImage");
    if (fadeObject != null)
      fadeObject.GetComponent<Image>().color = Color.black;

    menuState = MainMenuState.Between;
    StartCoroutine(PlayAnimation(animation, anim, false, () =>
    {
      menuState = state;
    }));
  }

  public void ShowLevelSelect()
  {
    FadeWorlds();
    menuState = MainMenuState.Between;
    StartCoroutine(PlayAnimation(animation, "MenuToWorldSelect", false, () =>
    {
      menuState = MainMenuState.WorldSelect;
    }));
  }

  public void HideLevelSelect()
  {
    if (menuState == MainMenuState.Between)
      return;

    if (worldCurrentlySelected)
    {
      worldCurrentlySelected = !worldCurrentlySelected;
      FlushWorldSelectAnimation();
      UpdateWorldSelectButtons();

      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "WorldToLevelSelect", true, () =>
      {
        menuState = MainMenuState.WorldSelect;
      }));
    }
    else
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "MenuToWorldSelect", true, () =>
      {
        menuState = MainMenuState.Menu;
      }));
    }
  }


  protected void Update()
  {
    if (modalBlocker.raycastTarget)
      return;

    if (menuState == MainMenuState.Title)
    {
      if (Input.GetButton("Fire1"))
      {
        menuState = MainMenuState.Between;
        StartCoroutine(PlayAnimation(animation, "TitleToMenu", false, () =>
        {
          menuState = MainMenuState.Menu;
        }));
      }
    }
    else if (menuState == MainMenuState.Menu)
    {
      if (Input.GetButton("Cancel"))
      {
        menuState = MainMenuState.Between;
        StartCoroutine(PlayAnimation(animation, "TitleToMenu", true, () =>
        {
          menuState = MainMenuState.Title;
        }));
      }
    }
    else if (menuState == MainMenuState.Options)
    {
      if (Input.GetButton("Cancel"))
      {
        menuState = MainMenuState.Between;
        StartCoroutine(PlayAnimation(animation, "MenuToOptions", true, () =>
        {
          menuState = MainMenuState.Menu;
        }));
      }
    }
  }

  public void ShowOptions()
  {
    if (modalBlocker.raycastTarget)
      return;

    FindObjectOfType<Settings>().InitPage();

    if (menuState == MainMenuState.Menu)
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "MenuToOptions", false, () =>
      {
        menuState = MainMenuState.Options;
      }));
    }
  }

  public void CloseOptions()
  {
    if (modalBlocker.raycastTarget)
      return;

    if (menuState == MainMenuState.Options)
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "MenuToOptions", true, () =>
      {
        menuState = MainMenuState.Menu;
      }));
    }
  }

  public void ShowStats()
  {
    if (modalBlocker.raycastTarget)
      return;

    if (menuState == MainMenuState.WorldSelect)
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "LevelSelectToStats", false, () =>
      {
        menuState = MainMenuState.Stats;
      }));
    }
  }

  public void CloseStats()
  {
    if (modalBlocker.raycastTarget)
      return;

    if (menuState == MainMenuState.Stats)
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "LevelSelectToStats", true, () =>
      {
        menuState = MainMenuState.WorldSelect;
      }));
    }
  }

  public void ShowUnlocks()
  {
    if (modalBlocker.raycastTarget)
      return;

    if (menuState == MainMenuState.WorldSelect)
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "LevelSelectToUnlocks", false, () =>
      {
        menuState = MainMenuState.Unlocks;
      }));
    }
  }

  public void CloseUnlocks()
  {
    if (modalBlocker.raycastTarget)
      return;

    if (menuState == MainMenuState.Unlocks)
    {
      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "LevelSelectToUnlocks", true, () =>
      {
        menuState = MainMenuState.WorldSelect;
      }));
    }
  }


  public void ShowCredits()
  {
    SceneManager.LoadScene("Credits");
  }

  public void Exit()
  {
    var fade = GameObject.Find("FadeImage")?.GetComponent<Image>();
    fade.color = Color.black;
    fade.CrossFadeAlpha(1.0f, 1.0f, true);
    StartCoroutine(ExitCoroutine(1.0f));
  }

  public IEnumerator ExitCoroutine(float seconds)
  {
    yield return new WaitForSeconds(seconds);
    Application.Quit();
  }

  public static IEnumerator PlayAnimation(Animation animation, string name, bool reverse, Action complete, bool instant = false)
  {
    Debug.Log($"Playing animation {name} " + (reverse ? "forwards" : "Backwards"));
    Debug.Log(animation);
    animation[name].speed = (reverse ? -1.0f : 1.0f) / Time.timeScale;
    if (instant)
      animation[name].time = reverse ? 0.0f : animation[name].length;
    else
      animation[name].time = reverse ? animation[name].length : 0.0f;
    animation.Play(name);
    while (animation.isPlaying)
    {
      yield return new WaitForSeconds(0.0f);
    }
    complete();
  }

  public void FlushWorldSelectAnimation(bool init = false)
  {
    worldAnimator.SetInteger("World", worldSelected);
    worldAnimator.SetBool("WorldSelected", worldCurrentlySelected);

    if (init)
    {
      Debug.Log($"Instant loading!! {worldCurrentlySelected}");
      worldAnimator.Play(worldCurrentlySelected ? $"World{worldSelected}Selected" : $"World{worldSelected}");

      if (menuState == MainMenuState.LevelSelect)
      {
        menuState = MainMenuState.Between;
        StartCoroutine(PlayAnimation(animation, "WorldToLevelSelect", false, () =>
        {
          menuState = MainMenuState.LevelSelect;
        }, true));
      }
    }
  }

  public void UpdateWorldSelectButtons()
  {
    if (worldCurrentlySelected)
    {
      GameObject.Find("FullLevelSelect/Next/Tap Area").GetComponent<Button>().interactable = (levelSelected < MaxLevel());
      GameObject.Find("FullLevelSelect/Previous/Tap Area").GetComponent<Button>().interactable = (levelSelected > MinLevel());
    }
    else
    {
      GameObject.Find("FullLevelSelect/Next/Tap Area").GetComponent<Button>().interactable = (worldSelected < MaxWorld());
      GameObject.Find("FullLevelSelect/Previous/Tap Area").GetComponent<Button>().interactable = (worldSelected > MinWorld());
    }

    GameObject.Find("CurrentWorld/Caption").GetComponent<Text>().text = $"World {worldSelected}";
    GameObject.Find("CurrentLevel/Caption").GetComponent<Text>().text = $"Level {levelSelected}";
  }

  public void NextWorld(bool force = false)
  {
    if (menuState == MainMenuState.Between && force == false)
      return;

    // These act as level select if a world is selected
    if (worldCurrentlySelected && levelSelected < MaxLevel())
    {
      int oldLevel = levelSelected;
      levelSelected++;
      LoadPreview(false, worldSelected, oldLevel);
      LoadPreview(true, worldSelected, levelSelected);
      StartCoroutine(PlayAnimation(levelAnimation, "NextLevel", false, () => { }));
    }
    else if (!worldCurrentlySelected && worldSelected < MaxWorld())
    {
      worldSelected++;
    }

    FlushWorldSelectAnimation();
    UpdateWorldSelectButtons();
  }

  public void PrevWorld()
  {
    if (menuState == MainMenuState.Between)
      return;

    // These act as level select if a world is selected
    if (worldCurrentlySelected && levelSelected > MinLevel())
    {
      int oldLevel = levelSelected;
      levelSelected--;
      LoadPreview(false, worldSelected, levelSelected);
      LoadPreview(true, worldSelected, oldLevel);
      StartCoroutine(PlayAnimation(levelAnimation, "NextLevel", true, () => { }));
    }
    else if (!worldCurrentlySelected && worldSelected > MinWorld())
    {
      worldSelected--;
    }

    FlushWorldSelectAnimation();
    UpdateWorldSelectButtons();
  }

  private void LoadPreview(bool next, int world, int number)
  {
    var level = GetLevel(world, number);
    Text label = GameObject.Find(next ? "NextLevel/LevelPreview/Text" : "CurLevel/LevelPreview/Text").GetComponent<Text>();
    label.text = $"World {world} - Level {number}";
    if (level != null && level.PreviewImage != null)
    {
      Image image = GameObject.Find(next ? "NextLevel/LevelPreview/PreviewImage" : "CurLevel/LevelPreview/PreviewImage").GetComponent<Image>();
      image.sprite = level.PreviewImage;
    }
  }

  public void SelectWorld()
  {
    if (menuState == MainMenuState.Between)
      return;

    // These act as level select if a world is selected
    if (worldCurrentlySelected)
    {
      var lvl = CurLevel();
      LevelSelectGUI.currentLevel = lvl;
      SceneManager.LoadScene(lvl.name);
    }
    else
    {
      worldCurrentlySelected = true;
      levelSelected = MaxLevel();
      LoadPreview(false, worldSelected, levelSelected);
      FlushWorldSelectAnimation();
      UpdateWorldSelectButtons();

      menuState = MainMenuState.Between;
      StartCoroutine(PlayAnimation(animation, "WorldToLevelSelect", false, () =>
      {
        menuState = MainMenuState.LevelSelect;
      }));
    }
  }

  private int MinLevel()
  {
    int res = GameObject.Find($"World{worldSelected}").GetComponent<Levels>().levels.First().number;
    Debug.Log("min level: " + res);
    return res;
  }

  private int MaxLevel()
  {
    int res = GameObject.Find($"World{worldSelected}").GetComponent<Levels>().levels.Last().number;
    if (SaveManager.save.worldUnlocked <= worldSelected && SaveManager.save.levelUnlocked < res)
      res = SaveManager.save.levelUnlocked;
    Debug.Log("max level: " + res);
    Debug.Log(SaveManager.save.levelUnlocked);
    return res;
  }

  private Level GetLevel(int world, int level)
  {
    var levels = GameObject.Find($"World{worldSelected}").GetComponent<Levels>().levels;
    return levels.SingleOrDefault(lvl => lvl.world == world && lvl.number == level);
  }

  private Level CurLevel()
  {
    var levels = GameObject.Find($"World{worldSelected}").GetComponent<Levels>().levels;
    return levels.Single(lvl => lvl.number == levelSelected);
  }

  public int MinWorld()
  {
    return 1;
  }

  public int MaxWorld()
  {
    if (SaveManager.save == null)
      return 1;
    var res = SaveManager.save.worldUnlocked;
    Debug.Log("Max world: " + res);
    return res;
  }

  public void FadeWorlds()
  {
    var worlds = GameObject.Find("FullLevelSelect/Worlds");
    for (int i = 0; i < worlds.transform.childCount; ++i)
    {
      var world = worlds.transform.GetChild(i);
      if (world.name.StartsWith("World") && int.TryParse(world.name.Substring(5), out int worldNum))
      {
        Debug.Log("Processing world " + worldNum);
        if (worldNum > MaxWorld())
        {
          var mat = world.GetComponent<Renderer>().material;
          var color = mat.GetColor("_Color");
          color.a = 0.25f;
          mat.SetColor("_Color", color);

          // fade
          mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
          mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
          mat.SetInt("_ZWrite", 0);
          mat.DisableKeyword("_ALPHATEST_ON");
          mat.EnableKeyword("_ALPHABLEND_ON");
          mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          mat.renderQueue = 3000;
        }
      }
    }
  }

  public IEnumerator UnlockNextWorldNow()
  {
    yield break;
    while (menuState != MainMenuState.WorldSelect)
      yield return new WaitForEndOfFrame();

    menuState = MainMenuState.Between;

    yield return new WaitForSeconds(1.0f);

    NextWorld();

    menuState = MainMenuState.WorldSelect;
  }

  public IEnumerator UnlockNextLevelNow()
  {
    Debug.Log("UnlockNextLevelNow: " + menuState);
    while (menuState != MainMenuState.LevelSelect)
      yield return new WaitForEndOfFrame();
    Debug.Log("UnlockNextLevelNow: " + menuState);

    menuState = MainMenuState.Between;
    //yield return new WaitForSeconds(1.0f);

    if (LevelCompleteSound != null)
    {
      source.volume = Settings.Current.MasterVolume * Settings.Current.SoundVolume;
      source.clip = LevelCompleteSound;
      source.Play();
    }

    if (LevelCompletePrefab != null)
    {
      GameObject.Instantiate(LevelCompletePrefab, new Vector3(481.8504f, 248.8504f, -116.0667f), Quaternion.identity);
    }

    yield return new WaitForSeconds(1.0f);
    NextWorld(true);

    menuState = MainMenuState.LevelSelect;
  }

  private void LoadLevelPreview()
  {
    StartCoroutine(CreateScenePreviewAsync());
  }

  private IEnumerator CreateScenePreviewAsync()
  {
    var parent = GameObject.Find("Levels/CurLevel");
    var level = CurLevel();
    LoadSceneParameters a = new LoadSceneParameters
    {
      loadSceneMode = LoadSceneMode.Additive,
      localPhysicsMode = LocalPhysicsMode.None
    };

    Debug.Log("Loading preview");
    var activeScene = SceneManager.GetActiveScene();
    var sceneLoad = SceneManager.LoadSceneAsync(level.name, a);
    //sceneLoad.allowSceneActivation = false;
    while (!sceneLoad.isDone)
      yield return null;
    Debug.Log("Preview loaded");
    SceneManager.SetActiveScene(activeScene);
    ProcessScenePreview(SceneManager.GetSceneByName(level.name), parent);
  }

  private void ProcessScenePreview(Scene scene, GameObject parent)
  {
    Debug.Log("Deactivating scene " + scene.name);
    return;

    // Deactivate objects we don't want, and put the rest in a master game object
    GameObject scenePreview = new GameObject($"Preview {scene.name}");

    foreach (GameObject obj in scene.GetRootGameObjects())
    {
      obj.transform.parent = scenePreview.transform;

      if (obj.GetComponent<Camera>() ||
        obj.GetComponent<Light>() ||
        obj.GetComponent<PlayerMovements>() ||
        obj.name.StartsWith("Planet"))
      {
        obj.SetActive(false);
      }

      var outline = obj.GetComponent<Outline>();
      if (outline != null)
      {
        var renderer = obj.GetComponent<Renderer>();
        foreach (var mat in renderer.materials)
        {
          if (mat.name.StartsWith("Outline"))
          {
            mat.renderQueue = 0;
          }
        }
        outline.enabled = false;
      }
    }

    // Get bounds and target scale
    var bounds = GetObjectBounds(scenePreview);
    float maxBounds = Math.Max(Math.Max(bounds.size.x, bounds.size.y), bounds.size.z);
    float targetScale = 40.0f / maxBounds;
    //scenePreview.AddComponent<ScaleFixer>().TargetScale = Vector3.one / maxBounds * 40.0f;

    // Center
    var center = new GameObject("Centerer");
    //center.transform.position = -bounds.center / targetScale;
    center.transform.parent = parent.transform;
    center.transform.localPosition = Vector3.zero;
    center.AddComponent<ScaleFixer>().TargetScale = Vector3.one;

    //scenePreview.transform.position -= bounds.center;
    for (int i = 0; i < scenePreview.transform.childCount; ++i)
    {
      scenePreview.transform.GetChild(i).transform.position -= bounds.center;
    }

    // Position
    scenePreview.transform.position = center.transform.position;
    scenePreview.transform.parent = center.transform;

    // Scale
    scenePreview.transform.localScale = targetScale * Vector3.one;

    // Add spinner
    var rotator = scenePreview.AddComponent<Rotator>();
    rotator.axis = Vector3.up;
    rotator.angle = 1.0f;

    // Might as well unload the scene after this
    SceneManager.UnloadSceneAsync(scene);
  }

  private Bounds GetObjectBounds(GameObject obj)
  {
    var mainRenderer = obj.GetComponent<Renderer>();
    var combinedBounds = mainRenderer != null ? mainRenderer.bounds : new Bounds();
    var renderers = obj.GetComponentsInChildren<Renderer>();
    foreach (var renderer in renderers)
    {
      combinedBounds.Encapsulate(renderer.bounds);
    }
    return combinedBounds;
  }
}
