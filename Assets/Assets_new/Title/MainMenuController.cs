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

  private MainMenuState menuState = MainMenuState.Between;

  private Animation animation;

  private Animator worldAnimator;

  private Animation levelAnimation;

  private Image modalBlocker;

  private static int worldSelected = 1;
  private static bool worldCurrentlySelected = false;
  private static int levelSelected = 1;

  protected void Awake()
  {
    animation = GetComponent<Animation>();
    worldAnimator = GameObject.Find("Worlds").GetComponent<Animator>();
    modalBlocker = GameObject.Find("ModalBlocker").GetComponent<Image>();
    levelAnimation = GameObject.Find("FullLevelSelect/Levels").GetComponent<Animation>();
    UpdateWorldSelectButtons();
    FlushWorldSelectAnimation();
    if (worldCurrentlySelected)
      NextStateToLoad = MainMenuState.LevelSelect;
    FadeWorlds();
  }

  protected void Start()
  {
    switch (NextStateToLoad)
    {
      case MainMenuState.LevelSelect:
        ShowPageInstant("WorldToLevelSelect", NextStateToLoad);
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

  public static IEnumerator PlayAnimation(Animation animation, string name, bool reverse, Action complete)
  {
    Debug.Log($"Playing animation {name} " + (reverse ? "forwards" : "Backwards"));
    Debug.Log(animation);
    animation[name].speed = (reverse ? -1.0f : 1.0f) / Time.timeScale;
    animation[name].time = reverse ? animation[name].length : 0.0f;
    animation.Play(name);
    while (animation.isPlaying)
    {
      yield return new WaitForSeconds(0.0f);
    }
    complete();
  }

  public void FlushWorldSelectAnimation()
  {
    worldAnimator.SetInteger("World", worldSelected);
    worldAnimator.SetBool("WorldSelected", worldCurrentlySelected);
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
      GameObject.Find("FullLevelSelect/Next/Tap Area").GetComponent<Button>().interactable = (worldSelected < MinWorld());
      GameObject.Find("FullLevelSelect/Previous/Tap Area").GetComponent<Button>().interactable = (worldSelected > MaxWorld());
    }

    GameObject.Find("CurrentWorld/Caption").GetComponent<Text>().text = $"World {worldSelected}";
    GameObject.Find("CurrentLevel/Caption").GetComponent<Text>().text = CurLevel().name;
  }

  public void NextWorld()
  {
    // These act as level select if a world is selected
    if (worldCurrentlySelected && levelSelected < MaxLevel())
    {
      levelSelected++;
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
    // These act as level select if a world is selected
    if (worldCurrentlySelected && levelSelected > MinLevel())
    {
      levelSelected--;
      StartCoroutine(PlayAnimation(levelAnimation, "NextLevel", true, () => { }));
    }
    else if (!worldCurrentlySelected && worldSelected > MinWorld())
    {
      worldSelected--;
    }

    FlushWorldSelectAnimation();
    UpdateWorldSelectButtons();
  }

  public void SelectWorld()
  {
    // These act as level select if a world is selected
    if (worldCurrentlySelected)
    {
      SceneManager.LoadScene(CurLevel().name);
    }
    else
    {
      worldCurrentlySelected = true;
      levelSelected = 1;
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
    return SaveManager.save.worldUnlocked;
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
        if (worldNum > SaveManager.save.worldUnlocked)
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
}
