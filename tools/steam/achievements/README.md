# Achievement icons

`icons/` has a 256x256 JPG for each achievement, named by its API name, plus a
`_locked` version for the Unachieved Icon slot in Steamworks (Stats & Achievements >
Achievements).

To change them, edit `compose.py` and run `python3 tools/steam/achievements/compose.py`
(needs ImageMagick 7). The 3D subjects in `renders/` come from the game itself; remake
them by running `render.cs` in the Unity editor with
`unity command eval_file -- --file tools/steam/achievements/render.cs`.

| API name | Icon |
|---|---|
| finishworld1-5 | That world's planet with its number |
| droneshirkduty | The back of the Stellar Flame with a question mark |
| speedy | Drone with speed lines, 20 minutes |
| speedrunner | The crown, 60 minutes |
| allunlocks | Picolinium crystal and a pile of hats |
| dronecrash | Drone tumbling into red, 30 crashes |
| deathless | Drone in a green ring, 0 drones lost |
| hanginout | Drone hanging from its grapple above a planet |
| deckedout | Drone wearing a mustache and a cowboy hat |
| doublegrapple | Two grapples on the same rock |
| topspeed | Drone with long amber speed streaks |
| world1_6 | Energy barrier, crossed out |
| world2_3, world4_3 | That level's rock, crossed out |
| world3_2 | Dash pad, crossed out |
| world5_3 | Rotating grate, crossed out |
