using CodMod.Models;
using CodMod.Services;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Menus;

public class LevelUpMenu
{
    private readonly RankService _rankService;

    public LevelUpMenu(RankService rankService)
    {
        _rankService = rankService;
    }

    public void Open(
        CCSPlayerController player,
        Action<CCSPlayerController, CodMenu> registerActiveMenu,
        Action<CCSPlayerController> onStatsChanged,
        Action<CCSPlayerController> onQuit,
        int selectedIndex = 0)
    {
        if (!player.IsValid) return;

        var codPlayer = _rankService.GetPlayer(player.SteamID);
        var progress = codPlayer?.GetActiveClassProgress();
        if (codPlayer == null || progress == null)
        {
            onQuit(player);
            return;
        }

        if (progress.AvailableStatPoints <= 0)
        {
            onQuit(player);
            return;
        }

        var menu = new CodMenu("Stats");

        menu.AddOption($"Points: {progress.AvailableStatPoints}", _ =>
        {
            Open(player, registerActiveMenu, onStatsChanged, onQuit, 0);
        });

        menu.AddOption($"Health: {progress.HealthPoints}/100 (+2 HP)", p =>
        {
            if (progress.TryAddHealthPoint())
            {
                p.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Added 1 point to Health.");
                onStatsChanged(p);
            }
            Open(p, registerActiveMenu, onStatsChanged, onQuit, 1);
        });

        menu.AddOption($"Speed: {progress.SpeedPoints}/100 (+ 1 speed)", p =>
        {
            if (progress.TryAddSpeedPoint())
            {
                p.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Added 1 point to Speed.");
                onStatsChanged(p);
            }
            Open(p, registerActiveMenu, onStatsChanged, onQuit, 2);
        });

        menu.AddOption($"Inteligence: {progress.IntelligencePoints}/100 (+ 1 AP)", p =>
        {
            if (progress.TryAddIntelligencePoint())
            {
                p.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Added 1 point to Inteligence.");
                onStatsChanged(p);
            }
            Open(p, registerActiveMenu, onStatsChanged, onQuit, 3);
        });

        menu.AddOption($"Endurance: {progress.EndurancePoints}/100 (- 0.1 DMG)", p =>
        {
            if (progress.TryAddEndurancePoint())
            {
                p.PrintToChat($" {ChatColors.Green}[COD MOD]{ChatColors.Default} Added 1 point to Endurance.");
                onStatsChanged(p);
            }
            Open(p, registerActiveMenu, onStatsChanged, onQuit, 4);
        });

        menu.Open(player, selectedIndex);
        registerActiveMenu(player, menu);
    }

    public void Open(
        CCSPlayerController player,
        Action<CCSPlayerController, CodMenu> registerActiveMenu,
        Action<CCSPlayerController> onStatsChanged,
        Action<CCSPlayerController> onQuit)
    {
        Open(player, registerActiveMenu, onStatsChanged, onQuit, 0);
    }
}
