using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
  public Animation animation;

  public Image modalBlocker;

  private MainMenuController MainMenu;

  public void Start()
  {
    animation = GetComponent<Animation>();
    modalBlocker = GameObject.Find("ModalBlocker").GetComponent<Image>();
    MainMenu = FindObjectOfType<MainMenuController>();
  }

  public void StartNewGame()
  {
    if (SaveManager.save == null)
    {
      ReallyStartNewGame();
    }
    else
    {
      StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowDialog", false, () => { }));
      modalBlocker.raycastTarget = true;
    }
  }

  public void ReallyStartNewGame()
  {
    SaveManager.Create();
    MainMenu.ShowLevelSelect();
    FindObjectOfType<SimpleLevelSelector>().Start();
    //SceneManager.LoadScene("Title");
  }

  public void ConfirmDialog()
  {
    CancelDialog();
    ReallyStartNewGame();
  }

  public void CancelDialog()
  {
    StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowDialog", true, () => { }));
    modalBlocker.raycastTarget = false;
  }
}
