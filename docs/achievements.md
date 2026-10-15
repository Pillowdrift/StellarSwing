# Achievements

What each achievement actually checks, from the code, for testing. They only unlock when
Steam is running. Achievements that check play time, drones lost, crashes or purchases
read them from the save, so test those with a fresh save.

| Achievement | API name | What unlocks it | Where |
|---|---|---|---|
| Just Beginning | `finishworld1` | Reaching the World 1 to World 2 cutscene | `World 1 To World 2` scene |
| Explorer | `finishworld2` | Reaching the World 2 to World 3 cutscene | `World 2 To World 3` scene |
| Deep Space | `finishworld3` | Reaching the World 3 to World 4 cutscene | `World 3 To World 4` scene |
| Far From Home | `finishworld4` | Reaching the World 4 to World 5 cutscene | `World 4 To World 5` scene |
| The End | `finishworld5` | Reaching the credits | `Credits` scene |
| What's Back Here? | `droneshirkduty` | Flying into the hidden trigger behind the Stellar Flame in level 1-1 | `AchievementCubeTrigger` in World 1 Level 1 |
| Speedy | `speedy` | Reaching the credits with 20 minutes or less of total play time | `CreditsUnlocker` |
| Speedrunner | `speedrunner` | Reaching the credits in under an hour of total play time (also gives the crown) | `SpeedrunCrown` |
| Spendthrift | `allunlocks` | Owning every shop item (all hats and powerups; the crown doesn't count). Checked on each purchase and when opening Unlocks | `Unlockable`, `MainMenuController` |
| Disposable | `dronecrash` | 30 crashes in total, across all levels | `EndDeathScript` |
| Frugal | `deathless` | Reaching the credits with no drones lost on the save | `CreditsUnlocker` |
| Hangin' Out | `hanginout` | Staying grappled for 30 seconds in a row (paused time doesn't count) | `GrapplingHook` |
| Decked Out | `deckedout` | Equipping a second hat | `Unlockable` |
| Wait—I Was Using That! | `doublegrapple` | Grappling the same object twice in a row | `GrapplingHook` |
| Top Speed | `topspeed` | Going faster than 300 while grappled | `PlayerMovements` |
| No Thanks | `world1_6` | Finishing level **1-10** without flying through either hole in the glass barriers | `AchievementEndScript1_6` in World 1 Level 10 |
| Rockless | `world2_3` | Finishing level 2-3 without touching a rock | `AchievementEndScript2_3` |
| Dashless | `world3_2` | Finishing level 3-2 without using a dash pad | `AchievementEndScript3_2` |
| Rockless 2 | `world4_3` | Finishing level 4-3 without touching a rock | `AchievementEndScript4_3` |
| Grateless | `world5_3` | Finishing level 5-3 without touching a rotating grate | `AchievementEndScript5_3` |

## Things to check while testing

- **No Thanks**: the Steam description says level 1-6, but the barriers and checks are in
  level 1-10. Either the description should say 1-10, or the level got moved at some point.
- **Top Speed**: the fastest the old save ever recorded was about 130, so 300 might not be
  reachable any more (the physics changed in the Unity 6 upgrade). If you can't get it,
  the threshold is in `Assets/Scripts/PlayerMovements.cs`.
- **Speedy, Speedrunner and Frugal** need a whole run on one save, and Frugal means never
  losing a drone, so they're the slowest to test. Play time and drones lost are stored in
  the save file, so they can be tested by editing the save (`playTime` is in seconds).
