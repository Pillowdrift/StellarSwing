using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryButton : MonoBehaviour
{
  private Button button;

  void Start()
  {
    button = GetComponent<Button>();
  }

  void Update()
  {
    if (SaveManager.save == null || (SaveManager.save.worldUnlocked == 1 && SaveManager.save.levelUnlocked == 1))
    {
      button.interactable = false;
    }
    else
    {
      button.interactable = true;
    }
  }
}
