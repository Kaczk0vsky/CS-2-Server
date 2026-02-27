using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CodMod.Models;
using CodMod.Services;

namespace CodMod.Menus;

/// <summary>
/// Persistent player overview window that stays on screen showing nickname, class, level, and XP progress.
/// Automatically hides when class menu is opened and returns when class is selected.
/// </summary>
public class StatsMenu
{
    private readonly BasePlugin _plugin;
    private readonly RankService _rankService;
    private readonly Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer> _activeTimers = new();
    private readonly Dictionary<ulong, bool> _windowVisible = new();

    public StatsMenu(BasePlugin plugin, RankService rankService)
    {
        _plugin = plugin;
        _rankService = rankService;
    }

    /// <summary>
    /// Shows the persistent overview window for a player.
    /// Sets up a repeating timer to keep the window refreshed.
    /// </summary>
    public void ShowOverview(CCSPlayerController player)
    {
        if (!player.IsValid) return;

        var steamId = player.SteamID;
        _windowVisible[steamId] = true;

        // Kill existing timer if any
        if (_activeTimers.TryGetValue(steamId, out var oldTimer))
        {
            oldTimer.Kill();
        }

        // Set up repeating timer to refresh the window
        var timer = _plugin.AddTimer(0.01f, () =>
        {
            if (!player.IsValid)
            {
                _windowVisible.Remove(steamId);
                return;
            }

            if (_windowVisible.TryGetValue(steamId, out var isVisible) && isVisible)
            {
                RefreshOverview(player);
            }
        }, TimerFlags.REPEAT);

        _activeTimers[steamId] = timer;

        // Initial display
        RefreshOverview(player);
    }

    /// <summary>
    /// Hides the persistent overview window for a player (e.g., when opening class menu).
    /// </summary>
    public void HideOverview(CCSPlayerController player)
    {
        if (!player.IsValid) return;

        var steamId = player.SteamID;
        _windowVisible[steamId] = false;

        // Clear the center HTML
        player.PrintToCenterHtml("");
    }

    /// <summary>
    /// Closes and cleans up the window for a player.
    /// Should be called when player disconnects.
    /// </summary>
    public void CloseWindow(CCSPlayerController player)
    {
        if (!player.IsValid) return;

        var steamId = player.SteamID;

        // Kill timer
        if (_activeTimers.TryGetValue(steamId, out var timer))
        {
            timer.Kill();
            _activeTimers.Remove(steamId);
        }

        _windowVisible.Remove(steamId);
        player.PrintToCenterHtml("");
    }

    /// <summary>
    /// Refreshes the overview window display for a player.
    /// </summary>
    private void RefreshOverview(CCSPlayerController player)
    {
        var codPlayer = _rankService.GetPlayer(player.SteamID);
        if (codPlayer == null) return;

        var html = BuildOverviewHtml(player, codPlayer);
        player.PrintToCenterHtml(html, 1); // 1 second duration, repeats every 0.05s
    }

    /// <summary>
    /// Builds the HTML for the overview window.
    /// </summary>
    private string BuildOverviewHtml(CCSPlayerController player, CodPlayer codPlayer)
    {
        var className = codPlayer.SelectedClassName ?? "No Class";
        
        // Get class level and XP progress
        var classProgress = codPlayer.GetActiveClassProgress();
        int classLevel = classProgress?.Level ?? 1;
        int currentXp = classProgress?.Xp ?? 0;
        int nextLevelXp = RankService.GetXpForNextLevel(classLevel);
        int xpPercent = nextLevelXp > 0 ? (currentXp * 100) / nextLevelXp : 0;

        // Build progress bar
        string progressBar = GenerateProgressBar(xpPercent, 12);

        // Build HTML
        var html = "<center>" +
                   $"<h3><font color='#FFD700'>{player.PlayerName}</font></h3>" +
                   $"<font color='white'>Class: <font color='#00FF00'>{className}</font></font><br/>" +
                   $"<font color='white'>Level: <font color='#00FF00'>{classLevel}</font></font><br/>" +
                   $"<font color='white'>{progressBar} {xpPercent}%</font><br/>" +
                   $"<font color='#808080' size='1'>XP: {currentXp}/{nextLevelXp}</font><br/>" +
                   "</center>";

        return html;
    }

    /// <summary>
    /// Generates a visual progress bar using block characters.
    /// </summary>
    private static string GenerateProgressBar(int percent, int width = 20)
    {
        percent = Math.Clamp(percent, 0, 100);
        int filled = (percent * width) / 100;
        int empty = width - filled;

        string filledBlocks = new string('█', filled);
        string emptyBlocks = new string('░', empty);

        return $"[<font color='#B0B0B0'>{filledBlocks}</font><font color='#606060'>{emptyBlocks}</font>]";
    }
}