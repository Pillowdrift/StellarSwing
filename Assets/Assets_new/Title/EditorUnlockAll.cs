#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

// Developer shortcut: while playing in the Unity editor, the title screen shows an
// "Unlock all" button in the top-right corner that unlocks every world and level in the
// current save and reloads the title on the world select. The whole file is compiled out
// of player builds, and it creates itself at startup, so no scene needs to reference it.
public class EditorUnlockAll : MonoBehaviour
{
	const string TitleScene = "Title_new";
	const int LastWorld = 5;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Install()
	{
		var go = new GameObject("[Editor] Unlock All");
		go.hideFlags = HideFlags.DontSave;
		DontDestroyOnLoad(go);
		go.AddComponent<EditorUnlockAll>();
	}

	void OnGUI()
	{
		if (SceneManager.GetActiveScene().name != TitleScene)
			return;

		float scale = Screen.height / 1080.0f;
		var style = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(22 * scale) };
		var rect = new Rect(Screen.width - 240 * scale, 16 * scale, 220 * scale, 48 * scale);

		if (GUI.Button(rect, "DEV: Unlock all", style))
			UnlockAll();
	}

	static void UnlockAll()
	{
		if (SaveManager.save == null)
			SaveManager.Create();

		SaveManager.save.worldUnlocked = LastWorld;
		SaveManager.save.levelUnlocked = 99;
		SaveManager.save.Write();
		Debug.Log("[Editor] Unlocked all worlds and levels");

		MainMenuController.NextStateToLoad = MainMenuController.MainMenuState.WorldSelect;
		SceneManager.LoadScene(TitleScene);
	}
}
#endif
