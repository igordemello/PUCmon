using UnityEngine;

/// <summary>
/// The numbers of the laboratory's care (the Economia asset, in Resources): how much affinity each friendship level
/// takes, how fast hunger and mood drop, and what feeding, playing and petting earn. Labels and tips are in plain
/// Portuguese for whoever balances the game. The server's half (the "cuidar" function) is made from it by the button
/// in its Inspector, as Supabase/4-economia.sql.
/// </summary>
[CreateAssetMenu(menuName = "PUCmon/Economia", fileName = "Economia")]
public class Economy : ScriptableObject
{
    [Header("Amizade: afinidade total para chegar em cada nível")]
    [Label("Nível 1 (Curioso)")] public int level1 = 50;
    [Label("Nível 2 (Conhecido)")] public int level2 = 120;
    [Label("Nível 3 (Colega)")] public int level3 = 220;
    [Label("Nível 4 (Amigo)")] public int level4 = 340;
    [Label("Nível 5 (Inseparável)")] public int level5 = 460;

    [Header("Fome e humor (0 a 100; caem com o tempo, mesmo com o jogo fechado)")]
    [Label("Horas para a fome zerar"), Tooltip("Quanto tempo um PUCmon de barriga cheia (100) leva para ficar morrendo de fome (0).")]
    public float hungerHours = 4f;
    [Label("Horas para o humor zerar"), Tooltip("Quanto tempo um PUCmon feliz (100) leva para ficar totalmente desanimado (0).")]
    public float moodHours = 4f;
    [Label("Fome logo depois do registro", 100f), Tooltip("Com quanto na barra de fome ele chega ao laboratório.")]
    public float newHunger = 60f;
    [Label("Humor logo depois do registro", 100f), Tooltip("Com quanto na barra de humor ele chega (bravo, porque levou pedrada).")]
    public float newMood = 20f;
    [Label("Avisar \"precisa de cuidado\" abaixo de", 100f), Tooltip("O laboratório marca o PUCmon quando a fome ou o humor fica abaixo disto.")]
    public float careWarning = 30f;

    [Header("Alimentar")]
    [Label("Afinidade por refeição")] public int feedAffinity = 30;
    [Label("Quanto a refeição enche a fome", 100f)] public float feedFill = 30f;
    [Label("Só rende afinidade com fome abaixo de", 100f), Tooltip("Alimentar quem já está cheio não rende nada (evita ficar tocando sem parar).")]
    public float feedBelow = 80f;

    [Header("Brincar (minijogo das 5 bolinhas)")]
    [Label("Afinidade errando todas")] public int playAffinityMin = 20;
    [Label("Afinidade acertando todas"), Tooltip("Acertando só algumas, fica no meio do caminho.")] public int playAffinityMax = 40;
    [Label("Quanto alegra errando todas", 100f)] public float playMoodMin = 20f;
    [Label("Quanto alegra acertando todas", 100f)] public float playMoodMax = 40f;
    [Label("Só rende afinidade com humor abaixo de", 100f), Tooltip("Brincar com quem já está animado não rende nada.")]
    public float playBelow = 80f;

    [Header("Carinho (passar o dedo no PUCmon)")]
    [Label("Afinidade por carinho")] public int petAffinity = 5;
    [Label("Minutos entre carinhos que rendem"), Tooltip("Carinho antes disso ele gosta, mas não rende afinidade.")]
    public float petMinutes = 10f;
    [Label("Quanto o carinho alegra", 100f)] public float petMood = 2f;

    static Economy current;
    int[] levels;

    /// <summary>The game's economy: Resources/Economia.</summary>
    public static Economy Current => current ? current : current = Resources.Load<Economy>("Economia");

    /// <summary>Total affinity for each friendship level, 0 to 5 (level 0 needs none).</summary>
    public int[] Levels => levels ??= new[] { 0, level1, level2, level3, level4, level5 };

    void OnValidate()
    {
        level1 = Mathf.Max(1, level1);  // each level a step above the last
        level2 = Mathf.Max(level1 + 1, level2);
        level3 = Mathf.Max(level2 + 1, level3);
        level4 = Mathf.Max(level3 + 1, level4);
        level5 = Mathf.Max(level4 + 1, level5);
        hungerHours = Mathf.Max(0.1f, hungerHours);
        moodHours = Mathf.Max(0.1f, moodHours);
        levels = null;
    }
}

/// <summary>Shows a field under this name in the Inspector (with a 0..max slider when max is given) instead of its code name.</summary>
public class LabelAttribute : PropertyAttribute
{
    public readonly string text;
    public readonly float max;
    public LabelAttribute(string text, float max = 0f) => (this.text, this.max) = (text, max);
}
