using UnityEngine;
using UnityEngine.UI;

public class CreditsButton : ExtraMenuButton
{
  void Start()
  {
    bool show = SaveManager.save != null && SaveManager.save.worldUnlocked >= 5 && SaveManager.save.levelUnlocked >= 6;
    gameObject.active = show;
    if (show)
      ResizeMenu();
  }
}
