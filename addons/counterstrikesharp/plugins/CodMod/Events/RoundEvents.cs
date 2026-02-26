using CodMod.Services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Events;

public class RoundEvents
{
    private readonly RankService _rankService;
    private readonly HudService _hudService;

    public RoundEvents(RankService rankService, HudService hudService)
    {
        _rankService = rankService;
        _hudService = hudService;
    }

    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {   
        // Ensure players start with armor + kevlar
        Server.ExecuteCommand("mp_free_armor 2");
        
        // Reset kill streaks
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot) continue;
            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer != null)
                _rankService.ResetStreak(codPlayer);
        }

        // Show stats summary in chat at round start
        Server.NextFrame(() =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player == null || !player.IsValid || player.IsBot) continue;
                if (player.TeamNum < 2) continue;
                PrintStatsToChat(player);
            }
        });

        if (!_rankService.IsPointsAllowed())
        {
            Server.PrintToChatAll(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"Points disabled — minimum {RankService.MIN_PLAYERS} players required.");
        }

        return HookResult.Continue;
    }

    private void PrintStatsToChat(CCSPlayerController player)
    {
        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (codPlayer == null) return;

        var rank = _rankService.GetGlobalRank(codPlayer.GlobalPoints);
        var nextRank = _rankService.GetNextRank(codPlayer.GlobalPoints);

        player.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} ——— Your Stats ———");
        player.PrintToChat(
            $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
            $"Rank: {RankService.GetRankColorCode(rank)}{rank.Name} {rank.Tag}" +
            $"{ChatColors.Default} | Points: {ChatColors.Blue}{codPlayer.GlobalPoints}");

        if (codPlayer.SelectedClassName != null)
        {
            var progress = codPlayer.GetActiveClassProgress();
            if (progress != null)
            {
                int xpPct   = RankService.GetXpPercentage(progress);
                int xpNeeded = RankService.GetXpForNextLevel(progress.Level);
                player.PrintToChat(
                    $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                    $"Class: {ChatColors.Yellow}{codPlayer.SelectedClassName}{ChatColors.Default} " +
                    $"(Lvl {ChatColors.Green}{progress.Level}{ChatColors.Default}) " +
                    $"— {ChatColors.Blue}{progress.Xp}/{xpNeeded}{ChatColors.Default} XP ({xpPct}%)");
            }
        }
        else
        {
            player.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"Class: {ChatColors.Red}none{ChatColors.Default} — type {ChatColors.Yellow}klasa{ChatColors.Default} to select one");
        }

        if (nextRank != null)
        {
            int needed = nextRank.Points - codPlayer.GlobalPoints;
            player.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"Next rank {RankService.GetRankColorCode(nextRank)}{nextRank.Name}{ChatColors.Default}: " +
                $"{ChatColors.Yellow}{needed} pts needed");
        }
    }

    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;

        int winnerTeam = @event.Winner;

        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot) continue;
            if (player.TeamNum < 2) continue;

            bool won    = player.TeamNum == winnerTeam;
            int  pts    = won ? RankService.POINTS_ROUND_WIN : RankService.POINTS_ROUND_LOSE;
            string reason = won ? "round win" : "round loss";

            var result = _rankService.AddPoints(player, pts, reason);

            string sign   = pts >= 0 ? $"+{pts}" : $"{pts}";
            string color  = pts >= 0 ? $"{ChatColors.Green}" : $"{ChatColors.Red}";
            string prefix = pts >= 0 ? $"{ChatColors.Green}[COD MOD]" : $"{ChatColors.Red}[COD MOD]";

            player.PrintToChat($" {prefix}{ChatColors.Default} {color}{sign}{ChatColors.Default} ({reason})");
            _hudService.ShowRankChange(player, result.oldRank, result.newRank);
            if (result.classLeveledUp)
            {
                var codPlayer = _rankService.GetPlayer(player.SteamID);
                if (codPlayer?.SelectedClassName != null)
                    _hudService.ShowClassLevelUp(player, codPlayer.SelectedClassName, result.classNewLevel);
            }
        }

        return HookResult.Continue;
    }

    public HookResult OnRoundMvp(EventRoundMvp @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;

        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            var result = _rankService.AddPoints(player, RankService.POINTS_MVP, "MVP");
            player.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"{ChatColors.Green}+{RankService.POINTS_MVP}{ChatColors.Default} (MVP)");
            _hudService.ShowRankChange(player, result.oldRank, result.newRank);
            if (result.classLeveledUp)
            {
                var codPlayer = _rankService.GetPlayer(player.SteamID);
                if (codPlayer?.SelectedClassName != null)
                    _hudService.ShowClassLevelUp(player, codPlayer.SelectedClassName, result.classNewLevel);
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            var result = _rankService.AddPoints(player, RankService.POINTS_BOMB_PLANT, "bomb plant");
            player.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"{ChatColors.Green}+{RankService.POINTS_BOMB_PLANT}{ChatColors.Default} (bomb plant)");
            _hudService.ShowRankChange(player, result.oldRank, result.newRank);
            if (result.classLeveledUp)
            {
                var codPlayer = _rankService.GetPlayer(player.SteamID);
                if (codPlayer?.SelectedClassName != null)
                    _hudService.ShowClassLevelUp(player, codPlayer.SelectedClassName, result.classNewLevel);
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            var result = _rankService.AddPoints(player, RankService.POINTS_BOMB_DEFUSED, "bomb defuse");
            player.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"{ChatColors.Green}+{RankService.POINTS_BOMB_DEFUSED}{ChatColors.Default} (bomb defuse)");
            _hudService.ShowRankChange(player, result.oldRank, result.newRank);
            if (result.classLeveledUp)
            {
                var codPlayer = _rankService.GetPlayer(player.SteamID);
                if (codPlayer?.SelectedClassName != null)
                    _hudService.ShowClassLevelUp(player, codPlayer.SelectedClassName, result.classNewLevel);
            }

            foreach (var ct in Utilities.GetPlayers().Where(p =>
                p?.IsValid == true && !p.IsBot &&
                p.Team == CsTeam.CounterTerrorist &&
                p.SteamID != player.SteamID))
            {
                var ctResult = _rankService.AddPoints(ct, RankService.POINTS_BOMB_DEFUSED_OTHERS, "bomb defuse (team)");
                ct.PrintToChat(
                    $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                    $"{ChatColors.Green}+{RankService.POINTS_BOMB_DEFUSED_OTHERS}{ChatColors.Default} (bomb defuse — team)");
                _hudService.ShowRankChange(ct, ctResult.oldRank, ctResult.newRank);
                if (ctResult.classLeveledUp)
                {
                    var codCt = _rankService.GetPlayer(ct.SteamID);
                    if (codCt?.SelectedClassName != null)
                        _hudService.ShowClassLevelUp(ct, codCt.SelectedClassName, ctResult.classNewLevel);
                }
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnBombExploded(EventBombExploded @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;
        foreach (var terrorist in Utilities.GetPlayers().Where(p =>
            p?.IsValid == true && !p.IsBot && p.Team == CsTeam.Terrorist))
        {
            var result = _rankService.AddPoints(terrorist, RankService.POINTS_BOMB_EXPLODED, "bomb explode");
            terrorist.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"{ChatColors.Green}+{RankService.POINTS_BOMB_EXPLODED}{ChatColors.Default} (bomb explode)");
            _hudService.ShowRankChange(terrorist, result.oldRank, result.newRank);
            if (result.classLeveledUp)
            {
                var codPlayer = _rankService.GetPlayer(terrorist.SteamID);
                if (codPlayer?.SelectedClassName != null)
                    _hudService.ShowClassLevelUp(terrorist, codPlayer.SelectedClassName, result.classNewLevel);
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnBombPickup(EventBombPickup @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            var result = _rankService.AddPoints(player, RankService.POINTS_BOMB_PICKUP, "bomb pickup");
            player.PrintToChat(
                $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                $"{ChatColors.Green}+{RankService.POINTS_BOMB_PICKUP}{ChatColors.Default} (bomb pickup)");
            _hudService.ShowRankChange(player, result.oldRank, result.newRank);
            if (result.classLeveledUp)
            {
                var codPlayer = _rankService.GetPlayer(player.SteamID);
                if (codPlayer?.SelectedClassName != null)
                    _hudService.ShowClassLevelUp(player, codPlayer.SelectedClassName, result.classNewLevel);
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnBombDropped(EventBombDropped @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            var result = _rankService.AddPoints(player, RankService.POINTS_BOMB_DROP, "bomb drop");
            player.PrintToChat(
                $" {ChatColors.Red}[COD MOD]{ChatColors.Default} " +
                $"{ChatColors.Red}{RankService.POINTS_BOMB_DROP}{ChatColors.Default} (bomb drop)");
            _hudService.ShowRankChange(player, result.oldRank, result.newRank);
        }
        return HookResult.Continue;
    }
}
