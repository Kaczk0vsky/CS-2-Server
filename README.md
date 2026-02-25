# Guide
Basic overview how to setup everything and how to work with the cs 2 server itself.

## First setup:
1. Clone the repo - it is required to have 60+ GB on disc.
2. Download Docker Desktop: https://www.docker.com/
3. Create Token: https://steamcommunity.com/dev/managegameservers
4. Copy .env.template as .env and fill with your information.
5. Open Powershell console in the folder with server and write: `docker compose up` and wait for the download to complete.
6. Copy the `addons` folder to: `cs2-data/game/csgo/`.
7. Open another Powershell console and type:
`./ARRCON.exe -H localhost -P 28015 -p changeme -i`.
8. Type: `docker attach cs2-server` and write `meta` to check if Metamod works. If it does not work go manually to `/home/steam/cs2-dedicated/game/csgo/gameinfo.gi` in your cs2-server container and add `Game    csgo/addons/metamod` directly above `Game    csgo`. Then close and save the file and restart the containers.
9. Open: `https://steamid.io/lookup` and paste your profile url into the lookup window. Then copy the steamID64.
10. Attach to cs2-server container and type:
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