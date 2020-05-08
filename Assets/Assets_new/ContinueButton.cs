using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
  private Button btn;

  public void Start()
  {
    btn = GetComponent<Button>();
    btn.interactable = SaveManager.save != null;
  }

  public void Click()
  {
    SceneManager.LoadScene("Title");
  }
}
