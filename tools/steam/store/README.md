# Steam store page

Text for the store page (Steamworks > Edit Store Page), and a list of the other things on
it that are out of date. The short description has a 300 character limit.

- `short_description.txt`: the Short Description field (also shown at the top of the page)
- `about_this_game.bbcode`: the About This Game field, in Steam's BBCode

## Other things to update

- **Screenshots and trailer**: the current ones are from 2020. Fresh 1920x1080 shots are
  in `screenshots/` at the root of the repo (the non-thumb ones).
- **System requirements**: Mac says macOS 10 and Linux says Ubuntu 14.04, which Unity 6
  doesn't support. Unity 6 players need macOS 11 (Big Sur) or later and Ubuntu 22.04 or
  later. Windows 10 and DirectX 11 are still right.
- **Remote Play Together**: listed in the features, but the game is single player, so
  it should probably come off.
- **Steam Cloud**: listed, but only set up for Windows. Either add Mac and Linux save
  paths in Steamworks > Steam Cloud, or expect Mac/Linux saves not to sync.
- **Website**: empty; could be https://pillowdrift.com/
- **Release date**: still "Coming soon".
- **Price**: the store currently reports it as free. Check that's what you want before
  release.
