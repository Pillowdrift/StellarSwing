# tools

Scripts that generate assets checked into the project. Run from the repo root.

- `gen_obo.py <out.obj>` — builds the 3D Obo mascot mesh used by the splash
  screen (Assets/Models/OboMascot/obo_mascot.obj). Pure Python, no
  dependencies. The body profile is traced from the 2012 drawing
  (Assets/Assets_new/Title/logo_splash_1024.png); eyes, nose, paws and the
  inverted-hull ink outline are parametric. Materials come from the sibling
  .mtl (OboBody, OboEyes, OboDark, OboInk); OboInk is switched to Unlit/Color
  black in the editor.

  ```
  python3 tools/gen_obo.py Assets/Models/OboMascot/obo_mascot.obj
  ```
