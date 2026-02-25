using System;
using System.Collections.Generic;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using OnPlayerButtonsChanged = CounterStrikeSharp.API.Core.Listeners.OnPlayerButtonsChanged;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;

namespace CodMod;

public class CodMod : BasePlugin
{
    public override string ModuleName => "Cod Mod";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kaczk0vsky, RafGor";
    public override string ModuleDescription => "Simple Cod Mod";

    private readonly Dictionary<ulong, string> _playerClasses = new();
    private readonly Dictionary<ulong, string> _pendingClasses = new();
    private readonly Dictionary<ulong, int> _jumpCount = new();
    private readonly Dictionary<ulong, CodMenu> _activeMenus = new();

    public override void Load(bool hotReload)
    {
        Console.WriteLine("CodMod loaded!");

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
            _activeMenus.Remove(player.SteamID);
        });


        AddCommand("klasa", "Wybierz klase", (player, info) =>
        {
            if (player == null || !player.IsValid)
                return;

            OpenClassMenu(player);
        });

        AddCommand("menu_cancel", "Menu cancel/close", (player, info) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;
            menu.Close(player);
            _activeMenus.Remove(player.SteamID);
        });


        RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            // aktywacja pending klasy
            if (_pendingClasses.TryGetValue(player.SteamID, out var newClass))
            {
                _playerClasses[player.SteamID] = newClass;
                _pendingClasses.Remove(player.SteamID);

                player.PrintToChat(
                    $"{ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                    $"Grasz jako: {ChatColors.Blue}{newClass}"
                );
            }

            if (!_playerClasses.TryGetValue(player.SteamID, out var cls))
                return HookResult.Continue;

            _jumpCount[player.SteamID] = 0;

            // Delay żeby silnik nie nadpisał statów
            AddTimer(0.2f, () =>
            {
                if (!player.IsValid || !player.PawnIsAlive)
                    return;

                GiveClassEquipment(player, cls);
                ApplyClassStats(player, cls);
            });

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerHurt>((@event, info) =>
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;

            if (attacker == null || victim == null)
                return HookResult.Continue;

            if (!attacker.PawnIsAlive || !victim.PawnIsAlive)
                return HookResult.Continue;

            if (!_playerClasses.TryGetValue(attacker.SteamID, out var cls))
                return HookResult.Continue;

            if (cls != "Ninja" && cls != "Komandos")
                return HookResult.Continue;

            if (@event.Weapon != null && @event.Weapon.Contains("knife"))
            {
                victim.PlayerPawn.Value!.Health = 0;
            }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerJump>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                return HookResult.Continue;

            if (!_playerClasses.TryGetValue(player.SteamID, out var cls))
                return HookResult.Continue;

            if (cls != "Komandos")
                return HookResult.Continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null)
                return HookResult.Continue;

            // Check if player is on the ground
            bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;
            
            // Initialize jump count if needed
            if (!_jumpCount.ContainsKey(player.SteamID))
                _jumpCount[player.SteamID] = 0;

            if (onGround)
            {
                // Reset jump count when landing
                _jumpCount[player.SteamID] = 1; // Set to 1 since this IS a jump
                return HookResult.Continue;
            }

            // In the air - allow one extra jump (double jump)
            if (_jumpCount[player.SteamID] >= 2)
                return HookResult.Continue;

            _jumpCount[player.SteamID]++;
            
            // Apply boost upward for double jump
            var currentVelocity = pawn.AbsVelocity;
            pawn.Teleport(null, null, new Vector(currentVelocity.X, currentVelocity.Y, 300));
            
            return HookResult.Continue;
        });

        AddTimer(0.2f, CheckNinjaInvisibility, TimerFlags.REPEAT);
        AddTimer(0.5f, MaintainClassGravity, TimerFlags.REPEAT);

        // Register button listener for menu navigation
        RegisterListener<OnPlayerButtonsChanged>((player, pressed, released) =>
        {
            if (player == null || !player.IsValid) return;
            if (!_activeMenus.TryGetValue(player.SteamID, out var menu) || !menu.IsOpen) return;

            // Forward (W) = navigate up
            if ((pressed & PlayerButtons.Forward) != 0)
            {
                menu.PreviousOption(player);
            }
            // Back (S) = navigate down  
            if ((pressed & PlayerButtons.Back) != 0)
            {
                menu.NextOption(player);
            }
            // Use (E) = select
            if ((pressed & PlayerButtons.Use) != 0)
            {
                menu.SelectCurrent(player);
                _activeMenus.Remove(player.SteamID);
            }
            // Scoreboard (TAB) = cancel
            if ((pressed & PlayerButtons.Scoreboard) != 0)
            {
                menu.Close(player);
                _activeMenus.Remove(player.SteamID);
            }
        });
    }

    private void OpenClassMenu(CCSPlayerController player)
    {
        var menu = new CodMenu("Wybierz klasę");
        menu.AddOption("Snajper", p => SetClass(p, "Snajper"));
        menu.AddOption("Komandos", p => SetClass(p, "Komandos"));
        menu.AddOption("Strzelec wyborowy", p => SetClass(p, "Strzelec wyborowy"));
        menu.AddOption("Ninja", p => SetClass(p, "Ninja"));

        menu.Open(player);
        _activeMenus[player.SteamID] = menu;

        AddTimer(0.02f, () =>
        {
            if (!player.IsValid) return;
            if (_activeMenus.TryGetValue(player.SteamID, out var m) && m.IsOpen)
                m.Refresh(player);
        }, TimerFlags.REPEAT);
    }

    private void SetClass(CCSPlayerController player, string className)
    {
        _pendingClasses[player.SteamID] = className;
        player.PrintToChat(
            $"{ChatColors.Green}[COD MOD]{ChatColors.Default} " +
            $"Wybrano: {ChatColors.Blue}{className}"
        );
    }

    private void GiveClassEquipment(CCSPlayerController player, string className)
    {
        if (!player.IsValid || !player.PawnIsAlive)
            return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null)
            return;

        player.RemoveWeapons();
        pawn.Render = Color.FromArgb(255, 255, 255, 255);

        switch (className)
        {
            case "Snajper":
                player.GiveNamedItem("weapon_awp");
                player.GiveNamedItem("weapon_deagle");
                break;

            case "Komandos":
                player.GiveNamedItem("weapon_deagle");
                break;

            case "Strzelec wyborowy":
                player.GiveNamedItem("weapon_ak47");
                player.GiveNamedItem("weapon_fiveseven");
                break;

            case "Ninja":
                pawn.Render = Color.FromArgb(50, 255, 255, 255);
                break;
        }

        player.GiveNamedItem("weapon_knife");
    }

    private void ApplyClassStats(CCSPlayerController player, string className)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn == null)
            return;

        pawn.GravityScale = 1.0f;
        pawn.VelocityModifier = 1.0f;
        pawn.Health = 100;
        pawn.MaxHealth = 100;
        pawn.ArmorValue = 0;

        switch (className)
        {
            case "Snajper":
                pawn.Health = pawn.MaxHealth = 110;
                pawn.ArmorValue = 100;
                break;

            case "Komandos":
                pawn.Health = pawn.MaxHealth = 105;
                pawn.ArmorValue = 100;
                pawn.VelocityModifier = 1.2f;
                break;

            case "Strzelec wyborowy":
                pawn.Health = pawn.MaxHealth = 200;
                pawn.ArmorValue = 100;
                pawn.VelocityModifier = 0.5f;
                pawn.GravityScale = 2f;
                break;

            case "Ninja":
                pawn.Health = pawn.MaxHealth = 50;
                pawn.ArmorValue = 100;
                pawn.GravityScale = 0.1f;
                pawn.VelocityModifier = 1.25f;
                break;
        }

        // Synchronize gravity with proper state changed
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_flVelocityModifier");
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_flGravityScale");
        
        pawn.Teleport(pawn.AbsOrigin, pawn.AbsRotation, pawn.AbsVelocity);
        
        // Extra: Schedule another gravity update to ensure it sticks
        AddTimer(0.05f, () =>
        {
            if (!player.IsValid || !player.PawnIsAlive) return;
            var updatedPawn = player.PlayerPawn.Value;
            if (updatedPawn != null)
                Utilities.SetStateChanged(updatedPawn, "CBaseEntity", "m_flGravityScale");
        });
    }

    private void CheckNinjaInvisibility()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            if (!_playerClasses.TryGetValue(player.SteamID, out var cls))
                continue;

            if (cls != "Ninja")
                continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null)
                continue;

            if (pawn.AbsVelocity.Length() < 5f)
                pawn.Render = Color.FromArgb(0, 255, 255, 255);
            else
                pawn.Render = Color.FromArgb(50, 255, 255, 255);
        }
    }

    private void MaintainClassGravity()
    {
        // Continuously maintain gravity for classes that need it
        // This ensures gravity persists even if the engine tries to reset it
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            if (!_playerClasses.TryGetValue(player.SteamID, out var cls))
                continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null)
                continue;

            // Apply gravity based on class
            if (cls == "Strzelec wyborowy")
                pawn.GravityScale = 2f;
            else if (cls == "Ninja")
                pawn.GravityScale = 0.1f;
            else
                pawn.GravityScale = 1.0f;
        }
    }
}