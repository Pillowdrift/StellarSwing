// The "finish world N" achievements. The World N To World N+1 cutscenes and the credits grant
// them on load, but the cutscenes only play when Next is pressed straight after first unlocking
// a world (restarting the level, going back to the menu, quitting or Steam being down then
// missed them for good), and the scene unlockers had been switched off anyway. So they're
// also granted from the save, whenever a level is finished and when the title loads (which
// catches up saves that already missed them).
public static class WorldAchievements
{
	// Worlds with a "finishworldN" achievement in the Steamworks app
	const int LastWorld = 5;
	const int LastLevel = 5;

	public static void CatchUp()
	{
		var save = SaveManager.save;
		if (save == null || !SteamManager.Initialized)
			return;

		// worldUnlocked is the furthest world reached, so every world before it is finished. The
		// last world never unlocks another, so it's finished once its last level has a score.
		int finished = System.Math.Min(save.worldUnlocked - 1, LastWorld - 1);
		if (save.levelHighscores.ContainsKey((LastWorld, LastLevel)))
			finished = LastWorld;
		for (int world = 1; world <= finished; world++)
			AchievementUnlocker.MakeUnlocker("finishworld" + world).UnlockAchievement();
	}
}
