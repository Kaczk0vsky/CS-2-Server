using System.Drawing;
using CodMod.Extensions;
using CodMod.Services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Models;

/// <summary>
/// Cała logika zdolności klas.
/// Sprawdzenia zdolności korzystają z ClassAbilityFlags (data-driven) zamiast
/// hard-coded nazw klas — dodanie klasy z istniejącą zdolnością nie wymaga
/// żadnych zmian w tym pliku.
/// </summary>
public static class ClassAbilities
{
    // ═══════════════════════════════════════════════════════════════
    // SPRAWDZENIA OBECNOŚCI ZDOLNOŚCI  (data-driven via flags)
    // ═══════════════════════════════════════════════════════════════

    public static bool IsKnifeOneHitClass(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.KnifeInstakill);

    public static bool HasDoubleJump(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.DoubleJump);

    public static bool HasStealthInvisibility(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.StealthInvisibility);

    public static bool HasHealOnKill(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.HealOnKill);

    // ═══════════════════════════════════════════════════════════════
    // KNIFE INSTAKILL  (Ninja, Komandos)
    // ═══════════════════════════════════════════════════════════════

    public static bool TryApplyKnifeInstakill(
        CCSPlayerController attacker,
        CCSPlayerController victim,
        string? attackerClassName,
        string? weaponName,
        HashSet<ulong> rightClicking)
    {
        if (!IsKnifeOneHitClass(attackerClassName)) return false;
        if (string.IsNullOrEmpty(weaponName) || !weaponName.Contains("knife")) return false;

        bool right = false;
        try { right = (attacker.Buttons & PlayerButtons.Attack2) != 0; }
        catch (ArgumentException) { }

        if (!right)
            right = rightClicking.Contains(attacker.SteamID);

        if (!right) return false;

        var victimPawn = victim.PlayerPawn.Value;
        if (victimPawn == null) return false;

        victimPawn.Health = 0;
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // DOUBLE JUMP  (Komandos)
    // ═══════════════════════════════════════════════════════════════

    public static void HandleDoubleJump(
        CCSPlayerController player,
        PlayerButtons pressed,
        RankService rankService,
        Dictionary<ulong, int> jumpCount)
    {
        if (!player.PawnIsAlive) return;

        var codPlayer = rankService.GetPlayer(player.SteamID);
        if (codPlayer == null || !HasDoubleJump(codPlayer.SelectedClassName)) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        const PlayerButtons JumpMask = (PlayerButtons)2; // IN_JUMP bit
        bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;

        if (onGround)
            jumpCount[player.SteamID] = 0;

        if ((pressed & JumpMask) == 0 || onGround) return;

        jumpCount.TryGetValue(player.SteamID, out int count);
        if (count >= 2) return;

        jumpCount[player.SteamID] = count + 1;
        var vel = pawn.AbsVelocity;
        pawn.Teleport(null, null, new Vector(vel.X, vel.Y, 300));
    }

    // ═══════════════════════════════════════════════════════════════
    // STEALTH INVISIBILITY  (Ninja — niewidzialny gdy stoi)
    // ═══════════════════════════════════════════════════════════════

    public static void ApplyStealthInvisibility(CCSPlayerController player, string? className)
    {
        if (!HasStealthInvisibility(className)) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        var def = CodClasses.Get(className);
        bool isStationary = pawn.AbsVelocity.Length() < 5f;

        pawn.Render = isStationary
            ? Color.FromArgb(def.IdleAlpha, 255, 255, 255)
            : Color.FromArgb(def.MovingAlpha, 255, 255, 255);
    }

    // ═══════════════════════════════════════════════════════════════
    // HEAL ON KILL  (Wampir — +15 HP za każde zabójstwo)
    // ═══════════════════════════════════════════════════════════════

    public const int HealOnKillAmount = 15;

    public static void ApplyHealOnKill(CCSPlayerController attacker)
    {
        if (!attacker.PawnIsAlive) return;

        var pawn = attacker.PlayerPawn.Value;
        if (pawn == null) return;

        int newHp = Math.Min(pawn.MaxHealth, pawn.Health + HealOnKillAmount);
        attacker.SetHp(newHp);
    }
}
