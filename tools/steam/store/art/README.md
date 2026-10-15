# Store and library art

`out/` has every image Steam asks for, made from the current title screen and logo.
Upload them in Steamworks under Store Page Admin > Graphical Assets (store) and
Library Assets (library).

| File | Steamworks slot |
|---|---|
| `header_capsule_920x430.jpg` | Header Capsule |
| `small_capsule_462x174.jpg` | Small Capsule |
| `main_capsule_1232x706.jpg` | Main Capsule |
| `vertical_capsule_748x896.jpg` | Vertical Capsule |
| `page_background_1438x810.jpg` | Page Background (optional) |
| `library_capsule_600x900.jpg` | Library Capsule |
| `library_header_920x430.jpg` | Library Header |
| `library_hero_3840x1240.jpg` | Library Hero (no logo; Steam puts the logo on top) |
| `library_logo_1280.png` | Library Logo (transparent; "centered bottom" or "left bottom" both work) |

`src/` holds the title scene with the UI hidden, rendered straight from the title camera
(not the game view, which stretches to the requested size): one render at each wide
asset's exact shape and a 6144x3456 master the tall ones are cropped from, plus the title logo (`Assets/Assets_new/Title/ss_title.png`). To change the layout, edit
`compose.py` and run `python3 tools/steam/store/art/compose.py` (needs ImageMagick 7).
