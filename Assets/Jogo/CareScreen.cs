using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A captured PUCmon's pen (the collection's tamagotchi): its hunger, mood and friendship, and the ways to care for
/// it. Feed it (a snack flies to it), rub it (drag a finger over it) or play (throw paper balls for it to catch);
/// Supabase decides the affinity each earns (<see cref="Account.Care"/>). Friendship levels unlock its actions and, at
/// the last one, its passive ability, listed on its page (the sheet). Sits on the pen panel, which gets the drags
/// its children leave unhandled.
/// </summary>
public class CareScreen : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public HomeScreen home;

    [Header("Recinto")]
    public TMP_Text title, friendship, affinity, feedback;
    public Image portrait;
    [Tooltip("One per friendship level, filled up to the current one.")]
    public Image[] hearts;
    public RectTransform affinityFill, hungerFill, moodFill;
    public Button backButton, feedButton, playButton, sheetButton;
    [Tooltip("Row of care buttons, hidden while playing.")]
    public GameObject buttons;
    [Tooltip("Snack that flies to the PUCmon when it is fed.")]
    public RectTransform snack;
    public Sprite heartSprite;
    [Tooltip("Rubbing (canvas units of finger travel over the PUCmon) that makes one petting.")]
    public float rubToPet = 900f;

    [Header("Brincadeira")]
    [Tooltip("See-through, full-screen catcher of the throwing swipes, shown while playing.")]
    public GameObject playArea;
    public RectTransform ball;
    public TMP_Text playText;
    [Min(1)] public int balls = 5;
    [Tooltip("Seconds a ball takes to reach the PUCmon.")]
    public float ballSeconds = 0.45f;
    [Tooltip("How far (canvas units) and how fast it runs from side to side while playing.")]
    public float playRange = 220f, playSpeed = 2.2f;
    [Tooltip("Catch zone, as a share of the PUCmon's width.")]
    [Range(0.1f, 1f)] public float catchWidth = 0.7f;

    [Header("Subiu de nível")]
    public GameObject levelUp;
    public TMP_Text levelUpText;
    public Button levelUpOk;

    [Header("Ficha")]
    public GameObject sheet;
    public Button sheetBack;
    public Image sheetPortrait;
    public TMP_Text sheetTitle, sheetBody;

    [Header("Primeira visita")]
    [Tooltip("Plays the PUCmon's own conversation (catalog: Lab Intro) the first time its pen is opened.")]
    public Dialogue dialogue;

    static readonly Color Faded = new Color(1f, 1f, 1f, 0.22f);

    Catalog.Creature creature;
    Bond bond;
    int shownLevel;
    bool busy, playing, ballFlying, petting, petted;
    float rubbed, sayUntil, hopStart = -10f, flinchStart = -10f, playStart, nextHeart;
    int thrown, caught;
    Vector2 portraitHome, ballHome, snackHome, swipeStart;
    readonly List<(RectTransform heart, float born)> floating = new List<(RectTransform, float)>();

    void Awake()
    {
        portraitHome = portrait.rectTransform.anchoredPosition;
        ballHome = ball.anchoredPosition;
        snackHome = snack.anchoredPosition;
        backButton.onClick.AddListener(() => home.ShowCollection());
        feedButton.onClick.AddListener(() => { if (!busy) StartCoroutine(Feed()); });
        playButton.onClick.AddListener(() => { if (!busy) StartPlay(); });
        sheetButton.onClick.AddListener(ShowSheet);
        sheetBack.onClick.AddListener(() => home.Show(gameObject));
        levelUpOk.onClick.AddListener(() => levelUp.SetActive(false));
    }

    /// <summary>Opens the pen of this captured PUCmon.</summary>
    public void Open(Catalog.Creature newCreature)
    {
        creature = newCreature;
        bond = Account.Bonds.TryGetValue(creature.poster, out var saved) ? saved : new Bond { criatura = creature.poster };
        shownLevel = bond.Level;
        title.text = creature.displayName;
        portrait.sprite = creature.portrait;
        busy = false;
        StopAllCoroutines();
        EndPlay();
        levelUp.SetActive(false);
        feedback.text = "";
        home.Show(gameObject);
        if (shownLevel == 0)
            Say("Ele ainda desconfia de você...");
        string seen = $"pucmon.beltran.recinto.{Account.Username}.{creature.poster}";
        if (creature.labIntro != null && creature.labIntro.Length > 0 && PlayerPrefs.GetInt(seen) == 0)
            dialogue.Play(creature.labIntro, () =>
            {
                PlayerPrefs.SetInt(seen, 1);
                PlayerPrefs.Save();
            });
    }

    void Update()
    {
        if (creature == null)
            return;
        int level = bond.Level;
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].color = i < level ? Color.white : Faded;
        friendship.text = Bond.LevelNames[level];
        affinity.text = level < Bond.MaxLevel
            ? $"{bond.afinidade - Bond.Levels[level]}/{Bond.Levels[level + 1] - Bond.Levels[level]} até {Bond.LevelNames[level + 1]}"
            : "Amizade máxima!";
        Fill(affinityFill, level < Bond.MaxLevel ? (bond.afinidade - Bond.Levels[level]) / (float)(Bond.Levels[level + 1] - Bond.Levels[level]) : 1f);
        Fill(hungerFill, bond.Fullness / 100f);
        Fill(moodFill, bond.Mood / 100f);

        // The PUCmon breathes; hops when happy, flinches when it does not trust you yet, runs around while playing.
        float now = Time.unscaledTime;
        float hop = Bump(now - hopStart, 0.4f), flinch = Bump(now - flinchStart, 0.35f);
        float x = playing ? Mathf.Sin((now - playStart) * playSpeed) * playRange : Mathf.Sin(now * 40f) * 8f * flinch;
        portrait.rectTransform.anchoredPosition = portraitHome + new Vector2(x, Mathf.Sin(now * 2f) * 4f + hop * 50f);
        portrait.rectTransform.localScale = new Vector3(1f + hop * 0.06f, 1f - hop * 0.06f + Mathf.Sin(now * 2f) * 0.015f, 1f);

        feedback.alpha = Mathf.Clamp01((sayUntil - now) / 0.4f);
        for (int i = floating.Count - 1; i >= 0; i--)  // hearts from petting float up and fade
        {
            var (heart, born) = floating[i];
            float age = now - born;
            heart.anchoredPosition += Vector2.up * 90f * Time.unscaledDeltaTime;
            heart.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f - age);
            if (age >= 1f)
            {
                Destroy(heart.gameObject);
                floating.RemoveAt(i);
            }
        }
    }

    // ---------- feeding ----------
    IEnumerator Feed()
    {
        busy = true;
        snack.gameObject.SetActive(true);
        var from = (Vector2)Local(feedButton.transform.position);
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.55f)
        {
            snack.anchoredPosition = Vector2.Lerp(from, portrait.rectTransform.anchoredPosition, t) + Vector2.up * Mathf.Sin(t * Mathf.PI) * 160f;
            snack.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, t);
            yield return null;
        }
        snack.gameObject.SetActive(false);
        snack.anchoredPosition = snackHome;
        hopStart = Time.unscaledTime;
        Care("alimentar", 1f);
    }

    // ---------- petting and throwing (drags the panel's children do not take) ----------
    public void OnBeginDrag(PointerEventData e)
    {
        swipeStart = e.position;
        petting = !playing && !busy && e.pointerPressRaycast.gameObject == portrait.gameObject;
        rubbed = 0f;
        petted = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!petting)
            return;
        rubbed += e.delta.magnitude / Scale;
        if (Time.unscaledTime >= nextHeart)
        {
            nextHeart = Time.unscaledTime + 0.12f;
            var heart = new GameObject("Coração", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            heart.SetParent(transform, false);
            heart.GetComponent<Image>().sprite = heartSprite;
            heart.GetComponent<Image>().raycastTarget = false;
            heart.sizeDelta = new Vector2(54f, 48f);
            heart.anchoredPosition = Local(e.position);
            floating.Add((heart, Time.unscaledTime));
        }
        if (bond.Level == 0)
            flinchStart = Mathf.Max(flinchStart, Time.unscaledTime - 0.2f);
        if (rubbed >= rubToPet && !petted)
        {
            petted = true;
            hopStart = Time.unscaledTime;
            Care("carinho", 1f);
        }
    }

    public void OnEndDrag(PointerEventData e)
    {
        petting = false;
        var swipe = (e.position - swipeStart) / Scale;
        if (playing && !ballFlying && thrown < balls && swipe.y > 60f)
            StartCoroutine(Throw(Local(e.position).x));
    }

    // ---------- playing: throw paper balls for it to catch ----------
    void StartPlay()
    {
        playing = true;
        thrown = caught = 0;
        playStart = Time.unscaledTime;
        playArea.SetActive(true);
        buttons.SetActive(false);
        playText.gameObject.SetActive(true);
        playText.text = $"Deslize para cima e jogue a bolinha! {balls}";
    }

    IEnumerator Throw(float x)
    {
        ballFlying = true;
        thrown++;
        ball.gameObject.SetActive(true);
        var from = new Vector2(Mathf.Clamp(x, -playRange - 100f, playRange + 100f), ballHome.y);
        float catchY = portraitHome.y;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / ballSeconds)
        {
            ball.anchoredPosition = new Vector2(from.x, Mathf.Lerp(from.y, catchY, t));
            ball.localRotation = Quaternion.Euler(0f, 0f, t * 400f);
            yield return null;
        }
        bool hit = Mathf.Abs(from.x - portrait.rectTransform.anchoredPosition.x) <= portrait.rectTransform.rect.width * catchWidth / 2f;
        if (hit)
        {
            caught++;
            hopStart = Time.unscaledTime;
            Say("Pegou!");
        }
        else
        {
            Say("Passou longe!");
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.3f)  // flies past
            {
                ball.anchoredPosition = new Vector2(from.x, catchY + t * 300f);
                yield return null;
            }
        }
        ball.gameObject.SetActive(false);
        ball.anchoredPosition = ballHome;
        playText.text = thrown < balls ? $"Bolinhas: {balls - thrown}" : $"Ele pegou {caught} de {balls}!";
        ballFlying = false;
        if (thrown >= balls)
        {
            yield return new WaitForSecondsRealtime(0.8f);
            EndPlay();
            Care("brincar", caught / (float)balls);
        }
    }

    void EndPlay()
    {
        playing = ballFlying = false;
        playArea.SetActive(false);
        buttons.SetActive(true);
        playText.gameObject.SetActive(false);
        ball.gameObject.SetActive(false);
        snack.gameObject.SetActive(false);
    }

    // ---------- saving ----------
    async void Care(string kind, float performance)
    {
        var who = creature;
        busy = true;
        var (saved, error) = await Account.Care(who.poster, kind, performance);
        if (!this || who != creature)
            return;
        busy = false;
        if (error != null)
        {
            Say(error, 3f);
            return;
        }
        bond = saved;
        if (saved.ganho > 0)
            Say($"+{saved.ganho} de afinidade");
        else
            Say(kind == "alimentar" ? "Ele já está satisfeito." : kind == "brincar" ? "Ele já está animado." : "Ele gostou do carinho.");
        if (bond.Level > shownLevel)
            ShowLevelUp(bond.Level);
        shownLevel = bond.Level;
    }

    void ShowLevelUp(int level)
    {
        var learned = level < Bond.MaxLevel
            ? "Aprendeu uma ação nova: " + SkillName(Skill(level - 1), level)
            : "Destravou a habilidade passiva: " + SkillName(creature.passive, level);
        levelUpText.text = $"{creature.displayName} agora é seu {Bond.LevelNames[level]}!\n\n{learned}";
        levelUp.SetActive(true);
    }

    // ---------- the sheet ----------
    void ShowSheet()
    {
        sheetPortrait.sprite = creature.portrait;
        sheetTitle.text = creature.displayName;
        int level = bond.Level;
        var text = new StringBuilder();
        if (!string.IsNullOrEmpty(creature.represents))
            text.AppendLine($"<b>Representa:</b> {creature.represents}");
        text.AppendLine($"<b>Amizade:</b> {Bond.LevelNames[level]} (nível {level})");
        if (!string.IsNullOrEmpty(creature.description))
            text.AppendLine().AppendLine(creature.description);
        text.AppendLine().AppendLine("<b>AÇÕES</b>");
        for (int i = 0; i < 4; i++)  // action n is learned at friendship n
            text.AppendLine(Entry(Skill(i), i + 1, level > i));
        text.AppendLine().AppendLine("<b>HABILIDADE PASSIVA</b>");
        text.AppendLine(Entry(creature.passive, Bond.MaxLevel, level >= Bond.MaxLevel));
        sheetBody.text = Dialogue.KeepWhole(text.ToString());
        home.Show(sheet);
    }

    string Entry(Catalog.Skill skill, int unlockLevel, bool known) => known
        ? $"<b>{SkillName(skill, unlockLevel)}</b>{(string.IsNullOrEmpty(skill?.description) ? "" : ": " + skill.description)}"
        : $"<color=#8A8378>??? (amizade {unlockLevel}: {Bond.LevelNames[unlockLevel]})</color>";

    Catalog.Skill Skill(int i) => creature.actions != null && i < creature.actions.Length ? creature.actions[i] : null;

    static string SkillName(Catalog.Skill skill, int unlockLevel) =>
        !string.IsNullOrEmpty(skill?.name) ? skill.name : unlockLevel >= Bond.MaxLevel ? "a passiva" : $"Ação {unlockLevel}";

    // ---------- helpers ----------
    void Say(string text, float seconds = 1.8f)
    {
        feedback.text = text;
        sayUntil = Time.unscaledTime + seconds;
    }

    float Scale => GetComponentInParent<Canvas>().rootCanvas.scaleFactor;

    // Screen or world point -> this panel's coordinates (its children are anchored at its centre).
    Vector2 Local(Vector2 screen)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out var local);
        return local;
    }

    Vector2 Local(Vector3 world) => Local(RectTransformUtility.WorldToScreenPoint(null, world));

    static void Fill(RectTransform fill, float share) => fill.anchorMax = new Vector2(Mathf.Clamp01(share), 1f);

    // 0 -> 1 -> 0 over `seconds` after an event, 0 otherwise.
    static float Bump(float since, float seconds) => since >= 0f && since < seconds ? Mathf.Sin(since / seconds * Mathf.PI) : 0f;
}
