# Pluginy i Mody CS2

## Zainstalowane pluginy

| Plugin | Opis | Status |
|--------|------|--------|
| **CS2-SimpleAdmin** | Panel administracyjny, bany, kicki | ✅ Zainstalowany |
| **MenuManagerCore** | System menu dla pluginów | ✅ Zainstalowany |
| **PlayerSettings** | Ustawienia graczy | ✅ Zainstalowany |

## Rekomendowane pluginy do zainstalowania

| Plugin | Opis | Link | Priorytet |
|--------|------|------|-----------|
| **K4-System** | Rangi, statystyki, czas gry | github.com/K4ryuu/K4-System | WYSOKI |
| **SharpTimer** | System czasów, leaderboardy | github.com/K4ryuu/SharpTimer | Średni |
| **MatchZy** | System meczy, Get5-like | github.com/shobhit-pathak/MatchZy | Średni |
| **CS2-Retakes** | Tryb retake | github.com/B3none/cs2-retakes | Średni |

## Instalacja pluginu

```bash
# 1. Pobierz plugin z GitHub releases
# 2. Rozpakuj do: addons/counterstrikesharp/plugins/<NazwaPluginu>/
# 3. Skopiuj do cs2-data/game/csgo/addons/counterstrikesharp/plugins/
# 4. Restart serwera lub: css_plugins reload <NazwaPluginu>
```

## Tworzenie własnego pluginu

### Struktura projektu
```
MyPlugin/
├── MyPlugin.cs
├── MyPlugin.csproj
└── lang/
    └── en.json
```

### MyPlugin.csproj
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

### Przykład: Hello World Plugin
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
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);
    }

    [ConsoleCommand("css_hello", "Powitanie")]
    public void OnHelloCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;
        player.PrintToChat($" \x04[Server]\x01 Cześć, {player.PlayerName}!");
    }

    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;
        Server.PrintToChatAll($" \x04[Server]\x01 {player.PlayerName} dołączył!");
        return HookResult.Continue;
    }
}
```

### Kompilacja
```bash
cd MyPlugin
dotnet build -c Release
# Wynik: bin/Release/net8.0/MyPlugin.dll
```

## COD Mod - Plan implementacji

### Struktura COD Mod
```
CodMod/
├── CodMod.cs                 # Entry point
├── CodMod.csproj
├── Models/
│   ├── PlayerData.cs         # XP, level, klasa
│   ├── PlayerClass.cs        # Definicja klasy
│   └── Perk.cs               # Definicja perku
├── Services/
│   ├── DatabaseService.cs    # Zapis/odczyt z DB
│   ├── ClassService.cs       # Logika klas
│   └── XPService.cs          # System doświadczenia
└── Config/
    └── config.json
```

### System XP
```csharp
// XP za różne akcje
private const int XP_KILL = 100;
private const int XP_HEADSHOT_BONUS = 50;
private const int XP_ASSIST = 25;
private const int XP_MVP = 200;
private const int XP_BOMB_PLANT = 50;
private const int XP_BOMB_DEFUSE = 100;
```

### Przykładowe klasy
| Klasa | Opis | Wymagany poziom | Bonusy |
|-------|------|-----------------|--------|
| Assault | Zbalansowana | 1 | +10 HP |
| Recon | Szybka | 5 | +15% speed, -10 HP |
| Support | Tank | 10 | +20 HP, +25 Armor |
| Sniper | Precyzyjna | 15 | +20% DMG, -20 HP |

## Zasoby

- **CounterStrikeSharp Docs**: https://docs.cssharp.dev
- **CSS Discord**: https://discord.gg/cssharp
- **GitHub Pluginów**: https://github.com/topics/counterstrikesharp
