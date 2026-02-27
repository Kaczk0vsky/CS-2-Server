using System.Drawing;
using CodMod.Events;
using CodMod.Models;
using CodMod.Services;
using CodMod.Extensions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using OnPlayerButtonsChanged = CounterStrikeSharp.API.Core.Listeners.OnPlayerButtonsChanged;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;

namespace CodMod;

public class CodMod : BasePlugin
{
    public override string ModuleName => "Cod Mod";

    // remember which players have already been shown the initial class menu
    private readonly HashSet<ulong> _initialMenuShown = new();
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "Kaczk0vsky, RafGor";
    public override string ModuleDescription => "CoD Mod with integrated ranking system";

    // ─── Shared Player Data (used by both class logic and ranking) ──
    private readonly Dictionary<ulong, CodPlayer> _players = new();

    // ─── Services ──────────────────────────────────────────────────
    private RankService _rankService = null!;
    private HudService _hudService = null!;

    // ─── Event Handlers ────────────────────────────────────────────
    private KillEvents _killEvents = null!;
    private RoundEvents _roundEvents = null!;

    // ─── Class System (existing V1 logic) ──────────────────────────
    private readonly Dictionary<ulong, int> _jumpCount = new();
    private readonly HashSet<ulong> _rightClicking = new(); // players currently holding right‑click
    private readonly Dictionary<ulong, CodMenu> _activeMenus = new();

    // Available class names (kept simple — class models managed elsewhere)
    private static readonly string[] ClassNames = { "Snajper", "Komandos", "Strzelec wyborowy", "Ninja" };

    // Define allowed weapons for each class
    private static readonly Dictionary<string, HashSet<string>> ClassWeapons = new()
    {
        { "None", new() { "weapon_hkp2000", "weapon_usp_silencer", "weapon_glock", "weapon_knife", "weapon_knife_t" } },
        { "Snajper", new() { "weapon_awp", "weapon_deagle", "weapon_knife", "weapon_knife_t" } },
        { "Komandos", new() { "weapon_deagle", "weapon_knife", "weapon_knife_t" } },
        { "Strzelec wyborowy", new() { "weapon_ak47", "weapon_fiveseven", "weapon_knife", "weapon_knife_t" } },
        { "Ninja", new() { "weapon_knife", "weapon_knife_t" } }
    };

    public override void Load(bool hotReload)
    {
        Console.WriteLine($"[CodMod] Loading plugin {ModuleVersion}...");

        ApplyServerSettings();

        // Re-apply on every map start so gamemode configs don't override our settings
        RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            // Delay to run after gamemode config files finish executing
            AddTimer(1.0f, ApplyServerSettings);
        });

        // Initialize services
        _rankService = new RankService(_players);
        _hudService = new HudService(_rankService);
        _killEvents = new KillEvents(_rankService, _hudService);
        _roundEvents = new RoundEvents(_rankService, _hudService);

        RegisterClassCommands();
        RegisterRankCommands();
        RegisterMenuCommands();
        RegisterCommandsListeners();
        RegisterGameEvents();
        RegisterTimers();

        Console.WriteLine("[CodMod] Plugin loaded!");
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMANDS — Class Selection
    // ═══════════════════════════════════════════════════════════════

    private void RegisterClassCommands()
    {
        AddCommand("klasa", "Wybierz klasę", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            OpenClassMenu(player);
        });
    }

    private void RegisterRankCommands()
    {
        AddCommand("css_rank", "Pokaż swój rank", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            ShowRankInfo(player);
        });

        AddCommand("css_myrank", "Pokaż swój rank", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            ShowRankInfo(player);
        });

        AddCommand("css_codstats", "Pokaż statystyki COD", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            _hudService.ShowPlayerStats(player);
        });

        AddCommand("css_top", "Top 10 graczy", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            ShowTopPlayers(player);
        });

        AddCommand("css_ranks", "Lista wszystkich rang", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            ShowAllRanks(player);
        });
    }

    private void ShowRankInfo(CCSPlayerController player)
    {
        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (codPlayer == null)
        {
            player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Brak danych!");
            return;
        }

        var rank = _rankService.GetGlobalRank(codPlayer.GlobalPoints);
        var nextRank = _rankService.GetNextRank(codPlayer.GlobalPoints);

        player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} === Twój Rank ===");
        player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Ranga: {RankService.GetRankColorCode(rank)}{rank.Name} {rank.Tag}");
        player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Punkty: {ChatColors.Blue}{codPlayer.GlobalPoints}");

        if (codPlayer.SelectedClassName != null)
        {
            var progress = codPlayer.GetActiveClassProgress();
            if (progress != null)
            {
                int xpPct = RankService.GetXpPercentage(progress);
                player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Klasa: {ChatColors.Yellow}{codPlayer.SelectedClassName}{ChatColors.Default} (Lvl {ChatColors.Green}{progress.Level}{ChatColors.Default}) — {xpPct}% XP");
            }
        }

        if (nextRank != null)
        {
            int needed = nextRank.Points - codPlayer.GlobalPoints;
            player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Do {nextRank.Name}: {ChatColors.Yellow}{needed} punktów");
        }
        else
        {
            player.PrintToChat($" {ChatColors.Gold}[COD MOD]{ChatColors.Default} Maksymalna ranga!");
        }
    }

    private void ShowTopPlayers(CCSPlayerController player)
    {
        var top = _players.Values.OrderByDescending(p => p.GlobalPoints).Take(10).ToList();

        player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} === TOP 10 ===");

        for (int i = 0; i < top.Count; i++)
        {
            var p = top[i];
            var rank = _rankService.GetGlobalRank(p.GlobalPoints);
            player.PrintToChat(
                $" {ChatColors.Yellow}#{i + 1}{ChatColors.Default} {p.Name} - " +
                $"{RankService.GetRankColorCode(rank)}{rank.Tag}{ChatColors.Default} " +
                $"{ChatColors.Blue}{p.GlobalPoints} pts");
        }
    }

    private void ShowAllRanks(CCSPlayerController player)
    {
        player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} === Lista Rang ===");

        foreach (var rank in RankService.Ranks.Where(r => r.Points >= 0))
        {
            player.PrintToChat(
                $" {RankService.GetRankColorCode(rank)}{rank.Tag} {rank.Name}{ChatColors.Default} - {rank.Points}+ pkt");
        }
    }

    // ═════════════���═════════════════════════════════════════════════
    // COMMANDS — Menu Navigation
    // ═══════════════════════════════════════════════════════════════

    private void RegisterMenuCommands()
    {
        AddCommand("menu_up", "Menu navigate up", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;
            menu.PreviousOption(player);
        });

        AddCommand("menu_down", "Menu navigate down", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;
            menu.NextOption(player);
        });

        AddCommand("menu_select", "Menu select current", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;
            menu.SelectCurrent(player);
            _activeMenus.Remove(player.SteamID);
        });

        AddCommand("menu_cancel", "Menu cancel/close", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;
            menu.Close(player);
            _activeMenus.Remove(player.SteamID);
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMANDS LISTENERS
    // ═══════════════════════════════════════════════════════════════

    private void RegisterCommandsListeners()
    {
        AddCommandListener("buy", (_, _) => HookResult.Stop);
        AddCommandListener("autobuy", (_, _) => HookResult.Stop);
        AddCommandListener("rebuy", (_, _) => HookResult.Stop);
    }

    // ═══════════════════════════════════════════════════════════════
    // GAME EVENTS
    // ═══════════════════════════════════════════════════════════════

    private void RegisterGameEvents()
    {
        // --- Ranking events (delegated to event classes) ---
        RegisterEventHandler<EventPlayerDeath>(_killEvents.OnPlayerDeath);
        RegisterEventHandler<EventRoundStart>(_roundEvents.OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(_roundEvents.OnRoundEnd);
        RegisterEventHandler<EventRoundMvp>(_roundEvents.OnRoundMvp);
        RegisterEventHandler<EventBombPlanted>(_roundEvents.OnBombPlanted);
        RegisterEventHandler<EventBombDefused>(_roundEvents.OnBombDefused);
        RegisterEventHandler<EventBombExploded>(_roundEvents.OnBombExploded);
        RegisterEventHandler<EventBombPickup>(_roundEvents.OnBombPickup);
        RegisterEventHandler<EventBombDropped>(_roundEvents.OnBombDropped);

        RegisterEventHandler<EventBuymenuOpen>((@event, info) =>
        {
            return HookResult.Stop;
        });

        // --- Player connect/disconnect ---
        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

            var codPlayer = _rankService.GetOrCreatePlayer(player.SteamID, player.PlayerName);

            Server.NextFrame(() =>
            {
                if (!player.IsValid) return;
                var rank = _rankService.GetGlobalRank(codPlayer.GlobalPoints);

                player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Witaj {ChatColors.Yellow}{player.PlayerName}{ChatColors.Default}!");
                player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Ranga: {RankService.GetRankColorCode(rank)}{rank.Name}{ChatColors.Default} | Punkty: {ChatColors.Blue}{codPlayer.GlobalPoints}");

                if (codPlayer.SelectedClassName == null)
                    player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Wpisz {ChatColors.Yellow}/klasa{ChatColors.Default} aby wybrać klasę!");
                else
                    player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Klasa: {ChatColors.Green}{codPlayer.SelectedClassName}");
            });

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            // Remove initial menu flag so the player will be prompted again if they reconnect
            var player = @event.Userid;
            if (player != null && player.IsValid)
            {
                _initialMenuShown.Remove(player.SteamID);
                _rightClicking.Remove(player.SteamID);
            }

            // Keep data in memory for reconnection. DB save would go here.
            return HookResult.Continue;
        });

        // --- Player spawn (class logic) ---
        RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid) return HookResult.Continue;

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer == null) return HookResult.Continue;

            // For non-bot players: show class menu on first team join
            if (!player.IsBot)
            {
                // when player first enters either team, show class menu once
                if (!_initialMenuShown.Contains(player.SteamID) &&
                    codPlayer.SelectedClassName == null &&
                    (player.Team == CsTeam.CounterTerrorist || player.Team == CsTeam.Terrorist))
                {
                    _initialMenuShown.Add(player.SteamID);
                    OpenClassMenu(player);
                    return HookResult.Continue;
                }
            }

            // Activate pending class
            if (codPlayer.PendingClassName != null)
            {
                codPlayer.SelectedClassName = codPlayer.PendingClassName;
                codPlayer.PendingClassName = null;

                player.PrintToChat(
                    $"{ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                    $"Grasz jako: {ChatColors.Blue}{codPlayer.SelectedClassName}");
            }

            // If no class selected yet and no pending selection, skip equipment
            // (player is still choosing from the class menu)
            if (codPlayer.SelectedClassName == null)
            {
                return HookResult.Continue;
            }

            _jumpCount[player.SteamID] = 0;

            // Delay to prevent engine from overriding stats
            AddTimer(0.2f, () =>
            {
                if (!player.IsValid || !player.PawnIsAlive) return;
                GiveClassEquipment(player, codPlayer.SelectedClassName);
                ApplyClassStats(player, codPlayer.SelectedClassName);
            });

            return HookResult.Continue;
        });

        // --- Player hurt (class abilities: knife 1-hit) ---
        RegisterEventHandler<EventPlayerHurt>((@event, info) =>
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;

            if (attacker == null || victim == null) return HookResult.Continue;
            if (!attacker.PawnIsAlive || !victim.PawnIsAlive) return HookResult.Continue;

            var codPlayer = _rankService.GetPlayer(attacker.SteamID);
            if (codPlayer == null) return HookResult.Continue;

            var victimPawn = victim.PlayerPawn.Value;
            if (victimPawn == null)
                return HookResult.Continue;

            int damage = @event.DmgHealth;
            int currentHp = victimPawn.Health;

            if (currentHp - damage <= 0)
            {
                bool victimHadC4 = HasC4(victim);
                victim.RemoveWeapons();
                if (victimHadC4)
                {
                    victim.GiveNamedItem("weapon_c4");
                }
            }

            if (codPlayer.SelectedClassName != "Ninja" && codPlayer.SelectedClassName != "Komandos")
                return HookResult.Continue;

            if (@event.Weapon != null && @event.Weapon.Contains("knife"))
            {
                bool right = false;
                try
                {
                    right = (attacker.Buttons & PlayerButtons.Attack2) != 0;
                }
                catch (ArgumentException) { }
                if (!right)
                    right = _rightClicking.Contains(attacker.SteamID);
                if (!right)
                    return HookResult.Continue; // only kill when right-click

                victim.PlayerPawn.Value!.Health = 0;
            }

            return HookResult.Continue;
        });

        // --- Button listener for menu navigation and class abilities ---
        RegisterListener<OnPlayerButtonsChanged>((player, pressed, released) =>
        {
            if (player == null || !player.IsValid) return;

            // track right‑click state for knife instant‑kill
            try
            {
                if ((pressed & PlayerButtons.Attack2) != 0) _rightClicking.Add(player.SteamID);
                if ((released & PlayerButtons.Attack2) != 0) _rightClicking.Remove(player.SteamID);
            }
            catch (ArgumentException) { }

            // navigate active COD menu if one is open
            if (_activeMenus.TryGetValue(player.SteamID, out var menu) && menu.IsOpen)
            {
                if ((pressed & PlayerButtons.Forward) != 0) menu.PreviousOption(player);
                if ((pressed & PlayerButtons.Back) != 0) menu.NextOption(player);
                if ((pressed & PlayerButtons.Use) != 0)
                {
                    menu.SelectCurrent(player);
                    _activeMenus.Remove(player.SteamID);
                }
                if ((pressed & PlayerButtons.Scoreboard) != 0)
                {
                    menu.Close(player);
                    _activeMenus.Remove(player.SteamID);
                }
            }

            // --- double jump for Komandos ---
            const PlayerButtons JumpMask = (PlayerButtons)2; // IN_JUMP bit (source engine)
            if (player.PawnIsAlive)
            {
                var codPlayer = _rankService.GetPlayer(player.SteamID);
                if (codPlayer != null && codPlayer.SelectedClassName == "Komandos")
                {
                    var pawn = player.PlayerPawn.Value;
                    if (pawn != null)
                    {
                        bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;

                        if (onGround)
                        {
                            _jumpCount[player.SteamID] = 0;
                        }

                        // when jump key is pressed while airborne, give second boost
                        if ((pressed & JumpMask) != 0 && !onGround)
                        {
                            int count = 0;
                            _jumpCount.TryGetValue(player.SteamID, out count);
                            if (count < 2)
                            {
                                _jumpCount[player.SteamID] = count + 1;
                                var vel = pawn.AbsVelocity;
                                pawn.Teleport(null, null, new Vector(vel.X, vel.Y, 300));
                            }
                        }
                    }
                }
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // TIMERS
    // ═══════════════════════════════════════════════════════════════

    private void RegisterTimers()
    {
        // keep ninja invisibility updated
        AddTimer(0.2f, CheckNinjaInvisibility, TimerFlags.REPEAT);

        // reset jump counters when players land on ground (safety net)
        AddTimer(0.2f, () =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player == null || !player.IsValid || !player.PawnIsAlive) continue;
                var pawn = player.PlayerPawn.Value;
                if (pawn == null) continue;
                bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;
                if (onGround)
                    _jumpCount.Remove(player.SteamID);
            }
        }, TimerFlags.REPEAT);
    }

    // ═══════════════════════════════════════════════════════════════
    // CLASS SYSTEM (preserved from V1)
    // ═══════════════════════════════════════════════════════════════

    private void OpenClassMenu(CCSPlayerController player)
    {
        var menu = new CodMenu("Wybierz klasę");
        foreach (var className in ClassNames)
        {
            var cn = className; // closure capture
            menu.AddOption(cn, p => SetClass(p, cn));
        }

        menu.Open(player);
        _activeMenus[player.SteamID] = menu;

        AddTimer(0.02f, () =>
        {
            if (!player.IsValid) return;
            if (_activeMenus.TryGetValue(player.SteamID, out var m) && m.IsOpen)
                m.Refresh(player);
        }, TimerFlags.REPEAT);
    }

    private void SetClass(CCSPlayerController player, string className)
    {
        var codPlayer = _rankService.GetOrCreatePlayer(player.SteamID, player.PlayerName);
        bool wasNew = codPlayer.SelectedClassName == null;

        codPlayer.PendingClassName = className;

        // Ensure class progress exists
        codPlayer.GetClassProgress(className);

        var progress = codPlayer.GetClassProgress(className);
        player.PrintToChat(
            $"{ChatColors.Green}[COD MOD]{ChatColors.Default} " +
            $"Wybrano: {ChatColors.Blue}{className}{ChatColors.Default} " +
            $"(Lvl {ChatColors.Green}{progress.Level}{ChatColors.Default})");

        // if this is the very first selection and the pawn is alive, apply immediately
        if (wasNew && player.PawnIsAlive)
        {
            codPlayer.SelectedClassName = className;
            codPlayer.PendingClassName = null;
            GiveClassEquipment(player, className);
            ApplyClassStats(player, className);
        }
    }

    /// <summary>
    /// Checks whether the player currently has C4 in their inventory.
    /// </summary>
    private static bool HasC4(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn?.WeaponServices == null) return false;

        foreach (var weapon in pawn.WeaponServices.MyWeapons)
        {
            if (weapon.Value?.DesignerName == "weapon_c4")
                return true;
        }
        return false;
    }

    private void GiveClassEquipment(CCSPlayerController player, string className)
    {
        if (!player.IsValid || !player.PawnIsAlive) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        bool hadC4 = HasC4(player);
        player.RemoveWeapons();
        pawn.Render = Color.FromArgb(255, 255, 255, 255);

        switch (className)
        {
            case "None":
                // Default weapons based on team
                if (player.Team == CsTeam.CounterTerrorist)
                {
                    player.GiveNamedItem("weapon_usp_silencer");
                }
                else if (player.Team == CsTeam.Terrorist)
                {
                    player.GiveNamedItem("weapon_glock");
                }
                break;
            case "Snajper":
                player.GiveNamedItem("weapon_awp");
                player.GiveNamedItem("weapon_deagle");
                break;
            case "Komandos":
                player.GiveNamedItem("weapon_deagle");
                break;
            case "Strzelec wyborowy":
                player.GiveNamedItem("weapon_ak47");
                player.GiveNamedItem("weapon_fiveseven");
                break;
            case "Ninja":
                pawn.Render = Color.FromArgb(50, 255, 255, 255);
                break;
        }

        player.GiveNamedItem("weapon_knife");

        if (hadC4)
        {
            player.GiveNamedItem("weapon_c4");
        }
    }

    private void ApplyClassStats(CCSPlayerController player, string className)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        // Reset to defaults before applying class modifiers
        player.SetHp(100);
        pawn.MaxHealth = 100;
        player.SetSpeed(1.0f);
        player.SetGravity(1.0f);

        switch (className)
        {
            case "None":
                // None class
                break;
            case "Snajper":
                player.SetHp(110);
                break;
            case "Komandos":
                player.SetHp(105);
                player.SetSpeed(1.4f);
                break;
            case "Strzelec wyborowy":
                player.SetHp(200);
                player.SetSpeed(0.6f);
                player.SetGravity(1.5f);
                break;
            case "Ninja":
                player.SetHp(50);
                pawn.MaxHealth = 50;
                player.SetGravity(0.25f);
                player.SetSpeed(1.1f);
                break;
        }
    }

    private void ApplyServerSettings()
    {
        // Prevent weapon dropping
        Server.ExecuteCommand("mp_drop_knife_enable 0");
        Server.ExecuteCommand("mp_death_drop_gun 0");
        Server.ExecuteCommand("mp_death_drop_grenade 0");
        Server.ExecuteCommand("mp_death_drop_taser 0");
        Server.ExecuteCommand("mp_death_drop_healthshot 0");

        // Prevent buy menu / in-game shop
        Server.ExecuteCommand("mp_buy_anywhere 0");
        Server.ExecuteCommand("mp_buy_during_immunity 0");
        Server.ExecuteCommand("sv_buy_status_override 1");

        // Add armor + kevlar as default
        Server.ExecuteCommand("mp_max_armor 2");
    }

    private void CheckNinjaInvisibility()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) continue;

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer == null || codPlayer.SelectedClassName != "Ninja") continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null) continue;

            if (pawn.AbsVelocity.Length() < 5f)
                pawn.Render = Color.FromArgb(0, 255, 255, 255);
            else
                pawn.Render = Color.FromArgb(50, 255, 255, 255);
        }
    }
}