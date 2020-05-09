using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
  public enum MainMenuState
  {
    Between,
    Title,
    Menu,
    Options,
    LevelSelect
  }

  public static MainMenuState NextStateToLoad = MainMenuState.Between;

  private MainMenuState menuState = MainMenuState.Between;

  private Animation animation;

  private Image modalBlocker;

  protected void Awake()
  {
    animation = GetComponent<Animation>();
    modalBlocker = GameObject.Find("ModalBlocker").GetComponent<Image>();
  }

  protected void Start()
  {
    switch (NextStateToLoad)
    {
      case MainMenuState.LevelSelect:
        ShowPageInstant("MenuToLevelSelect", NextStateToLoad);
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
    menuState = MainMenuState.Between;
    StartCoroutine(PlayAnimation(animation, "MenuToLevelSelect", false, () =>
    {
      menuState = MainMenuState.LevelSelect;
    }));
  }

  public void HideLevelSelect()
  {
    menuState = MainMenuState.Between;
    StartCoroutine(PlayAnimation(animation, "MenuToLevelSelect", true, () =>
    {
      menuState = MainMenuState.Menu;
    }));
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

  public void ShowCredits()
  {
    SceneManager.LoadScene("Credits");
  }

  public void Exit()
  {
    Application.Quit();
  }

  public static IEnumerator PlayAnimation(Animation animation, string name, bool reverse, Action complete)
  {
    Debug.Log($"Playing animation {name} " + (reverse ? "forwards" : "Backwards"));
    animation[name].speed = (reverse ? -1.0f : 1.0f) / Time.timeScale;
    animation[name].time = reverse ? animation[name].length : 0.0f;
    animation.Play(name);
    while (animation.isPlaying)
    {
      yield return new WaitForSeconds(0.0f);
    }
    complete();
  }
}
