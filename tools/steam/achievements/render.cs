// Renders the 3D subjects for the achievement icons into tools/steam/achievements/renders/.
// Run in the Unity editor (edit mode) with: unity command eval_file -- --file tools/steam/achievements/render.cs
// then python3 tools/steam/achievements/compose.py. Opens several scenes, so save your work first.
var outDir = System.IO.Path.GetFullPath("tools/steam/achievements/renders") + "/";
var sb = new System.Text.StringBuilder();
// keep(renderer) decides what's drawn and framed
System.Func<GameObject, string, Vector3, float, System.Func<Renderer,bool>, string> render = (inst, name, dirIn, zoom, keep) => {
  var pr = new UnityEditor.PreviewRenderUtility();
  pr.camera.clearFlags = CameraClearFlags.SolidColor; pr.camera.backgroundColor = new Color(0,0,0,0);
  inst.transform.position = Vector3.zero;
  foreach (var c in inst.GetComponentsInChildren<MonoBehaviour>(true)) c.enabled = false;
  pr.AddSingleGO(inst);
  var b = new Bounds(); bool first = true;
  foreach (var r in inst.GetComponentsInChildren<Renderer>(true)) { bool k = r.gameObject.activeInHierarchy && keep(r) && !(r is ParticleSystemRenderer) && !(r is LineRenderer); r.enabled = k; if (!k) continue; if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
  var dir = dirIn.normalized; float rad = b.extents.magnitude;
  pr.camera.fieldOfView = 26f; pr.camera.aspect = 1f;
  pr.camera.transform.position = b.center + dir * rad / Mathf.Sin(13f * Mathf.Deg2Rad) * zoom; pr.camera.transform.LookAt(b.center);
  pr.camera.nearClipPlane = rad * 0.05f; pr.camera.farClipPlane = rad * 40f;
  pr.lights[0].intensity = 1.2f; pr.lights[0].color = new Color(1f,0.95f,0.88f); pr.lights[0].transform.rotation = Quaternion.LookRotation(-(dir + Vector3.up*0.8f + Vector3.right*0.5f).normalized);
  pr.lights[1].intensity = 0.6f; pr.lights[1].color = new Color(0.75f,0.8f,1f); pr.lights[1].transform.rotation = Quaternion.LookRotation((dir - Vector3.up*0.3f).normalized);
  pr.ambientColor = new Color(0.35f,0.35f,0.4f);
  pr.BeginPreview(new Rect(0,0,512,512), GUIStyle.none); pr.camera.Render();
  var tex = pr.EndPreview() as RenderTexture; var old = RenderTexture.active; RenderTexture.active = tex;
  var t2 = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false); t2.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0); t2.Apply(); RenderTexture.active = old;
  System.IO.File.WriteAllBytes(outDir + name + ".png", t2.EncodeToPNG()); pr.Cleanup();
  return name + " " + b.size + "\n";
};
var tq = new Vector3(-0.75f, 0.55f, 1f);
var player = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Default Scene/Player.prefab");
System.Func<string[], bool, GameObject> drone = (hats, shield) => {
  var g = (GameObject)UnityEngine.Object.Instantiate(player);
  foreach (Transform h in g.transform.Find("Hats")) { bool on = System.Array.IndexOf(hats, h.name) >= 0; h.gameObject.SetActive(on); if (on) foreach (var t in h.GetComponentsInChildren<Transform>(true)) t.gameObject.SetActive(true); }
  var sh = g.GetComponentsInChildren<Transform>(true); foreach (var t in sh) if (t.name == "Shield") t.gameObject.SetActive(shield);
  return g;
};
System.Func<Renderer,bool> body = r => r.transform.parent == null || r.transform.IsChildOf(r.transform.root.Find("Hats")) || r.name == "Shield";
sb.Append(render(drone(new string[0], false), "drone", tq, 0.8f, body));
sb.Append(render(drone(new string[0], false), "drone_side", new Vector3(-1f, 0.25f, 0.15f), 0.8f, body));
sb.Append(render(drone(new[]{"Mustache","CowboyHat"}, false), "drone_hats", tq, 0.8f, body));
System.Func<string, GameObject> P = p => (GameObject)UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(p));
for (int w = 1; w <= 5; w++) sb.Append(render(P("Assets/Prefabs/World"+w+"/Planet"+w+".prefab"), "planet"+w, new Vector3(-0.3f,0.2f,1f), 0.75f, x => true));
// first object called objName in a scene
System.Func<string, string, GameObject> find = (scene, objName) => {
  var sc = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
  foreach (var r in sc.GetRootGameObjects()) foreach (var t in r.GetComponentsInChildren<Transform>(true)) if (t.name == objName) return (GameObject)UnityEngine.Object.Instantiate(t.gameObject);
  return null;
};
sb.Append(render(find("Assets/Scenes/World 1/World 1 Level 1.unity", "StellarFlame"), "ship_a", new Vector3(0.8f,-0.5f,-0.6f), 0.75f, x => true));
sb.Append(render(find("Assets/Scenes/World 1/World 1 Level 6.unity", "EndObject"), "crystal", tq, 0.8f, x => true));
sb.Append(render(find("Assets/Scenes/World 2/World 2 Level 3.unity", "Platform2"), "rock", tq, 0.8f, x => true));
sb.Append(render(find("Assets/Scenes/World 3/World 3 Level 2.unity", "Dasher"), "dasher", new Vector3(-0.5f,1f,0.8f), 0.8f, x => true));
sb.Append(render(find("Assets/Scenes/World 5/World 5 Level 3.unity", "MetalPanel"), "grate", new Vector3(-0.4f,0.5f,1f), 0.8f, x => true));
var s10 = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/World 1/World 1 Level 10.unity");
var grp = new GameObject("Barrier");
// the glass blocks around the barrier holes that level 1-6 (World 1 Level 10.unity) checks
foreach (var r in s10.GetRootGameObjects()) if (r.name == "SeeThruDeathObject") { var c = (GameObject)UnityEngine.Object.Instantiate(r); c.transform.SetParent(grp.transform, true); }
var gc = Vector3.zero; int n = 0; foreach (Transform c in grp.transform) { gc += c.position; n++; } foreach (Transform c in grp.transform) c.position -= gc / n;
sb.Append(render(grp, "barrier", new Vector3(-0.35f, 0.15f, 1f), 0.8f, x => true));
var s43 = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/World 4/World 4 Level 3.unity");
GameObject rock4 = null; // the last rock checker in the scene
foreach (var r in s43.GetRootGameObjects()) foreach (var c in r.GetComponentsInChildren<AchievementRockChecker>(true)) rock4 = c.gameObject;
sb.Append(render((GameObject)UnityEngine.Object.Instantiate(rock4), "rock4", tq, 0.8f, x => true));
foreach (var h in new[]{"Crown","Sombrero","MagicianHat","VikingHelmet"}) System.IO.File.Copy("Assets/Textures/UI/Hats/" + h + ".png", outDir + h + ".png", true);
return sb.ToString();
