using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
  public Animation animation;

  public Image modalBlocker;

  public void Start()
  {
    animation = GetComponent<Animation>();
    modalBlocker = GameObject.Find("ModalBlocker").GetComponent<Image>();
  }

  public void StartNewGame()
  {
    if (SaveManager.save == null)
    {
      SaveManager.Create();
      SceneManager.LoadScene("Title");
    }
    else
    {
      StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowDialog", false, () => { }));
      modalBlocker.raycastTarget = true;
    }
  }

  public void ConfirmDialog()
  {
    SaveManager.Create();
    CancelDialog();
    SceneManager.LoadScene("Title");
  }

  public void CancelDialog()
  {
    StartCoroutine(MainMenuController.PlayAnimation(animation, "ShowDialog", true, () => { }));
    modalBlocker.raycastTarget = false;
  }
}
