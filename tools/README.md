# tools

Scripts that generate assets checked into the project. Run from the repo root.

- `gen_obo.py <out.obj>` — builds the 3D Obo mascot mesh used by the splash
  screen (Assets/Models/OboMascot/obo_mascot.obj). Pure Python, no
  dependencies. The body profile is traced from the 2012 drawing
  (Assets/Assets_new/Title/logo_splash_1024.png); the paws are soft lobes in the
  revolved profile (Catmull-Rom through traced knots), eyes and nose are
  parametric. Materials are Assets/Models/OboMascot/Materials (OboBody,
  OboDark), remapped onto the importer.

  ```
  python3 tools/gen_obo.py Assets/Models/OboMascot/obo_mascot.obj
  ```
