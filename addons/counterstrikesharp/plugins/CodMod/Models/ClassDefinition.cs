namespace CodMod.Models;

/// <summary>
/// Bit flags deklarujące wbudowane zdolności klasy.
/// Nowa klasa z istniejącą zdolnością = tylko wpis w CodClasses, zero zmian w ClassAbilities.
/// Nowy TYP zdolności = flaga tutaj + implementacja w ClassAbilities + wywołanie w CodMod.
/// </summary>
[Flags]
public enum ClassAbilityFlags
{
    None                = 0,
    KnifeInstakill      = 1 << 0,  // prawy klik nożem zabija natychmiastowo
    DoubleJump          = 1 << 1,  // podwójny skok w powietrzu
    StealthInvisibility = 1 << 2,  // niewidzialny gdy stoi w miejscu
    HealOnKill          = 1 << 3,  // regeneruje HP po zabiciu
    BerserkOnLowHp      = 1 << 4,  // speed boost gdy HP ≤ 40
    CrouchStealth       = 1 << 5,  // niewidzialny podczas kucania
    HeadshotInstakill   = 1 << 6,  // X% szansy na instant kill z HS (specyficzna broń)
    InstakillChance     = 1 << 7,  // X% szansy na instant kill z każdego trafienia
}

public class ClassDefinition
{
    public string Name { get; }
    public int BaseHealth { get; }
    public float BaseSpeed { get; }
    public float BaseGravity { get; }
    public byte IdleAlpha { get; }
    public byte MovingAlpha { get; }
    public bool UsesTeamDefaultPistol { get; }
    public IReadOnlyList<string> Weapons { get; }
    public ClassAbilityFlags Abilities { get; }
    public string? AbilityWeapon { get; }
    public int AbilityChance { get; }
    public int AbilityHeal { get; }
    public int AbilityThreshold { get; }
    public float AbilitySpeed { get; }

    public ClassDefinition(
        string name,
        int baseHealth = 100,
        float baseSpeed = 1.0f,
        float baseGravity = 1.0f,
        byte idleAlpha = 255,
        byte movingAlpha = 255,
        bool usesTeamDefaultPistol = false,
        IReadOnlyList<string>? weapons = null,
        ClassAbilityFlags abilities = ClassAbilityFlags.None,
        string? abilityWeapon = null,
        int abilityChance = 0,
        int abilityHeal = 0,
        int abilityThreshold = 0,
        float abilitySpeed = 0f)
    {
        Name = name;
        BaseHealth = baseHealth;
        BaseSpeed = baseSpeed;
        BaseGravity = baseGravity;
        IdleAlpha = idleAlpha;
        MovingAlpha = movingAlpha;
        UsesTeamDefaultPistol = usesTeamDefaultPistol;
        Weapons = weapons ?? Array.Empty<string>();
        Abilities = abilities;
        AbilityWeapon = abilityWeapon;
        AbilityChance = abilityChance;
        AbilityHeal = abilityHeal;
        AbilityThreshold = abilityThreshold;
        AbilitySpeed = abilitySpeed;
    }
}

/// <summary>
/// Centralny rejestr klas.
/// Żeby dodać klasę: (1) const string, (2) wpis w Definitions, (3) do SelectableClassNames.
/// </summary>
public static class CodClasses
{
    public const string None             = "None";
    public const string Snajper          = "Snajper";
    public const string Komandos         = "Komandos";
    public const string StrzelecWyborowy = "Strzelec wyborowy";
    public const string Ninja            = "Ninja";
    public const string Wampir           = "Wampir";
    public const string Berserker        = "Berserker";
    public const string Widmo            = "Widmo";
    public const string Gladiator        = "Gladiator";
    public const string Egzekutor        = "Egzekutor";

    private static readonly Dictionary<string, ClassDefinition> Definitions =
        new(StringComparer.Ordinal)
    {
        [None] = new ClassDefinition(
            name:                  None,
            usesTeamDefaultPistol: true),

        [Snajper] = new ClassDefinition(
            name:       Snajper,
            baseHealth: 110,
            weapons:    new[] { "weapon_awp", "weapon_deagle" }),

        [Komandos] = new ClassDefinition(
            name:       Komandos,
            baseHealth: 105,
            baseSpeed:  1.4f,
            abilities:  ClassAbilityFlags.KnifeInstakill | ClassAbilityFlags.DoubleJump,
            weapons:    new[] { "weapon_deagle" }),

        [StrzelecWyborowy] = new ClassDefinition(
            name:        StrzelecWyborowy,
            baseHealth:  200,
            baseSpeed:   0.6f,
            baseGravity: 1.5f,
            weapons:     new[] { "weapon_ak47", "weapon_fiveseven" }),

        [Ninja] = new ClassDefinition(
            name:        Ninja,
            baseHealth:  50,
            baseSpeed:   1.1f,
            baseGravity: 0.25f,
            idleAlpha:   0,
            movingAlpha: 50,
            abilities:   ClassAbilityFlags.KnifeInstakill | ClassAbilityFlags.StealthInvisibility),

        [Wampir] = new ClassDefinition(
            name:        Wampir,
            baseHealth:  80,
            baseSpeed:   1.1f,
            abilities:   ClassAbilityFlags.HealOnKill,
            weapons:     new[] { "weapon_deagle" },
            abilityHeal: 15),

        [Berserker] = new ClassDefinition(
            name:              Berserker,
            baseHealth:        130,
            baseSpeed:         0.85f,
            abilities:         ClassAbilityFlags.BerserkOnLowHp,
            weapons:           new[] { "weapon_sg556", "weapon_p250" },
            abilityThreshold:  40,
            abilitySpeed:      1.8f),

        [Widmo] = new ClassDefinition(
            name:       Widmo,
            baseHealth: 90,
            baseSpeed:  1.05f,
            idleAlpha:  25,
            abilities:  ClassAbilityFlags.CrouchStealth,
            weapons:    new[] { "weapon_mp9", "weapon_p250" }),

        [Gladiator] = new ClassDefinition(
            name:          Gladiator,
            baseHealth:    110,
            baseSpeed:     1.0f,
            abilities:     ClassAbilityFlags.HeadshotInstakill,
            weapons:       new[] { "weapon_famas", "weapon_deagle" },
            abilityWeapon: "famas",
            abilityChance: 50),

        [Egzekutor] = new ClassDefinition(
            name:          Egzekutor,
            baseHealth:    95,
            baseSpeed:     1.35f,
            abilities:     ClassAbilityFlags.InstakillChance,
            weapons:       new[] { "weapon_tec9", "weapon_p250" },
            abilityWeapon: "tec9",
            abilityChance: 15),
    };

    public static readonly string[] SelectableClassNames =
    {
        Snajper,
        Komandos,
        StrzelecWyborowy,
        Ninja,
        Wampir,
        Berserker,
        Widmo,
        Gladiator,
        Egzekutor,
    };

    public static ClassDefinition Get(string? className)
    {
        if (className != null && Definitions.TryGetValue(className, out var definition))
            return definition;

        return Definitions[None];
    }
}
