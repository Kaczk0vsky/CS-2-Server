namespace CodMod.Models;

/// <summary>
/// Player data container. Designed with clear properties for future DB serialization.
/// Each player has a global rank (server-wide points) and per-class XP progression.
/// </summary>
public class CodPlayer
{
    public ulong SteamId { get; set; }
    public string Name { get; set; }

    // --- Global Server Rank (SimpleRank-style points) ---
    public int GlobalPoints { get; set; }

    // --- Per-Class Progression ---
    public string? SelectedClassName { get; set; }
    public string? PendingClassName { get; set; }

    /// <summary>
    /// Class name → ClassProgress. Tracks XP/Level per class independently.
    /// </summary>
    public Dictionary<string, ClassProgress> ClassData { get; set; } = new();

    // --- Kill Streak (resets per round) ---
    public int CurrentStreak { get; set; }
    public DateTime LastKillTime { get; set; } = DateTime.MinValue;

    public CodPlayer(ulong steamId, string name)
    {
        SteamId = steamId;
        Name = name;
    }

    /// <summary>
    /// Gets or creates the ClassProgress for a given class name.
    /// </summary>
    public ClassProgress GetClassProgress(string className)
    {
        if (!ClassData.TryGetValue(className, out var progress))
        {
            progress = new ClassProgress();
            ClassData[className] = progress;
        }
        return progress;
    }

    /// <summary>
    /// Returns the active class progress (for currently selected class), or null.
    /// </summary>
    public ClassProgress? GetActiveClassProgress()
    {
        if (SelectedClassName == null) return null;
        return GetClassProgress(SelectedClassName);
    }
}

/// <summary>
/// Per-class level/XP progression. Stored per class name in CodPlayer.ClassData.
/// </summary>
public class ClassProgress
{
    public int Level { get; set; } = 1;
    public int Xp { get; set; } = 0;
}
