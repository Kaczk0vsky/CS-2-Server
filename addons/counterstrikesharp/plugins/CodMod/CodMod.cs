using System.Drawing;
using CodMod.Events;
using CodMod.Menus;
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
    private StatsMenu _overviewWindow = null!;
    private LevelUpMenu _levelUpMenu = null!;

    // ─── Class System (existing V1 logic) ──────────────────────────
    private readonly Dictionary<ulong, int> _jumpCount = new();
    private readonly HashSet<ulong> _rightClicking = new(); // players currently holding right‑click
    private readonly Dictionary<ulong, CodMenu> _activeMenus = new();
    private readonly HashSet<ulong> _pendingLevelUpMenus = new();
    private readonly HashSet<ulong> _pendingStatResets = new();

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
        _levelUpMenu = new LevelUpMenu(_rankService);
        _killEvents = new KillEvents(_rankService, _hudService, QueueLevelUpMenu);
        _roundEvents = new RoundEvents(_rankService, _hudService, QueueLevelUpMenu);
        _roundEvents.SetRoundStartPlayerHandler(TryShowQueuedLevelUpMenuAtRoundStart);
        _overviewWindow = new StatsMenu(this, _rankService);

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
        AddCommand("reset", "Resetuj statystyki na początku następnej rundy", (player, info) =>
        {
            if (player == null || !player.IsValid) return;

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            var progress = codPlayer?.GetActiveClassProgress();
            if (codPlayer?.SelectedClassName == null || progress == null)
            {
                player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Najpierw wybierz klasę.");
                return;
            }

            if (progress.Level <= 1)
            {
                player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Nie możesz resetować statystyk na 1 poziomie.");
                return;
            }

            _pendingStatResets.Add(player.SteamID);
            player.PrintToChat($" {ChatColors.Gold}[COD MOD]{ChatColors.Default} Statystyki zostaną zresetowane na początku następnej rundy.");
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

        AddCommand("perk", "Pokaż aktualny perk", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            ShowPerkInfo(player);
        });

        AddCommand("item", "Pokaż aktualny perk", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            ShowPerkInfo(player);
        });

        AddCommand("drop", "Usuń aktualny perk", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            DropCurrentPerk(player);
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

    private void ShowPerkInfo(CCSPlayerController player)
    {
        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (codPlayer == null)
        {
            player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Brak danych!");
            return;
        }

        var perk = Perks.FindByName(codPlayer.ActivePerkName);
        if (perk == null)
        {
            player.PrintToChat($" {ChatColors.Gold}[COD MOD]{ChatColors.Default} Nie masz aktualnie perka.");
            return;
        }

        player.PrintToChat(
            $" {ChatColors.Green}[COD MOD]{ChatColors.Default} Perk: {ChatColors.Green}{perk.Name}{ChatColors.Default} - {perk.Description}");
    }

    private void DropCurrentPerk(CCSPlayerController player)
    {
        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (codPlayer == null)
        {
            player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Brak danych!");
            return;
        }

        if (string.IsNullOrWhiteSpace(codPlayer.ActivePerkName))
        {
            player.PrintToChat($" {ChatColors.Gold}[COD MOD]{ChatColors.Default} Nie masz aktualnie perka.");
            return;
        }

        string droppedPerk = codPlayer.ActivePerkName;
        codPlayer.ActivePerkName = null;

        if (player.PawnIsAlive && codPlayer.SelectedClassName != null)
        {
            GiveClassEquipment(player, codPlayer.SelectedClassName);
            ApplyClassStats(player, codPlayer.SelectedClassName);
        }

        player.PrintToChat(
            $" {ChatColors.Green}[COD MOD]{ChatColors.Default} Usunięto perk: {ChatColors.Red}{droppedPerk}{ChatColors.Default}.");
    }

    // ══════════════════════════════════════════════════════════════
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
            if (_activeMenus.TryGetValue(player.SteamID, out var currentMenu) && ReferenceEquals(currentMenu, menu))
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
                _overviewWindow.CloseWindow(player);
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
                codPlayer.SelectedClassName = CodClasses.None;
            }

            _jumpCount[player.SteamID] = 0;

            // Delay to prevent engine from overriding stats
            AddTimer(0.2f, () =>
            {
                if (!player.IsValid || !player.PawnIsAlive) return;
                GiveClassEquipment(player, codPlayer.SelectedClassName);
                ApplyClassStats(player, codPlayer.SelectedClassName);
            });

            _overviewWindow.ShowOverview(player);

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

            var victimCodPlayer = _rankService.GetPlayer(victim.SteamID);
            var victimProgress = victimCodPlayer?.GetActiveClassProgress();
            if (victimProgress != null && victimProgress.EndurancePoints > 0 && currentHp > 0)
            {
                int preventedDamage = victimProgress.EndurancePoints / 10;
                if (preventedDamage > 0)
                {
                    int restoredHp = Math.Min(victimPawn.MaxHealth, currentHp + preventedDamage);
                    victim.SetHp(restoredHp);
                }
            }

            if (currentHp - damage <= 0)
            {
                bool victimHadC4 = HasC4(victim);
                victim.RemoveWeapons();
                if (victimHadC4)
                {
                    victim.GiveNamedItem("weapon_c4");
                }
            }

            bool hasHeInstakillPerk = Perks.HasHeGrenadeInstantKill(codPlayer.ActivePerkName);
            bool isHeDamage = !string.IsNullOrEmpty(@event.Weapon) &&
                              @event.Weapon.Contains("hegrenade", StringComparison.OrdinalIgnoreCase);
            bool isEnemy = attacker.Team != victim.Team;

            if (hasHeInstakillPerk && isHeDamage && isEnemy)
            {
                victimPawn.Health = 0;
                return HookResult.Continue;
            }

            ClassAbilities.TryApplyKnifeInstakill(
                attacker,
                victim,
                codPlayer.SelectedClassName,
                @event.Weapon,
                _rightClicking);

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
                    if (_activeMenus.TryGetValue(player.SteamID, out var currentMenu) && ReferenceEquals(currentMenu, menu))
                        _activeMenus.Remove(player.SteamID);
                }
                if ((pressed & PlayerButtons.Scoreboard) != 0)
                {
                    menu.Close(player);
                    _activeMenus.Remove(player.SteamID);
                }
            }

            // --- double jump ability ---
            ClassAbilities.HandleDoubleJump(player, pressed, _rankService, _jumpCount);
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
        _overviewWindow.HideOverview(player);

        var menu = new CodMenu("Wybierz klasę");
        foreach (var className in CodClasses.SelectableClassNames)
        {
            var cn = className; // closure capture
            menu.AddOption(cn, p => SetClass(p, cn));
        }

        menu.Open(player);
        _activeMenus[player.SteamID] = menu;

        AddTimer(0.01f, () =>
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

        _overviewWindow.ShowOverview(player);
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

        var classDefinition = CodClasses.Get(className);

        player.RemoveWeapons();
        pawn.Render = Color.FromArgb(classDefinition.MovingAlpha, 255, 255, 255);

        if (classDefinition.UsesTeamDefaultPistol)
        {
            if (player.Team == CsTeam.CounterTerrorist)
                player.GiveNamedItem("weapon_usp_silencer");
            else if (player.Team == CsTeam.Terrorist)
                player.GiveNamedItem("weapon_glock");
        }

        foreach (var weapon in classDefinition.Weapons)
        {
            player.GiveNamedItem(weapon);
        }

        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (Perks.HasHeGrenadeInstantKill(codPlayer?.ActivePerkName))
        {
            player.GiveNamedItem("weapon_hegrenade");
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

        var classDefinition = CodClasses.Get(className);
        int baseHp = classDefinition.BaseHealth;
        float baseSpeed = classDefinition.BaseSpeed;
        float baseGravity = classDefinition.BaseGravity;

        var codPlayer = _rankService.GetPlayer(player.SteamID);
        var progress = codPlayer?.GetActiveClassProgress();
        float perkSpeedBonus = Perks.GetSpeedBonus(codPlayer?.ActivePerkName);
        int perkHealthBonus = Perks.GetHealthBonus(codPlayer?.ActivePerkName);

        int bonusHp = (progress?.HealthPoints ?? 0) * 2;
        float bonusSpeed = (progress?.SpeedPoints ?? 0) * 0.005f;

        int finalHp = baseHp + bonusHp + perkHealthBonus;
        float finalSpeed = Math.Clamp(baseSpeed + bonusSpeed + perkSpeedBonus, 0.1f, 2.5f);

        player.SetHp(finalHp);
        pawn.MaxHealth = finalHp;
        player.SetSpeed(finalSpeed);
        player.SetGravity(baseGravity);
    }

    private void QueueLevelUpMenu(CCSPlayerController player)
    {
        if (!player.IsValid || player.IsBot) return;

        var codPlayer = _rankService.GetPlayer(player.SteamID);
        var progress = codPlayer?.GetActiveClassProgress();
        if (progress == null || progress.AvailableStatPoints <= 0) return;

        _pendingLevelUpMenus.Add(player.SteamID);
    }

    private void TryShowQueuedLevelUpMenuAtRoundStart(CCSPlayerController player)
    {
        if (!player.IsValid || player.IsBot) return;

        if (_pendingStatResets.Contains(player.SteamID))
        {
            _pendingStatResets.Remove(player.SteamID);

            var codPlayer = _rankService.GetPlayer(player.SteamID);
            var progress = codPlayer?.GetActiveClassProgress();
            if (progress != null)
            {
                if (progress.Level <= 1)
                {
                    player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Reset statystyk wymaga minimum 2 poziomu.");
                    return;
                }

                progress.HealthPoints = 0;
                progress.SpeedPoints = 0;
                progress.IntelligencePoints = 0;
                progress.EndurancePoints = 0;
                progress.AvailableStatPoints = Math.Max(0, progress.Level - 1) * 2;

                if (codPlayer?.SelectedClassName != null && player.PawnIsAlive)
                    ApplyClassStats(player, codPlayer.SelectedClassName);

                OpenLevelUpMenu(player, true);
                return;
            }
        }

        if (!_pendingLevelUpMenus.Contains(player.SteamID)) return;

        if (OpenLevelUpMenu(player, false))
            _pendingLevelUpMenus.Remove(player.SteamID);
    }

    private bool OpenLevelUpMenu(CCSPlayerController player, bool allowWithoutPoints)
    {
        if (!player.IsValid || player.IsBot) return false;

        var codPlayer = _rankService.GetPlayer(player.SteamID);
        var progress = codPlayer?.GetActiveClassProgress();
        if (progress == null)
        {
            player.PrintToChat($" {ChatColors.Red}[COD MOD]{ChatColors.Default} Najpierw wybierz klasę.");
            return false;
        }

        if (!allowWithoutPoints && progress.AvailableStatPoints <= 0) return false;

        if (_activeMenus.TryGetValue(player.SteamID, out var existing) && existing.IsOpen)
        {
            existing.Close(player);
            _activeMenus.Remove(player.SteamID);
        }

        _overviewWindow.HideOverview(player);

        _levelUpMenu.Open(
            player,
            (p, menu) => _activeMenus[p.SteamID] = menu,
            p =>
            {
                var pData = _rankService.GetPlayer(p.SteamID);
                if (pData?.SelectedClassName != null && p.PawnIsAlive)
                    ApplyClassStats(p, pData.SelectedClassName);
            },
            p =>
            {
                _activeMenus.Remove(p.SteamID);
                _overviewWindow.ShowOverview(p);
            });

        return true;
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
            if (codPlayer == null) continue;

            ClassAbilities.ApplyStealthInvisibility(player, codPlayer.SelectedClassName);
        }
    }
}