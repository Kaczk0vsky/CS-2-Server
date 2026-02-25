# Setup i Przygotowanie Serwera CS2

## Wymagania techniczne

```
PROGRAMIŚCI MUSZĄ ZNAĆ:
├── C# (główny język pluginów CS2!)
├── .NET 8.0+
├── Podstawy SQL (MySQL/PostgreSQL)
├── Git (wersjonowanie)
├── Linux/Docker (serwery)
└── Opcjonalnie: TypeScript/React (web panel)
```

## CS2 vs CS 1.6 - Kluczowe różnice

| Aspekt | CS 1.6 | CS2 |
|--------|--------|-----|
| **Silnik** | GoldSrc | Source 2 |
| **Framework modów** | AMX Mod X | CounterStrikeSharp |
| **Język pluginów** | Pawn | **C# (.NET)** |
| **COD/Pokemon mody** | Gotowe, do pobrania | **Trzeba napisać od zera!** |

## Instalacja narzędzi deweloperskich

### .NET SDK 8.0+
- Windows: https://dotnet.microsoft.com/download
- Weryfikacja: `dotnet --version`

### IDE - wybierz jedno:
- **Visual Studio 2022** (Windows, pełne IDE) - REKOMENDOWANE
- **JetBrains Rider** (cross-platform, płatne)
- **VS Code + C# extension** (lekkie, cross-platform)

### Docker Desktop
- https://www.docker.com/products/docker-desktop

### Narzędzia do baz danych
- HeidiSQL (Windows, MySQL)
- DBeaver (cross-platform)

## Uruchomienie serwera (Docker)

### 1. Przygotowanie
```bash
# Sklonuj repo
git clone <repo_url>
cd CS-2-Server

# Skopiuj i wypełnij .env
cp .env.template .env
# Edytuj .env - uzupełnij SRCDS_TOKEN, STEAMUSER, STEAMPASSWORD
```

### 2. GSLT Token (WYMAGANE!)
1. Idź na: https://steamcommunity.com/dev/managegameservers
2. Zaloguj się kontem Steam
3. App ID: 730
4. Wygeneruj token
5. Wklej do `.env` jako `SRCDS_TOKEN`

### 3. Uruchomienie
```bash
docker compose up
```

### 4. Konfiguracja Metamod
Po pierwszym uruchomieniu:
1. Skopiuj folder `addons` do `cs2-data/game/csgo/`
2. Edytuj `cs2-data/game/csgo/gameinfo.gi`
3. Dodaj `Game csgo/addons/metamod` bezpośrednio nad `Game csgo`
4. Zrestartuj kontenery

### 5. Test połączenia
```bash
# W CS2
connect localhost
```

### 6. Dodanie admina
```bash
# Wejdź do kontenera
docker attach cs2-server

# Dodaj admina (znajdź SteamID64 na steamid.io)
css_addadmin <steamID64> <nick> @css/root 99
```

## RCON
```bash
./ARRCON.exe -H localhost -P 28015 -p changeme -i
```

## Struktura projektu
```
CS-2-Server/
├── addons/                    # Pluginy do skopiowania
│   ├── metamod/
│   └── counterstrikesharp/
│       └── plugins/           # Zainstalowane pluginy
├── cs2-data/                  # Dane serwera (po uruchomieniu)
├── docs/                      # Dokumentacja
├── docker-compose.yml         # Konfiguracja Docker
└── .env                       # Zmienne środowiskowe (nie commituj!)
```

## Nauka C# (KRYTYCZNE!)

| Dzień | Temat | Zasoby |
|-------|-------|--------|
| 1-2 | Podstawy C# | Microsoft Learn, YouTube |
| 3-4 | OOP w C# (klasy, interfejsy) | docs.microsoft.com |
| 5-6 | Async/await, wydarzenia | docs.microsoft.com |
| 7 | .NET CLI, projekty | dotnet.microsoft.com |
| 8-10 | CounterStrikeSharp API | docs.cssharp.dev |

### Zasoby darmowe:
- Microsoft Learn C#: https://learn.microsoft.com/pl-pl/dotnet/csharp/
- W3Schools C#: https://www.w3schools.com/cs/
- YouTube: "C# for beginners"

## Decyzje do podjęcia

| Decyzja | Opcje | Rekomendacja |
|---------|-------|--------------|
| **Główny tryb gry** | COD-like / Pokemon / Zombie | Zacznij od gotowych pluginów + custom perki |
| **Hosting prod** | Game hosting / VPS | VPS (więcej kontroli) |
| **Baza danych** | MySQL / PostgreSQL | MySQL (już skonfigurowane) |
