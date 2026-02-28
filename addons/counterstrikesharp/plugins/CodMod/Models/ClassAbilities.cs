using System.Drawing;
using CodMod.Extensions;
using CodMod.Services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Models;

/// <summary>
/// Cała logika zdolności klas.
/// Sprawdzenia używają ClassAbilityFlags — dodanie klasy z istniejącą zdolnością
/// nie wymaga żadnych zmian w tym pliku.
/// Nowy TYP zdolności: flaga + blok metod tutaj + wywołanie w CodMod.
/// </summary>
public static class ClassAbilities
{
    private static readonly Random Rng = new();

    // ═══════════════════════════════════════════════════════════════
    // SPRAWDZENIA OBECNOŚCI ZDOLNOŚCI
    // ═══════════════════════════════════════════════════════════════

    public static bool IsKnifeOneHitClass(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.KnifeInstakill);

    public static bool HasDoubleJump(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.DoubleJump);

    public static bool HasStealthInvisibility(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.StealthInvisibility);

    public static bool HasHealOnKill(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.HealOnKill);

    public static bool HasBerserkOnLowHp(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.BerserkOnLowHp);

    public static bool HasCrouchStealth(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.CrouchStealth);

    public static bool HasHeadshotInstakill(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.HeadshotInstakill);

    public static bool HasInstakillChance(string? className)
        => CodClasses.Get(className).Abilities.HasFlag(ClassAbilityFlags.InstakillChance);

    /// <summary>Losuje true z prawdopodobieństwem percent% (0–100).</summary>
    public static bool RollChance(int percent) => Rng.Next(0, 100) < percent;

    // ═══════════════════════════════════════════════════════════════
    // KNIFE INSTAKILL  (Ninja, Komandos — prawy klik nożem)
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
        if (attacker.Team == victim.Team) return false;

        bool rightClick = false;
        try { rightClick = (attacker.Buttons & PlayerButtons.Attack2) != 0; }
        catch (ArgumentException) { }

        if (!rightClick)
            rightClick = rightClicking.Contains(attacker.SteamID);

        if (!rightClick) return false;

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

        const PlayerButtons JumpMask = (PlayerButtons)2;
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
    // HEAL ON KILL  (Wampir — +15 HP za zabójstwo)
    // ═══════════════════════════════════════════════════════════════

    public static void ApplyHealOnKill(CCSPlayerController attacker, string? className)
    {
        if (!attacker.PawnIsAlive) return;

        var pawn = attacker.PlayerPawn.Value;
        if (pawn == null) return;

        int healAmount = CodClasses.Get(className).AbilityHeal;
        int newHp = Math.Min(pawn.MaxHealth, pawn.Health + healAmount);
        attacker.SetHp(newHp);
    }

    // ═══════════════════════════════════════════════════════════════
    // BERSERK ON LOW HP  (Berserker — speed 1.8 gdy HP ≤ 40)
    // ═══════════════════════════════════════════════════════════════

    public static void ApplyBerserkMode(
        CCSPlayerController player,
        string? className,
        RankService rankService)
    {
        if (!HasBerserkOnLowHp(className)) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        var codPlayer = rankService.GetPlayer(player.SteamID);
        var progress  = codPlayer?.GetActiveClassProgress();
        var def       = CodClasses.Get(className);

        float normalSpeed = Math.Clamp(
            def.BaseSpeed + (progress?.SpeedPoints ?? 0) * 0.005f,
            0.1f, 2.5f);

        float targetSpeed = pawn.Health <= def.AbilityThreshold ? def.AbilitySpeed : normalSpeed;

        if (Math.Abs(pawn.VelocityModifier - targetSpeed) > 0.01f)
            player.SetSpeed(targetSpeed);
    }

    // ═══════════════════════════════════════════════════════════════
    // CROUCH STEALTH  (Widmo — prawie niewidzialny przy kucaniu)
    // ═══════════════════════════════════════════════════════════════

    public static void ApplyCrouchStealth(CCSPlayerController player, string? className)
    {
        if (!HasCrouchStealth(className)) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        var def = CodClasses.Get(className);
        bool isCrouching = (pawn.Flags & (uint)PlayerFlags.FL_DUCKING) != 0;
        byte targetAlpha = isCrouching ? def.IdleAlpha : (byte)255;

        if (pawn.Render.A != targetAlpha)
            pawn.Render = Color.FromArgb(targetAlpha, 255, 255, 255);
    }

    // ═══════════════════════════════════════════════════════════════
    // HEADSHOT INSTAKILL  (Gladiator — 50% szansy z HS z broni klasy)
    // ═══════════════════════════════════════════════════════════════

    private const int HitgroupHead = 1;

    public static bool TryApplyHeadshotInstakill(
        CCSPlayerController attacker,
        CCSPlayerController victim,
        string? attackerClassName,
        string? weaponName,
        int hitgroup,
        bool isEnemy)
    {
        if (!HasHeadshotInstakill(attackerClassName)) return false;
        if (!isEnemy) return false;
        if (hitgroup != HitgroupHead) return false;

        var def = CodClasses.Get(attackerClassName);
        if (def.AbilityWeapon != null &&
            (string.IsNullOrEmpty(weaponName) ||
             !weaponName.Contains(def.AbilityWeapon, StringComparison.OrdinalIgnoreCase))) return false;

        if (!RollChance(def.AbilityChance)) return false;

        var victimPawn = victim.PlayerPawn.Value;
        if (victimPawn == null) return false;

        victimPawn.Health = 0;
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // INSTAKILL CHANCE  (Egzekutor — 15% szansy z broni klasy)
    // ═══════════════════════════════════════════════════════════════

    public static bool TryApplyInstakillChance(
        CCSPlayerController attacker,
        CCSPlayerController victim,
        string? attackerClassName,
        string? weaponName,
        bool isEnemy)
    {
        if (!HasInstakillChance(attackerClassName)) return false;
        if (!isEnemy) return false;

        var def = CodClasses.Get(attackerClassName);
        if (def.AbilityWeapon != null &&
            (string.IsNullOrEmpty(weaponName) ||
             !weaponName.Contains(def.AbilityWeapon, StringComparison.OrdinalIgnoreCase))) return false;

        if (!RollChance(def.AbilityChance)) return false;

        var victimPawn = victim.PlayerPawn.Value;
        if (victimPawn == null) return false;

        victimPawn.Health = 0;
        return true;
    }
}
