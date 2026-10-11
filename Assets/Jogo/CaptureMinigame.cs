using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The capture minigame. While a PUCmon not caught yet is on screen it lives in its wall hole (sways, hides now and
/// then), a ring shrinks over it, and the player swipes up to throw a student card at it. A hit holds it while the card
/// shakes; it stays caught with a chance that grows the smaller the ring was at the throw. All the numbers that set the
/// difficulty are here, plus catchRate / agility / shyness per PUCmon in the catalog.
/// Sits on a full-screen, see-through UI image of the AR interface that catches the swipes.
/// </summary>
public class CaptureMinigame : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Tentativas")]
    [Tooltip("Cards per attempt.")]
    [Min(1)] public int throws = 3;
    [Tooltip("Seconds the PUCmon stays away after an attempt runs out of cards; then a new attempt starts.")]
    public float fleeSeconds = 6f;
    [Tooltip("Seconds a result message (\"Errou!\", \"Escapou!\"...) stays on screen.")]
    public float messageSeconds = 1.4f;

    [Header("Anel")]
    [Tooltip("Seconds for the colored ring to shrink from the white circle down to its smallest; then it starts over.")]
    public float ringSeconds = 1.6f;
    [Tooltip("Smallest size of the colored ring, relative to the white circle.")]
    [Range(0.1f, 0.9f)] public float ringSmallest = 0.3f;
    [Tooltip("Size of the white circle (where a card must land to hit), relative to the PUCmon's size.")]
    public float hitArea = 0.8f;

    [Header("Captura")]
    [Tooltip("Extra catch chance for a small ring: chance = catchRate x (1 + bonus x how small the ring was, 0 to 1).")]
    public float ringBonus = 1f;
    [Tooltip("Times the card shakes before the result (a PUCmon that breaks free may leave earlier).")]
    [Range(1, 5)] public int shakes = 3;
    [Tooltip("Seconds per shake.")]
    public float shakeSeconds = 0.55f;

    [Header("Lançamento")]
    [Tooltip("Shortest upward swipe that throws, as a fraction of the screen height.")]
    public float minSwipe = 0.08f;
    [Tooltip("Slower swipes (screen heights per second) fall short of the PUCmon.")]
    public float minSwipeSpeed = 0.6f;
    [Tooltip("Seconds the card flies.")]
    public float flightSeconds = 0.55f;
    [Tooltip("How high the card arcs, in poster units (the poster is 2 tall).")]
    public float arc = 0.5f;
    [Tooltip("Card width in poster units.")]
    public float cardSize = 0.4f;

    [Header("Comportamento do PUCmon")]
    [Tooltip("Seconds between hides (random in this range) for shyness 1.")]
    public Vector2 hideEvery = new Vector2(2.5f, 6f);
    [Tooltip("Seconds it stays hidden (random in this range).")]
    public Vector2 hiddenFor = new Vector2(0.6f, 1.4f);
    [Tooltip("Seconds to go into or come out of the hole.")]
    public float hideMove = 0.3f;
    [Tooltip("How far into the hole it goes, in poster units.")]
    public float hideDepth = 1.5f;
    [Tooltip("Sideways sway in poster units.")]
    public float sway = 0.1f;
    [Tooltip("Sway speed for agility 1.")]
    public float swaySpeed = 1.3f;

    [Header("Referências")]
    public Camera arCamera;
    public Sprite cardSprite;
    public RectTransform hitCircle, ring, handCard;
    public Image ringImage;
    [Tooltip("Boxes shown while aiming (the swipe hint, the cards left) and after a throw (the result message).")]
    public GameObject hintBox, throwsBox, messageBox;
    public TMP_Text throwsLeft, message;
    public Color ringBig = new Color32(0x5E, 0xAF, 0xD3, 0xFF), ringSmall = new Color32(0xE0, 0xB8, 0x4F, 0xFF);

    /// <summary>Raised when a throw catches the PUCmon.</summary>
    public event System.Action<Catalog.Creature> Caught;

    ImageTarget target;
    Catalog.Creature creature;
    readonly List<Transform> body = new List<Transform>();  // the poster's children that make up the PUCmon (not its hole)
    readonly List<(Vector3 position, Vector3 scale)> home = new List<(Vector3, Vector3)>();
    Vector2 center;  // PUCmon's middle on the poster
    float radius;  // and its size there
    readonly Dictionary<string, int> cardsLeft = new Dictionary<string, int>();
    readonly Dictionary<string, float> awayUntil = new Dictionary<string, float>();
    float ringStart, nextHide, hiddenUntil, hidden, inside, messageUntil;  // hidden: 0 out .. 1 in the hole; inside: 1 = in the card
    bool busy, pressing;
    Vector2 pressAt, handHome;
    float pressTime;
    SpriteRenderer card;

    void Awake() => handHome = handCard.anchoredPosition;

    /// <summary>Plays with this PUCmon on its poster; null stops (poster out of view, already caught...).</summary>
    public void Play(ImageTarget newTarget, Catalog.Creature newCreature)
    {
        if (newTarget == target && newCreature == creature)
            return;
        Stop();
        if (!newTarget || newCreature == null)
            return;
        target = newTarget;
        creature = newCreature;
        foreach (Transform child in target.transform)
            if (!child.GetComponent<WallHole>())
            {
                body.Add(child);
                home.Add((child.localPosition, child.localScale));
            }
        Measure();
        if (!cardsLeft.ContainsKey(creature.poster))
            cardsLeft[creature.poster] = throws;
        ringStart = Time.time;
        nextHide = Time.time + NextHideDelay();
    }

    void Stop()
    {
        StopAllCoroutines();
        for (int i = 0; i < body.Count; i++)
            if (body[i])
            {
                body[i].localPosition = home[i].position;
                body[i].localScale = home[i].scale;
            }
        body.Clear();
        home.Clear();
        if (card)
            Destroy(card.gameObject);
        target = null;
        creature = null;
        busy = pressing = false;
        hidden = inside = 0f;
        handCard.anchoredPosition = handHome;
    }

    void Update()
    {
        bool on = target;
        float now = Time.time;
        bool away = on && awayUntil.TryGetValue(creature.poster, out var until) && now < until;
        if (on && !away && awayUntil.Remove(creature.poster))
            cardsLeft[creature.poster] = throws;  // back for a new attempt
        bool idle = on && !away && !busy;
        Toggle(hitCircle.gameObject, on && !away && inside == 0f);
        Toggle(ring.gameObject, on && !away && inside == 0f);
        Toggle(handCard.gameObject, idle);
        Toggle(throwsBox, idle);
        Toggle(hintBox, idle && !pressing);
        Toggle(messageBox, now < messageUntil);
        if (!on)
            return;

        // Hide in the hole now and then (always while away), unless held in the card.
        if (now >= nextHide && !away)
        {
            hiddenUntil = now + Random.Range(hiddenFor.x, hiddenFor.y);
            nextHide = hiddenUntil + NextHideDelay();
        }
        hidden = Mathf.MoveTowards(hidden, away || now < hiddenUntil ? 1f : 0f, Time.deltaTime / Mathf.Max(0.01f, hideMove));
        var offset = Offset(now);
        for (int i = 0; i < body.Count; i++)
        {
            body[i].localPosition = home[i].position + offset;
            body[i].localScale = home[i].scale * (1f - inside);
        }

        // The white circle and the shrinking ring over the PUCmon, sized as it shows on screen.
        var canvas = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
        var middle = target.transform.TransformPoint((Vector3)center + offset);
        Vector2 onScreen = arCamera.WorldToScreenPoint(middle);
        float pixels = Vector2.Distance(onScreen, arCamera.WorldToScreenPoint(middle + target.transform.right * radius * hitArea)) * 2f;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, onScreen, null, out var local);
        float size = pixels / canvas.GetComponent<Canvas>().scaleFactor;  // screen pixels -> canvas units
        float t = RingT();
        hitCircle.anchoredPosition = ring.anchoredPosition = local;
        hitCircle.sizeDelta = Vector2.one * size;
        ring.sizeDelta = Vector2.one * size * Mathf.Lerp(1f, ringSmallest, t);
        ringImage.color = Color.Lerp(ringBig, ringSmall, t);
        throwsLeft.text = "x" + cardsLeft[creature.poster];
    }

    // ---------- throwing ----------
    public void OnPointerDown(PointerEventData e)
    {
        if (!target || busy || Away())
            return;
        pressing = true;
        pressAt = e.position;
        pressTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!pressing)
            return;
        var canvas = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, e.position, null, out var local);
        handCard.anchoredPosition = local;  // the card follows the finger (anchored to the canvas centre)
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!pressing)
            return;
        pressing = false;
        handCard.anchoredPosition = handHome;
        var swipe = e.position - pressAt;
        if (swipe.y < minSwipe * Screen.height)
            return;  // not an upward swipe
        float speed = swipe.magnitude / Screen.height / Mathf.Max(0.05f, Time.unscaledTime - pressTime);
        StartCoroutine(Throw(e.position, swipe.normalized, speed));
    }

    IEnumerator Throw(Vector2 release, Vector2 direction, float speed)
    {
        busy = true;
        cardsLeft[creature.poster]--;
        float ringAtThrow = RingT();

        // Aim along the swipe, up to the PUCmon's height on screen; a slow swipe drops short.
        var poster = target.transform;
        Vector2 creatureOnScreen = arCamera.WorldToScreenPoint(poster.TransformPoint((Vector3)center + Offset(Time.time)));
        var aim = release + direction * Mathf.Max(0f, (creatureOnScreen.y - release.y) / Mathf.Max(0.05f, direction.y));
        if (speed < minSwipeSpeed)
            aim = Vector2.Lerp(release, aim, 0.6f * speed / minSwipeSpeed);
        var ray = arCamera.ScreenPointToRay(aim);
        Vector3 land = new Plane(poster.forward, poster.position).Raycast(ray, out var distance)
            ? poster.InverseTransformPoint(ray.GetPoint(distance)) : (Vector3)center + Vector3.down * 3f;
        land.z = -0.06f;  // just in front of the wall

        card = new GameObject("Carteirinha").AddComponent<SpriteRenderer>();
        card.sprite = cardSprite;
        card.transform.SetParent(poster, false);
        card.transform.localScale = Vector3.one * (cardSize / cardSprite.bounds.size.x);
        var cam = arCamera.transform;
        var from = poster.InverseTransformPoint(cam.position + cam.forward * 0.6f - cam.up * 0.3f);
        var peak = (from + land) / 2f + poster.InverseTransformDirection(cam.up) * arc;
        for (float t = 0f; t < 1f; t += Time.deltaTime / flightSeconds)
        {
            card.transform.localPosition = (1 - t) * (1 - t) * from + 2 * (1 - t) * t * peak + t * t * land;
            card.transform.localRotation = Quaternion.Euler(0f, t * 720f, 0f);  // spins like a thrown card
            yield return null;
        }
        card.transform.localPosition = land;
        card.transform.localRotation = Quaternion.identity;

        var offset = Offset(Time.time);
        bool hit = hidden < 0.3f && Vector2.Distance(land, center + (Vector2)offset) <= radius * hitArea;
        if (!hit)
        {
            Say(hidden >= 0.3f ? "Ele se escondeu!" : "Errou!");
            yield return Fall();
        }
        else
        {
            Say(ringAtThrow > 0.66f ? "Excelente!" : ringAtThrow > 0.33f ? "Ótimo!" : "Boa!");
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)  // into the card
            {
                inside = t;
                yield return null;
            }
            inside = 1f;
            bool caught = Random.value < Mathf.Clamp01(creature.catchRate * (1f + ringBonus * ringAtThrow));
            int times = caught ? shakes : Random.Range(1, shakes + 1);
            for (int i = 0; i < times; i++)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime / shakeSeconds)
                {
                    card.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * 22f * (t < 0.6f ? 1f : 0f));
                    yield return null;
                }
                card.transform.localRotation = Quaternion.identity;
            }
            if (caught)
            {
                Caught?.Invoke(creature);  // the capture screen takes over (and stops this game)
                yield break;
            }
            Say("Escapou!");
            for (float t = 1f; t > 0f; t -= Time.deltaTime / 0.25f)  // out again
            {
                inside = t;
                yield return null;
            }
            inside = 0f;
            yield return Fall();
        }
        if (cardsLeft[creature.poster] <= 0)
        {
            awayUntil[creature.poster] = Time.time + fleeSeconds;
            Say("Fugiu! Volte daqui a pouco.", fleeSeconds);
        }
        busy = false;
    }

    // A card that did not hold: drops down the wall and vanishes.
    IEnumerator Fall()
    {
        var start = card.transform.localPosition;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)
        {
            card.transform.localPosition = start + Vector3.down * (1.5f * t * t);
            card.transform.localRotation = Quaternion.Euler(0f, 0f, t * 120f);
            card.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }
        Destroy(card.gameObject);
    }

    // ---------- helpers ----------
    float RingT() => Mathf.Repeat((Time.time - ringStart) / Mathf.Max(0.05f, ringSeconds), 1f);

    bool Away() => creature != null && awayUntil.TryGetValue(creature.poster, out var until) && Time.time < until;

    float NextHideDelay() => creature.shyness <= 0f ? float.PositiveInfinity : Random.Range(hideEvery.x, hideEvery.y) / creature.shyness;

    // Sway and a little bob, plus the way into the hole (+Z goes into the wall).
    Vector3 Offset(float now)
    {
        float t = now * swaySpeed * creature.agility;
        return new Vector3(Mathf.Sin(t) * sway, Mathf.Abs(Mathf.Sin(t * 2f)) * sway * 0.3f, hidden * hidden * hideDepth);
    }

    void Say(string text, float seconds = -1f)
    {
        message.text = text;
        messageUntil = Time.time + (seconds < 0f ? messageSeconds : seconds);
    }

    // The PUCmon's middle and size on the poster plane, from its meshes (the poster's own scale left out, since it
    // grows in when found).
    void Measure()
    {
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = -min;
        foreach (var part in body)
            foreach (var r in part.GetComponentsInChildren<Renderer>(true))
            {
                Bounds b;
                if (r is SkinnedMeshRenderer skinned) b = skinned.localBounds;
                else if (r.TryGetComponent<MeshFilter>(out var filter) && filter.sharedMesh) b = filter.sharedMesh.bounds;
                else continue;
                var m = Matrix4x4.identity;
                for (var t = r.transform; t != target.transform; t = t.parent)
                    m = Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale) * m;
                for (int i = 0; i < 8; i++)
                {
                    var corner = m.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) * 2 - 1, (i & 2) - 1, (i & 4) / 2 - 1)));
                    min = Vector2.Min(min, corner);
                    max = Vector2.Max(max, corner);
                }
            }
        center = (min + max) / 2f;
        radius = Mathf.Max(max.x - min.x, max.y - min.y) / 2f;
    }

    static void Toggle(GameObject go, bool on)
    {
        if (go.activeSelf != on)
            go.SetActive(on);
    }
}
