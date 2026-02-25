using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace SimpleRank;

public class SimpleRank : BasePlugin
{
    public override string ModuleName => "Simple Rank";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "CS2-Server Team";
    public override string ModuleDescription => "XP and Rank system based on K4-System";

    // Player data (in-memory, TODO: MySQL)
    private Dictionary<ulong, PlayerData> _players = new();
    
    // Kill streak tracking
    private Dictionary<ulong, (int streak, DateTime lastKill)> _killStreaks = new();

    // Settings (matching K4-System defaults)
    private const int MIN_PLAYERS = 1;
    private const bool WARMUP_POINTS = false;
    private const int SECONDS_BETWEEN_KILLS = 0; // 0 = disabled
    private const int LONG_DISTANCE = 30; // meters

    #region Point Values (K4-System defaults)
    private const int POINTS_KILL = 8;
    private const int POINTS_DEATH = -5;
    private const int POINTS_HEADSHOT = 5;
    private const int POINTS_PENETRATED = 3;
    private const int POINTS_NOSCOPE = 15;
    private const int POINTS_THRUSMOKE = 15;
    private const int POINTS_BLIND_KILL = 5;
    private const int POINTS_TEAMKILL = -10;
    private const int POINTS_SUICIDE = -5;
    private const int POINTS_ASSIST = 5;
    private const int POINTS_ASSIST_FLASH = 7;
    private const int POINTS_ROUND_WIN = 5;
    private const int POINTS_ROUND_LOSE = -2;
    private const int POINTS_MVP = 10;
    private const int POINTS_BOMB_PLANT = 10;
    private const int POINTS_BOMB_EXPLODED = 10;
    private const int POINTS_BOMB_DEFUSED = 10;
    private const int POINTS_BOMB_DEFUSED_OTHERS = 3;
    private const int POINTS_BOMB_PICKUP = 2;
    private const int POINTS_BOMB_DROP = -2;
    private const int POINTS_LONG_DISTANCE = 8;
    
    // Kill streaks
    private const int POINTS_DOUBLE_KILL = 5;
    private const int POINTS_TRIPLE_KILL = 10;
    private const int POINTS_DOMINATION = 15;
    private const int POINTS_RAMPAGE = 20;
    private const int POINTS_MEGA_KILL = 25;
    private const int POINTS_OWNAGE = 30;
    private const int POINTS_ULTRA_KILL = 35;
    private const int POINTS_KILLING_SPREE = 40;
    private const int POINTS_MONSTER_KILL = 45;
    private const int POINTS_UNSTOPPABLE = 50;
    private const int POINTS_GODLIKE = 60;
    
    // Special kills
    private const int POINTS_KNIFE_KILL = 15;
    private const int POINTS_TASER_KILL = 20;
    private const int POINTS_GRENADE_KILL = 30;
    private const int POINTS_INFERNO_KILL = 30;
    private const int POINTS_IMPACT_KILL = 100;
    #endregion

    // Ranks (K4-System CS:GO style)
    private static readonly List<RankInfo> Ranks = new()
    {
        new RankInfo(-1, "None", "[N]", "default"),
        new RankInfo(100, "Silver I", "[SI]", "white"),
        new RankInfo(500, "Silver II", "[SII]", "white"),
        new RankInfo(900, "Silver III", "[SIII]", "white"),
        new RankInfo(1300, "Silver IV", "[SIV]", "white"),
        new RankInfo(1700, "Silver Elite", "[SE]", "white"),
        new RankInfo(2100, "Silver Elite Master", "[SEM]", "white"),
        new RankInfo(2600, "Gold Nova I", "[GNI]", "gold"),
        new RankInfo(3100, "Gold Nova II", "[GNII]", "gold"),
        new RankInfo(3600, "Gold Nova III", "[GNIII]", "gold"),
        new RankInfo(4100, "Gold Nova Master", "[GNM]", "gold"),
        new RankInfo(4700, "Master Guardian I", "[MGI]", "green"),
        new RankInfo(5300, "Master Guardian II", "[MGII]", "green"),
        new RankInfo(5900, "Master Guardian Elite", "[MGE]", "green"),
        new RankInfo(6500, "Distinguished Master Guardian", "[DMG]", "green"),
        new RankInfo(7200, "Legendary Eagle", "[LE]", "blue"),
        new RankInfo(7900, "Legendary Eagle Master", "[LEM]", "blue"),
        new RankInfo(8600, "Supreme Master First Class", "[SMFC]", "purple"),
        new RankInfo(9300, "Global Elite", "[GE]", "red")
    };

    private CCSGameRules? _gameRules;

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("[SimpleRank] Plugin loaded!");

        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvp);
        RegisterEventHandler<EventBombPlanted>(OnBombPlanted);
        RegisterEventHandler<EventBombDefused>(OnBombDefused);
        RegisterEventHandler<EventBombExploded>(OnBombExploded);
        RegisterEventHandler<EventBombPickup>(OnBombPickup);
        RegisterEventHandler<EventBombDropped>(OnBombDropped);

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    private void OnMapStart(string mapName)
    {
        _gameRules = null;
    }

    private CCSGameRules? GetGameRules()
    {
        if (_gameRules == null)
        {
            var gameRulesEntities = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
            var gameRules = gameRulesEntities.FirstOrDefault();
            _gameRules = gameRules?.GameRules;
        }
        return _gameRules;
    }

    private bool IsPointsAllowed()
    {
        var gameRules = GetGameRules();
        if (gameRules == null) return false;

        // Check warmup
        if (gameRules.WarmupPeriod && !WARMUP_POINTS) return false;

        // Check minimum players
        int humanPlayers = Utilities.GetPlayers().Count(p => 
            p?.IsValid == true && 
            !p.IsBot && 
            !p.IsHLTV && 
            p.Connected == PlayerConnectedState.PlayerConnected);

        return humanPlayers >= MIN_PLAYERS;
    }

    #region Events
    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot) 
            return HookResult.Continue;

        var steamId = player.SteamID;
        
        if (!_players.ContainsKey(steamId))
        {
            _players[steamId] = new PlayerData(steamId, player.PlayerName);
        }
        else
        {
            _players[steamId].Name = player.PlayerName;
        }

        _killStreaks[steamId] = (0, DateTime.Now);

        var data = _players[steamId];
        var rank = GetRank(data.Points);
        
        Server.NextFrame(() =>
        {
            if (player.IsValid)
            {
                player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} Welcome {ChatColors.Yellow}{player.PlayerName}{ChatColors.Default}!");
                player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} Rank: {GetRankColor(rank)}{rank.Name}{ChatColors.Default} | Points: {ChatColors.Blue}{data.Points}");
            }
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && !player.IsBot)
        {
            _killStreaks.Remove(player.SteamID);
        }
        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        // Reset kill streaks at round start (optional, K4 has config for this)
        foreach (var key in _killStreaks.Keys.ToList())
        {
            _killStreaks[key] = (0, DateTime.Now);
        }

        if (!IsPointsAllowed())
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Rank]{ChatColors.Default} Points disabled - minimum {MIN_PLAYERS} players required.");
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        var victim = @event.Userid;
        var attacker = @event.Attacker;
        var assister = @event.Assister;

        // Victim loses points
        if (victim != null && victim.IsValid && !victim.IsBot)
        {
            _killStreaks[victim.SteamID] = (0, DateTime.Now);

            if (attacker == null || !attacker.IsValid || attacker.SteamID == victim.SteamID)
            {
                // Suicide
                ModifyPoints(victim, POINTS_SUICIDE, "suicide");
            }
            else
            {
                ModifyPoints(victim, POINTS_DEATH, "death");
            }
        }

        // Attacker gains points
        if (attacker != null && attacker.IsValid && !attacker.IsBot && victim != null)
        {
            if (attacker.SteamID == victim.SteamID) return HookResult.Continue; // Suicide already handled

            bool isTeamKill = attacker.Team == victim.Team;

            if (isTeamKill)
            {
                ModifyPoints(attacker, POINTS_TEAMKILL, "team kill");
            }
            else
            {
                // Base kill points
                ModifyPoints(attacker, POINTS_KILL, "kill");

                // Headshot bonus
                if (@event.Headshot)
                    ModifyPoints(attacker, POINTS_HEADSHOT, "headshot");

                // Penetration bonus
                if (@event.Penetrated > 0)
                    ModifyPoints(attacker, POINTS_PENETRATED * @event.Penetrated, "penetration");

                // Noscope bonus
                if (@event.Noscope)
                    ModifyPoints(attacker, POINTS_NOSCOPE, "noscope");

                // Through smoke bonus
                if (@event.Thrusmoke)
                    ModifyPoints(attacker, POINTS_THRUSMOKE, "through smoke");

                // Blind kill bonus
                if (@event.Attackerblind)
                    ModifyPoints(attacker, POINTS_BLIND_KILL, "blind kill");

                // Long distance bonus
                if (@event.Distance >= LONG_DISTANCE)
                    ModifyPoints(attacker, POINTS_LONG_DISTANCE, "long distance");

                // Weapon specific bonuses
                string weapon = @event.Weapon.ToLower();
                if (weapon.Contains("knife") || weapon.Contains("bayonet"))
                    ModifyPoints(attacker, POINTS_KNIFE_KILL, "knife kill");
                else if (weapon == "taser")
                    ModifyPoints(attacker, POINTS_TASER_KILL, "taser kill");
                else if (weapon.Contains("hegrenade"))
                    ModifyPoints(attacker, POINTS_GRENADE_KILL, "grenade kill");
                else if (weapon.Contains("inferno"))
                    ModifyPoints(attacker, POINTS_INFERNO_KILL, "molotov kill");
                else if (weapon.Contains("grenade") || weapon.Contains("molotov") || weapon.Contains("flashbang"))
                    ModifyPoints(attacker, POINTS_IMPACT_KILL, "impact kill");

                // Kill streak handling
                HandleKillStreak(attacker);
            }
        }

        // Assist bonus
        if (assister != null && assister.IsValid && !assister.IsBot && victim != null)
        {
            bool isTeamAssist = assister.Team == victim.Team;
            if (!isTeamAssist)
            {
                if (@event.Assistedflash)
                    ModifyPoints(assister, POINTS_ASSIST_FLASH, "flash assist");
                else
                    ModifyPoints(assister, POINTS_ASSIST, "assist");
            }
        }

        return HookResult.Continue;
    }

    private void HandleKillStreak(CCSPlayerController player)
    {
        var steamId = player.SteamID;
        if (!_killStreaks.ContainsKey(steamId))
            _killStreaks[steamId] = (0, DateTime.Now);

        var (streak, lastKill) = _killStreaks[steamId];
        bool isValidStreak = SECONDS_BETWEEN_KILLS <= 0 || 
                            (DateTime.Now - lastKill).TotalSeconds <= SECONDS_BETWEEN_KILLS;

        if (streak > 0 && isValidStreak)
        {
            streak++;
            _killStreaks[steamId] = (streak, DateTime.Now);

            var streakBonus = streak switch
            {
                2 => (POINTS_DOUBLE_KILL, "Double Kill"),
                3 => (POINTS_TRIPLE_KILL, "Triple Kill"),
                4 => (POINTS_DOMINATION, "Domination"),
                5 => (POINTS_RAMPAGE, "Rampage"),
                6 => (POINTS_MEGA_KILL, "Mega Kill"),
                7 => (POINTS_OWNAGE, "Ownage"),
                8 => (POINTS_ULTRA_KILL, "Ultra Kill"),
                9 => (POINTS_KILLING_SPREE, "Killing Spree"),
                10 => (POINTS_MONSTER_KILL, "Monster Kill"),
                11 => (POINTS_UNSTOPPABLE, "Unstoppable"),
                >= 12 => (POINTS_GODLIKE, "GODLIKE"),
                _ => (0, "")
            };

            if (streakBonus.Item1 > 0)
                ModifyPoints(player, streakBonus.Item1, streakBonus.Item2);
        }
        else
        {
            _killStreaks[steamId] = (1, DateTime.Now);
        }
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        int winnerTeam = @event.Winner;

        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot) continue;
            if (player.TeamNum < 2) continue; // Skip spectators

            if (player.TeamNum == winnerTeam)
                ModifyPoints(player, POINTS_ROUND_WIN, "round win");
            else
                ModifyPoints(player, POINTS_ROUND_LOSE, "round lose");
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundMvp(EventRoundMvp @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
            ModifyPoints(player, POINTS_MVP, "MVP");

        return HookResult.Continue;
    }

    private HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
            ModifyPoints(player, POINTS_BOMB_PLANT, "bomb plant");

        return HookResult.Continue;
    }

    private HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            ModifyPoints(player, POINTS_BOMB_DEFUSED, "bomb defuse");

            // Bonus for other CTs
            foreach (var ct in Utilities.GetPlayers().Where(p => 
                p?.IsValid == true && !p.IsBot && 
                p.Team == CsTeam.CounterTerrorist && 
                p.SteamID != player.SteamID))
            {
                ModifyPoints(ct, POINTS_BOMB_DEFUSED_OTHERS, "bomb defuse (team)");
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnBombExploded(EventBombExploded @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        foreach (var terrorist in Utilities.GetPlayers().Where(p => 
            p?.IsValid == true && !p.IsBot && p.Team == CsTeam.Terrorist))
        {
            ModifyPoints(terrorist, POINTS_BOMB_EXPLODED, "bomb explode");
        }

        return HookResult.Continue;
    }

    private HookResult OnBombPickup(EventBombPickup @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
            ModifyPoints(player, POINTS_BOMB_PICKUP, "bomb pickup");

        return HookResult.Continue;
    }

    private HookResult OnBombDropped(EventBombDropped @event, GameEventInfo info)
    {
        if (!IsPointsAllowed()) return HookResult.Continue;

        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
            ModifyPoints(player, POINTS_BOMB_DROP, "bomb drop");

        return HookResult.Continue;
    }
    #endregion

    #region Points & Ranks
    private void ModifyPoints(CCSPlayerController player, int amount, string reason)
    {
        var steamId = player.SteamID;
        if (!_players.ContainsKey(steamId)) return;

        var data = _players[steamId];
        var oldRank = GetRank(data.Points);

        data.Points += amount;
        if (data.Points < 0) data.Points = 0;

        var newRank = GetRank(data.Points);

        Server.NextFrame(() =>
        {
            if (!player.IsValid) return;

            // Show points change
            if (amount > 0)
                player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} +{amount} ({reason}) | Punkty: {ChatColors.Blue}{data.Points}");
            else if (amount < 0)
                player.PrintToChat($" {ChatColors.Red}[Rank]{ChatColors.Default} {amount} ({reason}) | Punkty: {ChatColors.Blue}{data.Points}");

            // Rank change notification
            if (oldRank.Name != newRank.Name)
            {
                if (newRank.Points > oldRank.Points)
                {
                    player.PrintToChat($" {ChatColors.Gold}[Rank]{ChatColors.Default} Promoted! New rank: {GetRankColor(newRank)}{newRank.Name}");
                    Server.PrintToChatAll($" {ChatColors.Gold}[Rank]{ChatColors.Default} {player.PlayerName} promoted to {GetRankColor(newRank)}{newRank.Name}{ChatColors.Default}!");
                }
                else
                {
                    player.PrintToChat($" {ChatColors.Red}[Rank]{ChatColors.Default} Demoted: {GetRankColor(newRank)}{newRank.Name}");
                }
            }
        });
    }

    private RankInfo GetRank(int points)
    {
        for (int i = Ranks.Count - 1; i >= 0; i--)
        {
            if (points >= Ranks[i].Points && Ranks[i].Points >= 0)
                return Ranks[i];
        }
        return Ranks[0]; // None rank
    }

    private string GetRankColor(RankInfo rank)
    {
        return rank.Color.ToLower() switch
        {
            "red" => $"{ChatColors.Red}",
            "green" => $"{ChatColors.Green}",
            "blue" => $"{ChatColors.Blue}",
            "gold" => $"{ChatColors.Gold}",
            "purple" => $"{ChatColors.Purple}",
            "white" => $"{ChatColors.White}",
            _ => $"{ChatColors.Default}"
        };
    }
    #endregion

    #region Commands
    [ConsoleCommand("css_rank", "Show your rank")]
    [ConsoleCommand("css_myrank", "Show your rank")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnRankCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;

        if (!_players.TryGetValue(player.SteamID, out var data))
        {
            player.PrintToChat($" {ChatColors.Red}[Rank]{ChatColors.Default} No data found!");
            return;
        }

        var rank = GetRank(data.Points);
        var nextRank = GetNextRank(data.Points);

        player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} === Your Stats ===");
        player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} Rank: {GetRankColor(rank)}{rank.Name} {rank.Tag}");
        player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} Points: {ChatColors.Blue}{data.Points}");

        if (nextRank != null)
        {
            int needed = nextRank.Points - data.Points;
            player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} To {nextRank.Name}: {ChatColors.Yellow}{needed} points");
        }
        else
        {
            player.PrintToChat($" {ChatColors.Gold}[Rank]{ChatColors.Default} Max rank achieved!");
        }
    }

    [ConsoleCommand("css_top", "Show top players")]
    [ConsoleCommand("css_ranktop", "Show top players")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnTopCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;

        var top = _players.Values.OrderByDescending(p => p.Points).Take(10).ToList();

        player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} === TOP 10 ===");

        for (int i = 0; i < top.Count; i++)
        {
            var p = top[i];
            var rank = GetRank(p.Points);
            player.PrintToChat($" {ChatColors.Yellow}#{i + 1}{ChatColors.Default} {p.Name} - {GetRankColor(rank)}{rank.Tag}{ChatColors.Default} {ChatColors.Blue}{p.Points} pts");
        }
    }

    [ConsoleCommand("css_ranks", "Show all ranks")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnRanksCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;

        player.PrintToChat($" {ChatColors.Green}[Rank]{ChatColors.Default} === Rank List ===");

        foreach (var rank in Ranks.Where(r => r.Points >= 0))
        {
            player.PrintToChat($" {GetRankColor(rank)}{rank.Tag} {rank.Name}{ChatColors.Default} - {rank.Points}+ points");
        }
    }

    private RankInfo? GetNextRank(int points)
    {
        var current = GetRank(points);
        var idx = Ranks.IndexOf(current);
        return idx < Ranks.Count - 1 ? Ranks[idx + 1] : null;
    }
    #endregion
}

public class PlayerData
{
    public ulong SteamId { get; set; }
    public string Name { get; set; }
    public int Points { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Headshots { get; set; }

    public PlayerData(ulong steamId, string name)
    {
        SteamId = steamId;
        Name = name;
        Points = 0;
    }
}

public class RankInfo
{
    public int Points { get; }
    public string Name { get; }
    public string Tag { get; }
    public string Color { get; }

    public RankInfo(int points, string name, string tag, string color)
    {
        Points = points;
        Name = name;
        Tag = tag;
        Color = color;
    }
}
