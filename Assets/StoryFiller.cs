using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryFiller : MonoBehaviour
{
  private GameObject storyTemplate;

  void Start()
  {
    storyTemplate = GetComponentInChildren<Text>().gameObject;

    int worldUnlocked = 1;
    int levelUnlocked = 1;
    bool gotToEnd = true;

    if (SaveManager.save != null)
    {
      levelUnlocked = SaveManager.save.levelUnlocked;
      worldUnlocked = SaveManager.save.worldUnlocked;

      // Set this to true, our loop later will set it back to false
      gotToEnd = true;
    }

    int currentWorld = 0;

    foreach (var part in Story.Parts)
    {
      if (part.Value == "")
        continue;

      var world = part.Key.Item1;
      var level = part.Key.Item2;

      if (world > worldUnlocked || (world == worldUnlocked && level >= levelUnlocked))
      {
        gotToEnd = false;
        continue;
      }

      // Add world heading
      if (currentWorld != world)
      {
        currentWorld = world;
        var story = GameObject.Instantiate(storyTemplate, transform);
        var text = story.GetComponent<Text>();
        text.text = "World " + currentWorld + "\n";
      }

      // Add actual story
      {
        var story = GameObject.Instantiate(storyTemplate, transform);
        var text = story.GetComponent<Text>();
        text.text = part.Value + "\n";
      }
    }

    if (!gotToEnd)
    {
      var story = GameObject.Instantiate(storyTemplate, transform);
      var text = story.GetComponent<Text>();
      text.text = "(to be continued)";
    }
  }

  void Update()
  {
  }
}
