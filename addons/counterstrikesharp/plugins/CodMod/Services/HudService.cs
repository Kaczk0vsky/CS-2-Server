using CodMod.Models;
using CodMod.Services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Services;

/// <summary>
/// Handles HUD display for player stats.
/// Shows info at round start and via command — not continuously.
/// </summary>
public class HudService
{
    private readonly RankService _rankService;

    public HudService(RankService rankService)
    {
        _rankService = rankService;
    }

    /// <summary>
    /// Shows the player's full stats card in center HTML.
    /// Called at round start and via !codstats command.
    /// </summary>
    public void ShowPlayerStats(CCSPlayerController player)
    {
        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (codPlayer == null) return;

        var rank = _rankService.GetGlobalRank(codPlayer.GlobalPoints);
        var nextRank = _rankService.GetNextRank(codPlayer.GlobalPoints);

        // Build class info
        string classInfo = "None";
        string classLevelInfo = "";
        if (codPlayer.SelectedClassName != null)
        {
            var progress = codPlayer.GetActiveClassProgress();
            if (progress != null)
            {
                int xpPercent = RankService.GetXpPercentage(progress);
                classInfo = codPlayer.SelectedClassName;
                classLevelInfo = $" (Lvl {progress.Level}) — {xpPercent}% XP";
            }
            else
            {
                classInfo = codPlayer.SelectedClassName;
            }
        }

        // Build next rank info
        string nextRankText = "";
        if (nextRank != null)
        {
            int needed = nextRank.Points - codPlayer.GlobalPoints;
            nextRankText = $"<br/><font color='gray'>Next: {nextRank.Name} ({needed} pts)</font>";
        }

        string html = "<center>" +
            $"<font color='yellow' class='fontSize-m'>{codPlayer.Name}</font><br/>" +
            $"<font color='white'>Rank: </font>" +
            $"<font color='{GetHtmlColor(rank.Color)}'>{rank.Tag} {rank.Name}</font>" +
            $" <font color='gray'>({codPlayer.GlobalPoints} pts)</font><br/>" +
            $"<font color='white'>Class: </font>" +
            $"<font color='#00ff88'>{classInfo}</font>" +
            $"<font color='#88ccff'>{classLevelInfo}</font>" +
            nextRankText +
            "</center>";

        player.PrintToCenterHtml(html, 8);
    }

    /// <summary>
    /// Shows rank promotion or demotion message in chat.
    /// </summary>
    public void ShowRankChange(CCSPlayerController player, RankInfo oldRank, RankInfo newRank)
    {
        if (oldRank.Name == newRank.Name) return;

        if (newRank.Points > oldRank.Points)
        {
            player.PrintToChat(
                $" {ChatColors.Gold}[COD MOD]{ChatColors.Default} Awansowałeś! Nowa ranga: " +
                $"{RankService.GetRankColorCode(newRank)}{newRank.Name}");
        }
        else
        {
            player.PrintToChat(
                $" {ChatColors.Red}[COD MOD]{ChatColors.Default} Degradacja: " +
                $"{RankService.GetRankColorCode(newRank)}{newRank.Name}");
        }
    }

    /// <summary>
    /// Shows class level-up message in chat.
    /// </summary>
    public void ShowClassLevelUp(CCSPlayerController player, string className, int newLevel)
    {
        player.PrintToChat(
            $" {ChatColors.Gold}[COD MOD]{ChatColors.LightYellow} LEVEL UP! " +
            $"{ChatColors.Default}Klasa {ChatColors.Green}{className}{ChatColors.Default} " +
            $"→ Poziom {ChatColors.Green}{newLevel}");
    }

    /// <summary>
    /// Shows kill streak announcement to all players.
    /// </summary>
    public void ShowStreakAnnouncement(CCSPlayerController player, string streakName, int bonusPoints)
    {
        player.PrintToChat(
            $" {ChatColors.Gold}[COD MOD]{ChatColors.LightRed} {streakName}! " +
            $"{ChatColors.Default}(+{bonusPoints} pts)");
    }

    private static string GetHtmlColor(string colorKey)
    {
        return colorKey.ToLower() switch
        {
            "red"    => "#ff4444",
            "green"  => "#44ff44",
            "blue"   => "#4488ff",
            "gold"   => "#ffcc00",
            "purple" => "#cc44ff",
            "white"  => "#ffffff",
            _        => "#cccccc"
        };
    }
}
