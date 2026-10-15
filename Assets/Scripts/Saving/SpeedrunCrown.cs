using UnityEngine;
using UnityEngine.SceneManagement;

// The Crown hat is the speedrun reward: reach the credits with under an hour on the save's
// play clock (the "Time Played" stat on the Stats screen) and it is unlocked, equipped and
// the Steam achievement is granted. A save that is already over an hour has to start a new game.
public static class SpeedrunCrown
{
	public const string HatName = "Crown";
	public const string AchievementId = "speedrunner";   // must exist in the Steamworks app's achievements
	public const float TimeLimitSeconds = 60.0f * 60.0f;
	const string CreditsScene = "Credits";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Install()
	{
		SceneManager.sceneLoaded += (scene, mode) =>
		{
			if (scene.name == CreditsScene)
				CheckAtCredits();
		};
	}

	public static void CheckAtCredits()
	{
		var save = SaveManager.save;
		if (save == null)
			return;

		if (save.playTime >= TimeLimitSeconds)
		{
			Debug.Log($"[SpeedrunCrown] {save.playTime / 60.0f:0.0} min played, over the hour; no crown");
			return;
		}

		Debug.Log($"[SpeedrunCrown] Credits reached in {save.playTime / 60.0f:0.0} min: crown earned");

		if (!save.unlocks.Contains(HatName))
		{
			save.unlocks.Add(HatName);
			if (!save.currentHats.Contains(HatName))
				save.currentHats.Add(HatName);
			save.Write();
		}

		AchievementUnlocker.MakeUnlocker(AchievementId).UnlockAchievement();
	}
}
