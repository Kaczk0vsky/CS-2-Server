# Guide
Basic overview how to setup everything and how to work with the cs 2 server itself.

## First setup:
1. Clone the repo - it is required to have 60+ GB on disc.
2. Download Docker Desktop: https://www.docker.com/
3. Create Token: https://steamcommunity.com/dev/managegameservers
4. Copy .env.template as .env and fill with your information.
5. Open Powershell console in the folder with server and write: `docker compose up` and wait for the download to complete.
6. Copy the `addons` folder to: `cs2-data/game/csgo/`.
7. Install Metamod & CounterStrikeSharp binaries (see [Troubleshooting](#troubleshooting) if `meta list` doesn't work).
8. Open another Powershell console and type:
`./ARRCON.exe -H localhost -P 28015 -p changeme -i`.
9. In ARRCON type `meta list` to check if Metamod works.
10. Open: `https://steamid.io/lookup` and paste your profile url into the lookup window. Then copy the steamID64.
11. In ARRCON type:
`css_addadmin <steamID64> <steam_username> @css/root 99`
It creates a super admin.

## Runing server and testing
1. `docker compose up` - run the server and database.
2. Open your local cs2 game, open console and type: `connect localhost`.
3. In game type `!admin` to check if everything works.

## Development
### New versions
1. Pull the newest version.
2. Copy the `addons` folder to: `cs2-data/game/csgo
3. Connect to the server!

### New plugins
1. Go to `addons/counterstrikesharp/plugins` and create new plugin by typing `dotnet new classlib -n <Your New Plugin>`.
2. After finishing development and to test changes go to you plugin folder in the console and type: `dotnet publish -c Release`.
3. Folder: `bin/Release/net8.0/publish/` will be created. Copy everything from `publish` to: `addons/counterstrikesharp/plugins/<Your New Plugin>/`.
4. Restart containers.
5. Attach to cs2-server container and type: `css_plugins list`. If you see your plugin then it is loaded. You can also manually load it: `css_plugins load <Your New Plugin>`.
6. Connect to server and check it out.

## Troubleshooting

### Metamod not loading (`meta list` returns "Unknown command")

The `addons` folder in this repo contains only config files. You need to download the actual binaries for Linux (Docker container runs Linux).

**Fix:**
```bash
# Download and install latest Metamod binaries
docker exec cs2-server bash -c "cd /tmp && wget -O metamod.tar.gz 'https://mms.alliedmods.net/mmsdrop/2.0/mmsource-2.0.0-git1387-linux.tar.gz' && tar -xzf metamod.tar.gz && cp -f addons/metamod/bin/linuxsteamrt64/* /home/steam/cs2-dedicated/game/csgo/addons/metamod/bin/linuxsteamrt64/"

# Download and install latest CounterStrikeSharp binaries
docker exec cs2-server bash -c "cd /tmp && wget -O css.zip 'https://github.com/roflmuffin/CounterStrikeSharp/releases/download/v1.0.362/counterstrikesharp-with-runtime-linux-1.0.362.zip' && unzip -o css.zip -d css_extract && cp -r css_extract/addons/counterstrikesharp/bin /home/steam/cs2-dedicated/game/csgo/addons/counterstrikesharp/"

# Restart server
docker compose restart cs2-server
```

**Also make sure `gameinfo.gi` has Metamod entry:**
```bash
docker exec cs2-server bash -c "grep 'metamod' /home/steam/cs2-dedicated/game/csgo/gameinfo.gi"
```
Should return: `Game	csgo/addons/metamod`

If not, add it manually or edit `cs2-data/game/csgo/gameinfo.gi` and add `Game csgo/addons/metamod` directly above `Game csgo` in SearchPaths section.

### Getting latest versions
- Metamod: https://www.sourcemm.net/downloads.php?branch=dev
- CounterStrikeSharp: https://github.com/roflmuffin/CounterStrikeSharp/releases