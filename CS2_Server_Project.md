# CS2 Server Project 

> **Autorzy:** [Twoje imię i koledzy]  
> **Data rozpoczęcia:** [uzupełnić]  
> **Status:** Planowanie  
> **Wersja dokumentu:** 1.0

---

## Spis treści

1. [Podsumowanie projektu](#1-podsumowanie-projektu)
2. [CS2 vs CS 1.6 - Kluczowe różnice](#2-cs2-vs-cs-16---kluczowe-różnice)
3. [Fazy projektu](#3-fazy-projektu)
4. [Faza 0: Przygotowanie](#4-faza-0-przygotowanie-tydzień-1-2)
5. [Faza 1: Podstawowy serwer](#5-faza-1-podstawowy-serwer-tydzień-3-4)
6. [Faza 2: Pluginy i mody](#6-faza-2-pluginy-i-mody-tydzień-5-8)
7. [Faza 3: Systemy zaawansowane](#7-faza-3-systemy-zaawansowane-tydzień-9-12)
8. [Faza 4: Web i społeczność](#8-faza-4-web-i-społeczność-tydzień-13-16)
9. [Faza 5: Monetyzacja i skalowanie](#9-faza-5-monetyzacja-i-skalowanie-tydzień-17)
10. [Budżet i koszty](#10-budżet-i-koszty)
11. [Zasoby i linki](#11-zasoby-i-linki)
12. [Checklisty](#12-checklisty)
13. [Ryzyka i problemy](#13-ryzyka-i-problemy)

---

## 1. Podsumowanie projektu

### Cel
Stworzenie popularnego serwera/serwerów CS2 z niestandardowymi trybami gry (inspirowanymi COD/Pokemon), systemem rang, VIP, stroną WWW i aktywną społecznością.

### Zespół
| Rola | Osoba | Odpowiedzialności |
|------|-------|-------------------|
| Lead Developer | [imię] | Pluginy C#, architektura |
| Backend Developer | [imię] | Bazy danych, API, web panel |
| Server Admin | [imię] | Konfiguracja, utrzymanie, DevOps |
| Community Manager | [imię] | Discord, marketing, gracze |

### Wymagania techniczne
```
PROGRAMIŚCI MUSZĄ ZNAĆ:
├── C# (główny język pluginów CS2!)
├── .NET 8.0+
├── Podstawy SQL (MySQL/PostgreSQL)
├── Git (wersjonowanie)
├── Linux (serwery działają na Linux)
└── Opcjonalnie: TypeScript/React (web panel)
```

### Timeline
```
Faza 0 (2 tygodnie)    ████░░░░░░░░░░░░░░░░  Przygotowanie + nauka C#
Faza 1 (2 tygodnie)    ████████░░░░░░░░░░░░  Podstawowy serwer
Faza 2 (4 tygodnie)    ████████████░░░░░░░░  Pluginy i mody
Faza 3 (4 tygodnie)    ████████████████░░░░  Systemy zaawansowane
Faza 4 (4 tygodnie)    ████████████████████  Web i społeczność
Faza 5 (ongoing)       ████████████████████  Monetyzacja i skalowanie

Całkowity czas do MVP: ~12 tygodni
Całkowity czas do pełnej wersji: ~17 tygodni
```

---

## 2. CS2 vs CS 1.6 - Kluczowe różnice

### Porównanie technologii

| Aspekt | CS 1.6 | CS2 |
|--------|--------|-----|
| **Silnik** | GoldSrc | Source 2 |
| **Framework modów** | AMX Mod X | CounterStrikeSharp |
| **Język pluginów** | Pawn | **C# (.NET)** |
| **Metamod** | Metamod (stary) | Metamod:Source 2.x |
| **Hosting** | Tanie, niskie wymagania | Droższe, wyższe wymagania |
| **Społeczność modów** | Ogromna, dojrzała | Rozwijająca się |
| **COD/Pokemon mody** | Gotowe, do pobrania | **Trzeba napisać od zera!** |

### WAŻNE: Stan modów w CS2

```
⚠️ UWAGA: W CS2 NIE MA gotowych modów COD ani Pokemon!

CS 1.6:
├── COD Mod - gotowy do pobrania z GitHub
├── Pokemon Mod - dostępny na forach
└── Setki gotowych pluginów

CS2 (2025):
├── COD Mod - NIE ISTNIEJE (trzeba napisać samemu w C#)
├── Pokemon Mod - NIE ISTNIEJE (trzeba napisać samemu w C#)
├── Dostępne: rankingi, VIP, admin tools, practice mode
└── Większość zaawansowanych trybów trzeba tworzyć od zera
```

### Co to oznacza dla Was?

**Opcja A: Napisać własny mod COD/Pokemon od zera**
- Czas: 2-4 miesiące intensywnej pracy
- Wymaga: dobra znajomość C#
- Rezultat: unikalny produkt, brak konkurencji

**Opcja B: Zacząć od prostszych trybów**
- Deathmatch, Retake, Zombie, Surf
- Wiele gotowych pluginów
- Szybszy start

**Opcja C: Hybrid**
- Zacznij od gotowych pluginów (rangi, VIP)
- Stopniowo buduj własne systemy COD-like

**Rekomendacja:** Zacznijcie od Opcji C - nauczcie się ekosystemu CS2, a potem budujcie własny mod.

---

## 3. Fazy projektu

| Faza | Nazwa | Czas | Koszt startu | Rezultat |
|------|-------|------|--------------|----------|
| 0 | Przygotowanie | 2 tyg | 0 zł | Środowisko dev, nauka C# |
| 1 | Podstawowy serwer | 2 tyg | ~40-80 zł/mies | Działający vanilla serwer |
| 2 | Pluginy i mody | 4 tyg | 0 zł | Serwer z podstawowymi pluginami |
| 3 | Systemy zaawansowane | 4 tyg | ~20-50 zł/mies (DB) | Rangi, VIP, custom gameplay |
| 4 | Web i społeczność | 4 tyg | ~50-100 zł/mies | Strona, Discord, marketing |
| 5 | Monetyzacja | ongoing | 0 zł | System donacji/VIP |

---

## 4. Faza 0: Przygotowanie (Tydzień 1-2)

### 4.1 Nauka C# i .NET (KRYTYCZNE!)

**Dlaczego C#?**
CounterStrikeSharp używa C# jako języka skryptowego. Bez znajomości C# nie napiszecie żadnego pluginu.

**Plan nauki (dla programistów znających inny język):**

| Dzień | Temat | Zasoby |
|-------|-------|--------|
| 1-2 | Podstawy C# | Microsoft Learn, YouTube |
| 3-4 | OOP w C# (klasy, interfejsy) | docs.microsoft.com |
| 5-6 | Async/await, wydarzenia | docs.microsoft.com |
| 7 | .NET CLI, projekty | dotnet.microsoft.com |
| 8-10 | CounterStrikeSharp API | docs.cssharp.dev |

**Zasoby do nauki C#:**
```
DARMOWE:
- Microsoft Learn C#: https://learn.microsoft.com/pl-pl/dotnet/csharp/
- W3Schools C#: https://www.w3schools.com/cs/
- YouTube: "C# for beginners"

PŁATNE (opcjonalnie):
- Udemy: "Complete C# Masterclass"
- Pluralsight: C# Path
```

### 4.2 Instalacja narzędzi deweloperskich

**Wymagane oprogramowanie:**

```bash
# 1. .NET SDK 8.0+
# Windows: https://dotnet.microsoft.com/download
# Linux:
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# 2. IDE - wybierz jedno:
# - Visual Studio 2022 (Windows, pełne IDE) - REKOMENDOWANE
# - JetBrains Rider (cross-platform, płatne ale świetne)
# - VS Code + C# extension (lekkie, cross-platform)

# 3. Git
# Windows: https://git-scm.com/download/win
# Linux: sudo apt install git

# 4. Docker (opcjonalnie, do testów lokalnych)
# https://www.docker.com/products/docker-desktop

# 5. Narzędzia do baz danych
# - HeidiSQL (Windows, MySQL)
# - DBeaver (cross-platform)
# - pgAdmin (PostgreSQL)
```

**Struktura projektu:**
```
cs2-server-project/
├── server/                    # Pliki serwera CS2
│   └── game/csgo/
│       ├── addons/
│       │   ├── metamod/
│       │   └── counterstrikesharp/
│       │       └── plugins/   # Wasze pluginy (.dll)
│       └── cfg/
├── plugins/                   # Źródła waszych pluginów
│   ├── RankSystem/
│   │   ├── RankSystem.cs
│   │   └── RankSystem.csproj
│   ├── VIPSystem/
│   └── CodMod/               # Wasz custom mod
├── web/                       # Strona WWW
│   ├── frontend/
│   └── backend/
├── database/
│   └── migrations/
└── docs/
```

### 4.3 Założenie kont

- [ ] **GitHub** - https://github.com (kod, współpraca)
- [ ] **Discord** - dla zespołu i społeczności
- [ ] **Steam** - konto do testowania (najlepiej z CS2)
- [ ] **CounterStrikeSharp Discord** - https://discord.gg/cssharp (pomoc)

### 4.4 Decyzje do podjęcia

| Decyzja | Opcje | Rekomendacja | Wasza decyzja |
|---------|-------|--------------|---------------|
| **Główny tryb gry** | COD-like / Pokemon / Zombie / Retake | Zacznij od Retake + custom perki | [ ] |
| **Hosting** | Game hosting / VPS | VPS (więcej kontroli) | [ ] |
| **System operacyjny** | Linux / Windows | Linux (stabilniejszy, tańszy) | [ ] |
| **Baza danych** | MySQL / PostgreSQL | MySQL (więcej tutoriali) | [ ] |
| **Język serwera** | Polski / Angielski | Multi-language | [ ] |
| **Nazwa serwera** | [do ustalenia] | | [ ] |

---

## 5. Faza 1: Podstawowy serwer (Tydzień 3-4)

### 5.1 Wybór i zakup hostingu

#### Opcja A: Game Hosting (łatwiejsze)

| Dostawca | Cena/mies | RAM | Lokalizacja | Link |
|----------|-----------|-----|-------------|------|
| **Wake Servers** | ~20-40 zł | 4-12GB | EU/US | wakeservers.com |
| **GGServers** | ~25-50 zł | 4-8GB | Global | ggservers.com |
| **WinterNode** | ~30-50 zł | 4-8GB | EU | winternode.com |
| **DatHost** | ~40-80 zł | 4GB+ | EU | dathost.net |
| **ZAP-Hosting** | ~40-60 zł | 4GB | DE | zap-hosting.com |

**Zalety:** Panel zarządzania, łatwa instalacja modów, support
**Wady:** Mniej kontroli, droższe, ograniczenia

#### Opcja B: VPS (REKOMENDOWANE dla programistów)

| Dostawca | Cena/mies | RAM | vCPU | SSD | Link |
|----------|-----------|-----|------|-----|------|
| **Hetzner** | ~40-80 zł | 4-8GB | 2-4 | 40-80GB | hetzner.com |
| **Contabo** | ~35-70 zł | 4-8GB | 2-4 | 50-100GB | contabo.com |
| **OVH** | ~40-80 zł | 4-8GB | 2-4 | 40-80GB | ovhcloud.com |
| **Vultr** | ~50-100 zł | 4-8GB | 2-4 | 50-100GB | vultr.com |
| **DigitalOcean** | ~50-100 zł | 4-8GB | 2 | 50GB | digitalocean.com |
| **Linode** | ~50-100 zł | 4-8GB | 2 | 50GB | linode.com |

**Zalety:** Pełna kontrola, można hostować wiele serwerów + web + DB, tańsze długoterminowo
**Wady:** Wymaga znajomości Linux, więcej pracy

#### Minimalne wymagania dla CS2:
```
CPU: 2+ rdzenie (dedykowane, nie shared!)
RAM: 4GB minimum, 8GB rekomendowane
SSD: 40GB minimum (sam CS2 ~35GB)
Przepustowość: 100Mbps+
System: Linux (Ubuntu 22.04 LTS rekomendowany)
```

### 5.2 Instalacja serwera CS2 (VPS)

**Krok 1: Przygotowanie VPS (Ubuntu 22.04)**
```bash
# Aktualizacja systemu
sudo apt update && sudo apt upgrade -y

# Instalacja wymaganych pakietów
sudo apt install -y lib32gcc-s1 lib32stdc++6 curl wget tar unzip screen

# Utworzenie użytkownika dla serwera (bezpieczeństwo!)
sudo useradd -m -s /bin/bash cs2server
sudo passwd cs2server
sudo usermod -aG sudo cs2server

# Przełączenie na użytkownika
su - cs2server
```

**Krok 2: Instalacja SteamCMD**
```bash
# Utwórz katalogi
mkdir -p ~/steamcmd ~/cs2server

# Pobierz i rozpakuj SteamCMD
cd ~/steamcmd
wget https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz
tar -xvzf steamcmd_linux.tar.gz
```

**Krok 3: Pobierz serwer CS2**
```bash
# Uruchom SteamCMD i pobierz CS2
./steamcmd.sh +force_install_dir ~/cs2server +login anonymous +app_update 730 validate +quit

# To może zająć 20-40 minut przy pierwszym pobraniu!
```

**Krok 4: Konfiguracja serwera**
```bash
# Utwórz skrypt startowy
cat > ~/cs2server/start.sh << 'EOF'
#!/bin/bash
cd ~/cs2server
./game/bin/linuxsteamrt64/cs2 \
    -dedicated \
    -console \
    -usercon \
    +game_type 0 \
    +game_mode 0 \
    +mapgroup mg_active \
    +map de_dust2 \
    +sv_setsteamaccount YOUR_GSLT_TOKEN \
    -port 27015 \
    -maxplayers 20
EOF

chmod +x ~/cs2server/start.sh
```

**Krok 5: GSLT Token (WYMAGANE!)**
```
1. Idź na: https://steamcommunity.com/dev/managegameservers
2. Zaloguj się kontem Steam
3. App ID: 730
4. Wygeneruj token
5. Wklej token do start.sh w miejscu YOUR_GSLT_TOKEN
```

**Krok 6: Konfiguracja firewall**
```bash
# Otwórz porty
sudo ufw allow 27015/tcp
sudo ufw allow 27015/udp
sudo ufw allow 27020/udp
sudo ufw allow 27005/udp
sudo ufw enable
```

**Krok 7: Uruchomienie serwera**
```bash
# Uruchom w screen (działa w tle)
screen -S cs2
./start.sh

# Aby wyjść z screen: Ctrl+A, potem D
# Aby wrócić: screen -r cs2
```

### 5.3 Instalacja Metamod:Source

```bash
# 1. Pobierz Metamod:Source 2.x (Dev Build!)
cd ~/cs2server/game/csgo
wget https://mms.alliedmods.net/mmsdrop/2.0/mmsource-2.0.0-git1313-linux.tar.gz
tar -xvzf mmsource-2.0.0-git*.tar.gz

# 2. Edytuj gameinfo.gi
nano ~/cs2server/game/csgo/gameinfo.gi

# Znajdź sekcję:
# Game_LowViolence	csgo_lv
# DODAJ POD NIĄ:
# Game	csgo/addons/metamod
```

**Przykład gameinfo.gi (fragment):**
```
	FileSystem
	{
		SearchPaths
		{
			Game_LowViolence	csgo_lv
			Game	csgo/addons/metamod    // <- DODAJ TĘ LINIĘ
			Game	csgo
			Game	csgo_imported
```

**Weryfikacja:**
```bash
# Uruchom serwer i w konsoli wpisz:
meta list

# Powinno pokazać Metamod załadowany
```

### 5.4 Instalacja CounterStrikeSharp

```bash
# 1. Pobierz CounterStrikeSharp (z runtime!)
cd ~/cs2server/game/csgo
wget https://github.com/roflmuffin/CounterStrikeSharp/releases/latest/download/counterstrikesharp-with-runtime-build-XXX-linux.zip
unzip counterstrikesharp-with-runtime-*.zip

# 2. Struktura powinna wyglądać tak:
# csgo/
#   addons/
#     metamod/
#     counterstrikesharp/
#       api/
#       bin/
#       configs/
#       plugins/
```

**Weryfikacja:**
```bash
# W konsoli serwera:
css_plugins list

# Powinno pokazać CounterStrikeSharp załadowany
```

### 5.5 Podstawowa konfiguracja serwera

**server.cfg (game/csgo/cfg/server.cfg):**
```
// ========================================
// CS2 SERVER CONFIG
// ========================================

// === INFO ===
hostname "[PL] Nazwa Serwera | Custom Mod | !help"
sv_password ""
sv_cheats 0

// === NETWORK ===
sv_maxrate 0
sv_minrate 128000
sv_mincmdrate 128
sv_minupdaterate 128

// === GAMEPLAY ===
mp_autoteambalance 1
mp_friendlyfire 0
mp_freezetime 5
mp_roundtime 2.0
mp_roundtime_defuse 2.0
mp_buy_anywhere 0
mp_buytime 20
mp_maxmoney 16000
mp_startmoney 800
mp_free_armor 0

// === COMPETITIVE SETTINGS ===
mp_match_can_clinch 1
mp_overtime_enable 1

// === MISC ===
sv_alltalk 0
sv_deadtalk 1

// === LOGGING ===
log on
sv_logbans 1
sv_logfile 1

// Załaduj konfigurację pluginów
exec plugins.cfg
```

### 5.6 Konfiguracja adminów

**admins.json (game/csgo/addons/counterstrikesharp/configs/admins.json):**
```json
{
  "TwojeImie": {
    "identity": "STEAM_0:1:12345678",
    "flags": ["@css/root"]
  },
  "KolegaAdmin": {
    "identity": "STEAM_0:0:87654321",
    "flags": ["@css/admin", "@css/kick", "@css/ban"]
  }
}
```

**Dostępne flagi:**
```
@css/root        - Pełne uprawnienia
@css/admin       - Podstawowe uprawnienia admina
@css/kick        - Kickowanie graczy
@css/ban         - Banowanie graczy
@css/slay        - Zabijanie graczy
@css/changemap   - Zmiana map
@css/cvar        - Zmiana cvarów
@css/config      - Ładowanie configów
@css/chat        - Komendy czatu
@css/vote        - Głosowania
@css/reservation - Zarezerwowany slot
@css/vip         - Uprawnienia VIP
@css/generic     - Podstawowe uprawnienia
```

---

## 6. Faza 2: Pluginy i mody (Tydzień 5-8)

### 6.1 Gotowe pluginy do zainstalowania

**Lista rekomendowanych pluginów:**

| Plugin | Opis | Link |
|--------|------|------|
| **SharpTimer** | System rangowania, czasy | github.com/K4ryuu/SharpTimer |
| **K4-System** | Rangi, statystyki, czas gry | github.com/K4ryuu/K4-System |
| **CS2-Admin** | Panel administracyjny | github.com/daffyyyy/CS2-Admin |
| **MatchZy** | System meczy, Get5-like | github.com/shobhit-pathak/MatchZy |
| **GameModeManager** | Zarządzanie trybami gry | github.com/nickj609/GameModeManager |
| **CS2-Practice** | Tryb treningowy | github.com/B3none/cs2-retakes |
| **CS2-Retakes** | Tryb retake | github.com/B3none/cs2-retakes |
| **Zombie Mod** | Tryb zombie | zombiemod.org |

**Instalacja pluginu (przykład K4-System):**
```bash
# 1. Pobierz plugin
cd ~/cs2server/game/csgo/addons/counterstrikesharp/plugins
wget https://github.com/K4ryuu/K4-System/releases/latest/download/K4-System-vX.X.X.zip
unzip K4-System-*.zip

# 2. Plugin powinien być w:
# plugins/K4-System/K4-System.dll

# 3. Restart serwera
# 4. Skonfiguruj w: configs/plugins/K4-System/
```

### 6.2 Tworzenie własnego pluginu (Hello World)

**Struktura projektu:**
```
MyFirstPlugin/
├── MyFirstPlugin.cs
├── MyFirstPlugin.csproj
└── lang/
    └── en.json
```

**MyFirstPlugin.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CounterStrikeSharp.API" Version="*" />
  </ItemGroup>

</Project>
```

**MyFirstPlugin.cs:**
```csharp
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace MyFirstPlugin;

public class MyFirstPlugin : BasePlugin
{
    public override string ModuleName => "My First Plugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "TwojeImie";

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("Plugin załadowany!");
        
        // Rejestracja eventu
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);
    }

    // Komenda gracza: !hello
    [ConsoleCommand("css_hello", "Powitanie")]
    public void OnHelloCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        
        player.PrintToChat($" \x04[Server]\x01 Cześć, {player.PlayerName}!");
    }

    // Event: gracz dołącza
    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;
        
        Server.PrintToChatAll($" \x04[Server]\x01 {player.PlayerName} dołączył do gry!");
        return HookResult.Continue;
    }
}
```

**Kompilacja i deployment:**
```bash
# W katalogu pluginu:
dotnet build -c Release

# Skopiuj wynik:
cp bin/Release/net8.0/MyFirstPlugin.dll ~/cs2server/game/csgo/addons/counterstrikesharp/plugins/MyFirstPlugin/

# Restart serwera lub:
# css_plugins reload MyFirstPlugin
```

### 6.3 Budowanie COD-like systemu (krok po kroku)

Ponieważ nie ma gotowego COD Mod dla CS2, musimy go zbudować. Oto plan:

**Struktura COD Mod:**
```
CodMod/
├── CodMod.cs                 # Główny plik
├── CodMod.csproj
├── Models/
│   ├── Player.cs             # Dane gracza (XP, level, klasa)
│   ├── Class.cs              # Definicja klasy
│   └── Perk.cs               # Definicja perku
├── Services/
│   ├── DatabaseService.cs    # Zapis/odczyt z DB
│   ├── ClassService.cs       # Logika klas
│   └── XPService.cs          # System doświadczenia
├── Commands/
│   ├── ClassCommands.cs      # Komendy wyboru klasy
│   └── StatsCommands.cs      # Komendy statystyk
└── Config/
    └── config.json           # Konfiguracja
```

**Przykład: System XP (XPService.cs):**
```csharp
using CounterStrikeSharp.API.Core;

namespace CodMod.Services;

public class XPService
{
    private readonly Dictionary<ulong, int> _playerXP = new();
    
    // XP za różne akcje
    private const int XP_KILL = 100;
    private const int XP_HEADSHOT_BONUS = 50;
    private const int XP_ASSIST = 25;
    private const int XP_MVP = 200;
    private const int XP_BOMB_PLANT = 50;
    private const int XP_BOMB_DEFUSE = 100;
    
    // Poziomy
    private readonly int[] _levelThresholds = 
    {
        0, 500, 1200, 2000, 3000, 4500, 6500, 9000, 12000, 15500,
        20000, 25000, 31000, 38000, 46000, 55000, 65000, 76000, 88000, 100000
    };
    
    public int GetLevel(int xp)
    {
        for (int i = _levelThresholds.Length - 1; i >= 0; i--)
        {
            if (xp >= _levelThresholds[i])
                return i + 1;
        }
        return 1;
    }
    
    public void AddKillXP(CCSPlayerController player, bool headshot)
    {
        var steamId = player.SteamID;
        if (!_playerXP.ContainsKey(steamId))
            _playerXP[steamId] = 0;
            
        int xpGained = XP_KILL + (headshot ? XP_HEADSHOT_BONUS : 0);
        _playerXP[steamId] += xpGained;
        
        int newLevel = GetLevel(_playerXP[steamId]);
        player.PrintToChat($" \x04[COD]\x01 +{xpGained} XP | Poziom: {newLevel}");
    }
}
```

**Przykład: System klas (Models/Class.cs):**
```csharp
namespace CodMod.Models;

public class PlayerClass
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int RequiredLevel { get; set; }
    public int BonusHP { get; set; }
    public int BonusArmor { get; set; }
    public float SpeedMultiplier { get; set; } = 1.0f;
    public float DamageMultiplier { get; set; } = 1.0f;
    public List<string> Perks { get; set; } = new();
}

public static class Classes
{
    public static readonly Dictionary<string, PlayerClass> All = new()
    {
        ["assault"] = new PlayerClass
        {
            Name = "Assault",
            Description = "Zbalansowana klasa dla początkujących",
            RequiredLevel = 1,
            BonusHP = 10,
            BonusArmor = 0,
            SpeedMultiplier = 1.0f,
            Perks = ["fast_reload"]
        },
        ["recon"] = new PlayerClass
        {
            Name = "Recon",
            Description = "Szybka klasa wywiadowcza",
            RequiredLevel = 5,
            BonusHP = -10,
            BonusArmor = 0,
            SpeedMultiplier = 1.15f,
            Perks = ["silent_step", "radar"]
        },
        ["support"] = new PlayerClass
        {
            Name = "Support",
            Description = "Klasa wspierająca drużynę",
            RequiredLevel = 10,
            BonusHP = 20,
            BonusArmor = 25,
            SpeedMultiplier = 0.95f,
            Perks = ["ammo_regen", "team_heal"]
        },
        ["sniper"] = new PlayerClass
        {
            Name = "Sniper",
            Description = "Klasa dla precyzyjnych strzałów",
            RequiredLevel = 15,
            BonusHP = -20,
            BonusArmor = 0,
            DamageMultiplier = 1.2f,
            Perks = ["steady_aim", "hold_breath"]
        }
    };
}
```

### 6.4 Alternatywa: Pokemon-like system

Jeśli chcecie Pokemon mod zamiast COD:

**Koncept:**
```
Pokemon CS2 Mod:
├── Gracze "łapią" pokemony za zabójstwa
├── Pokemony dają bonusy (HP, DMG, Speed)
├── System ewolucji (3 kills = ewolucja)
├── Walki 1v1 między pokemonami
└── Trading system
```

**Struktura:**
```csharp
public class Pokemon
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public string Type { get; set; }  // Fire, Water, Grass, etc.
    public string? EvolvesTo { get; set; }
    public int EvolutionKills { get; set; }
}
```

---

## 7. Faza 3: Systemy zaawansowane (Tydzień 9-12)

### 7.1 Baza danych MySQL

**Instalacja MySQL na VPS:**
```bash
# Instalacja
sudo apt update
sudo apt install mysql-server -y

# Zabezpieczenie
sudo mysql_secure_installation

# Tworzenie bazy i użytkownika
sudo mysql
```

```sql
-- W konsoli MySQL:
CREATE DATABASE cs2_server CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'cs2user'@'localhost' IDENTIFIED BY 'TajneHaslo123!';
GRANT ALL PRIVILEGES ON cs2_server.* TO 'cs2user'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

**Struktura bazy danych:**
```sql
-- Tabela graczy
CREATE TABLE players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steam_id VARCHAR(32) UNIQUE NOT NULL,
    name VARCHAR(64),
    xp INT DEFAULT 0,
    level INT DEFAULT 1,
    class VARCHAR(32) DEFAULT 'assault',
    kills INT DEFAULT 0,
    deaths INT DEFAULT 0,
    headshots INT DEFAULT 0,
    playtime INT DEFAULT 0,
    vip_until DATETIME NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_steam (steam_id),
    INDEX idx_xp (xp DESC),
    INDEX idx_level (level DESC)
);

-- Tabela VIP
CREATE TABLE vip_players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steam_id VARCHAR(32) NOT NULL,
    tier ENUM('bronze', 'silver', 'gold') NOT NULL,
    expires_at DATETIME NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_steam (steam_id),
    INDEX idx_expires (expires_at)
);

-- Tabela banów
CREATE TABLE bans (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steam_id VARCHAR(32) NOT NULL,
    admin_steam_id VARCHAR(32),
    reason VARCHAR(255),
    duration INT, -- minuty, NULL = permban
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME,
    active BOOLEAN DEFAULT TRUE,
    INDEX idx_steam (steam_id),
    INDEX idx_active (active)
);
```

**Połączenie z C# (DatabaseService.cs):**
```csharp
using MySqlConnector;

namespace CodMod.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string host, string database, string user, string password)
    {
        _connectionString = $"Server={host};Database={database};User={user};Password={password};";
    }

    public async Task<PlayerData?> GetPlayerAsync(ulong steamId)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SELECT * FROM players WHERE steam_id = @steamId", connection);
        command.Parameters.AddWithValue("@steamId", steamId.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PlayerData
            {
                SteamId = reader.GetString("steam_id"),
                Name = reader.GetString("name"),
                XP = reader.GetInt32("xp"),
                Level = reader.GetInt32("level"),
                Class = reader.GetString("class")
            };
        }
        return null;
    }

    public async Task SavePlayerAsync(PlayerData player)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(@"
            INSERT INTO players (steam_id, name, xp, level, class, kills, deaths, headshots)
            VALUES (@steamId, @name, @xp, @level, @class, @kills, @deaths, @headshots)
            ON DUPLICATE KEY UPDATE
                name = @name,
                xp = @xp,
                level = @level,
                class = @class,
                kills = @kills,
                deaths = @deaths,
                headshots = @headshots,
                last_seen = NOW()
        ", connection);

        command.Parameters.AddWithValue("@steamId", player.SteamId);
        command.Parameters.AddWithValue("@name", player.Name);
        command.Parameters.AddWithValue("@xp", player.XP);
        command.Parameters.AddWithValue("@level", player.Level);
        command.Parameters.AddWithValue("@class", player.Class);
        command.Parameters.AddWithValue("@kills", player.Kills);
        command.Parameters.AddWithValue("@deaths", player.Deaths);
        command.Parameters.AddWithValue("@headshots", player.Headshots);

        await command.ExecuteNonQueryAsync();
    }
}
```

### 7.2 System VIP

**Struktura pakietów:**
```
┌─────────────────────────────────────────────────────────────┐
│                      SYSTEM VIP CS2                         │
├─────────────────────────────────────────────────────────────┤
│  🥉 BRONZE VIP (15 zł/mies)                                 │
│  ├── +15 HP na start rundy                                  │
│  ├── Kolorowy nick w scoreboard                             │
│  ├── 1.25x XP bonus                                         │
│  ├── Dostęp do /vip menu                                    │
│  └── Prefix [VIP] w chacie                                  │
├─────────────────────────────────────────────────────────────┤
│  🥈 SILVER VIP (25 zł/mies)                                 │
│  ├── Wszystko z Bronze +                                    │
│  ├── +25 HP, +25 Armor na start                             │
│  ├── 1.5x XP bonus                                          │
│  ├── Zarezerwowany slot                                     │
│  ├── Smoke color customization                              │
│  └── Prefix [VIP+] w chacie (niebieski)                     │
├─────────────────────────────────────────────────────────────┤
│  🥇 GOLD VIP (40 zł/mies)                                   │
│  ├── Wszystko z Silver +                                    │
│  ├── +40 HP, +50 Armor na start                             │
│  ├── 2x XP bonus                                            │
│  ├── Custom weapon skins (workshop)                         │
│  ├── Custom kill sound                                      │
│  ├── Priority queue                                         │
│  └── Prefix [VIP★] w chacie (złoty)                         │
└─────────────────────────────────────────────────────────────┘
```

**VIP Plugin (VIPSystem.cs):**
```csharp
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace VIPSystem;

public enum VIPTier
{
    None = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3
}

public class VIPSystem : BasePlugin
{
    public override string ModuleName => "VIP System";
    public override string ModuleVersion => "1.0.0";

    private readonly Dictionary<ulong, VIPTier> _vipPlayers = new();
    
    private readonly Dictionary<VIPTier, VIPPerks> _perks = new()
    {
        [VIPTier.Bronze] = new VIPPerks { BonusHP = 15, BonusArmor = 0, XPMultiplier = 1.25f },
        [VIPTier.Silver] = new VIPPerks { BonusHP = 25, BonusArmor = 25, XPMultiplier = 1.5f, ReservedSlot = true },
        [VIPTier.Gold] = new VIPPerks { BonusHP = 40, BonusArmor = 50, XPMultiplier = 2.0f, ReservedSlot = true, CustomSkins = true }
    };

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        
        // Załaduj VIPów z bazy danych
        LoadVIPsFromDatabase();
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        var tier = GetVIPTier(player.SteamID);
        if (tier == VIPTier.None) return HookResult.Continue;

        var perks = _perks[tier];
        
        // Dodaj bonusy
        AddTimer(0.1f, () =>
        {
            if (!player.IsValid) return;
            
            var pawn = player.PlayerPawn.Value;
            if (pawn == null) return;

            pawn.Health += perks.BonusHP;
            pawn.ArmorValue += perks.BonusArmor;
            
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        });

        return HookResult.Continue;
    }

    [ConsoleCommand("css_vip", "Pokaż menu VIP")]
    public void OnVIPCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;

        var tier = GetVIPTier(player.SteamID);
        
        if (tier == VIPTier.None)
        {
            player.PrintToChat(" \x04[VIP]\x01 Nie masz aktywnego VIP!");
            player.PrintToChat(" \x04[VIP]\x01 Kup VIP na: \x0Bwww.naszserver.pl/vip");
            return;
        }

        var perks = _perks[tier];
        player.PrintToChat($" \x04[VIP]\x01 Twój pakiet: \x0B{tier}");
        player.PrintToChat($" \x04[VIP]\x01 Bonusy: +{perks.BonusHP} HP, +{perks.BonusArmor} Armor");
        player.PrintToChat($" \x04[VIP]\x01 XP Multiplier: x{perks.XPMultiplier}");
    }

    private VIPTier GetVIPTier(ulong steamId)
    {
        return _vipPlayers.TryGetValue(steamId, out var tier) ? tier : VIPTier.None;
    }

    private void LoadVIPsFromDatabase()
    {
        // TODO: Załaduj z MySQL
    }
}

public class VIPPerks
{
    public int BonusHP { get; set; }
    public int BonusArmor { get; set; }
    public float XPMultiplier { get; set; } = 1.0f;
    public bool ReservedSlot { get; set; }
    public bool CustomSkins { get; set; }
}
```

### 7.3 System rang z HUD

**Rangi do zaimplementowania:**
```csharp
public static class Ranks
{
    public static readonly Dictionary<int, RankInfo> All = new()
    {
        [1] = new("Rekrut", 0, "⭐"),
        [2] = new("Szeregowy", 500, "⭐⭐"),
        [3] = new("Starszy Szeregowy", 1200, "⭐⭐⭐"),
        [4] = new("Kapral", 2000, "🎖️"),
        [5] = new("Starszy Kapral", 3000, "🎖️⭐"),
        [6] = new("Sierżant", 4500, "🎖️⭐⭐"),
        [7] = new("Starszy Sierżant", 6500, "🎖️⭐⭐⭐"),
        [8] = new("Chorąży", 9000, "🏅"),
        [9] = new("Podporucznik", 12000, "🏅⭐"),
        [10] = new("Porucznik", 15500, "🏅⭐⭐"),
        [11] = new("Kapitan", 20000, "🎯"),
        [12] = new("Major", 25000, "🎯⭐"),
        [13] = new("Podpułkownik", 31000, "🎯⭐⭐"),
        [14] = new("Pułkownik", 38000, "⚔️"),
        [15] = new("Generał Brygady", 46000, "⚔️⭐"),
        [16] = new("Generał Dywizji", 55000, "⚔️⭐⭐"),
        [17] = new("Generał Broni", 65000, "👑"),
        [18] = new("Generał", 76000, "👑⭐"),
        [19] = new("Marszałek", 88000, "👑⭐⭐"),
        [20] = new("Legenda", 100000, "🏆")
    };
}

public record RankInfo(string Name, int RequiredXP, string Icon);
```

---

## 8. Faza 4: Web i społeczność (Tydzień 13-16)

### 8.1 Strona WWW

**Opcja A: Gotowy panel (szybko)**

| Rozwiązanie | Funkcje | Koszt |
|-------------|---------|-------|
| **GameCMS** | Stats, bany, admini | Free/Premium |
| **IKS Admin** | Panel adminów | Free |
| **Custom WordPress** | Blog + pluginy | ~100 zł/rok |

**Opcja B: Custom aplikacja (rekomendowane)**

**Stack technologiczny:**
```
Frontend: React/Vue/Next.js
Backend: Node.js/Express lub .NET 8 Web API
Baza: MySQL (wspólna z serwerem)
Hosting: Na tym samym VPS lub Vercel/Railway
```

**Funkcje strony:**
```
📊 Dashboard
├── Live status serwera (gracze, mapa)
├── Top 10 graczy tygodnia
└── Ostatnie mecze

🏆 Rankingi
├── Globalna tabela (sortowanie, filtrowanie)
├── Top broni
├── Top klas (dla COD mod)
└── Wyszukiwarka gracza

👤 Profile graczy
├── Statystyki szczegółowe
├── Historia poziomów
├── Osiągnięcia
└── Linkowanie Steam

💎 VIP Shop
├── Prezentacja pakietów
├── Integracja płatności (PayU/Stripe)
├── Auto-aktywacja VIP
└── Historia transakcji

📋 Admin Panel
├── Lista graczy online
├── Zarządzanie banami
├── Zarządzanie VIP
└── Logi serwera
```

**Koszty domeny i hostingu:**

| Pozycja | Koszt/rok | Gdzie |
|---------|-----------|-------|
| Domena .pl | ~50 zł | nazwa.pl, ovh.pl |
| Domena .eu | ~40 zł | ovh.pl |
| SSL | 0 zł | Let's Encrypt |
| Hosting (na VPS) | 0 zł | Już mamy VPS |
| Hosting (osobny) | ~200-400 zł | shared hosting |

### 8.2 Discord Server

**Struktura:**
```
🏠 SERWER CS2

📢 INFO
├── #regulamin
├── #aktualności
├── #changelog
└── #faq

💬 SPOŁECZNOŚĆ
├── #ogólny
├── #szukam-gry
├── #propozycje
├── #memy-i-klipy
└── #off-topic

🎮 GRA
├── #ranking-live (bot)
├── #pomoc
├── #zgłoś-cheatera
└── #report-bug

🔊 GŁOSOWE
├── 🔊 Lobby
├── 🔊 Team 1
├── 🔊 Team 2
└── 🔊 VIP Lounge

💎 VIP
├── #vip-chat
└── #vip-pomoc

🔧 ADMIN
├── #team-chat
├── #logi
└── #ban-appeals
```

**Bot Discord - Live Status:**
```javascript
// Użyj biblioteki: discord.js + gamedig

const { Client, EmbedBuilder } = require('discord.js');
const Gamedig = require('gamedig');

const client = new Client({ intents: ['Guilds', 'GuildMessages'] });

async function updateServerStatus() {
    try {
        const state = await Gamedig.query({
            type: 'cs2',
            host: 'TWOJ_IP',
            port: 27015
        });

        const embed = new EmbedBuilder()
            .setTitle('🎮 ' + state.name)
            .setColor(state.players.length > 0 ? 0x00ff00 : 0xff9900)
            .addFields(
                { name: '👥 Gracze', value: `${state.players.length}/${state.maxplayers}`, inline: true },
                { name: '🗺️ Mapa', value: state.map, inline: true },
                { name: '📡 Ping', value: `${state.ping}ms`, inline: true }
            )
            .setFooter({ text: 'Aktualizacja co minutę' })
            .setTimestamp();

        if (state.players.length > 0) {
            const playerList = state.players.map(p => p.name).join(', ');
            embed.addFields({ name: '🎮 Online', value: playerList.substring(0, 1024) });
        }

        // Aktualizuj wiadomość w kanale
        const channel = await client.channels.fetch('CHANNEL_ID');
        const messages = await channel.messages.fetch({ limit: 1 });
        const lastMessage = messages.first();

        if (lastMessage?.author.id === client.user.id) {
            await lastMessage.edit({ embeds: [embed] });
        } else {
            await channel.send({ embeds: [embed] });
        }
    } catch (error) {
        console.error('Server query failed:', error);
    }
}

client.on('ready', () => {
    console.log('Bot ready!');
    setInterval(updateServerStatus, 60000);
    updateServerStatus();
});

client.login('YOUR_BOT_TOKEN');
```

### 8.3 Marketing

**Gdzie reklamować CS2 server:**

| Platforma | Opis | Priorytet |
|-----------|------|-----------|
| **Steam Server Browser** | Gracze szukają serwerów | Automatyczne |
| **GameTracker.com** | Monitoring serwerów | Wysoki |
| **BattleMetrics** | Statystyki, ranking | Wysoki |
| **Reddit r/cs2** | Społeczność CS2 | Średni |
| **Discord serwery CS2** | Targetowani gracze | Wysoki |
| **YouTube/TikTok** | Klipy z gameplay | Średni |
| **Polskie grupy FB** | Polscy gracze | Średni |

**Strategie retencji:**
1. **Weekly Events** - turnieje, double XP
2. **Season Rewards** - reset rang co 3 miesiące, nagrody
3. **Referral System** - zaproś znajomego = bonus XP
4. **Daily Challenges** - codzienne wyzwania
5. **Leaderboards** - competitiveness

---

## 9. Faza 5: Monetyzacja i skalowanie (Tydzień 17+)

### 9.1 Model przychodów

```
┌────────────────────────────────────────────┐
│           ŹRÓDŁA PRZYCHODÓW                │
├────────────────────────────────────────────┤
│  💎 VIP Subscriptions (główne ~80%)        │
│     Bronze: 15 zł/mies                     │
│     Silver: 25 zł/mies                     │
│     Gold: 40 zł/mies                       │
├────────────────────────────────────────────┤
│  🎁 Donacje (supporting ~15%)              │
│     PayPal, BLIK, PSC                      │
├────────────────────────────────────────────┤
│  🛒 Cosmetics Shop (~5%)                   │
│     Custom name colors                     │
│     Chat effects                           │
│     Profile badges                         │
└────────────────────────────────────────────┘
```

### 9.2 Systemy płatności

| System | Prowizja | Dla kogo | Integracja |
|--------|----------|----------|------------|
| **PayU** | 1.9-2.5% | Polscy gracze, BLIK | Łatwa |
| **Przelewy24** | ~2% | Polscy gracze | Średnia |
| **Stripe** | 1.4% + 0.25€ | Międzynarodowo | Łatwa |
| **PayPal** | 2.9% + 0.30$ | Międzynarodowo | Łatwa |
| **HotPay SMS** | 15-20% | Młodzi gracze | Łatwa |

### 9.3 ROI Calculation

```
KOSZTY MIESIĘCZNE (Scenariusz optymalny):
├── VPS (4GB RAM): ~60 zł
├── Domena (rocznie/12): ~5 zł
├── MySQL: 0 zł (na VPS)
└── RAZEM: ~65 zł/mies

BREAK-EVEN:
├── 5x Bronze (75 zł) ✓
├── 3x Silver (75 zł) ✓
└── 2x Gold (80 zł) ✓

PRZY 100 AKTYWNYCH GRACZY DZIENNIE:
├── Conversion rate: 3-5%
├── Spodziewani VIPowie: 3-5
├── Przychód: 60-150 zł/mies
└── ROI: Break-even przy ~80-100 aktywnych graczy
```

### 9.4 Skalowanie

**Kiedy dodać drugi serwer:**
- Pierwszy serwer regularnie 15+ graczy
- Stabilne przychody > koszty
- Społeczność prosi o więcej

**Typy dodatkowych serwerów:**
| Typ | Trudność | Popularność |
|-----|----------|-------------|
| **Retake** | Łatwa | Wysoka |
| **Deathmatch** | Łatwa | Wysoka |
| **Surf** | Średnia | Średnia |
| **KZ (Kreedz)** | Średnia | Niszowa |
| **1v1 Arena** | Średnia | Wysoka |
| **Zombie** | Wysoka | Średnia |

---

## 10. Budżet i koszty

### 10.1 Scenariusz MINIMUM (testowanie)

| Pozycja | Jednorazowo | Miesięcznie |
|---------|-------------|-------------|
| Game hosting (basic) | 0 zł | 40-60 zł |
| Domena | 0 zł | 0 zł |
| Pluginy (open source) | 0 zł | 0 zł |
| **RAZEM** | **0 zł** | **40-60 zł** |

### 10.2 Scenariusz OPTYMALNY (produkcja)

| Pozycja | Jednorazowo | Miesięcznie |
|---------|-------------|-------------|
| VPS (4-8GB RAM) | 0 zł | 50-80 zł |
| Domena .pl | 50 zł/rok | ~4 zł |
| SSL | 0 zł | 0 zł |
| MySQL (na VPS) | 0 zł | 0 zł |
| Backup (opcja) | 0 zł | 10-20 zł |
| **RAZEM** | **~50 zł** | **65-105 zł** |

### 10.3 Scenariusz PRO (pełna skala)

| Pozycja | Jednorazowo | Miesięcznie |
|---------|-------------|-------------|
| Dedykowany serwer | 0 zł | 200-400 zł |
| Domena + subdomeny | 100 zł/rok | ~8 zł |
| DDoS Protection | 0 zł | 50-100 zł |
| CDN (CloudFlare Pro) | 0 zł | 80 zł |
| Monitoring | 0 zł | 0-50 zł |
| **RAZEM** | **~100 zł** | **350-650 zł** |

### 10.4 Porównanie roczne

| Scenariusz | Rok 1 | Rok 2+ |
|------------|-------|--------|
| **Minimum** | ~600 zł | ~600 zł |
| **Optymalny** | ~850 zł | ~800 zł |
| **Pro** | ~4500 zł | ~4400 zł |

---

## 11. Zasoby i linki

### 11.1 Oficjalne i kluczowe

| Zasób | Link | Opis |
|-------|------|------|
| **CounterStrikeSharp** | https://github.com/roflmuffin/CounterStrikeSharp | Framework pluginów |
| **CSS Docs** | https://docs.cssharp.dev | Dokumentacja API |
| **CSS Discord** | https://discord.gg/cssharp | Pomoc, społeczność |
| **Metamod:Source** | https://www.sourcemm.net | Metamod dla Source 2 |
| **AlliedMods** | https://forums.alliedmods.net | Forum, pluginy |

### 11.2 Repozytoria pluginów

| Plugin | Link | Opis |
|--------|------|------|
| K4-System | github.com/K4ryuu/K4-System | Rangi, statystyki |
| MatchZy | github.com/shobhit-pathak/MatchZy | System meczy |
| SharpTimer | github.com/K4ryuu/SharpTimer | Timer, KZ |
| CS2-Admin | github.com/daffyyyy/CS2-Admin | Admin panel |
| cs2-modded-server | github.com/kus/cs2-modded-server | Gotowy setup |

### 11.3 Nauka C#

| Zasób | Link | Poziom |
|-------|------|--------|
| Microsoft Learn | learn.microsoft.com/dotnet/csharp | Początkujący |
| W3Schools C# | w3schools.com/cs | Początkujący |
| C# Yellow Book | csharpcourse.com | Średni |
| Pluralsight | pluralsight.com | Średni-Zaawansowany |

### 11.4 Narzędzia

| Narzędzie | Link | Opis |
|-----------|------|------|
| Visual Studio 2022 | visualstudio.microsoft.com | IDE (Windows) |
| JetBrains Rider | jetbrains.com/rider | IDE (cross-platform) |
| VS Code | code.visualstudio.com | Lekki edytor |
| FileZilla | filezilla-project.org | FTP |
| PuTTY | putty.org | SSH (Windows) |
| HeidiSQL | heidisql.com | MySQL GUI |

---

## 12. Checklisty

### 12.1 Przed startem
- [ ] Zespół zna podstawy C#
- [ ] Zainstalowano .NET SDK 8.0+
- [ ] Zainstalowano IDE (VS/Rider/VS Code)
- [ ] Założono GitHub, Discord
- [ ] Ustalono budżet miesięczny (~65-100 zł)
- [ ] Wybrano hosting (VPS rekomendowany)
- [ ] Zdecydowano o głównym trybie gry

### 12.2 Faza 1: Serwer
- [ ] Zakupiono VPS/hosting
- [ ] Zainstalowano CS2 dedicated server
- [ ] Wygenerowano GSLT token
- [ ] Zainstalowano Metamod:Source 2.x
- [ ] Zainstalowano CounterStrikeSharp
- [ ] Skonfigurowano server.cfg
- [ ] Skonfigurowano admins.json
- [ ] Serwer widoczny w browser
- [ ] Test: można połączyć i grać

### 12.3 Faza 2: Pluginy
- [ ] Zainstalowano podstawowe pluginy (admin, stats)
- [ ] Napisano pierwszy własny plugin (test)
- [ ] Zaimplementowano system XP (podstawy)
- [ ] Zaimplementowano system klas/perków
- [ ] Przetestowano wszystkie funkcje
- [ ] Naprawiono błędy

### 12.4 Faza 3: Zaawansowane
- [ ] Skonfigurowano MySQL
- [ ] Połączono pluginy z bazą danych
- [ ] Zaimplementowano system VIP
- [ ] Zaimplementowano rankingi
- [ ] Zrobiono backup bazy

### 12.5 Faza 4: Społeczność
- [ ] Zakupiono domenę
- [ ] Postawiono stronę WWW
- [ ] Utworzono Discord
- [ ] Skonfigurowano boty
- [ ] Zarejestrowano na trackingach
- [ ] Rozpoczęto marketing

### 12.6 Faza 5: Monetyzacja
- [ ] Skonfigurowano płatności
- [ ] Zautomatyzowano VIP
- [ ] Przygotowano regulamin
- [ ] Przetestowano cały flow

---

## 13. Ryzyka i problemy

### 13.1 Główne ryzyka

| Ryzyko | Prawdopod. | Wpływ | Mitygacja |
|--------|------------|-------|-----------|
| **Brak znajomości C#** | Wysokie | Krytyczny | Nauka przed startem! |
| **CS2 updates łamią pluginy** | Wysokie | Wysoki | Śledzenie CSS Discord, szybkie poprawki |
| **DDoS attacks** | Średnie | Wysoki | DDoS protection, backup IP |
| **Mało graczy** | Średnie | Wysoki | Marketing, eventy, jakość |
| **Koszty > przychody** | Średnie | Średni | Budżetowanie, start od minimum |

### 13.2 Typowe problemy

**Problem: Plugin nie ładuje się**
```
Rozwiązanie:
1. Sprawdź logi: game/csgo/addons/counterstrikesharp/logs/
2. Sprawdź wersję .NET (musi być 8.0+)
3. Sprawdź czy wszystkie dependencies są
4. css_plugins list - sprawdź czy widoczny
```

**Problem: Serwer nie widoczny w browser**
```
Rozwiązanie:
1. Sprawdź GSLT token (musi być ważny)
2. Sprawdź firewall (porty 27015 TCP/UDP)
3. sv_lan musi być 0
4. Sprawdź czy Steam login działa
```

**Problem: MySQL connection refused**
```
Rozwiązanie:
1. Sprawdź bind-address w my.cnf
2. Sprawdź firewall na porcie 3306
3. Sprawdź user privileges
4. Test: mysql -u user -p -h localhost
```

**Problem: Update CS2 zepsul pluginy**
```
Rozwiązanie:
1. Sprawdź CSS Discord na ogłoszenia
2. Zaktualizuj CounterStrikeSharp
3. Zaktualizuj pluginy do najnowszych wersji
4. Poczekaj na fix od autorów
```

---

## Appendix A: Przykładowy COD Mod - Pełna struktura

```
CodModCS2/
├── CodModCS2.sln
├── src/
│   └── CodModCS2/
│       ├── CodModCS2.cs              # Entry point
│       ├── CodModCS2.csproj
│       ├── Config/
│       │   ├── PluginConfig.cs
│       │   └── config.json
│       ├── Models/
│       │   ├── PlayerData.cs
│       │   ├── PlayerClass.cs
│       │   ├── Perk.cs
│       │   └── Killstreak.cs
│       ├── Services/
│       │   ├── DatabaseService.cs
│       │   ├── XPService.cs
│       │   ├── ClassService.cs
│       │   ├── PerkService.cs
│       │   └── KillstreakService.cs
│       ├── Commands/
│       │   ├── ClassCommands.cs
│       │   ├── StatsCommands.cs
│       │   └── AdminCommands.cs
│       ├── Events/
│       │   ├── PlayerEvents.cs
│       │   ├── RoundEvents.cs
│       │   └── KillEvents.cs
│       └── Utils/
│           ├── ChatColors.cs
│           └── Helpers.cs
├── database/
│   └── schema.sql
└── README.md
```

---

## Appendix B: Komendy serwera

**Komendy gracza:**
```
!help / /help        - Lista komend
!class / /class      - Wybór klasy
!stats / /stats      - Twoje statystyki
!top / /top          - Top 10 graczy
!rank / /rank        - Twoja pozycja w rankingu
!vip / /vip          - Menu VIP
!perks / /perks      - Lista perków
```

**Komendy admina:**
```
css_kick <player>           - Wyrzuć gracza
css_ban <player> <minutes>  - Zbanuj gracza
css_slay <player>           - Zabij gracza
css_map <mapname>           - Zmień mapę
css_rcon <command>          - RCON
css_plugins list            - Lista pluginów
css_plugins reload <name>   - Przeładuj plugin
```

---

**Dokument przygotowany dla:** [Nazwa zespołu]  
**Technologia:** CS2 + CounterStrikeSharp (C#)  
**Ostatnia aktualizacja:** [data]  
**Kontakt:** [Discord serwera]
