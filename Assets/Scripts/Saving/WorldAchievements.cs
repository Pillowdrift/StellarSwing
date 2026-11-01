// The "finish world N" achievements. The World N To World N+1 cutscenes grant them on load, but
// those only play when Next is pressed straight after first unlocking a world: restarting the
// level, going back to the menu, quitting or Steam being down at that moment missed them for
// good. So they're also granted from the save, whenever a level is beaten and when the title
// loads (which catches up saves that already missed them).
public static class WorldAchievements
{
	// Worlds with a "finishworldN" achievement in the Steamworks app (5 is covered by the credits)
	const int LastWorldWithAchievement = 4;

	public static void CatchUp()
	{
		var save = SaveManager.save;
		if (save == null || !SteamManager.Initialized)
			return;

		// worldUnlocked is the furthest world reached, so every world before it is finished
		int finished = System.Math.Min(save.worldUnlocked - 1, LastWorldWithAchievement);
		for (int world = 1; world <= finished; world++)
			AchievementUnlocker.MakeUnlocker("finishworld" + world).UnlockAchievement();
	}
}
