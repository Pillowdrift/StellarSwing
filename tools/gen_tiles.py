#!/usr/bin/env python3
"""Synthesise seamless stone and rust tiles from the Stellar Swing logo texture.

Usage: python3 tools/gen_tiles.py
Writes Assets/Textures/UI/stone_tile.png and rust_tile.png (256x256), used by gen_ui.py.
Random feathered patches are scattered with wrap-around so the tile repeats without visible seams.
"""
import os, random, subprocess, tempfile
ROOT = os.path.join(os.path.dirname(__file__), '..')
LOGO = os.path.join(ROOT, 'Assets', 'Assets_new', 'Title', 'ss_title_source.png')
if not os.path.exists(LOGO):
    LOGO = os.path.join(ROOT, 'Assets', 'Assets_new', 'Title', 'ss_title.png')
OUT = os.path.join(ROOT, 'Assets', 'Textures', 'UI')
os.makedirs(OUT, exist_ok=True)
TMP = tempfile.mkdtemp()
def im(*a):
    a = [str(x) for x in a]
    if a[-1].endswith('.png'):  # keep every intermediate RGBA so grey patches don't coerce the canvas to greyscale
        a = a[:-1] + ['-define', 'png:color-type=6', a[-1]]
    subprocess.run(['magick', *a], check=True)
def q(*a): return subprocess.run(['magick', *map(str, a)], check=True, capture_output=True, text=True).stdout.split()
W, H, BS, STEP, T = 1532, 234, 40, 12, 256
blocks = []
for y in range(28, H - BS, STEP):
    for x in range(40, W - BS, STEP):
        a, r, g, b = map(float, q(LOGO, '-crop', f'{BS}x{BS}+{x}+{y}', '+repage', '-format', '%[fx:minima.a] %[fx:mean.r] %[fx:mean.g] %[fx:mean.b]', 'info:'))
        if a < 0.98: continue
        sat = (max(r, g, b) - min(r, g, b)) / (max(r, g, b) + 1e-6)
        blocks.append((x, y, sat))
feather = os.path.join(TMP, 'feather.png')
im('-size', f'{BS}x{BS}', 'xc:black', '-fill', 'white', '-draw', f'roundrectangle 6,6 {BS-7},{BS-7} 10,10', '-blur', '0x4', feather)
def synth(name, pick, n=140, seed=1):
    rnd = random.Random(seed)
    src = [b for b in blocks if pick(b[2])]
    canvas = os.path.join(TMP, f'{name}.png')
    im('-size', f'{T}x{T}', 'xc:gray40', '-type', 'TrueColor', canvas)
    for i in range(n):
        x, y, _ = rnd.choice(src)
        patch = os.path.join(TMP, 'patch.png')
        im(LOGO, '-crop', f'{BS}x{BS}+{x}+{y}', '+repage', '-alpha', 'off', feather, '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', '-type', 'TrueColorAlpha', patch)
        px, py = rnd.randrange(T), rnd.randrange(T)
        for dx in (0, -T):
            for dy in (0, -T):
                im(canvas, patch, '-geometry', f'{px+dx:+d}{py+dy:+d}', '-compose', 'Over', '-composite', canvas)
    im(canvas, '-alpha', 'off', os.path.join(OUT, f'{name}_tile.png'))
    print(name, 'from', len(src), 'source blocks')
synth('stone', lambda s: s < 0.2)
synth('rust', lambda s: s > 0.42, seed=2)
