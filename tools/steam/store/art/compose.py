# Builds the Steam store and library images from title-screen captures in src/
# (title scene with the UI hidden, rendered at each asset's shape) and the title logo.
# Needs ImageMagick 7. Run: python3 tools/steam/store/art/compose.py
import subprocess, os
HERE = os.path.dirname(os.path.abspath(__file__))
S = HERE + '/src/'; O = HERE + '/out/'
os.makedirs(O, exist_ok=True)
def m(*a): subprocess.run(['magick', *a], check=True)

LOGO = S + 'logo.png'

def capsule(src, w, h, name, logo_w, logo_y, shade=0.55, fmt='jpg'):
    """Background scaled to w x h, darkened toward the bottom, logo centred at logo_y (0-1 from top)."""
    tmp = O + '_tmp.png'
    m(S + src, '-resize', f'{w}x{h}^', '-gravity', 'center', '-extent', f'{w}x{h}',
      # darken a band behind the logo so it reads against the planet
      '(', '-size', f'{w}x{h}', f'gradient:none-#000000{int(shade*255):02x}', ')', '-composite', tmp)
    m(tmp, '(', LOGO, '-trim', '+repage', '-resize', f'{logo_w}x', ')', '-gravity', 'north',
      '-geometry', f'+0+{int(h*logo_y)}', '-composite', '-quality', '95', O + name + '.' + fmt)
    os.remove(tmp)

# store
capsule('sa_1840x860.png',   920,  430, 'header_capsule_920x430',   800, 0.64)
capsule('sa_924x348.png',    462,  174, 'small_capsule_462x174',    440, 0.38, shade=0.65)
capsule('sa_2464x1412.png', 1232,  706, 'main_capsule_1232x706',   1040, 0.70)
capsule('sa_1496x1792.png',  748,  896, 'vertical_capsule_748x896', 690, 0.80)
# library
capsule('sa_1200x1800.png',  600,  900, 'library_capsule_600x900',  560, 0.82)
capsule('sa_1840x860.png',   920,  430, 'library_header_920x430',   800, 0.64)
m(S + 'sa_3840x1240.png', '-quality', '95', O + 'library_hero_3840x1240.jpg')
m(LOGO, '-trim', '+repage', '-resize', '1280x', O + 'library_logo_1280.png')
# optional store page background: dim and soft so it doesn't compete with the page
m(S + 'sa_3840x2160.png', '-resize', '1438x810^', '-gravity', 'center', '-extent', '1438x810',
  '-blur', '0x6', '-modulate', '55,80', '-quality', '92', O + 'page_background_1438x810.jpg')
