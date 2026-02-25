using CodMod.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Services;

/// <summary>
/// Unified ranking service. Manages:
/// - Global server rank (points-based, Silver I → Global Elite)
/// - Per-class XP/level progression (level 1–55)
/// 
/// Designed so that a future DB layer can simply read/write CodPlayer fields
/// without changing this service's logic.
/// </summary>
public class RankService
{
    private readonly Dictionary<ulong, CodPlayer> _players;

    #region Point Values (K4-System defaults from SimpleRank)
    public const int POINTS_KILL = 8;
    public const int POINTS_DEATH = -5;
    public const int POINTS_HEADSHOT = 5;
    public const int POINTS_PENETRATED = 3;
    public const int POINTS_NOSCOPE = 15;
    public const int POINTS_THRUSMOKE = 15;
    public const int POINTS_BLIND_KILL = 5;
    public const int POINTS_TEAMKILL = -10;
    public const int POINTS_SUICIDE = -5;
    public const int POINTS_ASSIST = 5;
    public const int POINTS_ASSIST_FLASH = 7;
    public const int POINTS_ROUND_WIN = 5;
    public const int POINTS_ROUND_LOSE = -2;
    public const int POINTS_MVP = 10;
    public const int POINTS_BOMB_PLANT = 10;
    public const int POINTS_BOMB_EXPLODED = 10;
    public const int POINTS_BOMB_DEFUSED = 10;
    public const int POINTS_BOMB_DEFUSED_OTHERS = 3;
    public const int POINTS_BOMB_PICKUP = 2;
    public const int POINTS_BOMB_DROP = -2;
    public const int POINTS_LONG_DISTANCE = 8;

    // Kill streaks
    public const int POINTS_DOUBLE_KILL = 5;
    public const int POINTS_TRIPLE_KILL = 10;
    public const int POINTS_DOMINATION = 15;
    public const int POINTS_RAMPAGE = 20;
    public const int POINTS_MEGA_KILL = 25;
    public const int POINTS_OWNAGE = 30;
    public const int POINTS_ULTRA_KILL = 35;
    public const int POINTS_KILLING_SPREE = 40;
    public const int POINTS_MONSTER_KILL = 45;
    public const int POINTS_UNSTOPPABLE = 50;
    public const int POINTS_GODLIKE = 60;

    // Special weapon kills
    public const int POINTS_KNIFE_KILL = 15;
    public const int POINTS_TASER_KILL = 20;
    public const int POINTS_GRENADE_KILL = 30;
    public const int POINTS_INFERNO_KILL = 30;
    public const int POINTS_IMPACT_KILL = 100;
    #endregion

    #region Settings
    public const int MAX_CLASS_LEVEL = 55;
    public const int MIN_PLAYERS = 1;
    public const bool WARMUP_POINTS = false;
    public const int SECONDS_BETWEEN_KILLS = 0; // 0 = disabled
    public const int LONG_DISTANCE = 30; // meters
    #endregion

    #region Rank Ladder
    public static readonly List<RankInfo> Ranks = new()
    {
        new RankInfo(-1,   "None",                          "[N]",    "default"),
        new RankInfo(100,  "Silver I",                      "[SI]",   "white"),
        new RankInfo(500,  "Silver II",                     "[SII]",  "white"),
        new RankInfo(900,  "Silver III",                    "[SIII]", "white"),
        new RankInfo(1300, "Silver IV",                     "[SIV]",  "white"),
        new RankInfo(1700, "Silver Elite",                  "[SE]",   "white"),
        new RankInfo(2100, "Silver Elite Master",           "[SEM]",  "white"),
        new RankInfo(2600, "Gold Nova I",                   "[GNI]",  "gold"),
        new RankInfo(3100, "Gold Nova II",                  "[GNII]", "gold"),
        new RankInfo(3600, "Gold Nova III",                 "[GNIII]","gold"),
        new RankInfo(4100, "Gold Nova Master",              "[GNM]",  "gold"),
        new RankInfo(4700, "Master Guardian I",             "[MGI]",  "green"),
        new RankInfo(5300, "Master Guardian II",            "[MGII]", "green"),
        new RankInfo(5900, "Master Guardian Elite",         "[MGE]",  "green"),
        new RankInfo(6500, "Distinguished Master Guardian", "[DMG]",  "green"),
        new RankInfo(7200, "Legendary Eagle",               "[LE]",   "blue"),
        new RankInfo(7900, "Legendary Eagle Master",        "[LEM]",  "blue"),
        new RankInfo(8600, "Supreme Master First Class",    "[SMFC]", "purple"),
        new RankInfo(9300, "Global Elite",                  "[GE]",   "red"),
    };
    #endregion

    public RankService(Dictionary<ulong, CodPlayer> players)
    {
        _players = players;
    }

    // ─── Player Management ─────────────────────────────────────────

    public CodPlayer GetOrCreatePlayer(ulong steamId, string name)
    {
        if (!_players.TryGetValue(steamId, out var player))
        {
            player = new CodPlayer(steamId, name);
            _players[steamId] = player;
        }
        else
        {
            player.Name = name;
        }
        return player;
    }

    public CodPlayer? GetPlayer(ulong steamId)
    {
        return _players.TryGetValue(steamId, out var p) ? p : null;
    }

    // ─── Global Rank ───────────────────────────────────────────────

    /// <summary>
    /// Adds global points and class XP simultaneously from a single game event.
    /// Global points affect server rank (Silver → Global Elite).
    /// Class XP affects per-class level (1–55).
    /// Returns rankings change info + new point total for chat display.
    /// </summary>
    public (RankInfo oldRank, RankInfo newRank, bool classLeveledUp, int classNewLevel, int newGlobalPoints) AddPoints(
        CCSPlayerController player, int amount, string reason)
    {
        var codPlayer = GetPlayer(player.SteamID);
        if (codPlayer == null)
            return (Ranks[0], Ranks[0], false, 0, 0);

        // --- Global points ---
        var oldRank = GetGlobalRank(codPlayer.GlobalPoints);
        codPlayer.GlobalPoints += amount;
        if (codPlayer.GlobalPoints < 0) codPlayer.GlobalPoints = 0;
        var newRank = GetGlobalRank(codPlayer.GlobalPoints);

        // --- Class XP (only positive values contribute to class XP) ---
        bool classLeveledUp = false;
        int classNewLevel = 0;

        if (amount > 0 && codPlayer.SelectedClassName != null)
        {
            var progress = codPlayer.GetActiveClassProgress();
            if (progress != null && progress.Level < MAX_CLASS_LEVEL)
            {
                int oldLevel = progress.Level;
                progress.Xp += amount;

                // Level up loop
                while (progress.Level < MAX_CLASS_LEVEL && progress.Xp >= GetXpForNextLevel(progress.Level))
                {
                    progress.Xp -= GetXpForNextLevel(progress.Level);
                    progress.Level++;
                }

                if (progress.Level > oldLevel)
                {
                    classLeveledUp = true;
                    classNewLevel = progress.Level;
                }
            }
        }

        return (oldRank, newRank, classLeveledUp, classNewLevel, codPlayer.GlobalPoints);
    }

    public RankInfo GetGlobalRank(int points)
    {
        for (int i = Ranks.Count - 1; i >= 0; i--)
        {
            if (points >= Ranks[i].Points && Ranks[i].Points >= 0)
                return Ranks[i];
        }
        return Ranks[0];
    }

    public RankInfo? GetNextRank(int points)
    {
        var current = GetGlobalRank(points);
        var idx = Ranks.IndexOf(current);
        return idx < Ranks.Count - 1 ? Ranks[idx + 1] : null;
    }

    // ─── Class Level ───────────────────────────────────────────────

    /// <summary>
    /// XP required to go from 'currentLevel' to 'currentLevel+1'.
    /// Formula: 100 + (level * 20)
    /// </summary>
    public static int GetXpForNextLevel(int currentLevel)
    {
        return 100 + (currentLevel * 20);
    }

    /// <summary>
    /// Returns XP progress as percentage (0–100) toward next class level.
    /// </summary>
    public static int GetXpPercentage(ClassProgress progress)
    {
        if (progress.Level >= MAX_CLASS_LEVEL) return 100;
        int needed = GetXpForNextLevel(progress.Level);
        if (needed <= 0) return 100;
        return (int)((progress.Xp * 100.0) / needed);
    }

    // ─── Kill Streaks ──────────────────────────────────────────────

    /// <summary>
    /// Processes a kill for streak tracking. Returns bonus points and streak name, or (0, null).
    /// </summary>
    public (int bonusPoints, string? streakName) ProcessKillStreak(CodPlayer codPlayer)
    {
        bool isValidStreak = SECONDS_BETWEEN_KILLS <= 0 ||
                             (DateTime.Now - codPlayer.LastKillTime).TotalSeconds <= SECONDS_BETWEEN_KILLS;

        if (codPlayer.CurrentStreak > 0 && isValidStreak)
        {
            codPlayer.CurrentStreak++;
            codPlayer.LastKillTime = DateTime.Now;

            var (bonus, name) = codPlayer.CurrentStreak switch
            {
                2  => (POINTS_DOUBLE_KILL,   "Double Kill"),
                3  => (POINTS_TRIPLE_KILL,   "Triple Kill"),
                4  => (POINTS_DOMINATION,    "Domination"),
                5  => (POINTS_RAMPAGE,       "Rampage"),
                6  => (POINTS_MEGA_KILL,     "Mega Kill"),
                7  => (POINTS_OWNAGE,        "Ownage"),
                8  => (POINTS_ULTRA_KILL,    "Ultra Kill"),
                9  => (POINTS_KILLING_SPREE, "Killing Spree"),
                10 => (POINTS_MONSTER_KILL,  "Monster Kill"),
                11 => (POINTS_UNSTOPPABLE,   "Unstoppable"),
                >= 12 => (POINTS_GODLIKE,    "GODLIKE"),
                _  => (0, (string?)null)
            };

            if (bonus > 0) return (bonus, name);
        }
        else
        {
            codPlayer.CurrentStreak = 1;
            codPlayer.LastKillTime = DateTime.Now;
        }

        return (0, null);
    }

    public void ResetStreak(CodPlayer codPlayer)
    {
        codPlayer.CurrentStreak = 0;
        codPlayer.LastKillTime = DateTime.Now;
    }

    // ─── Game Rules Check ──────────────────────────────────────────

    private CCSGameRules? _gameRules;

    public void ResetGameRules() => _gameRules = null;

    public bool IsPointsAllowed()
    {
        if (_gameRules == null)
        {
            var proxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                .FirstOrDefault();
            _gameRules = proxy?.GameRules;
        }

        if (_gameRules == null) return false;
        if (_gameRules.WarmupPeriod && !WARMUP_POINTS) return false;

        int humanPlayers = Utilities.GetPlayers().Count(p =>
            p?.IsValid == true &&
            !p.IsBot &&
            !p.IsHLTV &&
            p.Connected == PlayerConnectedState.PlayerConnected);

        return humanPlayers >= MIN_PLAYERS;
    }

    // ─── Utility ───────────────────────────────────────────────────

    public static string GetRankColorCode(RankInfo rank)
    {
        return rank.Color.ToLower() switch
        {
            "red"    => $"{ChatColors.Red}",
            "green"  => $"{ChatColors.Green}",
            "blue"   => $"{ChatColors.Blue}",
            "gold"   => $"{ChatColors.Gold}",
            "purple" => $"{ChatColors.Purple}",
            "white"  => $"{ChatColors.White}",
            _        => $"{ChatColors.Default}"
        };
    }
}
