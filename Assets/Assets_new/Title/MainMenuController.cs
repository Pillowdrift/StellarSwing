using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
  private enum MainMenuState
  {
    Between,
    Title,
    Menu,
    Options
  }

  private MainMenuState menuState = MainMenuState.Title;

  private Animation animation;

  private Image modalBlocker;

  protected void Awake()
  {
    animation = GetComponent<Animation>();
    modalBlocker = GameObject.Find("ModalBlocker").GetComponent<Image>();
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

  public void Exit()
  {
    Application.Quit();
  }

  public static IEnumerator PlayAnimation(Animation animation, string name, bool reverse, Action complete)
  {
    Debug.Log($"Playing animation {name} " + (reverse ? "forwards" : "Backwards"));
    animation[name].speed = reverse ? -1.0f : 1.0f;
    animation[name].time = reverse ? animation[name].length : 0.0f;
    animation.Play(name);
    while (animation.isPlaying)
    {
      yield return new WaitForSeconds(0.0f);
    }
    complete();
  }
}
