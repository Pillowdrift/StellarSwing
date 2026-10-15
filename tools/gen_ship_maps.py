#!/usr/bin/env python3
"""Higher-fidelity maps for the drone and the mothership, derived from the original art.

Usage: python3 tools/gen_ship_maps.py
Writes to Assets/Textures/ShipHD. Requires ImageMagick 7. Sources are never modified.
  ship_albedo.png       4x Lanczos upscale of Textures/ship.png with a light unsharp mask
  ship_metalgloss.png   R = metallic, A = smoothness, derived from colour: grey hull is
                        polished metal, saturated paint and dark rubber are matte
  detail_albedo.png     tiling grey micro-variation (Standard 'detail x2', 0.5 = neutral)
  detail_height.png     tiling fine scratch/grain height, imported as a normal map
  flame_albedo.png      2x upscale of Textures/flame_difuse.png
  flame_emission.png    the orange/yellow lights and green orb only, for emission + bloom
"""
import os, subprocess
ROOT = os.path.join(os.path.dirname(__file__), '..')
OUT = os.path.join(ROOT, 'Assets', 'Textures', 'ShipHD')
os.makedirs(OUT, exist_ok=True)
def im(*a):
    a = [str(x) for x in a]
    if a[-1].endswith('.png'):
        a = a[:-1] + ['-define', 'png:color-type=6', a[-1]]
    subprocess.run(['magick', *a], check=True)
o = lambda n: os.path.join(OUT, n)
ship = os.path.join(ROOT, 'Assets', 'Textures', 'ship.png')
flame = os.path.join(ROOT, 'Assets', 'Textures', 'flame_difuse.png')

# --- drone
im(ship, '-alpha', 'off', '-filter', 'Lanczos', '-resize', '2048x2048', '-unsharp', '0x1.6+0.7+0.01', o('ship_albedo.png'))
# saturation and luminance (1024 is plenty for these smooth masks)
im(ship, '-alpha', 'off', '-resize', '1024x1024', '-colorspace', 'HSL', '-channel', 'G', '-separate', '+channel', o('_sat.png'))
im(ship, '-alpha', 'off', '-resize', '1024x1024', '-grayscale', 'Rec709Luma', o('_lum.png'))
# metallic: unsaturated and not near-black
im(o('_sat.png'), o('_lum.png'), '-alpha', 'off', '-fx', '0.55*(1-(min(max((u-0.12)/(0.35-0.12),0),1)*min(max((u-0.12)/(0.35-0.12),0),1)*(3-2*min(max((u-0.12)/(0.35-0.12),0),1))))*(min(max((v-0.10)/(0.30-0.10),0),1)*min(max((v-0.10)/(0.30-0.10),0),1)*(3-2*min(max((v-0.10)/(0.30-0.10),0),1)))', '-blur', '0x0.8', o('_metal.png'))
# smoothness: polished hull ~0.62, painted ~0.38, dark rubber ~0.18
im(o('_sat.png'), o('_lum.png'), '-alpha', 'off', '-fx', '(0.18 + 0.44*(1-(min(max((u-0.12)/(0.35-0.12),0),1)*min(max((u-0.12)/(0.35-0.12),0),1)*(3-2*min(max((u-0.12)/(0.35-0.12),0),1))))*(min(max((v-0.10)/(0.30-0.10),0),1)*min(max((v-0.10)/(0.30-0.10),0),1)*(3-2*min(max((v-0.10)/(0.30-0.10),0),1))) + 0.20*(min(max((u-0.12)/(0.35-0.12),0),1)*min(max((u-0.12)/(0.35-0.12),0),1)*(3-2*min(max((u-0.12)/(0.35-0.12),0),1))))', '-blur', '0x0.8', o('_gloss.png'))
im(o('_metal.png'), '-resize', '2048x2048', o('_metal2k.png')); im(o('_gloss.png'), '-resize', '2048x2048', o('_gloss2k.png'))
im(o('_metal2k.png'), o('_metal2k.png'), o('_metal2k.png'), o('_gloss2k.png'), '-channel', 'RGBA', '-combine', o('ship_metalgloss.png'))
# tiling detail: micro grain + fine scratches, wrapped so it tiles
im('-size', '512x512', 'xc:gray50', '-seed', '11', '+noise', 'Gaussian', '-colorspace', 'gray', '-virtual-pixel', 'tile', '-blur', '0x0.6', '-level', '30%,70%', o('_grain.png'))
im('-size', '512x512', 'xc:gray50', '-seed', '12', '+noise', 'Impulse', '-colorspace', 'gray', '-virtual-pixel', 'tile', '-motion-blur', '0x14+20', '-level', '35%,65%', o('_scratch.png'))
im(o('_grain.png'), o('_scratch.png'), '-compose', 'Overlay', '-composite', '-virtual-pixel', 'tile', '-blur', '0x0.4', o('detail_height.png'))
im(o('detail_height.png'), '-level', '-260%,360%', '-evaluate', 'Multiply', '1.0', o('detail_albedo.png'))
# --- mothership
im(flame, '-alpha', 'off', '-filter', 'Lanczos', '-resize', '2048x2048', '-unsharp', '0x1.4+0.6+0.01', o('flame_albedo.png'))
# emission: bright, saturated warm or green pixels (the light strips and the orb)
im(o('flame_albedo.png'), '-colorspace', 'HSL', '-channel', 'G', '-separate', '+channel', o('_fsat.png'))
im(o('flame_albedo.png'), '-colorspace', 'gray', o('_flum.png'))
im(o('_fsat.png'), o('_flum.png'), '-alpha', 'off', '-fx', '(min(max((u-0.45)/(0.7-0.45),0),1)*min(max((u-0.45)/(0.7-0.45),0),1)*(3-2*min(max((u-0.45)/(0.7-0.45),0),1)))*(min(max((v-0.45)/(0.7-0.45),0),1)*min(max((v-0.45)/(0.7-0.45),0),1)*(3-2*min(max((v-0.45)/(0.7-0.45),0),1)))', o('_fmask.png'))
im(o('flame_albedo.png'), o('_fmask.png'), '-compose', 'Multiply', '-composite', o('flame_emission.png'))
for f in ([] if os.environ.get('KEEP_TMP') else os.listdir(OUT)):
    if f.startswith('_'):
        os.remove(o(f))
print('written to', OUT, sorted(os.listdir(OUT)))
