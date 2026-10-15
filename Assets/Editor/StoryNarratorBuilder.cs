using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds the story narrator overlay prefab and puts it in the world transition scenes.
public static class StoryNarratorBuilder
{
  const string Dir = "Assets/Assets_new/Story/";
  const string PrefabPath = Dir + "StoryNarrator.prefab";
  // See-through letterbox, so the scene still shows behind the text
  const float BarAlpha = 0.6f;

  static readonly (string scene, string script)[] Transitions =
  {
    ("Assets/Scenes/Prelude.unity", "Prelude"),
    ("Assets/Scenes/World 1 To World 2.unity", "World1To2"),
    ("Assets/Scenes/World 2 To World 3.unity", "World2To3"),
    ("Assets/Scenes/World 3 To World 4.unity", "World3To4"),
    ("Assets/Scenes/World 4 To World 5.unity", "World4To5"),
  };

  [MenuItem("Stellar Swing/Build Story Narrator")]
  public static void BuildAll()
  {
    BuildPrefab();
    foreach (var (scene, script) in Transitions)
      InstallInScene(scene, script);
    InstallInCredits();
  }

  // Credits: the epilogue plays through the narrator, then the credits roll on their own
  public static void InstallInCredits()
  {
    const string path = "Assets/Scenes/Credits.unity";
    InstallInScene(path, "Epilogue");
    var scene = EditorSceneManager.OpenScene(path);

    foreach (var go in scene.GetRootGameObjects())
    {
      var narrator = go.GetComponent<StoryNarrator>();
      if (narrator != null)
      {
        var so = new SerializedObject(narrator);
        so.FindProperty("ClickToContinue").boolValue = false;
        so.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(narrator);
      }

      var container = go.transform.Find("Credits container");
      if (container == null)
        continue;

      // The old 2 minute scroll animation moved the story and credits together
      Object.DestroyImmediate(container.GetComponent<CreditsStart>());
      Object.DestroyImmediate(container.GetComponent<Animation>());
      var crt = (RectTransform)container;
      crt.anchorMin = Vector2.zero;
      crt.anchorMax = Vector2.one;
      crt.offsetMin = crt.offsetMax = Vector2.zero;

      var story = container.Find("Story");
      if (story != null)
        Object.DestroyImmediate(story.gameObject);

      var credits = (RectTransform)container.Find("Credits");
      credits.anchorMin = credits.anchorMax = new Vector2(0.5f, 0);
      credits.pivot = new Vector2(0.5f, 1);
      credits.sizeDelta = new Vector2(800, credits.GetComponent<Text>().preferredHeight);
      credits.anchoredPosition = Vector2.zero;
      if (credits.GetComponent<CreditsRoll>() == null)
        credits.gameObject.AddComponent<CreditsRoll>();
    }

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene);
  }

  public static GameObject BuildPrefab()
  {
    var scrimPath = Dir + "story_scrim.png";
    var imp = (TextureImporter)AssetImporter.GetAtPath(scrimPath);
    if (imp.textureType != TextureImporterType.Sprite || imp.textureCompression != TextureImporterCompression.Uncompressed)
    {
      imp.textureType = TextureImporterType.Sprite;
      imp.alphaIsTransparency = true;
      imp.mipmapEnabled = false;
      imp.wrapMode = TextureWrapMode.Clamp;
      imp.textureCompression = TextureImporterCompression.Uncompressed;
      imp.SaveAndReimport();
    }
    var scrimSprite = AssetDatabase.LoadAssetAtPath<Sprite>(scrimPath);
    var regular = AssetDatabase.LoadAssetAtPath<Font>("Assets/SpaceGUI/fonts/Roboto-Regular.ttf");
    var bold = AssetDatabase.LoadAssetAtPath<Font>("Assets/SpaceGUI/fonts/Roboto-Bold.ttf");

    var root = new GameObject("StoryNarrator", typeof(RectTransform));
    var canvas = root.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 50;
    var scaler = root.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);
    scaler.matchWidthOrHeight = 1;

    // Cinematic letterbox bars; the text sits in the bottom one
    var bars = Child(root, "Letterbox");
    Stretch(bars, Vector2.zero, Vector2.one);
    var scrimGroup = bars.AddComponent<CanvasGroup>();
    scrimGroup.blocksRaycasts = false;
    var topBar = Bar(bars, "Top", true, 96, scrimSprite);
    var bottomBar = Bar(bars, "Bottom", false, 250, scrimSprite);

    var body = Child(root, "Body");
    var brt = (RectTransform)body.transform;
    brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0);
    brt.pivot = new Vector2(0.5f, 0);
    brt.sizeDelta = new Vector2(1500, 210);
    brt.anchoredPosition = new Vector2(0, 26);
    var text = body.AddComponent<Text>();
    text.font = regular;
    text.fontSize = 34;
    text.lineSpacing = 1.15f;
    text.alignment = TextAnchor.MiddleCenter;
    text.horizontalOverflow = HorizontalWrapMode.Wrap;
    text.verticalOverflow = VerticalWrapMode.Overflow;
    text.supportRichText = true;
    text.raycastTarget = false;
    var shadow = body.AddComponent<Shadow>();
    shadow.effectColor = new Color(0, 0, 0, 0.8f);
    shadow.effectDistance = new Vector2(2, -2);
    var bodyGroup = body.AddComponent<CanvasGroup>();
    bodyGroup.blocksRaycasts = false;

    var prompt = Child(root, "Prompt");
    var prt = (RectTransform)prompt.transform;
    prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(1, 0);
    prt.sizeDelta = new Vector2(700, 50);
    prt.anchoredPosition = new Vector2(-40, 14);
    prt.sizeDelta = new Vector2(700, 36);
    var ptext = prompt.AddComponent<Text>();
    ptext.font = bold;
    ptext.fontSize = 24;
    ptext.alignment = TextAnchor.LowerRight;
    ptext.raycastTarget = false;
    var pshadow = prompt.AddComponent<Shadow>();
    pshadow.effectColor = new Color(0, 0, 0, 0.8f);
    pshadow.effectDistance = new Vector2(2, -2);

    var narrator = root.AddComponent<StoryNarrator>();
    narrator.Body = text;
    narrator.BodyGroup = bodyGroup;
    narrator.ScrimGroup = scrimGroup;
    narrator.Prompt = ptext;
    narrator.TopBar = (RectTransform)topBar.transform;
    narrator.BottomBar = (RectTransform)bottomBar.transform;

    var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
    Object.DestroyImmediate(root);
    return prefab;
  }

  public static void InstallInScene(string scenePath, string script)
  {
    var scene = EditorSceneManager.OpenScene(scenePath);
    foreach (var go in scene.GetRootGameObjects())
    {
      // Old all-at-once story canvas, and any narrator from a previous run
      if (go.name == "Story" || go.name == "StoryNarrator")
        Object.DestroyImmediate(go);
    }

    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
    var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
    var narrator = inst.GetComponent<StoryNarrator>();
    var so = new SerializedObject(narrator);
    so.FindProperty("Script").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(Dir + "Text/" + script + ".txt");
    so.ApplyModifiedProperties();
    PrefabUtility.RecordPrefabInstancePropertyModifications(narrator);

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene);
  }

  // Solid black bar with a short soft fade on its inner edge
  static GameObject Bar(GameObject parent, string name, bool top, float height, Sprite fade)
  {
    var bar = Child(parent, name);
    var rt = (RectTransform)bar.transform;
    rt.anchorMin = new Vector2(0, top ? 1 : 0);
    rt.anchorMax = new Vector2(1, top ? 1 : 0);
    rt.pivot = new Vector2(0.5f, top ? 1 : 0);
    rt.sizeDelta = new Vector2(0, height);
    var img = bar.AddComponent<Image>();
    img.color = new Color(0, 0, 0, BarAlpha);
    img.raycastTarget = false;

    var edge = Child(bar, "Edge");
    var ert = (RectTransform)edge.transform;
    ert.anchorMin = new Vector2(0, top ? 0 : 1);
    ert.anchorMax = new Vector2(1, top ? 0 : 1);
    ert.pivot = new Vector2(0.5f, top ? 1 : 0);
    ert.sizeDelta = new Vector2(0, 48);
    ert.localScale = new Vector3(1, top ? -1 : 1, 1);
    ert.pivot = new Vector2(0.5f, 0);
    var eimg = edge.AddComponent<Image>();
    eimg.sprite = fade;
    eimg.color = new Color(1, 1, 1, BarAlpha);
    eimg.raycastTarget = false;
    return bar;
  }

  static GameObject Child(GameObject parent, string name)
  {
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent.transform, false);
    return go;
  }

  static void Stretch(GameObject go, Vector2 min, Vector2 max)
  {
    var rt = (RectTransform)go.transform;
    rt.anchorMin = min;
    rt.anchorMax = max;
    rt.offsetMin = rt.offsetMax = Vector2.zero;
  }
}
