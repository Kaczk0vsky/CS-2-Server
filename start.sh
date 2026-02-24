#!/bin/bash
set -e

echo "Logging into Steam..."

if [ -z "$STEAMUSER" ] || [ -z "$STEAMPASSWORD" ]; then
  echo "Steam credentials not set!"
  exit 1
fi

/home/steam/steamcmd/steamcmd.sh +login "$STEAMUSER" "$STEAMPASSWORD" \
    +force_install_dir /home/steam/cs2-dedicated \
    +app_update 730 validate +quit

echo "Starting CS2 server..."

cd /home/steam/cs2-dedicated/game/bin/linuxsteamrt64

./cs2 -dedicated \
  -usercon \
  -port 27015 \
  +sv_setsteamaccount ${SRCDS_TOKEN} \
  +map de_inferno \
  +maxplayers 32