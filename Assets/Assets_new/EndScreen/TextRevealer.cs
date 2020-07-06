using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextRevealer : MonoBehaviour
{
  public string Text;
  public float Rate = 10;
  public bool Run = false;

  private Text _text;
  // The counter is how many characters of text from the original string to show
  public float Counter = 0;

  public bool TextFromStory = false;

  private void Start()
  {
    _text = GetComponent<Text>();

    if (TextFromStory)
    {
      int world = MainMenuController.worldSelected;
      int level = MainMenuController.levelSelected;
      Debug.Log("Getting story for world " + world + " level " + level);

      if (Story.Parts.TryGetValue((world, level), out string story))
      {
        Debug.Log("Got story: " + story);
        Text = story;
      }
      else
      {
        Debug.LogError("Couldn't get story!!");
      }
    }
  }

  void Update()
  {
    if (!Run)
      return;

    Counter += Rate * Time.deltaTime;

    int charsToShow = Mathf.Min((int)Counter, Text.Length);
    if (charsToShow != _text.text.Length)
    {
      _text.text = Text.Substring(0, charsToShow);
    }
  }

  public void Reset()
  {
    _text.text = "";
    Counter = 0;
  }
}
