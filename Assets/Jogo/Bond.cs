using System;
using System.Globalization;
using UnityEngine;

/// <summary>
/// The player's friendship with one captured PUCmon, as Supabase keeps it (a row of "capturas"; field names are its
/// columns). Hunger and mood are saved as of <see cref="atualizado_em"/> and drop with time from there, by the same
/// rule as the "cuidar" function in Supabase/4-economia.sql (both from the <see cref="Economy"/> asset).
/// </summary>
[Serializable]
public class Bond
{
    /// <summary>Total affinity needed for each friendship level (level 0 needs none).</summary>
    public static int[] Levels => Economy.Current.Levels;
    public static readonly string[] LevelNames = { "Desconfiado", "Curioso", "Conhecido", "Colega", "Amigo", "Inseparável" };
    public const int MaxLevel = 5;

    public string criatura;
    public int afinidade;
    public float fome = 60f, humor = 20f;  // 100 = full / happy (a new capture is a bit hungry and very upset)
    public string atualizado_em, carinho_em;
    /// <summary>Affinity the last care action earned (only in "cuidar" answers).</summary>
    public int ganho;

    public int Level
    {
        get
        {
            int level = 0;
            while (level < MaxLevel && afinidade >= Levels[level + 1])
                level++;
            return level;
        }
    }

    /// <summary>Hunger and mood right now: 0 starving / miserable .. 100 full / happy.</summary>
    public float Fullness => Drop(fome, Economy.Current.hungerHours);
    public float Mood => Drop(humor, Economy.Current.moodHours);
    public bool NeedsCare => Fullness < Economy.Current.careWarning || Mood < Economy.Current.careWarning;

    float Drop(float saved, float hoursToEmpty)
    {
        if (string.IsNullOrEmpty(atualizado_em) || !DateTimeOffset.TryParse(atualizado_em, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var since))
            return saved;
        return Mathf.Max(0f, saved - (float)(DateTimeOffset.UtcNow - since).TotalHours * 100f / hoursToEmpty);
    }
}
