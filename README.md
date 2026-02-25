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
