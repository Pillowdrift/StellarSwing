using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleLevelSelector : MonoBehaviour
{
  public GameObject LevelTemplate;

  private List<GameObject> created = new List<GameObject>();

  public void Start()
  {
    if (SaveManager.save == null)
      return;

    TutorialCamera.ResetShownTutorials();

    foreach (var tile in created)
    {
      Destroy(tile);
    }
    created.Clear();

    var worlds = FindObjectsOfType<Levels>().ToList();
    foreach (var world in worlds.OrderBy(w => w.levels[0].world))
    {
      foreach (var level in world.levels)
      {
        if (level.world > SaveManager.save.worldUnlocked)
          continue;
        if (level.world == SaveManager.save.worldUnlocked && level.number > SaveManager.save.levelUnlocked)
          continue;

        Debug.Log("Loading level " + level.name);

        var newTile = GameObject.Instantiate(LevelTemplate, transform);
        newTile.SetActive(true);

        string displayName = (level.name == $"World {level.world} Level {level.number}" ? $"{level.world}-{level.number}" : level.name);

        newTile.name = level.name;
        newTile.GetComponentInChildren<Text>().text = displayName;

        newTile.GetComponent<Button>().onClick.AddListener(() => { LevelSelectGUI.currentLevel = level; SceneManager.LoadScene(level.name); });

        created.Add(newTile);
      }
    }
  }
}
