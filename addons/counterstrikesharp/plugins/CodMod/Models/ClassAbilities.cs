using System.Drawing;
using CodMod.Services;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Models;

public static class ClassAbilities
{
    public static bool IsKnifeOneHitClass(string? className)
    {
        return className == CodClasses.Ninja || className == CodClasses.Komandos;
    }

    public static bool HasDoubleJump(string? className)
    {
        return className == CodClasses.Komandos;
    }

    public static bool HasStealthInvisibility(string? className)
    {
        return className == CodClasses.Ninja;
    }

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
        try
        {
            right = (attacker.Buttons & PlayerButtons.Attack2) != 0;
        }
        catch (ArgumentException)
        {
        }

        if (!right)
            right = rightClicking.Contains(attacker.SteamID);

        if (!right) return false;

        var victimPawn = victim.PlayerPawn.Value;
        if (victimPawn == null) return false;

        victimPawn.Health = 0;
        return true;
    }

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

        const PlayerButtons JumpMask = (PlayerButtons)2; // IN_JUMP bit (source engine)
        bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;

        if (onGround)
        {
            jumpCount[player.SteamID] = 0;
        }

        if ((pressed & JumpMask) == 0 || onGround) return;

        jumpCount.TryGetValue(player.SteamID, out int count);
        if (count >= 2) return;

        jumpCount[player.SteamID] = count + 1;
        var vel = pawn.AbsVelocity;
        pawn.Teleport(null, null, new Vector(vel.X, vel.Y, 300));
    }

    public static void ApplyStealthInvisibility(CCSPlayerController player, string? className)
    {
        if (!HasStealthInvisibility(className)) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        var classDefinition = CodClasses.Get(className);
        bool isStationary = pawn.AbsVelocity.Length() < 5f;

        pawn.Render = isStationary
            ? Color.FromArgb(classDefinition.IdleAlpha, 255, 255, 255)
            : Color.FromArgb(classDefinition.MovingAlpha, 255, 255, 255);
    }
}
