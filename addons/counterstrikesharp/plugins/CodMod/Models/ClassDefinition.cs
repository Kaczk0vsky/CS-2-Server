namespace CodMod.Models;

/// <summary>
/// Bit flags deklarujące wbudowane zdolności klasy.
/// Żeby dodać nową klasę korzystającą z istniejącej zdolności:
///   wystarczy wpisać flagę tutaj — zero zmian w ClassAbilities.
/// Żeby dodać NOWY TYP zdolności:
///   (1) flaga tutaj, (2) implementacja w ClassAbilities, (3) wywołanie w CodMod.
/// </summary>
[Flags]
public enum ClassAbilityFlags
{
    None                = 0,
    KnifeInstakill      = 1 << 0,  // prawy klik nożem zabija natychmiastowo
    DoubleJump          = 1 << 1,  // podwójny skok w powietrzu
    StealthInvisibility = 1 << 2,  // niewidzialny gdy stoi w miejscu
    HealOnKill          = 1 << 3,  // regeneruje HP po każdym zabiciu
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

    public ClassDefinition(
        string name,
        int baseHealth = 100,
        float baseSpeed = 1.0f,
        float baseGravity = 1.0f,
        byte idleAlpha = 255,
        byte movingAlpha = 255,
        bool usesTeamDefaultPistol = false,
        IReadOnlyList<string>? weapons = null,
        ClassAbilityFlags abilities = ClassAbilityFlags.None)
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
    }
}

/// <summary>
/// Centralny rejestr klas.
/// Żeby dodać klasę: (1) const string, (2) wpis w Definitions, (3) do SelectableClassNames.
/// Nic poza tym nie trzeba ruszać dla klas korzystających z istniejących flag.
/// </summary>
public static class CodClasses
{
    public const string None             = "None";
    public const string Snajper          = "Snajper";
    public const string Komandos         = "Komandos";
    public const string StrzelecWyborowy = "Strzelec wyborowy";
    public const string Ninja            = "Ninja";
    public const string Wampir           = "Wampir";

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

        // Wampir — niskie HP, regeneruje się z każdego zabójstwa
        [Wampir] = new ClassDefinition(
            name:       Wampir,
            baseHealth: 80,
            baseSpeed:  1.1f,
            abilities:  ClassAbilityFlags.HealOnKill,
            weapons:    new[] { "weapon_deagle", "weapon_cz75a" }),
    };

    public static readonly string[] SelectableClassNames =
    {
        Snajper,
        Komandos,
        StrzelecWyborowy,
        Ninja,
        Wampir,
    };

    public static ClassDefinition Get(string? className)
    {
        if (className != null && Definitions.TryGetValue(className, out var definition))
            return definition;

        return Definitions[None];
    }
}
