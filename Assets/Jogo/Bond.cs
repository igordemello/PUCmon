using System;
using System.Globalization;
using UnityEngine;

/// <summary>
/// The player's friendship with one captured PUCmon, as Supabase keeps it (a row of "capturas"; field names are its
/// columns). Hunger and mood are saved as of <see cref="atualizado_em"/> and drop with time from there, by the same
/// rule as the "cuidar" function in Supabase/2-colecao.sql.
/// </summary>
[Serializable]
public class Bond
{
    /// <summary>Total affinity needed for each friendship level (level 0 needs none).</summary>
    public static readonly int[] Levels = { 0, 100, 300, 600, 1000, 1500 };
    public static readonly string[] LevelNames = { "Desconfiado", "Curioso", "Conhecido", "Colega", "Amigo", "Inseparável" };
    public const int MaxLevel = 5;
    const float DropPerHour = 100f / 24f;  // full to empty in a day

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
    public float Fullness => Drop(fome);
    public float Mood => Drop(humor);
    public bool NeedsCare => Fullness < 30f || Mood < 30f;

    float Drop(float saved)
    {
        if (string.IsNullOrEmpty(atualizado_em) || !DateTimeOffset.TryParse(atualizado_em, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var since))
            return saved;
        return Mathf.Max(0f, saved - (float)(DateTimeOffset.UtcNow - since).TotalHours * DropPerHour);
    }
}
