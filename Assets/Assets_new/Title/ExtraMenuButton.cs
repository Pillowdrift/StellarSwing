using UnityEngine;
using UnityEngine.UI;

public class ExtraMenuButton : MonoBehaviour
{
  public RectTransform MainMenu;
  public RectTransform Buttons;

  private Button btn;
  private RectTransform rectTransform;

  protected void ResizeMenu()
  {
    btn = GetComponent<Button>();
    rectTransform = GetComponent<RectTransform>();
    gameObject.active = SaveManager.save != null;

    MainMenu.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MainMenu.rect.height + rectTransform.rect.height + 30);
    var menuAnchor = MainMenu.anchoredPosition;
    menuAnchor.y -= rectTransform.rect.height / 2 + 20;
    MainMenu.anchoredPosition = menuAnchor;

    var buttonAnchor = Buttons.anchoredPosition;
    buttonAnchor.y += rectTransform.rect.height / 2 + 20;
    Buttons.anchoredPosition = buttonAnchor;
  }
}
