namespace CodMod.Models;

public class PerkEffects
{
    public float SpeedBonus { get; set; }
    public int HealthBonus { get; set; }
    public bool InstantKillOnHeDamage { get; set; }
}

public delegate void PerkAbility(PerkEffects effects, float argument);

public class PerkInfo
{
    public string Name { get; }
    public string Description { get; }
    public string AbilityName { get; }
    public float AbilityArgument { get; }
    public PerkAbility AbilityFunction { get; }

    public string Ability => Description;

    public PerkInfo(
        string name,
        string description,
        string abilityName,
        float abilityArgument,
        PerkAbility abilityFunction)
    {
        Name = name;
        Description = description;
        AbilityName = abilityName;
        AbilityArgument = abilityArgument;
        AbilityFunction = abilityFunction;
    }

    public void Apply(PerkEffects effects)
    {
        AbilityFunction(effects, AbilityArgument);
    }
}

public static class PerkAbilities
{
    public static void AddHp(PerkEffects effects, float amount)
    {
        effects.HealthBonus += (int)MathF.Round(amount);
    }

    public static void AddSpeed(PerkEffects effects, float amount)
    {
        effects.SpeedBonus += amount / 100f;
    }

    public static void InstantKillHeGrenade(PerkEffects effects, float enabled)
    {
        effects.InstantKillOnHeDamage = enabled > 0;
    }
}

public static class Perks
{
    private static readonly Random Rng = new();

    public static readonly IReadOnlyList<PerkInfo> Pool =
    [
        new PerkInfo("Sandaly Mojżesza", "Dodaje 50 Speeda", "add_speed", 50f, PerkAbilities.AddSpeed),
        new PerkInfo("Betonowe gacie", "Dodaje 50 HP", "add_hp", 50f, PerkAbilities.AddHp),
        new PerkInfo("Tajemnica generala", "Natychmiastowo zabija granatem HE", "he_insta_kill", 1f, PerkAbilities.InstantKillHeGrenade)
    ];

    public static PerkInfo GetRandom()
    {
        return Pool[Rng.Next(Pool.Count)];
    }

    public static PerkInfo? FindByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return Pool.FirstOrDefault(p => p.Name.Equals(name, StringComparison.Ordinal));
    }

    public static float GetSpeedBonus(string? name)
    {
        var effects = GetEffects(name);
        return effects.SpeedBonus;
    }

    public static int GetHealthBonus(string? name)
    {
        var effects = GetEffects(name);
        return effects.HealthBonus;
    }

    public static bool HasHeGrenadeInstantKill(string? name)
    {
        var effects = GetEffects(name);
        return effects.InstantKillOnHeDamage;
    }

    public static PerkEffects GetEffects(string? name)
    {
        var effects = new PerkEffects();
        var perk = FindByName(name);
        perk?.Apply(effects);
        return effects;
    }
}
