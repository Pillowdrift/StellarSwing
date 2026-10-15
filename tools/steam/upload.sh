#!/bin/zsh
# Uploads Builds/Windows, Builds/Linux and Builds/Mac to Steam (app 1319990) with steamcmd.
# Build first (Stellar Swing > Build All in Unity), then:
#   tools/steam/upload.sh <steam account> "what changed"
# The first login asks for the password and a Steam Guard code; steamcmd remembers it after.
# The upload doesn't go live by itself: set it live on a branch in the Steamworks partner
# site (SteamPipe > Builds).
set -e
account=${1:?usage: upload.sh <steam account> [description]}
desc=${2:-"Build $(date +%Y-%m-%d) $(git -C ${0:A:h} rev-parse --short HEAD)"}
here=${0:A:h}
steamcmd=${STEAMCMD:-$HOME/steamcmd/steamcmd.sh}

for p in Windows Linux Mac; do
  [[ -d $here/../../Builds/$p ]] || { echo "Missing Builds/$p - build first"; exit 1; }
done

# Fill the description into a temporary copy next to the original (so relative paths still work)
vdf=$here/.app_build_upload.vdf
sed "s|__DESC__|${desc//|/-}|" $here/app_build_1319990.vdf > $vdf
trap "rm -f $vdf" EXIT
$steamcmd +login $account +run_app_build $vdf +quit
