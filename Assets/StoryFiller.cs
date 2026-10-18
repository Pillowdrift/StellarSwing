using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Fills the Story Log with the parts of the story the player has seen: the prelude,
// each intermission once its world is beaten, and the epilogue once the game is.
// Uses the same scripts as the StoryNarrator scenes so the two never drift apart.
public class StoryFiller : MonoBehaviour
{
  public TextAsset Prelude;
  // Intermissions[i] plays between world i+1 and world i+2
  public TextAsset[] Intermissions;
  public TextAsset Epilogue;

  private GameObject storyTemplate;

  void Start()
  {
    storyTemplate = GetComponentInChildren<Text>().gameObject;

    var save = SaveManager.save;
    int worldUnlocked = save != null ? save.worldUnlocked : 1;
    // Beating the last level doesn't unlock anything, but it does leave a high score
    // (only stars are reliable: best times never get recorded)
    bool beatGame = save != null && save.GetHighScore(5, 5).Stars > 0;

    AddPart("Prelude", Prelude);

    for (int i = 0; i < Intermissions.Length; i++)
    {
      // Plays on beating the last level of world i+1, which unlocks world i+2
      if (worldUnlocked < i + 2)
        break;
      AddPart($"World {i + 1} to World {i + 2}", Intermissions[i]);
    }

    if (beatGame)
      AddPart("Epilogue", Epilogue);
    else
      AddText("(to be continued)");

    storyTemplate.SetActive(false);
  }

  void AddPart(string heading, TextAsset script)
  {
    if (script == null)
      return;

    var text = AddText(heading + "\n");
    text.fontStyle = FontStyle.Bold;
    text.fontSize = 40;

    // Each page of the narrator script becomes a paragraph
    foreach (var page in Pages(script.text))
      AddText(page + "\n");
  }

  Text AddText(string body)
  {
    var story = Instantiate(storyTemplate, transform);
    var text = story.GetComponent<Text>();
    text.text = body;
    return text;
  }

  // Same format as StoryNarrator: one beat per line, blank lines between pages
  static List<string> Pages(string script)
  {
    var pages = new List<string>();
    var beats = new List<string>();
    foreach (var raw in (script + "\n").Replace("\r", "").Split('\n'))
    {
      var line = raw.Trim();
      if (line.Length > 0)
      {
        beats.Add(line);
      }
      else if (beats.Count > 0)
      {
        pages.Add(string.Join(" ", beats));
        beats.Clear();
      }
    }
    return pages;
  }
}
