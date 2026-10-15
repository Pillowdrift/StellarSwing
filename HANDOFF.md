# StellarSwing — handoff notes for Claude

Written 2026-10-15 from a chat in ~/projects/tmp, before the checkout finished.
Read this first when a session opens in this folder, then delete or fold it
into CLAUDE.md once it has served its purpose.

## Context

- Unity game, roughly 95% finished. Cat shelved it about five years ago (~2021)
  when the last stretch of polish got tedious. Goal: finish it and publish on Steam.
- Cat's assessment: very little scene/editor work remains, mostly code and
  data polish plus the Steam release pipeline.
- Cat has macOS accessibility permissions enabled, so screen-driving the editor
  is possible as a fallback for "eyes" (screenshots of the Game view). Prefer MCP
  for hands, screenshots for eyes, direct YAML edits for bulk asset changes.

## Agreed plan for the first session

1. Check the Unity version: `cat ProjectSettings/ProjectVersion.txt`.
   Given the age, expect 2019/2020 LTS. MCP for Unity needs 2021.3 LTS or newer,
   so an upgrade to 2021.3 LTS (or 2022.3 LTS) is probably step one. Commit before
   upgrading; let Unity reimport; fix any API deprecations.
2. Install MCP for Unity (CoplayDev/unity-mcp): Package Manager → add package
   from git URL (MCPForUnity subfolder of the repo). Needs Python 3.10+ and `uv`.
   Then Window → MCP for Unity → Auto-Setup, confirm "Connected" for Claude Code.
   Setup guide: https://glama.ai/mcp/servers/@CoplayDev/unity-mcp
3. Build the real remaining-work list. Look for existing TODO/notes files,
   `// TODO` comments, disabled scenes in Build Settings, and anything half-wired.
   Split into: code/data (Claude), editor-only (Cat), Steam pipeline (Claude).
4. Steam side, when ready: Steamworks.NET or Facepunch.Steamworks, SteamCMD
   build scripts and depot config, achievements, cloud saves if wanted,
   Steam Deck checklist, store page assets.

## Things Claude can do here

- Read/refactor C#; edit scenes, prefabs and ScriptableObjects as YAML.
- Batch-mode builds and test runs from the CLI.
- Via MCP: create/modify GameObjects and components, read the console,
  enter/exit play mode, run the test runner.

## Things Claude cannot do

- Judge game feel. Cat playtests; Claude fixes what Cat reports.
- Fine visual work in Scene view, animation curves, shader graphs.

## Options rejected / deferred

- Unity's official MCP (Unity AI beta): needs Unity 6 and a Unity AI
  subscription, and may already be deprecated in favour of a Unity CLI route.
  Not worth it unless the project ends up on Unity 6 anyway.
