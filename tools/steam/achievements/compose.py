# Composes the Steam achievement icons (256x256 JPGs, plus greyed-out _locked versions)
# from the 3D renders in renders/, which render.cs makes in the Unity editor.
# Needs ImageMagick 7. Run: python3 tools/steam/achievements/compose.py
import subprocess, os, tempfile
HERE = os.path.dirname(os.path.abspath(__file__))
C = HERE + '/renders/'; O = HERE + '/icons/'; T = tempfile.mkdtemp() + '/'
os.makedirs(O, exist_ok=True)
FONT = os.path.join(HERE, '../../../Assets/SpaceGUI/fonts/Roboto-Black.ttf')
AMBER = '#CE6730'
def m(*a): subprocess.run(['magick', *a], check=True)

def base(out):
    # deep space backdrop with a few stars, gunmetal frame and a thin amber line
    m('-size', '256x256', 'radial-gradient:#2b2440-#0b0a12', '-gravity', 'center', T+'bg.png')
    import random; random.seed(7); d = ['-size', '256x256', 'xc:none']
    for _ in range(26):
        x, y, r, a = random.uniform(14, 242), random.uniform(14, 242), random.choice([0.6, 0.8, 1.1]), random.choice(['70', 'a0', 'd0'])
        d += ['-fill', '#ffffff' + a, '-draw', f'circle {x:.1f},{y:.1f} {x+r:.1f},{y:.1f}']
    m(*d, T+'stars.png')
    m(T+'bg.png', T+'stars.png', '-composite', out)

def frame(img, out):
    m(img, '(', '-size', '256x256', 'xc:none', '-fill', 'none', '-stroke', '#5d6168', '-strokewidth', '10', '-draw', 'roundrectangle 5,5 250,250 22,22',
      '-stroke', AMBER, '-strokewidth', '2', '-draw', 'roundrectangle 11,11 244,244 17,17', ')', '-composite',
      '(', '-size', '256x256', 'xc:black', '-fill', 'white', '-draw', 'roundrectangle 0,0 255,255 26,26', ')', '-alpha', 'off', '-compose', 'CopyOpacity', '-composite', out)

def place(bg, cut, size, x, y, out, rot=0, extra=()):
    m(bg, '(', C+cut, '-trim', '+repage', '-background', 'none', '-rotate', str(rot), '-resize', f'{size}x{size}', *extra, ')', '-gravity', 'center', '-geometry', f'{x:+d}{y:+d}', '-composite', out)

def shadowed(bg, cut, size, x, y, out, rot=0, flop=False):
    # soft glow behind the subject so dark models read on the dark background
    m(bg, '(', C+cut, '-trim', '+repage', *(['-flop'] if flop else []), '-background', 'none', '-rotate', str(rot), '-resize', f'{size}x{size}',
      '(', '+clone', '-background', '#8a7cc8', '-shadow', '60x8+0+0', ')', '+swap', '-background', 'none', '-layers', 'merge', '+repage', ')',
      '-gravity', 'center', '-geometry', f'{x:+d}{y:+d}', '-composite', out)

def badge(img, text, out, pos='southeast', size=30):
    m(img, '(', '-size', '74x74', 'xc:none', '-fill', '#1a1720', '-stroke', AMBER, '-strokewidth', '4', '-draw', 'circle 37,37 37,5',
      '-fill', 'white', '-stroke', 'none', '-font', FONT, '-pointsize', str(size), '-gravity', 'center', '-annotate', '+0+1', text, ')',
      '-gravity', pos, '-geometry', '+18+18', '-composite', out)

def nosign(img, out):
    m(img, '(', '-size', '256x256', 'xc:none', '-fill', 'none', '-stroke', '#e0342ccc', '-strokewidth', '16', '-draw', 'circle 128,128 128,30',
      '-draw', 'line 59,59 197,197', ')', '-composite', out)

def beam(img, x1, y1, x2, y2, out, col='#ff8a2a'):
    m(img, '(', '-size', '256x256', 'xc:none', '-stroke', col, '-strokewidth', '9', '-draw', f'line {x1},{y1} {x2},{y2}', '-blur', '0x4', ')', '-composite',
      '(', '-size', '256x256', 'xc:none', '-stroke', '#ffe0b0', '-strokewidth', '3', '-draw', f'line {x1},{y1} {x2},{y2}', ')', '-composite', out)

def streaks(img, out, col='#ffffff', lines=((92, 18, 92, 4), (118, 8, 80, 5), (144, 22, 96, 4), (168, 30, 88, 3))):
    d = ['-size', '256x256', 'xc:none', '-stroke', col, '-draw', 'stroke-linecap round']
    for y, x1, x2, w in lines:
        d += ['-strokewidth', str(w), '-draw', f'line {x1},{y} {x2},{y}']
    m(img, '(', *d, '-blur', '0x0.8', '-channel', 'A', '-evaluate', 'multiply', '0.75', '+channel', ')', '-composite', out)

def finish(name, work):
    frame(work, T + 'f.png')
    m(T + 'f.png', '-background', '#0b0a10', '-alpha', 'remove', O + name + '.png')
    m(O + name + '.png', '-quality', '95', O + name + '.jpg')
    m(O + name + '.png', '-colorspace', 'Gray', '-modulate', '85', '-brightness-contrast', '-8x-25', '-colorspace', 'sRGB', '-quality', '95', O + name + '_locked.jpg')
    os.remove(O + name + '.png')

W = T + 'w.png'
def new(): base(W); return W

for n in range(1, 6):
    new(); place(W, f'planet{n}.png', 196, -8, -6, W); badge(W, str(n), W); finish(f'finishworld{n}', W)

new(); shadowed(W, 'ship_a.png', 210, 0, 6, W)
m(W, '-font', FONT, '-pointsize', '78', '-fill', AMBER, '-stroke', '#1a1720', '-strokewidth', '3', '-gravity', 'northeast', '-annotate', '+30+18', '?', W); finish('droneshirkduty', W)

new(); streaks(W, W); shadowed(W, 'drone_side.png', 190, 22, 0, W, flop=True); badge(W, '20', W, size=28); finish('speedy', W)

new(); shadowed(W, 'Crown.png', 190, 0, -4, W); badge(W, '60', W, size=28); finish('speedrunner', W)

new(); place(W, 'crystal.png', 150, -44, -18, W); shadowed(W, 'Sombrero.png', 120, 38, 30, W); shadowed(W, 'MagicianHat.png', 96, 52, -44, W); shadowed(W, 'VikingHelmet.png', 98, -24, 58, W); finish('allunlocks', W)

new(); m(W, '(', '-size', '256x256', 'radial-gradient:#e0342c90-#e0342c00', ')', '-composite', W); shadowed(W, 'drone.png', 196, 0, 6, W, rot=-28); badge(W, '30', W, size=28); finish('dronecrash', W)

new(); m(W, '(', '-size', '256x256', 'xc:none', '-fill', 'none', '-stroke', '#6fe08a', '-strokewidth', '6', '-draw', 'circle 128,128 128,40', '-blur', '0x3', ')', '-composite', W)
shadowed(W, 'drone.png', 176, 0, 4, W); badge(W, '0', W); finish('deathless', W)

new(); place(W, 'planet3.png', 440, 0, 268, W); beam(W, 128, 0, 128, 92, W); shadowed(W, 'drone.png', 132, 0, 0, W); finish('hanginout', W)

new(); shadowed(W, 'drone_hats.png', 214, 0, 4, W); finish('deckedout', W)

new(); place(W, 'rock.png', 170, 10, -46, W); beam(W, 60, 214, 120, 96, W); beam(W, 96, 226, 132, 100, W, col='#ffb04a'); shadowed(W, 'drone.png', 92, -48, 76, W); badge(W, '2', W, pos='northwest'); finish('doublegrapple', W)

new(); m(C+'barrier.png', '-trim', '+repage', '-morphology', 'Dilate', 'Disk:1.5', '-fill', '#ff6ab0', '-colorize', '45', '(', '+clone', '-background', '#ff3a90', '-shadow', '90x5+0+0', ')', '+swap', '-background', 'none', '-layers', 'merge', '+repage', T+'barrier_t.png'); m(W, '(', T+'barrier_t.png', '-resize', '210x210', ')', '-gravity', 'center', '-composite', W); nosign(W, W); finish('world1_6', W)

new(); streaks(W, W, col='#ffb45a', lines=((80, 14, 104, 6), (104, 10, 92, 7), (128, 16, 108, 6), (152, 12, 96, 5), (176, 24, 88, 4))); shadowed(W, 'drone_side.png', 180, 30, 0, W, rot=-10, flop=True); finish('topspeed', W)

new(); shadowed(W, 'rock.png', 200, 0, 0, W); nosign(W, W); finish('world2_3', W)
new(); shadowed(W, 'dasher.png', 196, 0, 0, W); nosign(W, W); finish('world3_2', W)
new(); shadowed(W, 'rock4.png', 200, 0, 0, W); nosign(W, W); finish('world4_3', W)
new(); shadowed(W, 'grate.png', 200, 0, 0, W); nosign(W, W); finish('world5_3', W)
