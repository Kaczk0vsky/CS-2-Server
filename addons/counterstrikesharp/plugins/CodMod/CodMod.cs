using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CodMod.Events;
using CodMod.Models;
using CodMod.Services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using OnPlayerButtonsChanged = CounterStrikeSharp.API.Core.Listeners.OnPlayerButtonsChanged;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;

namespace CodMod;

public class CodMod : BasePlugin
{
    public override string ModuleName => "Cod Mod";
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
    private readonly Dictionary<ulong, CodMenu> _activeMenus = new();

    // Available class names (kept simple — class models managed elsewhere)
    private static readonly string[] ClassNames = { "Snajper", "Komandos", "Strzelec wyborowy", "Ninja" };

    public override void Load(bool hotReload)
    {
        Console.WriteLine("[CodMod] Loading plugin v2.0...");

        // Initialize services
        _rankService = new RankService(_players);
        _hudService = new HudService(_rankService);
        _killEvents = new KillEvents(_rankService, _hudService);
        _roundEvents = new RoundEvents(_rankService, _hudService);

        RegisterClassCommands();
        RegisterRankCommands();
        RegisterMenuCommands();
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

    // ═══════════════════════════════════════════════════════════════
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
                    player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Wpisz {ChatColors.Yellow}klasa{ChatColors.Default} aby wybrać klasę!");
                else
                    player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Klasa: {ChatColors.Green}{codPlayer.SelectedClassName}");
            });

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            // Keep data in memory for reconnection. DB save would go here.
            return HookResult.Continue;
        });

        // --- Player spawn (class logic) ---
        RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer == null) return HookResult.Continue;

            // Activate pending class
            if (codPlayer.PendingClassName != null)
            {
                codPlayer.SelectedClassName = codPlayer.PendingClassName;
                codPlayer.PendingClassName = null;

                player.PrintToChat(
                    $"{ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                    $"Grasz jako: {ChatColors.Blue}{codPlayer.SelectedClassName}");
            }

            if (codPlayer.SelectedClassName == null) return HookResult.Continue;

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

            if (codPlayer.SelectedClassName != "Ninja" && codPlayer.SelectedClassName != "Komandos")
                return HookResult.Continue;

            if (@event.Weapon != null && @event.Weapon.Contains("knife"))
            {
                victim.PlayerPawn.Value!.Health = 0;
            }

            return HookResult.Continue;
        });

        // --- Double jump (Komandos) ---
        RegisterEventHandler<EventPlayerJump>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || !player.PawnIsAlive) return HookResult.Continue;

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer == null || codPlayer.SelectedClassName != "Komandos") return HookResult.Continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null) return HookResult.Continue;

            bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;

            if (!_jumpCount.ContainsKey(player.SteamID))
                _jumpCount[player.SteamID] = 0;

            if (onGround)
            {
                _jumpCount[player.SteamID] = 1;
                return HookResult.Continue;
            }

            if (_jumpCount[player.SteamID] >= 2) return HookResult.Continue;

            _jumpCount[player.SteamID]++;
            var currentVelocity = pawn.AbsVelocity;
            pawn.Teleport(null, null, new Vector(currentVelocity.X, currentVelocity.Y, 300));

            return HookResult.Continue;
        });

        // --- Map start ---
        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            _rankService.ResetGameRules();
        });

        // --- Button listener for menu navigation ---
        RegisterListener<OnPlayerButtonsChanged>((player, pressed, released) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;

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
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // TIMERS
    // ═══════════════════════════════════════════════════════════════

    private void RegisterTimers()
    {
        AddTimer(0.2f, CheckNinjaInvisibility, TimerFlags.REPEAT);
        AddTimer(0.5f, MaintainClassGravity, TimerFlags.REPEAT);
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
        codPlayer.PendingClassName = className;

        // Ensure class progress exists
        codPlayer.GetClassProgress(className);

        var progress = codPlayer.GetClassProgress(className);
        player.PrintToChat(
            $"{ChatColors.Green}[COD MOD]{ChatColors.Default} " +
            $"Wybrano: {ChatColors.Blue}{className}{ChatColors.Default} " +
            $"(Lvl {ChatColors.Green}{progress.Level}{ChatColors.Default})");
    }

    private void GiveClassEquipment(CCSPlayerController player, string className)
    {
        if (!player.IsValid || !player.PawnIsAlive) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        player.RemoveWeapons();
        pawn.Render = Color.FromArgb(255, 255, 255, 255);

        switch (className)
        {
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
    }

    private void ApplyClassStats(CCSPlayerController player, string className)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        pawn.GravityScale = 1.0f;
        pawn.VelocityModifier = 1.0f;
        pawn.Health = 100;
        pawn.MaxHealth = 100;
        pawn.ArmorValue = 0;

        switch (className)
        {
            case "Snajper":
                pawn.Health = pawn.MaxHealth = 110;
                pawn.ArmorValue = 100;
                break;
            case "Komandos":
                pawn.Health = pawn.MaxHealth = 105;
                pawn.ArmorValue = 100;
                pawn.VelocityModifier = 1.2f;
                break;
            case "Strzelec wyborowy":
                pawn.Health = pawn.MaxHealth = 200;
                pawn.ArmorValue = 100;
                pawn.VelocityModifier = 0.5f;
                pawn.GravityScale = 2f;
                break;
            case "Ninja":
                pawn.Health = pawn.MaxHealth = 50;
                pawn.ArmorValue = 100;
                pawn.GravityScale = 0.1f;
                pawn.VelocityModifier = 1.25f;
                break;
        }

        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_flVelocityModifier");
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_flGravityScale");
        pawn.Teleport(pawn.AbsOrigin, pawn.AbsRotation, pawn.AbsVelocity);

        AddTimer(0.05f, () =>
        {
            if (!player.IsValid || !player.PawnIsAlive) return;
            var updatedPawn = player.PlayerPawn.Value;
            if (updatedPawn != null)
                Utilities.SetStateChanged(updatedPawn, "CBaseEntity", "m_flGravityScale");
        });
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

    private void MaintainClassGravity()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) continue;

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer == null || codPlayer.SelectedClassName == null) continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null) continue;

            if (codPlayer.SelectedClassName == "Strzelec wyborowy")
                pawn.GravityScale = 2f;
            else if (codPlayer.SelectedClassName == "Ninja")
                pawn.GravityScale = 0.1f;
            else
                pawn.GravityScale = 1.0f;
        }
    }
}