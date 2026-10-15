#!/usr/bin/env python3
"""Higher-fidelity maps for the level rocks and platforms, derived from the original art.

Usage: python3 tools/gen_rock_maps.py
For each source texture writes Assets/Textures/RockHD/<name>_albedo.png (2x Lanczos upscale,
light unsharp) and <name>_height.png (fine-detail height, imported in Unity as a normal map).
The height keeps only small-scale rock detail (luminance minus a blurred copy), so the broad
painted light/dark bands don't turn into lumps, and the black space between UV islands is
flattened to mid-grey so island edges don't produce ridges. Requires ImageMagick 7.
"""
import os, subprocess
ROOT = os.path.join(os.path.dirname(__file__), '..')
SRC = os.path.join(ROOT, 'Assets', 'Models', 'ThingsThatJiteshMade')
OUT = os.path.join(ROOT, 'Assets', 'Textures', 'RockHD')
SOURCES = ['Platforms/Textures/platformstraight.png', 'Platforms/Textures/platform1.png', 'Platforms/Textures/platform2.png',
           'Platforms/Textures/platformslope.png', 'Platforms/Textures/plat22.png', 'Platforms/Textures/smoother 1.png',
           'Platforms/Textures/smooth32.png', 'Asteroid/asteroid.png', 'Asteroid/asteroid2.png', 'Asteroid/asteroid3.png',
           'quartercircleuv.png', 'quartercirclewrampuv.png', 'sidething.png']
os.makedirs(OUT, exist_ok=True)
def im(*a):
    a = [str(x) for x in a]
    if a[-1].endswith('.png'):
        a = a[:-1] + ['-define', 'png:color-type=6', a[-1]]
    subprocess.run(['magick', *a], check=True)
for rel in SOURCES:
    src = os.path.join(SRC, rel)
    name = os.path.splitext(os.path.basename(rel))[0].replace(' ', '_')
    o = lambda s: os.path.join(OUT, f'{name}_{s}.png')
    im(src, '-alpha', 'off', '-filter', 'Lanczos', '-resize', '200%', '-unsharp', '0x1.4+0.6+0.01', o('albedo'))
    im(o('albedo'), '-grayscale', 'Rec709Luma', o('lum'))
    im(o('lum'), '-threshold', '3%', '-morphology', 'Erode', 'Disk:3', o('mask'))
    # fine detail only: lum - blur(lum) around mid grey
    im(o('lum'), '(', '+clone', '-blur', '0x8', ')', '-compose', 'Mathematics', '-define', 'compose:args=0,-1,1,0.5', '-composite', o('hp'))
    im(o('hp'), '-level', '25%,75%', '-blur', '0x0.6', o('hp2'))
    # outside the islands: flat mid grey
    im('-size', subprocess.run(['magick', o('hp2'), '-format', '%wx%h', 'info:'], capture_output=True, text=True).stdout, 'xc:gray50', o('flat'))
    im(o('flat'), o('hp2'), o('mask'), '-composite', o('height'))
    for t in ('lum', 'mask', 'hp', 'hp2', 'flat'):
        os.remove(o(t))
    print('wrote', name)
