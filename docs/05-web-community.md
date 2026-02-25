# Strona WWW i Społeczność

## Strona WWW

### Opcja A: Gotowy panel (szybko)
- GameCMS - Stats, bany, admini
- IKS Admin - Panel adminów
- Custom WordPress

### Opcja B: Custom aplikacja (rekomendowane)
```
Frontend: React/Vue/Next.js
Backend: Node.js lub .NET 8 Web API
Baza: MySQL (wspólna z serwerem)
Hosting: Ten sam VPS lub Vercel/Railway
```

### Funkcje strony
- **Dashboard** - live status, top gracze, ostatnie mecze
- **Rankingi** - tabela, top broni, wyszukiwarka
- **Profile** - statystyki, historia, osiągnięcia
- **VIP Shop** - pakiety, płatności, auto-aktywacja
- **Admin Panel** - gracze online, bany, VIP, logi

### Koszty
| Pozycja | Koszt/rok |
|---------|-----------|
| Domena .pl | ~50 zł |
| SSL | 0 zł (Let's Encrypt) |
| Hosting (na VPS) | 0 zł |

## Discord Server

### Struktura kanałów
```
📢 INFO
├── #regulamin
├── #aktualności
└── #faq

💬 SPOŁECZNOŚĆ
├── #ogólny
├── #szukam-gry
└── #propozycje

🎮 GRA
├── #ranking-live (bot)
├── #pomoc
└── #zgłoś-cheatera

🔊 GŁOSOWE
├── 🔊 Lobby
├── 🔊 Team 1/2
└── 🔊 VIP Lounge

💎 VIP
└── #vip-chat

🔧 ADMIN
├── #team-chat
└── #ban-appeals
```

### Bot Discord - Live Status
```javascript
const { Client, EmbedBuilder } = require('discord.js');
const Gamedig = require('gamedig');

async function updateServerStatus() {
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
            { name: '🗺️ Mapa', value: state.map, inline: true }
        )
        .setTimestamp();

    // Aktualizuj wiadomość w kanale
}

// Uruchamiaj co minutę
setInterval(updateServerStatus, 60000);
```

## Marketing

### Gdzie reklamować
| Platforma | Priorytet |
|-----------|-----------|
| Steam Server Browser | Automatyczne |
| GameTracker.com | Wysoki |
| BattleMetrics | Wysoki |
| Discord serwery CS2 | Wysoki |
| Reddit r/cs2 | Średni |
| YouTube/TikTok klipy | Średni |
| Polskie grupy FB | Średni |

## Checklisty

### Przed startem
- [ ] Zespół zna podstawy C#
- [ ] .NET SDK 8.0+ zainstalowane
- [ ] IDE gotowe (VS/Rider/VS Code)
- [ ] GitHub, Discord założone
- [ ] Budżet ustalony (~65-100 zł/mies)

### Faza 1: Serwer
- [ ] VPS/Docker skonfigurowany
- [ ] CS2 server działa
- [ ] GSLT token wygenerowany
- [ ] Metamod + CSS zainstalowane
- [ ] Admin dodany
- [ ] Test połączenia OK

### Faza 2: Pluginy
- [ ] Podstawowe pluginy zainstalowane
- [ ] System XP działa
- [ ] System klas/perków działa
- [ ] Testy przeszły

### Faza 3: Zaawansowane
- [ ] MySQL skonfigurowany
- [ ] Pluginy połączone z DB
- [ ] VIP system działa
- [ ] Rankingi działają

### Faza 4: Społeczność
- [ ] Domena kupiona
- [ ] Strona WWW działa
- [ ] Discord gotowy
- [ ] Marketing rozpoczęty

## Typowe problemy

**Plugin nie ładuje się:**
1. Sprawdź logi: `addons/counterstrikesharp/logs/`
2. Sprawdź wersję .NET (musi być 8.0+)
3. `css_plugins list` - sprawdź czy widoczny

**Serwer nie widoczny:**
1. Sprawdź GSLT token
2. Sprawdź firewall (porty 27015 TCP/UDP)
3. `sv_lan` musi być 0

**MySQL connection refused:**
1. Sprawdź host (cs2-db z kontenera, localhost z hosta)
2. Sprawdź user privileges
3. Test: `mysql -u user -p -h localhost`

## Zasoby

- CounterStrikeSharp: https://docs.cssharp.dev
- CSS Discord: https://discord.gg/cssharp
- AlliedMods Forum: https://forums.alliedmods.net
