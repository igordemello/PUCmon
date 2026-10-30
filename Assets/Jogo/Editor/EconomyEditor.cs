using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The Economia asset's Inspector: its fields under their plain names, how long a PUCmon takes to reach each
/// friendship level with these numbers, and the button that makes the server's half (Supabase/4-economia.sql: the
/// "cuidar" function and the new capture's hunger and mood) and copies it to paste into Supabase.
/// </summary>
[CustomEditor(typeof(Economy))]
class EconomyEditor : Editor
{
    const string SqlFile = "Supabase/4-economia.sql";
    const float Performance = 0.6f;  // the estimate's player catches 3 of the 5 balls
    string saved;

    void OnEnable() => saved = File.Exists(SqlFile) ? File.ReadAllText(SqlFile) : null;

    static string Lf(string text) => text?.Replace("\r\n", "\n");  // git may check either out with CRLF

    public override void OnInspectorGUI()
    {
        var economy = (Economy)target;
        EditorGUILayout.HelpBox("Os números do cuidado no Laboratório. Afinidade é o \"XP\" da amizade; fome e humor vão " +
            "de 0 a 100. Mude à vontade: lá embaixo aparece quanto tempo um PUCmon leva para chegar a cada nível.", MessageType.Info);
        EditorGUIUtility.labelWidth = Mathf.Max(200f, EditorGUIUtility.currentViewWidth * 0.55f);
        DrawDefaultInspector();

        EditorGUILayout.Space();
        var reach = Estimate(economy);
        var text = new StringBuilder("Tempo desde o registro, cuidando sempre que rende afinidade (e acertando 3 de 5 bolinhas):");
        for (int i = 0; i < reach.Length; i++)
            text.Append($"\n   Nível {i + 1} ({Bond.LevelNames[i + 1]}): {(reach[i] < 0f ? "não chega em 30 dias" : $"{(int)(reach[i] / 60f)}h{(int)(reach[i] % 60f):00}")}");
        EditorGUILayout.HelpBox(text.ToString(), MessageType.None);

        string sql = Sql(economy);
        if (Lf(sql) != Lf(saved))
            EditorGUILayout.HelpBox("Você mudou números que o servidor usa. Clique no botão abaixo e rode o SQL no Supabase; " +
                "até lá, o jogo continua com os números antigos.", MessageType.Warning);
        if (GUILayout.Button("Atualizar o servidor (copia o SQL)", GUILayout.Height(30f)))
        {
            File.WriteAllText(SqlFile, sql);
            saved = sql;
            EditorGUIUtility.systemCopyBuffer = sql;
            EditorUtility.DisplayDialog("Economia", $"SQL copiado (e salvo em {SqlFile}).\n\nAgora abra o Supabase > SQL Editor > " +
                "New query, cole (Ctrl+V) e clique em Run.", "OK");
        }
    }

    /// <summary>
    /// Minutes from registering to each friendship level (-1: not within 30 days) for a player who cares whenever it
    /// earns affinity, by the server's rules (<see cref="Sql"/>), checked once a minute.
    /// </summary>
    internal static float[] Estimate(Economy e)
    {
        var reach = new[] { -1f, -1f, -1f, -1f, -1f };
        float hunger = e.newHunger, mood = e.newMood, lastPet = float.MinValue;
        int affinity = 0, level = 0;
        for (float minute = 0f; minute < 30f * 24f * 60f && level < Bond.MaxLevel; minute++)
        {
            if (hunger < e.feedBelow)
            {
                affinity += e.feedAffinity;
                hunger = Mathf.Min(100f, hunger + e.feedFill);
            }
            if (mood < e.playBelow)
            {
                affinity += Mathf.RoundToInt(e.playAffinityMin + (e.playAffinityMax - e.playAffinityMin) * Performance);
                mood = Mathf.Min(100f, mood + e.playMoodMin + (e.playMoodMax - e.playMoodMin) * Performance);
            }
            if (minute - lastPet >= e.petMinutes)
            {
                affinity += e.petAffinity;
                mood = Mathf.Min(100f, mood + e.petMood);
                lastPet = minute;
            }
            while (level < Bond.MaxLevel && affinity >= e.Levels[level + 1])
                reach[level++] = minute;
            hunger = Mathf.Max(0f, hunger - 100f / (e.hungerHours * 60f));
            mood = Mathf.Max(0f, mood - 100f / (e.moodHours * 60f));
        }
        return reach;
    }

    static string N(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>The server's half of the economy: run in Supabase, after 2-colecao.sql (again after every change).</summary>
    internal static string Sql(Economy e) => $@"-- PUCmon, Economia: os números do cuidado no Laboratório, para o servidor. GERADO PELO UNITY, não edite aqui:
-- mude no asset Assets/Jogo/Resources/Economia, clique em ""Atualizar o servidor"" e rode este arquivo no Supabase
-- (SQL Editor > New query > Run), depois do 2-colecao.sql. Pode rodar quantas vezes precisar.

-- Com quanto de fome e humor um PUCmon chega ao laboratório.
alter table public.capturas
  alter column fome set default {N(e.newHunger)},
  alter column humor set default {N(e.newMood)};

-- Um cuidado com o PUCmon p_criatura: 'alimentar', 'brincar' (p_desempenho de 0 a 1 no minijogo) ou 'carinho'.
-- A fome zera em {N(e.hungerHours)} h e o humor em {N(e.moodHours)} h. A afinidade só sobe quando o cuidado era preciso
-- (alimentar quem está com fome, brincar com quem está desanimado, um carinho a cada {N(e.petMinutes)} min).
create or replace function public.cuidar(p_criatura text, p_tipo text, p_desempenho real default 1)
returns json language plpgsql security definer set search_path = public as $$
declare
  c capturas;
  horas real;
  ganho int := 0;
begin
  select * into c from capturas where user_id = auth.uid() and criatura = p_criatura for update;
  if not found then
    raise exception 'PUCmon não capturado: %', p_criatura;
  end if;
  horas := extract(epoch from now() - c.atualizado_em) / 3600;
  c.fome := greatest(0, c.fome - horas * 100 / {N(e.hungerHours)});
  c.humor := greatest(0, c.humor - horas * 100 / {N(e.moodHours)});
  p_desempenho := least(1, greatest(0, coalesce(p_desempenho, 0)));

  if p_tipo = 'alimentar' then
    if c.fome < {N(e.feedBelow)} then ganho := {e.feedAffinity}; end if;
    c.fome := least(100, c.fome + {N(e.feedFill)});
  elsif p_tipo = 'brincar' then
    if c.humor < {N(e.playBelow)} then ganho := round({e.playAffinityMin} + {e.playAffinityMax - e.playAffinityMin} * p_desempenho); end if;
    c.humor := least(100, c.humor + {N(e.playMoodMin)} + {N(e.playMoodMax - e.playMoodMin)} * p_desempenho);
  elsif p_tipo = 'carinho' then
    if c.carinho_em is null or now() - c.carinho_em >= {N(e.petMinutes)} * interval '1 minute' then
      ganho := {e.petAffinity};
      c.carinho_em := now();
    end if;
    c.humor := least(100, c.humor + {N(e.petMood)});
  else
    raise exception 'Cuidado desconhecido: %', p_tipo;
  end if;

  update capturas
     set afinidade = c.afinidade + ganho, fome = c.fome, humor = c.humor, atualizado_em = now(), carinho_em = c.carinho_em
   where user_id = c.user_id and criatura = c.criatura;
  return json_build_object('criatura', c.criatura, 'afinidade', c.afinidade + ganho, 'fome', c.fome, 'humor', c.humor,
                           'atualizado_em', now(), 'carinho_em', c.carinho_em, 'ganho', ganho);
end $$;
";
}

/// <summary>Draws a <see cref="LabelAttribute"/> field: its plain name, a 0..max slider when it has a max, never below 0.</summary>
[CustomPropertyDrawer(typeof(LabelAttribute))]
class LabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        var a = (LabelAttribute)attribute;
        label.text = a.text;
        if (a.max > 0f && property.propertyType == SerializedPropertyType.Float)
            EditorGUI.Slider(rect, property, 0f, a.max, label);
        else
            EditorGUI.PropertyField(rect, property, label);
        if (property.propertyType == SerializedPropertyType.Integer && property.intValue < 0)
            property.intValue = 0;
        if (property.propertyType == SerializedPropertyType.Float && property.floatValue < 0f)
            property.floatValue = 0f;
    }
}
