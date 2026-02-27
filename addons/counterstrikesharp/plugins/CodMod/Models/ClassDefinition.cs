namespace CodMod.Models;

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

    public ClassDefinition(
        string name,
        int baseHealth = 100,
        float baseSpeed = 1.0f,
        float baseGravity = 1.0f,
        byte idleAlpha = 255,
        byte movingAlpha = 255,
        bool usesTeamDefaultPistol = false,
        IReadOnlyList<string>? weapons = null)
    {
        Name = name;
        BaseHealth = baseHealth;
        BaseSpeed = baseSpeed;
        BaseGravity = baseGravity;
        IdleAlpha = idleAlpha;
        MovingAlpha = movingAlpha;
        UsesTeamDefaultPistol = usesTeamDefaultPistol;
        Weapons = weapons ?? Array.Empty<string>();
    }
}

public static class CodClasses
{
    public const string None = "None";
    public const string Snajper = "Snajper";
    public const string Komandos = "Komandos";
    public const string StrzelecWyborowy = "Strzelec wyborowy";
    public const string Ninja = "Ninja";

    private static readonly Dictionary<string, ClassDefinition> Definitions = new(StringComparer.Ordinal)
    {
        [None] = new ClassDefinition(
            name: None,
            usesTeamDefaultPistol: true),

        [Snajper] = new ClassDefinition(
            name: Snajper,
            baseHealth: 110,
            weapons: new[] { "weapon_awp", "weapon_deagle" }),

        [Komandos] = new ClassDefinition(
            name: Komandos,
            baseHealth: 105,
            baseSpeed: 1.4f,
            weapons: new[] { "weapon_deagle" }),

        [StrzelecWyborowy] = new ClassDefinition(
            name: StrzelecWyborowy,
            baseHealth: 200,
            baseSpeed: 0.6f,
            baseGravity: 1.5f,
            weapons: new[] { "weapon_ak47", "weapon_fiveseven" }),

        [Ninja] = new ClassDefinition(
            name: Ninja,
            baseHealth: 50,
            baseSpeed: 1.1f,
            baseGravity: 0.25f,
            idleAlpha: 0,
            movingAlpha: 50)
    };

    public static readonly string[] SelectableClassNames =
    {
        Snajper,
        Komandos,
        StrzelecWyborowy,
        Ninja
    };

    public static ClassDefinition Get(string? className)
    {
        if (className != null && Definitions.TryGetValue(className, out var definition))
            return definition;

        return Definitions[None];
    }
}
