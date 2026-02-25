using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace CodMod;

public class CodMod : BasePlugin
{
    public override string ModuleName => "CoD Mod";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "CS2-Server Team";
    public override string ModuleDescription => "Call of Duty style class and perk system for CS2";

    // Player data storage (in-memory, TODO: database)
    private Dictionary<ulong, CodPlayer> _players = new();

    // Class definitions
    private List<CodClass> _classes = new();



    #region XP Settings
    private const int XP_PER_KILL = 10;
    private const int XP_PER_HEADSHOT = 5;
    private const int XP_PER_ASSIST = 3;
    private const int XP_PER_WIN = 8;
    private const int XP_PER_MVP = 10;
    private const int XP_PER_BOMB_PLANT = 5;
    private const int XP_PER_BOMB_DEFUSE = 5;
    private const int MAX_LEVEL = 55;
    #endregion

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("[CodMod] Loading plugin...");

        InitializeClasses();
        RegisterEvents();
        RegisterCommands();

        Logger.LogInformation("[CodMod] Plugin loaded! {ClassCount} classes available.", _classes.Count);
    }

    private void InitializeClasses()
    {
        _classes = new List<CodClass>
        {
            new CodClass(
                id: 1,
                name: "Assault",
                description: "Balanced fighter with HP regeneration",
                skills: new List<CodSkill>
                {
                    new CodSkill("HP Regeneration", "Regenerate HP over time", SkillType.HpRegen, maxLevel: 3, valuePerLevel: 2),
                    new CodSkill("Extra Armor", "Start with additional armor", SkillType.BonusArmor, maxLevel: 3, valuePerLevel: 15),
                    new CodSkill("Fast Reload", "Reload weapons faster", SkillType.FastReload, maxLevel: 3, valuePerLevel: 10)
                },
                bonusHp: 10,
                bonusArmor: 25,
                speedMultiplier: 1.0f
            ),
            new CodClass(
                id: 2,
                name: "Sniper",
                description: "Long range specialist with improved accuracy",
                skills: new List<CodSkill>
                {
                    new CodSkill("Eagle Eye", "See enemies through walls briefly after scoping", SkillType.WallhackChance, maxLevel: 3, valuePerLevel: 5),
                    new CodSkill("Steady Aim", "Reduced recoil when scoped", SkillType.ReducedRecoil, maxLevel: 3, valuePerLevel: 10),
                    new CodSkill("Ghost", "Invisible to radar", SkillType.RadarInvisible, maxLevel: 1, valuePerLevel: 100)
                },
                bonusHp: -10,
                bonusArmor: 0,
                speedMultiplier: 0.95f
            ),
            new CodClass(
                id: 3,
                name: "Medic",
                description: "Support class that heals teammates",
                skills: new List<CodSkill>
                {
                    new CodSkill("Healing Aura", "Heal nearby teammates", SkillType.HealTeam, maxLevel: 3, valuePerLevel: 2),
                    new CodSkill("Revive Boost", "Reduced respawn time (if enabled)", SkillType.RespawnBoost, maxLevel: 3, valuePerLevel: 1),
                    new CodSkill("Vampirism", "Steal HP on hit", SkillType.Vampire, maxLevel: 3, valuePerLevel: 5)
                },
                bonusHp: 20,
                bonusArmor: 0,
                speedMultiplier: 1.0f
            ),
            new CodClass(
                id: 4,
                name: "Scout",
                description: "Fast and agile flanker",
                skills: new List<CodSkill>
                {
                    new CodSkill("Sprint", "Increased movement speed", SkillType.SpeedBoost, maxLevel: 3, valuePerLevel: 5),
                    new CodSkill("Long Jump", "Jump further", SkillType.LongJump, maxLevel: 3, valuePerLevel: 10),
                    new CodSkill("Stealth", "Reduced footstep sound", SkillType.SilentSteps, maxLevel: 3, valuePerLevel: 20)
                },
                bonusHp: -15,
                bonusArmor: 0,
                speedMultiplier: 1.15f
            )
        };
    }

    private void RegisterEvents()
    {
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvp);
        RegisterEventHandler<EventBombPlanted>(OnBombPlanted);
        RegisterEventHandler<EventBombDefused>(OnBombDefused);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnTick>(OnTick);
    }

    private void RegisterCommands()
    {
        AddCommand("css_cod", "Open CoD Mod menu", CmdCodMenu);
        AddCommand("css_class", "Select your class", CmdSelectClass);
        AddCommand("css_skills", "Manage your skills", CmdSkillsMenu);
        AddCommand("css_codstats", "Show your CoD stats", CmdStats);
        AddCommand("css_classes", "List all classes", CmdListClasses);
        AddCommand("css_resetskills", "Reset skill points", CmdResetSkills);
    }

    #region Event Handlers
    private void OnMapStart(string mapName)
    {
        // Reset any map-specific state if needed
    }

    private int _tickCounter = 0;
    private void OnTick()
    {
        _tickCounter++;
        if (_tickCounter < 64) return; // Run every ~1 second at 64 tick
        _tickCounter = 0;

        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot || !player.PawnIsAlive) continue;
            if (!_players.TryGetValue(player.SteamID, out var codPlayer)) continue;
            if (codPlayer.SelectedClass == null) continue;

            ApplyPassiveSkills(player, codPlayer);
        }
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

        var steamId = player.SteamID;

        if (!_players.ContainsKey(steamId))
        {
            _players[steamId] = new CodPlayer(steamId, player.PlayerName);
        }
        else
        {
            _players[steamId].Name = player.PlayerName;
        }

        Server.NextFrame(() =>
        {
            if (player.IsValid)
            {
                var codPlayer = _players[steamId];
                player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Welcome {ChatColors.Yellow}{player.PlayerName}{ChatColors.Default}!");
                player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Level: {ChatColors.Green}{codPlayer.Level}{ChatColors.Default} | XP: {ChatColors.Blue}{codPlayer.Xp}/{GetXpForNextLevel(codPlayer.Level)}");

                if (codPlayer.SelectedClass == null)
                {
                    player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Type {ChatColors.Yellow}!class{ChatColors.Default} to select a class!");
                }
                else
                {
                    player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Class: {ChatColors.Green}{codPlayer.SelectedClass.Name}");
                }
            }
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        // Data is kept in memory for reconnection
        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;
        if (player.TeamNum < 2) return HookResult.Continue; // Skip spectators

        if (!_players.TryGetValue(player.SteamID, out var codPlayer)) return HookResult.Continue;

        Server.NextFrame(() =>
        {
            if (!player.IsValid || !player.PawnIsAlive) return;
            ApplyClassBonuses(player, codPlayer);
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;
        var assister = @event.Assister;

        // Attacker XP
        if (attacker != null && attacker.IsValid && !attacker.IsBot && victim != null)
        {
            if (attacker.SteamID != victim.SteamID && attacker.Team != victim.Team)
            {
                int xpGain = XP_PER_KILL;
                if (@event.Headshot) xpGain += XP_PER_HEADSHOT;
                
                AddXp(attacker, xpGain, "kill");
            }
        }

        // Assister XP
        if (assister != null && assister.IsValid && !assister.IsBot && victim != null)
        {
            if (assister.Team != victim.Team)
            {
                AddXp(assister, XP_PER_ASSIST, "assist");
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var victim = @event.Userid;

        if (attacker == null || !attacker.IsValid || attacker.IsBot) return HookResult.Continue;
        if (victim == null || !victim.IsValid) return HookResult.Continue;
        if (attacker.Team == victim.Team) return HookResult.Continue;

        if (!_players.TryGetValue(attacker.SteamID, out var codPlayer)) return HookResult.Continue;
        if (codPlayer.SelectedClass == null) return HookResult.Continue;

        // Vampire skill
        int vampireLevel = codPlayer.GetSkillLevel(SkillType.Vampire);
        if (vampireLevel > 0)
        {
            var skill = codPlayer.SelectedClass.Skills.FirstOrDefault(s => s.Type == SkillType.Vampire);
            if (skill != null)
            {
                int healAmount = vampireLevel * skill.ValuePerLevel;
                int damage = @event.DmgHealth;
                int actualHeal = Math.Min(healAmount, damage / 2); // Max 50% of damage dealt

                var pawn = attacker.PlayerPawn?.Value;
                if (pawn != null && actualHeal > 0)
                {
                    int newHp = Math.Min(pawn.Health + actualHeal, pawn.MaxHealth);
                    pawn.Health = newHp;
                    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                }
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        int winnerTeam = @event.Winner;

        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot) continue;
            if (player.TeamNum < 2) continue;

            if (player.TeamNum == winnerTeam)
            {
                AddXp(player, XP_PER_WIN, "round win");
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundMvp(EventRoundMvp @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            AddXp(player, XP_PER_MVP, "MVP");
        }
        return HookResult.Continue;
    }

    private HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            AddXp(player, XP_PER_BOMB_PLANT, "bomb plant");
        }
        return HookResult.Continue;
    }

    private HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            AddXp(player, XP_PER_BOMB_DEFUSE, "bomb defuse");
        }
        return HookResult.Continue;
    }
    #endregion

    #region XP and Level System
    private void AddXp(CCSPlayerController player, int amount, string reason)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer)) return;

        int oldLevel = codPlayer.Level;
        codPlayer.Xp += amount;

        // Level up check
        while (codPlayer.Level < MAX_LEVEL && codPlayer.Xp >= GetXpForNextLevel(codPlayer.Level))
        {
            codPlayer.Xp -= GetXpForNextLevel(codPlayer.Level);
            codPlayer.Level++;
            codPlayer.SkillPoints++;
        }

        Server.NextFrame(() =>
        {
            if (!player.IsValid) return;

            player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} +{amount} XP ({reason})");

            if (codPlayer.Level > oldLevel)
            {
                player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.LightYellow} LEVEL UP! {ChatColors.Default}Level {ChatColors.Green}{codPlayer.Level}{ChatColors.Default} (+1 skill point)");
                Server.PrintToChatAll($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} {player.PlayerName} reached level {ChatColors.Green}{codPlayer.Level}{ChatColors.Default}!");
            }
        });
    }

    private int GetXpForNextLevel(int currentLevel)
    {
        // Simple formula: 100 + (level * 20)
        return 100 + (currentLevel * 20);
    }
    #endregion

    #region Class and Skill Application
    private void ApplyClassBonuses(CCSPlayerController player, CodPlayer codPlayer)
    {
        if (codPlayer.SelectedClass == null) return;

        var pawn = player.PlayerPawn?.Value;
        if (pawn == null) return;

        var codClass = codPlayer.SelectedClass;

        // Apply HP bonus
        if (codClass.BonusHp != 0)
        {
            int newMaxHp = 100 + codClass.BonusHp;
            pawn.MaxHealth = newMaxHp;
            pawn.Health = newMaxHp;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        // Apply armor bonus
        int totalArmor = codClass.BonusArmor;
        int armorSkillLevel = codPlayer.GetSkillLevel(SkillType.BonusArmor);
        if (armorSkillLevel > 0)
        {
            var skill = codClass.Skills.FirstOrDefault(s => s.Type == SkillType.BonusArmor);
            if (skill != null) totalArmor += armorSkillLevel * skill.ValuePerLevel;
        }

        if (totalArmor > 0)
        {
            var playerPawn = player.PlayerPawn?.Value as CCSPlayerPawn;
            if (playerPawn?.ItemServices != null)
            {
                var itemServices = new CCSPlayer_ItemServices(playerPawn.ItemServices.Handle);
                itemServices.HasHelmet = true;
            }

            pawn.ArmorValue = Math.Min(100, totalArmor);
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
        }

        // Apply speed bonus
        float speedMult = codClass.SpeedMultiplier;
        int speedSkillLevel = codPlayer.GetSkillLevel(SkillType.SpeedBoost);
        if (speedSkillLevel > 0)
        {
            var skill = codClass.Skills.FirstOrDefault(s => s.Type == SkillType.SpeedBoost);
            if (skill != null) speedMult += (speedSkillLevel * skill.ValuePerLevel) / 100f;
        }

        if (Math.Abs(speedMult - 1.0f) > 0.01f)
        {
            pawn.VelocityModifier = speedMult;
        }
    }

    private void ApplyPassiveSkills(CCSPlayerController player, CodPlayer codPlayer)
    {
        if (codPlayer.SelectedClass == null) return;

        var pawn = player.PlayerPawn?.Value;
        if (pawn == null) return;

        // HP Regeneration
        int hpRegenLevel = codPlayer.GetSkillLevel(SkillType.HpRegen);
        if (hpRegenLevel > 0)
        {
            var skill = codPlayer.SelectedClass.Skills.FirstOrDefault(s => s.Type == SkillType.HpRegen);
            if (skill != null)
            {
                int regenAmount = hpRegenLevel * skill.ValuePerLevel;
                if (pawn.Health < pawn.MaxHealth)
                {
                    pawn.Health = Math.Min(pawn.Health + regenAmount, pawn.MaxHealth);
                    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                }
            }
        }

        // Heal Team (Medic aura)
        int healTeamLevel = codPlayer.GetSkillLevel(SkillType.HealTeam);
        if (healTeamLevel > 0)
        {
            var skill = codPlayer.SelectedClass.Skills.FirstOrDefault(s => s.Type == SkillType.HealTeam);
            if (skill != null)
            {
                int healAmount = healTeamLevel * skill.ValuePerLevel;
                var playerPos = pawn.AbsOrigin;

                foreach (var teammate in Utilities.GetPlayers())
                {
                    if (teammate == null || !teammate.IsValid || teammate.IsBot || !teammate.PawnIsAlive) continue;
                    if (teammate.Team != player.Team || teammate.SteamID == player.SteamID) continue;

                    var teamPawn = teammate.PlayerPawn?.Value;
                    if (teamPawn == null) continue;

                    var teamPos = teamPawn.AbsOrigin;
                    if (playerPos == null || teamPos == null) continue;

                    float distance = (float)Math.Sqrt(
                        Math.Pow(playerPos.X - teamPos.X, 2) +
                        Math.Pow(playerPos.Y - teamPos.Y, 2) +
                        Math.Pow(playerPos.Z - teamPos.Z, 2));

                    if (distance <= 300) // 300 units radius
                    {
                        if (teamPawn.Health < teamPawn.MaxHealth)
                        {
                            teamPawn.Health = Math.Min(teamPawn.Health + healAmount, teamPawn.MaxHealth);
                            Utilities.SetStateChanged(teamPawn, "CBaseEntity", "m_iHealth");
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Commands
    private void CmdCodMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;

        var menu = new CenterHtmlMenu("CoD Mod Menu", this);
        menu.AddMenuOption("Select Class", (p, opt) => ShowClassMenu(p));
        menu.AddMenuOption("Manage Skills", (p, opt) => ShowSkillsMenu(p));
        menu.AddMenuOption("View Stats", (p, opt) => ShowStatsMenu(p));
        menu.AddMenuOption("List Classes", (p, opt) => ShowClassesInfo(p));
        menu.ExitButton = true;
        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    [ConsoleCommand("css_class")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CmdSelectClass(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;
        ShowClassMenu(player);
    }

    private void ShowClassMenu(CCSPlayerController player)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer))
        {
            player.PrintToChat($" {ChatColors.Red}[CoD Mod]{ChatColors.Default} Error loading data!");
            return;
        }

        var menu = new CenterHtmlMenu("Select Class", this);

        foreach (var codClass in _classes)
        {
            string selected = codPlayer.SelectedClass?.Id == codClass.Id ? " [*]" : "";
            string hpBonus = codClass.BonusHp >= 0 ? $"+{codClass.BonusHp}" : $"{codClass.BonusHp}";
            
            var classRef = codClass;
            menu.AddMenuOption($"{codClass.Name} (HP: {hpBonus}){selected}", (p, opt) =>
            {
                SelectClass(p, classRef);
            });
        }

        menu.ExitButton = true;
        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    private void SelectClass(CCSPlayerController player, CodClass codClass)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer)) return;

        var oldClass = codPlayer.SelectedClass;
        codPlayer.SelectedClass = codClass;

        // Reset skill allocations when changing class
        if (oldClass?.Id != codClass.Id)
        {
            codPlayer.SkillAllocations.Clear();
        }

        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Class selected: {ChatColors.Green}{codClass.Name}");
        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} {codClass.Description}");
        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Skills: {string.Join(", ", codClass.Skills.Select(s => s.Name))}");

        if (player.PawnIsAlive)
        {
            player.PrintToChat($" {ChatColors.Yellow}[CoD Mod]{ChatColors.Default} Bonuses will apply next spawn!");
        }
    }

    [ConsoleCommand("css_skills")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CmdSkillsMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;
        ShowSkillsMenu(player);
    }

    private void ShowSkillsMenu(CCSPlayerController player)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer))
        {
            player.PrintToChat($" {ChatColors.Red}[CoD Mod]{ChatColors.Default} Error loading data!");
            return;
        }

        if (codPlayer.SelectedClass == null)
        {
            player.PrintToChat($" {ChatColors.Red}[CoD Mod]{ChatColors.Default} Select a class first with !class");
            return;
        }

        var menu = new CenterHtmlMenu($"Skills (Points: {codPlayer.SkillPoints})", this);

        foreach (var skill in codPlayer.SelectedClass.Skills)
        {
            int currentLevel = codPlayer.GetSkillLevel(skill.Type);
            string levelDisplay = $"[{currentLevel}/{skill.MaxLevel}]";
            string canUpgrade = currentLevel < skill.MaxLevel && codPlayer.SkillPoints > 0 ? " +" : "";

            var skillRef = skill;
            menu.AddMenuOption($"{skill.Name} {levelDisplay}{canUpgrade}", (p, opt) =>
            {
                UpgradeSkill(p, skillRef);
                ShowSkillsMenu(p);
            });
        }

        menu.AddMenuOption($"Reset All ({codPlayer.GetTotalAllocatedPoints()} pts)", (p, opt) =>
        {
            ResetSkills(p);
            ShowSkillsMenu(p);
        });

        menu.ExitButton = true;
        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    private void UpgradeSkill(CCSPlayerController player, CodSkill skill)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer)) return;

        int currentLevel = codPlayer.GetSkillLevel(skill.Type);

        if (currentLevel >= skill.MaxLevel)
        {
            player.PrintToChat($" {ChatColors.Red}[CoD Mod]{ChatColors.Default} {skill.Name} is already maxed!");
            return;
        }

        if (codPlayer.SkillPoints <= 0)
        {
            player.PrintToChat($" {ChatColors.Red}[CoD Mod]{ChatColors.Default} No skill points available! Level up to earn more.");
            return;
        }

        codPlayer.SkillPoints--;
        codPlayer.SkillAllocations[skill.Type] = currentLevel + 1;

        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} {skill.Name} upgraded to level {ChatColors.Green}{currentLevel + 1}");
        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Remaining skill points: {ChatColors.Yellow}{codPlayer.SkillPoints}");
    }

    private void ResetSkills(CCSPlayerController player)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer)) return;

        int pointsReturned = codPlayer.GetTotalAllocatedPoints();
        codPlayer.SkillPoints += pointsReturned;
        codPlayer.SkillAllocations.Clear();

        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Skills reset! {ChatColors.Green}{pointsReturned}{ChatColors.Default} points returned.");
        player.PrintToChat($" {ChatColors.Gold}[CoD Mod]{ChatColors.Default} Total skill points: {ChatColors.Yellow}{codPlayer.SkillPoints}");
    }

    [ConsoleCommand("css_codstats")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CmdStats(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;
        ShowStatsMenu(player);
    }

    private void ShowStatsMenu(CCSPlayerController player)
    {
        if (!_players.TryGetValue(player.SteamID, out var codPlayer))
        {
            player.PrintToChat($" {ChatColors.Red}[CoD Mod]{ChatColors.Default} Error loading data!");
            return;
        }

        var menu = new CenterHtmlMenu("Your Stats", this);
        
        menu.AddMenuOption($"Level: {codPlayer.Level}/{MAX_LEVEL}", (p, opt) => { }, true);
        menu.AddMenuOption($"XP: {codPlayer.Xp}/{GetXpForNextLevel(codPlayer.Level)}", (p, opt) => { }, true);
        menu.AddMenuOption($"Skill Points: {codPlayer.SkillPoints}", (p, opt) => { }, true);
        menu.AddMenuOption($"Class: {codPlayer.SelectedClass?.Name ?? "None"}", (p, opt) => { }, true);

        if (codPlayer.SelectedClass != null)
        {
            foreach (var skill in codPlayer.SelectedClass.Skills)
            {
                int level = codPlayer.GetSkillLevel(skill.Type);
                if (level > 0)
                {
                    menu.AddMenuOption($"{skill.Name}: {level}/{skill.MaxLevel}", (p, opt) => { }, true);
                }
            }
        }

        menu.ExitButton = true;
        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    [ConsoleCommand("css_classes")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CmdListClasses(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;
        ShowClassesInfo(player);
    }

    private void ShowClassesInfo(CCSPlayerController player)
    {
        var menu = new CenterHtmlMenu("Available Classes", this);

        foreach (var codClass in _classes)
        {
            string hpBonus = codClass.BonusHp >= 0 ? $"+{codClass.BonusHp}" : $"{codClass.BonusHp}";

            var classRef = codClass;
            menu.AddMenuOption($"{codClass.Name} (HP:{hpBonus})", (p, opt) =>
            {
                ShowClassDetails(p, classRef);
            });
        }

        menu.ExitButton = true;
        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    private void ShowClassDetails(CCSPlayerController player, CodClass codClass)
    {
        var menu = new CenterHtmlMenu(codClass.Name, this);
        
        menu.AddMenuOption(codClass.Description, (p, opt) => { }, true);
        menu.AddMenuOption($"HP: {(codClass.BonusHp >= 0 ? "+" : "")}{codClass.BonusHp}", (p, opt) => { }, true);
        menu.AddMenuOption($"Armor: +{codClass.BonusArmor}", (p, opt) => { }, true);
        
        if (Math.Abs(codClass.SpeedMultiplier - 1.0f) > 0.01f)
            menu.AddMenuOption($"Speed: {(codClass.SpeedMultiplier * 100 - 100):+0;-0}%", (p, opt) => { }, true);
        
        foreach (var skill in codClass.Skills)
        {
            menu.AddMenuOption($"- {skill.Name}", (p, opt) => { }, true);
        }

        menu.AddMenuOption("< Back", (p, opt) => ShowClassesInfo(p));
        menu.ExitButton = true;
        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    [ConsoleCommand("css_resetskills")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void CmdResetSkills(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;
        ResetSkills(player);
    }
    #endregion
}

#region Data Classes
public class CodPlayer
{
    public ulong SteamId { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public int SkillPoints { get; set; }
    public CodClass? SelectedClass { get; set; }
    public Dictionary<SkillType, int> SkillAllocations { get; set; }

    public CodPlayer(ulong steamId, string name)
    {
        SteamId = steamId;
        Name = name;
        Level = 1;
        Xp = 0;
        SkillPoints = 1; // Start with 1 skill point
        SelectedClass = null;
        SkillAllocations = new Dictionary<SkillType, int>();
    }

    public int GetSkillLevel(SkillType type)
    {
        return SkillAllocations.TryGetValue(type, out var level) ? level : 0;
    }

    public int GetTotalAllocatedPoints()
    {
        return SkillAllocations.Values.Sum();
    }
}

public class CodClass
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public List<CodSkill> Skills { get; }
    public int BonusHp { get; }
    public int BonusArmor { get; }
    public float SpeedMultiplier { get; }

    public CodClass(int id, string name, string description, List<CodSkill> skills, 
        int bonusHp = 0, int bonusArmor = 0, float speedMultiplier = 1.0f)
    {
        Id = id;
        Name = name;
        Description = description;
        Skills = skills;
        BonusHp = bonusHp;
        BonusArmor = bonusArmor;
        SpeedMultiplier = speedMultiplier;
    }
}

public class CodSkill
{
    public string Name { get; }
    public string Description { get; }
    public SkillType Type { get; }
    public int MaxLevel { get; }
    public int ValuePerLevel { get; }

    public CodSkill(string name, string description, SkillType type, int maxLevel, int valuePerLevel)
    {
        Name = name;
        Description = description;
        Type = type;
        MaxLevel = maxLevel;
        ValuePerLevel = valuePerLevel;
    }
}

public enum SkillType
{
    HpRegen,
    BonusArmor,
    FastReload,
    WallhackChance,
    ReducedRecoil,
    RadarInvisible,
    HealTeam,
    RespawnBoost,
    Vampire,
    SpeedBoost,
    LongJump,
    SilentSteps
}
#endregion
