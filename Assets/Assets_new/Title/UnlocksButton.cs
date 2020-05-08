using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnlocksButton : MonoBehaviour
{
  private Button btn;

  public void Start()
  {
    btn = GetComponent<Button>();
    btn.interactable = SaveManager.save != null;
  }
}
