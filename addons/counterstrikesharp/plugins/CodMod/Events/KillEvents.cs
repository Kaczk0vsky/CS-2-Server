using CodMod.Services;
using CodMod.Models;
using CodMod.Extensions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CodMod.Events;

public class KillEvents
{
    private readonly RankService _rankService;
    private readonly HudService _hudService;
    private readonly Action<CCSPlayerController>? _showLevelUpMenu;

    public KillEvents(
        RankService rankService,
        HudService hudService,
        Action<CCSPlayerController>? showLevelUpMenu = null)
    {
        _rankService = rankService;
        _hudService = hudService;
        _showLevelUpMenu = showLevelUpMenu;
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!_rankService.IsPointsAllowed()) return HookResult.Continue;

        var victim = @event.Userid;
        var attacker = @event.Attacker;
        var assister = @event.Assister;

        // --- Victim loses points ---
        if (victim != null && victim.IsValid && !victim.IsBot)
        {
            var victimPlayer = _rankService.GetPlayer(victim.SteamID);
            if (victimPlayer != null)
                _rankService.ResetStreak(victimPlayer);

            if (attacker == null || !attacker.IsValid || attacker.SteamID == victim.SteamID)
                ApplyAndNotify(victim, RankService.POINTS_SUICIDE, "suicide");
            else
                ApplyAndNotify(victim, RankService.POINTS_DEATH, "death");
        }

        // --- Attacker gains points ---
        if (attacker != null && attacker.IsValid && !attacker.IsBot && victim != null)
        {
            if (attacker.SteamID == victim.SteamID) return HookResult.Continue;

            bool isTeamKill = attacker.Team == victim.Team;

            if (isTeamKill)
            {
                ApplyAndNotify(attacker, RankService.POINTS_TEAMKILL, "team kill");
            }
            else
            {
                // Accumulate bonuses into a single chat line
                int totalBonus = RankService.POINTS_KILL;
                var bonusReasons = new List<string> { "kill" };

                if (@event.Headshot)      { totalBonus += RankService.POINTS_HEADSHOT;                    bonusReasons.Add("HS"); }
                if (@event.Penetrated > 0){ totalBonus += RankService.POINTS_PENETRATED * @event.Penetrated; bonusReasons.Add("penetration"); }
                if (@event.Noscope)       { totalBonus += RankService.POINTS_NOSCOPE;                    bonusReasons.Add("noscope"); }
                if (@event.Thrusmoke)     { totalBonus += RankService.POINTS_THRUSMOKE;                  bonusReasons.Add("through smoke"); }
                if (@event.Attackerblind) { totalBonus += RankService.POINTS_BLIND_KILL;                 bonusReasons.Add("blind"); }
                if (@event.Distance >= RankService.LONG_DISTANCE) { totalBonus += RankService.POINTS_LONG_DISTANCE; bonusReasons.Add("long distance"); }

                string weapon = @event.Weapon.ToLower();
                if (weapon.Contains("knife") || weapon.Contains("bayonet"))
                    { totalBonus += RankService.POINTS_KNIFE_KILL;  bonusReasons.Add("knife"); }
                else if (weapon == "taser")
                    { totalBonus += RankService.POINTS_TASER_KILL;  bonusReasons.Add("taser"); }
                else if (weapon.Contains("hegrenade"))
                    { totalBonus += RankService.POINTS_GRENADE_KILL; bonusReasons.Add("grenade"); }
                else if (weapon.Contains("inferno"))
                    { totalBonus += RankService.POINTS_INFERNO_KILL; bonusReasons.Add("molotov"); }
                else if (weapon.Contains("grenade") || weapon.Contains("molotov") || weapon.Contains("flashbang"))
                    { totalBonus += RankService.POINTS_IMPACT_KILL;  bonusReasons.Add("impact"); }

                string reasonStr = string.Join(" + ", bonusReasons);
                var result = _rankService.AddPoints(attacker, totalBonus, reasonStr);

                attacker.PrintToChat(
                    $" {ChatColors.Green}[COD MOD]{ChatColors.Default} " +
                    $"{ChatColors.Green}+{totalBonus}{ChatColors.Default} ({reasonStr})");

                _hudService.ShowRankChange(attacker, result.oldRank, result.newRank);
                if (result.classLeveledUp)
                {
                    var codPlayer = _rankService.GetPlayer(attacker.SteamID);
                    if (codPlayer?.SelectedClassName != null)
                        _hudService.ShowClassLevelUp(attacker, codPlayer.SelectedClassName, result.classNewLevel);
                    _showLevelUpMenu?.Invoke(attacker);
                }

                // Kill streak
                var attackerPlayer = _rankService.GetPlayer(attacker.SteamID);
                if (attackerPlayer != null)
                {
                    if (string.IsNullOrWhiteSpace(attackerPlayer.ActivePerkName))
                    {
                        var perk = Perks.GetRandom();
                        attackerPlayer.ActivePerkName = perk.Name;

                        if (attacker.PawnIsAlive && Perks.HasHeGrenadeInstantKill(attackerPlayer.ActivePerkName))
                        {
                            attacker.GiveNamedItem("weapon_hegrenade");
                        }

                        if (attacker.PawnIsAlive && attackerPlayer.SelectedClassName != null)
                        {
                            var classDefinition = CodClasses.Get(attackerPlayer.SelectedClassName);
                            var progress = attackerPlayer.GetActiveClassProgress();

                            int statHealthBonus = (progress?.HealthPoints ?? 0) * 2;
                            int perkHealthBonus = Perks.GetHealthBonus(attackerPlayer.ActivePerkName);
                            int finalHp = classDefinition.BaseHealth + statHealthBonus + perkHealthBonus;

                            float statSpeedBonus = (progress?.SpeedPoints ?? 0) * 0.005f;
                            float perkSpeedBonus = Perks.GetSpeedBonus(attackerPlayer.ActivePerkName);
                            float finalSpeed = Math.Clamp(classDefinition.BaseSpeed + statSpeedBonus + perkSpeedBonus, 0.1f, 2.5f);

                            var attackerPawn = attacker.PlayerPawn.Value;
                            if (attackerPawn != null)
                                attackerPawn.MaxHealth = finalHp;

                            attacker.SetSpeed(finalSpeed);
                        }
                    }

                    var (bonus, streakName) = _rankService.ProcessKillStreak(attackerPlayer);
                    if (bonus > 0 && streakName != null)
                    {
                        var streakResult = _rankService.AddPoints(attacker, bonus, streakName);
                        attacker.PrintToChat(
                            $" {ChatColors.Gold}[COD MOD]{ChatColors.LightRed} {streakName}! " +
                            $"{ChatColors.Green}+{bonus}");
                    }
                }
            }
        }

        // --- Assist ---
        if (assister != null && assister.IsValid && !assister.IsBot && victim != null)
        {
            if (assister.Team != victim.Team)
            {
                bool flashAssist = @event.Assistedflash;
                int pts = flashAssist ? RankService.POINTS_ASSIST_FLASH : RankService.POINTS_ASSIST;
                string reason = flashAssist ? "flash assist" : "assist";
                ApplyAndNotify(assister, pts, reason);
            }
        }

        return HookResult.Continue;
    }

    private void ApplyAndNotify(CCSPlayerController player, int amount, string reason)
    {
        var result = _rankService.AddPoints(player, amount, reason);

        string sign   = amount >= 0 ? $"+{amount}" : $"{amount}";
        string color  = amount >= 0 ? $"{ChatColors.Green}" : $"{ChatColors.Red}";
        string prefix = amount >= 0 ? $"{ChatColors.Green}[COD MOD]" : $"{ChatColors.Red}[COD MOD]";

        player.PrintToChat($" {prefix}{ChatColors.Default} {color}{sign}{ChatColors.Default} ({reason})");

        _hudService.ShowRankChange(player, result.oldRank, result.newRank);
        if (result.classLeveledUp)
        {
            var codPlayer = _rankService.GetPlayer(player.SteamID);
            if (codPlayer?.SelectedClassName != null)
                _hudService.ShowClassLevelUp(player, codPlayer.SelectedClassName, result.classNewLevel);
            _showLevelUpMenu?.Invoke(player);
        }
    }
}
