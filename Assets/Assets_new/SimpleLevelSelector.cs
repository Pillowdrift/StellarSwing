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

        string lvl = $"{level.world}-{level.number}";
        Debug.Log("Loading level " + lvl);

        var newTile = GameObject.Instantiate(LevelTemplate, transform);
        newTile.SetActive(true);

        newTile.name = lvl;
        newTile.GetComponentInChildren<Text>().text = lvl;

        string sceneName = $"World {level.world} Level {level.number}";
        newTile.GetComponent<Button>().onClick.AddListener(() => { LevelSelectGUI.currentLevel = level; SceneManager.LoadScene(sceneName); });

        created.Add(newTile);
      }
    }
  }
}
