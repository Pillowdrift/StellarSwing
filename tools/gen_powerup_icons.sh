#!/bin/zsh
# Draws the powerup shop icons (amber gradient, dark outline) into the given folder.
# Usage: tools/gen_powerup_icons.sh Assets/Textures/UI/Powerups
set -e
out=${1:?output folder}
mkdir -p $out
tmp=$(mktemp -d)
C='-define png:color-type=6'

icon() {
  local name=$1 shapes=$2 details=$3
  magick -size 512x512 xc:none -fill white -draw "$shapes" ${=C} $tmp/mask.png
  magick -size 512x512 gradient:'#f6c070'-'#b5542a' ${=C} $tmp/grad.png
  magick $tmp/grad.png $tmp/mask.png -alpha off -compose CopyOpacity -composite ${=C} $tmp/fill.png
  magick -size 512x512 xc:none -fill none -stroke '#3a1c0c' -strokewidth 16 -draw "$shapes" ${=C} $tmp/line.png
  magick $tmp/fill.png $tmp/line.png -compose Over -composite ${=C} $tmp/icon.png
  if [[ -n $details ]]; then
    magick $tmp/icon.png -draw "$details" ${=C} $tmp/icon.png
  fi
  # Trim to the artwork and centre it on a square canvas like the hat renders
  magick $tmp/icon.png -trim +repage -resize 440x440 -background none -gravity center -extent 512x512 +repage ${=C} $out/$name.png
}

icon Jump \
  "path 'M 256 52 L 404 214 L 310 214 L 310 330 L 202 330 L 202 214 L 108 214 Z' roundrectangle 86,384 426,440 18,18" \
  ""
icon Jetpack \
  "roundrectangle 150,56 362,286 40,40 path 'M 256 300 C 330 360 352 400 322 450 C 306 476 282 470 278 448 C 270 470 242 470 234 448 C 230 470 206 476 190 450 C 160 400 182 360 256 300 Z'" \
  "fill #3a1c0c roundrectangle 206,110 306,134 12,12 roundrectangle 206,160 306,184 12,12"
icon Boost \
  "path 'M 64 100 L 208 100 L 364 256 L 208 412 L 64 412 L 220 256 Z' path 'M 244 100 L 332 100 L 488 256 L 332 412 L 244 412 L 400 256 Z'" \
  ""
icon BulletTime \
  "roundrectangle 216,30 296,86 14,14 circle 256,288 256,98" \
  "fill #2a1408 circle 256,288 256,146 fill #e0973c path 'M 256 288 L 256 160 A 128 128 0 0 1 366 352 Z' fill #f6c070 circle 256,288 256,270"
rm -rf $tmp
