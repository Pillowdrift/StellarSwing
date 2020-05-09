using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
  private Button btn;

  private MainMenuController MainMenu;

  public void Start()
  {
    btn = GetComponent<Button>();
    btn.interactable = SaveManager.save != null;
    MainMenu = FindObjectOfType<MainMenuController>();
  }

  public void Click()
  {
    //SceneManager.LoadScene("Title");
    MainMenu.ShowLevelSelect();
  }
}
