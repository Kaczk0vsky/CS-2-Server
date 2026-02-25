# System VIP i Monetyzacja

## Pakiety VIP

| Pakiet | Cena/mies | Bonusy |
|--------|-----------|--------|
| **Bronze** | 15 zł | +15 HP, 1.25x XP, prefix [VIP] |
| **Silver** | 25 zł | +25 HP, +25 Armor, 1.5x XP, slot zarezerwowany |
| **Gold** | 40 zł | +40 HP, +50 Armor, 2x XP, custom skiny |

## VIP Plugin - Przykład

```csharp
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace VIPSystem;

public enum VIPTier { None = 0, Bronze = 1, Silver = 2, Gold = 3 }

public class VIPSystem : BasePlugin
{
    public override string ModuleName => "VIP System";
    public override string ModuleVersion => "1.0.0";

    private readonly Dictionary<ulong, VIPTier> _vipPlayers = new();
    
    private readonly Dictionary<VIPTier, VIPPerks> _perks = new()
    {
        [VIPTier.Bronze] = new(15, 0, 1.25f),
        [VIPTier.Silver] = new(25, 25, 1.5f),
        [VIPTier.Gold] = new(40, 50, 2.0f)
    };

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        var tier = GetVIPTier(player.SteamID);
        if (tier == VIPTier.None) return HookResult.Continue;

        var perks = _perks[tier];
        
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
            player.PrintToChat(" \x04[VIP]\x01 Nie masz VIP! Kup na: www.naszserver.pl/vip");
            return;
        }

        var perks = _perks[tier];
        player.PrintToChat($" \x04[VIP]\x01 Pakiet: {tier} | +{perks.BonusHP} HP | x{perks.XPMultiplier} XP");
    }

    private VIPTier GetVIPTier(ulong steamId) =>
        _vipPlayers.TryGetValue(steamId, out var tier) ? tier : VIPTier.None;
}

public record VIPPerks(int BonusHP, int BonusArmor, float XPMultiplier);
```

## Systemy płatności

| System | Prowizja | Dla kogo |
|--------|----------|----------|
| **PayU** | 1.9-2.5% | Polscy gracze, BLIK |
| **Przelewy24** | ~2% | Polscy gracze |
| **Stripe** | 1.4% + 0.25€ | Międzynarodowo |
| **HotPay SMS** | 15-20% | Młodzi gracze |

## Koszty i ROI

### Koszty miesięczne (optymalny scenariusz)
| Pozycja | Koszt |
|---------|-------|
| VPS (4GB RAM) | ~60 zł |
| Domena | ~5 zł |
| MySQL | 0 zł (na VPS) |
| **RAZEM** | ~65 zł |

### Break-even
- 5x Bronze (75 zł) ✓
- 3x Silver (75 zł) ✓
- 2x Gold (80 zł) ✓

### Przy 100 aktywnych graczy dziennie
- Conversion rate: 3-5%
- Spodziewani VIPowie: 3-5
- Przychód: 60-150 zł/mies
- **ROI: Break-even przy ~80-100 aktywnych graczy**

## Strategie retencji
1. **Weekly Events** - turnieje, double XP
2. **Season Rewards** - reset rang co 3 miesiące
3. **Referral System** - zaproś znajomego = bonus XP
4. **Daily Challenges** - codzienne wyzwania
5. **Leaderboards** - competitiveness

## Skalowanie

Kiedy dodać drugi serwer:
- Pierwszy serwer regularnie 15+ graczy
- Stabilne przychody > koszty
- Społeczność prosi o więcej

Typy dodatkowych serwerów:
| Typ | Trudność | Popularność |
|-----|----------|-------------|
| Retake | Łatwa | Wysoka |
| Deathmatch | Łatwa | Wysoka |
| 1v1 Arena | Średnia | Wysoka |
| Surf | Średnia | Średnia |
