#!/usr/bin/env python3
"""Generate the gunmetal UI skin (replacements for the SpaceGUI sprites).

Usage: python3 tools/gen_ui.py <out_dir>
Writes PNGs with the same names, sizes and sheet layouts as Assets/SpaceGUI/img
so the existing scenes/prefabs (and their 9-slice borders) keep working.
Requires ImageMagick 7 (`magick`).
"""
import os, subprocess, sys, tempfile

SRC = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'SpaceGUI', 'img')
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)
TMP = tempfile.mkdtemp()

# palette (from the Stellar Swing logo: grey stone + rust streaks)
GUN_LIGHT = '#70757d'
GUN_MID   = '#454a52'
GUN_DARK  = '#202328'
GUN_DEEP  = '#141619'
RUST      = '#b5542a'
RUST_DARK = '#6e2f16'
AMBER     = '#e0973c'
STEEL_TINT= '#5b6675'

def im(*args):
    args = [str(a) for a in args]
    # every intermediate is written as a true-colour RGBA PNG: ImageMagick re-detects
    # grey-looking layers as greyscale on read, and that type then leaks into composites
    if args[-1].endswith('.png'):
        args = args[:-1] + ['-define', 'png:color-type=6', args[-1]]
    subprocess.run(['magick', *args], check=True)

def layer_from_mask(w, h, color, alpha_mask, name):
    """Solid `color` with alpha taken from `alpha_mask` (white = opaque)."""
    out = os.path.join(TMP, f'{name}.png')
    im('-size', f'{w}x{h}', f'xc:{color}', alpha_mask, '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', '-colorspace', 'sRGB', '-type', 'TrueColorAlpha', out)
    return out

def brushed(w, h, top=GUN_LIGHT, bottom=GUN_DARK, seed=7, strength='28%'):
    """Vertical gradient with a horizontal brushed-metal grain."""
    base = os.path.join(TMP, f'base_{w}x{h}_{seed}.png')
    im('-size', f'{w}x{h}', f'gradient:{top}-{bottom}', base)
    grain = os.path.join(TMP, f'grain_{w}x{h}_{seed}.png')
    lo = int(strength[:-1]); hi = 100 - lo
    im('-size', f'{w}x{h}', 'xc:gray50', '-seed', seed, '+noise', 'Gaussian', '-set', 'colorspace', 'sRGB', '-type', 'TrueColor',
       '-motion-blur', '0x18+0', '-auto-level', '-level', f'{lo}%,{hi}%', '-colorspace', 'gray', '-set', 'colorspace', 'sRGB', '-type', 'TrueColor', grain)
    out = os.path.join(TMP, f'brushed_{w}x{h}_{seed}.png')
    im(base, grain, '-compose', 'Overlay', '-composite', out)
    return out

def rust_streaks(w, h, seed=3, amount=0.55):
    """Transparent layer of rust-coloured streaks, denser toward the bottom."""
    mask = os.path.join(TMP, f'rustmask_{w}x{h}_{seed}.png')
    im('-size', f'{w}x{h}', '-seed', seed, 'plasma:fractal', '-colorspace', 'gray', '-set', 'colorspace', 'sRGB', '-type', 'TrueColor',
       '-blur', '0x1', '-motion-blur', '0x6+90', '-level', '55%,85%', mask)
    grad = os.path.join(TMP, f'rustgrad_{w}x{h}.png')
    im('-size', f'{w}x{h}', 'gradient:black-white', grad)
    im(mask, grad, '-compose', 'Multiply', '-composite', '-evaluate', 'Multiply', amount, mask)
    return layer_from_mask(w, h, RUST, mask, f'rust_{w}x{h}_{seed}')

_plate_n = [0]
def shaded_plate(w, h, mask, top=GUN_LIGHT, bottom=GUN_DARK, bevel=3, rust=0.5, edge=GUN_DEEP, seed=7):
    """Brushed plate cut to `mask` (white = plate) with a bevel, rust streaks and a dark edge line."""
    _plate_n[0] += 1; n = _plate_n[0]
    body = brushed(w, h, top, bottom, seed)
    bev = os.path.join(TMP, f'bevel_{n}.png')
    im(mask, '-blur', f'0x{bevel}', '-shade', '125x35', '-set', 'colorspace', 'sRGB', '-type', 'TrueColor', '-evaluate', 'Subtract', '7%', '-sigmoidal-contrast', '4x50%', bev)
    streaks = rust_streaks(w, h, seed + 1, rust)
    ring = os.path.join(TMP, 'edge_ring.png')
    im(mask, '-morphology', 'EdgeIn', 'Diamond:2', ring)
    edge_layer = layer_from_mask(w, h, edge, ring, 'edge_layer')
    stage = os.path.join(TMP, f'stage_{n}.png')
    im(body, bev, '-compose', 'Overlay', '-composite', stage)
    im(stage, streaks, '-compose', 'Over', '-composite', stage)
    im(stage, edge_layer, '-compose', 'Over', '-composite', stage)
    out = os.path.join(TMP, f'plate_{n}.png')
    im(stage, mask, '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', out)
    return out

def poly_mask(w, h, points, name='mask'):
    out = os.path.join(TMP, f'{name}.png')
    pts = ' '.join(f'{x},{y}' for x, y in points)
    im('-size', f'{w}x{h}', 'xc:black', '-fill', 'white', '-draw', f'polygon {pts}', out)
    return out

def rrect_mask(w, h, x0, y0, x1, y1, r, name='mask'):
    out = os.path.join(TMP, f'{name}.png')
    im('-size', f'{w}x{h}', 'xc:black', '-fill', 'white', '-draw', f'roundrectangle {x0},{y0} {x1},{y1} {r},{r}', out)
    return out

def circle_mask(w, h, cx, cy, r, name='mask'):
    out = os.path.join(TMP, f'{name}.png')
    im('-size', f'{w}x{h}', 'xc:black', '-fill', 'white', '-draw', f'circle {cx},{cy} {cx + r},{cy}', out)
    return out

def accent_line(w, h, y0, y1, color=RUST, x0=0, x1=None):
    """A thin horizontal accent bar as a transparent layer."""
    x1 = w if x1 is None else x1
    out = os.path.join(TMP, 'accent.png')
    im('-size', f'{w}x{h}', 'xc:none', '-fill', color, '-draw', f'rectangle {x0},{y0} {x1},{y1}', '-colorspace', 'sRGB', '-type', 'TrueColorAlpha', out)
    return out

def save(src, name):
    im(src, '-define', 'png:color-type=6', os.path.join(OUT, name))

# ---------------------------------------------------------------- window_header 404x83 (9-slice 27/29, bottom 6)
w, h = 404, 83
mask = poly_mask(w, h, [(16, 2), (388, 2), (402, 41), (388, 80), (16, 80), (2, 41)], 'hdr')
plate = shaded_plate(w, h, mask, GUN_LIGHT, GUN_DARK, bevel=3, rust=0.45)
acc = accent_line(w, h, 74, 77, RUST, 30, 374)
im(plate, acc, '-compose', 'Over', '-composite', os.path.join(TMP, 'hdr_stage.png'))
im(os.path.join(TMP, 'hdr_stage.png'), mask, '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', os.path.join(TMP, 'hdr_out.png'))
save(os.path.join(TMP, 'hdr_out.png'), 'window_header.png')

# ---------------------------------------------------------------- uie_btn / uie_btn2 410x90 (whole-sprite buttons)
for name, top, bottom, rust in (('uie_btn.png', '#4d525a', '#1b1e22', 0.35), ('uie_btn2.png', '#656a72', '#2a2e34', 0.35)):
    w, h = 410, 90
    mask = poly_mask(w, h, [(8, 2), (402, 2), (408, 10), (408, 80), (402, 88), (8, 88), (2, 80), (2, 10)], 'btn')
    plate = shaded_plate(w, h, mask, top, bottom, bevel=2, rust=rust)
    acc = accent_line(w, h, 82, 86, RUST, 10, 400)
    im(plate, acc, '-compose', 'Over', '-composite', os.path.join(TMP, 'btn_stage.png'))
    im(os.path.join(TMP, 'btn_stage.png'), mask, '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', os.path.join(TMP, 'btn_out.png'))
    save(os.path.join(TMP, 'btn_out.png'), name)

# ---------------------------------------------------------------- cbtn_big sheet 1660x350: blue, green, red, (mask), grey
W, H = 1660, 350
sheet = os.path.join(TMP, 'cbtn.png')
im('-size', f'{W}x{H}', 'xc:none', '-type', 'TrueColorAlpha', sheet)
variants = [(0, '#4e6275', '#232b34'), (332, '#8a6a2a', '#3a2c10'), (664, RUST, RUST_DARK), (1328, '#3b3f45', '#1a1c20')]
for i, (x, inner_top, inner_bottom) in enumerate(variants):
    cw, ch = 332, 350
    shadow = os.path.join(TMP, 'shadow.png')
    im('-size', f'{cw}x{ch}', 'xc:none', '-fill', 'rgba(0,0,0,0.55)', '-draw', 'circle 166,190 266,190', '-blur', '0x14', '-type', 'TrueColorAlpha', shadow)
    ring_mask = circle_mask(cw, ch, 166, 175, 100, 'ring')
    ring = shaded_plate(cw, ch, ring_mask, GUN_LIGHT, GUN_DARK, bevel=4, rust=0.3, seed=11 + i)
    inner_mask = circle_mask(cw, ch, 166, 175, 84, 'inner')
    inner = shaded_plate(cw, ch, inner_mask, inner_top, inner_bottom, bevel=5, rust=0.25, edge=GUN_DEEP, seed=21 + i)
    gloss = os.path.join(TMP, 'gloss.png')
    im('-size', f'{cw}x{ch}', 'xc:none', '-fill', 'rgba(255,255,255,0.18)', '-draw', 'ellipse 166,135 60,34 0,360', '-blur', '0x6', '-colorspace', 'sRGB', '-type', 'TrueColorAlpha', gloss)
    one = os.path.join(TMP, f'cbtn_{i}.png')
    im(shadow, ring, '-compose', 'Over', '-composite', inner, '-composite', gloss, '-composite', '-type', 'TrueColorAlpha', one)
    im(sheet, one, '-geometry', f'+{x}+0', '-compose', 'Over', '-composite', '-type', 'TrueColorAlpha', sheet)
# keep the original mask slot (white circle used for masking icons)
im(os.path.join(SRC, 'cbtn_big.png'), '-crop', '196x196+1064+77', '+repage', os.path.join(TMP, 'cbtn_mask.png'))
im(sheet, os.path.join(TMP, 'cbtn_mask.png'), '-geometry', '+1064+77', '-compose', 'Over', '-composite', '-type', 'TrueColorAlpha', sheet)
save(sheet, 'cbtn_big.png')

# ---------------------------------------------------------------- uie_lvl_bg / uie_lvl_bg_locked 188x188
for name, inner_top, inner_bottom, frame_top, frame_bottom in (('uie_lvl_bg.png', '#2a2e34', '#121416', '#5c6169', '#22252a'),
                                                               ('uie_lvl_bg_locked.png', '#1c1e22', '#0e0f11', GUN_MID, GUN_DEEP)):
    w = h = 188
    outer = rrect_mask(w, h, 3, 3, 184, 184, 10, 'lvl_outer')
    frame = shaded_plate(w, h, outer, frame_top, frame_bottom, bevel=3, rust=0.35, seed=31)
    inner_mask = rrect_mask(w, h, 14, 14, 173, 173, 6, 'lvl_inner')
    inner = shaded_plate(w, h, inner_mask, inner_top, inner_bottom, bevel=2, rust=0.15, seed=32)
    corner = os.path.join(TMP, 'corner.png')
    im('-size', f'{w}x{h}', 'xc:none', '-fill', RUST, '-draw', 'rectangle 14,170 60,173', '-draw', 'rectangle 127,170 173,173', '-colorspace', 'sRGB', '-type', 'TrueColorAlpha', corner)
    im(frame, inner, '-compose', 'Over', '-composite', corner, '-composite', os.path.join(TMP, 'lvl_out.png'))
    save(os.path.join(TMP, 'lvl_out.png'), name)

# ---------------------------------------------------------------- window 1200x725 (9-slice 8/3/8, top 216): tinted dark in-scene
w, h = 1200, 725
body = os.path.join(TMP, 'win_body.png')
im('-size', f'{w}x{h}', 'xc:rgba(18,16,20,0.45)', '-type', 'TrueColorAlpha', body)   # dark translucent interior, like the original
top_mask = poly_mask(w, h, [(0, 0), (w, 0), (w, 34), (0, 34)], 'win_top')
top = shaded_plate(w, h, top_mask, GUN_LIGHT, GUN_DARK, bevel=2, rust=0.3, seed=41)
lines = os.path.join(TMP, 'win_lines.png')
im('-size', f'{w}x{h}', 'xc:none', '-type', 'TrueColorAlpha', '-fill', GUN_LIGHT, '-draw', f'rectangle 0,34 2,{h}', '-draw', f'rectangle {w-3},34 {w},{h}',
   '-draw', f'rectangle 0,{h-3} {w},{h}', '-fill', RUST, '-draw', f'rectangle 0,{h-1} {w},{h}', lines)
im(body, top, '-compose', 'Over', '-composite', lines, '-composite', os.path.join(TMP, 'win_out.png'))
save(os.path.join(TMP, 'win_out.png'), 'window.png')

# ---------------------------------------------------------------- uie_quest_title_bg 78x50: two parallelogram ends, rust
w, h = 78, 50
im('-size', f'{w}x{h}', 'xc:none', '-fill', RUST, '-draw', 'polygon 0,0 22,0 39,50 17,50', '-fill', RUST_DARK, '-draw', 'polygon 39,0 61,0 78,50 56,50',
   '(', '-size', f'{w}x{h}', f'gradient:rgba(255,255,255,0.25)-rgba(0,0,0,0.35)', ')', '-compose', 'Overlay', '-composite', os.path.join(TMP, 'quest_out.png'))
save(os.path.join(TMP, 'quest_out.png'), 'uie_quest_title_bg.png')

# ---------------------------------------------------------------- screen_border 1920x1186: gunmetal frame with an amber inner line
w, h = 1920, 1186
im('-size', f'{w}x{h}', 'xc:none', '-fill', GUN_DEEP, '-draw', f'rectangle 0,0 {w},{h}', '-fill', 'none',
   '-stroke', GUN_MID, '-strokewidth', '6', '-draw', f'rectangle 3,3 {w-4},{h-4}',
   '-stroke', AMBER, '-strokewidth', '1', '-draw', f'rectangle 8,8 {w-9},{h-9}', os.path.join(TMP, 'border_stage.png'))
im('-size', f'{w}x{h}', 'xc:white', '-fill', 'black', '-draw', f'rectangle 9,9 {w-10},{h-10}', os.path.join(TMP, 'border_mask.png'))
im(os.path.join(TMP, 'border_stage.png'), os.path.join(TMP, 'border_mask.png'), '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', os.path.join(TMP, 'border_out.png'))
save(os.path.join(TMP, 'border_out.png'), 'screen_border.png')

# ---------------------------------------------------------------- screen_light_bottom 1920x396: warm glow instead of blue
w, h = 1920, 396
im('-size', f'{w}x{h}', 'gradient:#000000-#4a2614', '(', '-size', f'{w}x{h}', '-seed', 5, 'plasma:fractal', '-colorspace', 'gray', '-blur', '0x12', '-level', '40%,100%', ')',
   '-compose', 'SoftLight', '-composite', os.path.join(TMP, 'light_out.png'))
save(os.path.join(TMP, 'light_out.png'), 'screen_light_bottom.png')

# ---------------------------------------------------------------- steelify: desaturate blues to gunmetal, keep warm accents
def steelify(name, tint=STEEL_TINT, keep_warm=True):
    src = os.path.join(SRC, name)
    grey = os.path.join(TMP, 'st_grey.png')
    # grey-tinted version keeping the source alpha
    im(src, '-alpha', 'extract', os.path.join(TMP, 'st_alpha.png'))
    im(src, '-alpha', 'off', '-colorspace', 'gray', '-fill', tint, '-colorize', '35', os.path.join(TMP, 'st_tint.png'))
    im(os.path.join(TMP, 'st_tint.png'), os.path.join(TMP, 'st_alpha.png'), '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', grey)
    if keep_warm:
        warm = os.path.join(TMP, 'st_warm.png')
        # mask where red clearly exceeds blue (the orange accents)
        im(src, '-alpha', 'off', '-channel', 'R', '-separate', os.path.join(TMP, 'st_r.png'))
        im(src, '-alpha', 'off', '-channel', 'B', '-separate', os.path.join(TMP, 'st_b.png'))
        im(os.path.join(TMP, 'st_r.png'), os.path.join(TMP, 'st_b.png'), '-compose', 'MinusSrc', '-composite', '-threshold', '18%', '-blur', '0x1', os.path.join(TMP, 'st_warm_a.png'))
        im(src, os.path.join(TMP, 'st_warm_a.png'), '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', warm)
        out = os.path.join(TMP, 'st_out.png')
        im(grey, warm, '-compose', 'Over', '-composite', out)
    else:
        out = grey
    save(out, name)

for n in ('uie_resources_bg.png', 'uie_shop_item_btn.png', 'uie_shop_item_bg.png', 'uie_dropdown_background.png', 'uie_seekbar_background.png',
          'uie_seekbar_point.png', 'uie_chkbox_background.png', 'uie_rd_background.png', 'window_scroll_bg.png', 'uie_inventory_btn.png',
          'uie_inventory_talents_settings.png', 'uie_quest_icon_bg.png', 'uie_shop_item_pic_overlay.png', 'uie_lvl_bg_locked.png'):
    if n == 'uie_lvl_bg_locked.png':
        continue
    if os.path.exists(os.path.join(SRC, n)):
        steelify(n)

print('written to', OUT)
