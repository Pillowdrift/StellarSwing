# Builds the Steam store and library images from renders of the title scene in src/
# (UI hidden, rendered straight from the camera at each wide asset's exact shape, plus a
# 16:9 master the tall assets are cropped from) and the title logo.
# Needs ImageMagick 7. Run: python3 tools/steam/store/art/compose.py
import subprocess, os
HERE = os.path.dirname(os.path.abspath(__file__))
S = HERE + '/src/'; O = HERE + '/out/'
os.makedirs(O, exist_ok=True)
def m(*a): subprocess.run(['magick', *a], check=True)

LOGO = S + 'logo.png'

def capsule(src, w, h, name, logo_w, logo_y, shade=0.55, fmt='jpg', crop=None):
    """Background (optionally cropped to x,y,w,h first) scaled to w x h, darkened toward the
    bottom, logo centred at logo_y (0-1 from top)."""
    tmp = O + '_tmp.png'
    pre = ['-crop', '%dx%d+%d+%d' % (crop[2], crop[3], crop[0], crop[1]), '+repage'] if crop else []
    m(S + src, *pre, '-resize', f'{w}x{h}^', '-gravity', 'center', '-extent', f'{w}x{h}',
      # darken a band behind the logo so it reads against the planet
      '(', '-size', f'{w}x{h}', f'gradient:none-#000000{int(shade*255):02x}', ')', '-composite', tmp)
    m(tmp, '(', LOGO, '-trim', '+repage', '-resize', f'{logo_w}x', ')', '-gravity', 'north',
      '-geometry', f'+0+{int(h*logo_y)}', '-composite', '-quality', '95', O + name + '.' + fmt)
    os.remove(tmp)

# store
capsule('wide_1840x860.png',   920,  430, 'header_capsule_920x430',   800, 0.64)
capsule('small_924x348.png',    462,  174, 'small_capsule_462x174',    440, 0.38, shade=0.65)
capsule('main_2464x1412.png',  1232,  706, 'main_capsule_1232x706',   1040, 0.70)
# tall ones: crop the master around the drone (it spans about x 1660-4300 of 6144)
capsule('master_6144x3456.png', 748,  896, 'vertical_capsule_748x896', 690, 0.80, crop=(1537, 0, 2886, 3456))
# library
capsule('master_6144x3456.png', 600,  900, 'library_capsule_600x900',  560, 0.82, crop=(1748, 0, 2304, 3456))
capsule('wide_1840x860.png',   920,  430, 'library_header_920x430',   800, 0.64)
m(S + 'hero_3840x1240.png', '-quality', '95', O + 'library_hero_3840x1240.jpg')
m(LOGO, '-trim', '+repage', '-resize', '1280x', O + 'library_logo_1280.png')
# optional store page background: dim and soft so it doesn't compete with the page
m(S + 'master_6144x3456.png', '-resize', '1438x810^', '-gravity', 'center', '-extent', '1438x810',
  '-blur', '0x6', '-modulate', '55,80', '-quality', '92', O + 'page_background_1438x810.jpg')
