namespace CodMod.Models;

/// <summary>
/// Represents a single rank tier in the global ranking system.
/// Ported from SimpleRank (K4-System CS:GO style).
/// </summary>
public class RankInfo
{
    /// <summary>Minimum points required for this rank. -1 for the default/unranked tier.</summary>
    public int Points { get; }
    public string Name { get; }
    public string Tag { get; }
    public string Color { get; }

    public RankInfo(int points, string name, string tag, string color)
    {
        Points = points;
        Name = name;
        Tag = tag;
        Color = color;
    }
}
