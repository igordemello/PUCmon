using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// The capture minigame. While a PUCmon not caught yet is on screen it lives in its wall hole (sways, hides now and
/// then), a ring shrinks over it, and the player swipes up to throw rocks at it (the project has no money for
/// tranquilizers). Each hit takes some of its consciousness, more the smaller the ring was at the throw; at zero it is
/// knocked out and caught. Out of rocks first, it runs away for a while and comes back wide awake. All the numbers
/// that set the difficulty are here, plus knock / agility / shyness per PUCmon in the catalog.
/// Sits on a full-screen, see-through UI image of the AR interface that catches the swipes.
/// </summary>
public class CaptureMinigame : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Tentativas")]
    [Tooltip("Rocks per attempt.")]
    [Min(1)] public int throws = 3;
    [Tooltip("Seconds the PUCmon stays away after an attempt runs out of rocks; then it comes back wide awake.")]
    public float fleeSeconds = 6f;
    [Tooltip("Seconds a result message (\"Errou!\", \"Nocaute!\"...) stays on screen.")]
    public float messageSeconds = 1.4f;

    [Header("Anel")]
    [Tooltip("Seconds for the colored ring to shrink from the white circle down to its smallest; then it starts over.")]
    public float ringSeconds = 1.6f;
    [Tooltip("Smallest size of the colored ring, relative to the white circle.")]
    [Range(0.1f, 0.9f)] public float ringSmallest = 0.3f;
    [Tooltip("Size of the white circle (where a rock must land to hit), relative to the PUCmon's size.")]
    public float hitArea = 0.8f;

    [Header("Nocaute")]
    [Tooltip("Extra knock for a small ring: a hit takes knock x (1 + bonus x how small the ring was, 0 to 1) of its consciousness (knock is per PUCmon, in the catalog).")]
    public float ringBonus = 1f;
    [Tooltip("Seconds it reels after a hit that does not knock it out.")]
    public float reelSeconds = 0.45f;
    [Tooltip("Seconds it wobbles, dizzy, before falling over when knocked out.")]
    public float dizzySeconds = 1.3f;

    [Header("Lançamento")]
    [Tooltip("Shortest upward swipe that throws, as a fraction of the screen height.")]
    public float minSwipe = 0.08f;
    [Tooltip("Slower swipes (screen heights per second) fall short of the PUCmon.")]
    public float minSwipeSpeed = 0.6f;
    [Tooltip("Seconds the rock flies.")]
    public float flightSeconds = 0.55f;
    [Tooltip("How high the rock arcs, in poster units (the poster is 2 tall).")]
    public float arc = 0.5f;
    [Tooltip("Rock width in poster units.")]
    [FormerlySerializedAs("cardSize")] public float rockSize = 0.4f;

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
    [FormerlySerializedAs("cardSprite")] public Sprite rockSprite;
    public RectTransform hitCircle, ring;
    [FormerlySerializedAs("handCard")] public RectTransform handRock;
    public Image ringImage;
    [Tooltip("Boxes shown while aiming (the swipe hint, the rocks left) and after a throw (the result message).")]
    public GameObject hintBox, throwsBox, messageBox;
    public TMP_Text throwsLeft, message;
    public Color ringBig = new Color32(0x5E, 0xAF, 0xD3, 0xFF), ringSmall = new Color32(0xE0, 0xB8, 0x4F, 0xFF);
    [Tooltip("Consciousness bar over the PUCmon: full, and nearly out.")]
    public Color awakeFull = new Color32(0x5E, 0xAF, 0xD3, 0xFF), awakeLow = new Color32(0xD9, 0x57, 0x3B, 0xFF);

    /// <summary>Raised when a PUCmon is knocked out (caught).</summary>
    public event System.Action<Catalog.Creature> Caught;

    static readonly Color Navy = new Color32(0x10, 0x1F, 0x3F, 0xFF);

    ImageTarget target;
    Catalog.Creature creature;
    readonly List<Transform> body = new List<Transform>();  // the poster's children that make up the PUCmon (not its hole)
    readonly List<(Vector3 position, Quaternion rotation, Vector3 scale)> home = new List<(Vector3, Quaternion, Vector3)>();
    Vector2 center;  // PUCmon's middle on the poster
    float radius, bottom, top;  // and its size there
    readonly Dictionary<string, int> rocksLeft = new Dictionary<string, int>();
    readonly Dictionary<string, float> awake = new Dictionary<string, float>();  // consciousness left: 1 wide awake .. 0 out
    readonly Dictionary<string, float> awayUntil = new Dictionary<string, float>();
    float ringStart, nextHide, hiddenUntil, hidden, messageUntil, swayTime;  // hidden: 0 out .. 1 in the hole
    float tilt, squash;  // sideways lean (degrees, about its feet) and squash after a hit
    bool busy, pressing, knockedOut;
    Vector2 pressAt, handHome;
    float pressTime;
    SpriteRenderer rock;
    readonly List<SpriteRenderer> stars = new List<SpriteRenderer>();
    RectTransform awakeBar;
    Image awakeFill;
    static Sprite starSprite;

    void Awake()
    {
        handHome = handRock.anchoredPosition;
        // Consciousness bar on top of the white circle (so it follows the PUCmon): navy frame, colored level inside.
        awakeBar = NewImage("Consciência", hitCircle, Navy);
        awakeBar.anchorMin = new Vector2(0.15f, 1f);
        awakeBar.anchorMax = new Vector2(0.85f, 1f);
        awakeBar.pivot = new Vector2(0.5f, 0f);
        awakeBar.anchoredPosition = new Vector2(0f, 16f);
        awakeBar.sizeDelta = new Vector2(0f, 28f);
        var fill = NewImage("Nível", awakeBar, awakeFull);
        fill.anchorMin = Vector2.zero;
        fill.offsetMin = new Vector2(5f, 5f);
        fill.offsetMax = new Vector2(-5f, -5f);
        awakeFill = fill.GetComponent<Image>();
    }

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
                home.Add((child.localPosition, child.localRotation, child.localScale));
            }
        Measure();
        if (!rocksLeft.ContainsKey(creature.poster))
            rocksLeft[creature.poster] = throws;
        if (!awake.ContainsKey(creature.poster))
            awake[creature.poster] = 1f;
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
                body[i].localRotation = home[i].rotation;
                body[i].localScale = home[i].scale;
            }
        body.Clear();
        home.Clear();
        if (rock)
            Destroy(rock.gameObject);
        foreach (var star in stars)
            if (star)
                Destroy(star.gameObject);
        stars.Clear();
        target = null;
        creature = null;
        busy = pressing = knockedOut = false;
        hidden = tilt = squash = 0f;
        handRock.anchoredPosition = handHome;
    }

    void Update()
    {
        bool on = target;
        float now = Time.time;
        bool away = on && awayUntil.TryGetValue(creature.poster, out var until) && now < until;
        if (on && !away && awayUntil.Remove(creature.poster))
        {
            rocksLeft[creature.poster] = throws;  // back for a new attempt, wide awake
            awake[creature.poster] = 1f;
        }
        bool idle = on && !away && !busy;
        Toggle(hitCircle.gameObject, on && !away && !knockedOut);
        Toggle(ring.gameObject, on && !away && !knockedOut);
        Toggle(handRock.gameObject, idle);
        Toggle(throwsBox, idle);
        Toggle(hintBox, idle && !pressing);
        Toggle(messageBox, now < messageUntil);
        if (!on)
            return;

        // Hide in the hole now and then (always while away), but not once knocked out.
        if (now >= nextHide && !away && !knockedOut)
        {
            hiddenUntil = now + Random.Range(hiddenFor.x, hiddenFor.y);
            nextHide = hiddenUntil + NextHideDelay();
        }
        hidden = Mathf.MoveTowards(hidden, away || (now < hiddenUntil && !knockedOut) ? 1f : 0f, Time.deltaTime / Mathf.Max(0.01f, hideMove));
        if (!knockedOut)
            swayTime += Time.deltaTime * swaySpeed * creature.agility;
        var offset = Offset();
        var lean = Quaternion.Euler(0f, 0f, tilt);
        var feet = new Vector3(center.x, bottom, 0f);
        var stretch = new Vector3(1f + squash, 1f - squash, 1f);
        for (int i = 0; i < body.Count; i++)
        {
            body[i].localPosition = feet + lean * (home[i].position + offset - feet);
            body[i].localRotation = lean * home[i].rotation;
            body[i].localScale = Vector3.Scale(home[i].scale, stretch);
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
        throwsLeft.text = "x" + rocksLeft[creature.poster];

        float left = awake[creature.poster];
        awakeFill.rectTransform.anchorMax = new Vector2(left, 1f);
        awakeFill.color = Color.Lerp(awakeLow, awakeFull, left);
        Toggle(awakeFill.gameObject, left > 0.02f);

        // Stars circling over its head while knocked out.
        for (int i = 0; i < stars.Count; i++)
        {
            float a = now * 5f + i * Mathf.PI * 2f / stars.Count;
            stars[i].transform.localPosition = feet + lean * (new Vector3(center.x + Mathf.Cos(a) * radius * 0.45f, top + 0.12f + Mathf.Sin(a) * 0.06f, -0.15f) - feet);
        }
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
        handRock.anchoredPosition = local;  // the rock follows the finger (anchored to the canvas centre)
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!pressing)
            return;
        pressing = false;
        handRock.anchoredPosition = handHome;
        var swipe = e.position - pressAt;
        if (swipe.y < minSwipe * Screen.height)
            return;  // not an upward swipe
        float speed = swipe.magnitude / Screen.height / Mathf.Max(0.05f, Time.unscaledTime - pressTime);
        StartCoroutine(Throw(e.position, swipe.normalized, speed));
    }

    IEnumerator Throw(Vector2 release, Vector2 direction, float speed)
    {
        busy = true;
        rocksLeft[creature.poster]--;
        float ringAtThrow = RingT();

        // Aim along the swipe, up to the PUCmon's height on screen; a slow swipe drops short.
        var poster = target.transform;
        Vector2 creatureOnScreen = arCamera.WorldToScreenPoint(poster.TransformPoint((Vector3)center + Offset()));
        var aim = release + direction * Mathf.Max(0f, (creatureOnScreen.y - release.y) / Mathf.Max(0.05f, direction.y));
        if (speed < minSwipeSpeed)
            aim = Vector2.Lerp(release, aim, 0.6f * speed / minSwipeSpeed);
        var ray = arCamera.ScreenPointToRay(aim);
        Vector3 land = new Plane(poster.forward, poster.position).Raycast(ray, out var distance)
            ? poster.InverseTransformPoint(ray.GetPoint(distance)) : (Vector3)center + Vector3.down * 3f;
        land.z = -0.06f;  // just in front of the wall

        rock = new GameObject("Pedra").AddComponent<SpriteRenderer>();
        rock.sprite = rockSprite;
        rock.transform.SetParent(poster, false);
        rock.transform.localScale = Vector3.one * (rockSize / rockSprite.bounds.size.x);
        var cam = arCamera.transform;
        var from = poster.InverseTransformPoint(cam.position + cam.forward * 0.6f - cam.up * 0.3f);
        var peak = (from + land) / 2f + poster.InverseTransformDirection(cam.up) * arc;
        for (float t = 0f; t < 1f; t += Time.deltaTime / flightSeconds)
        {
            rock.transform.localPosition = (1 - t) * (1 - t) * from + 2 * (1 - t) * t * peak + t * t * land;
            rock.transform.localRotation = Quaternion.Euler(0f, 0f, -t * 540f);  // tumbles end over end
            yield return null;
        }
        rock.transform.localPosition = land;

        bool hit = hidden < 0.3f && Vector2.Distance(land, center + (Vector2)Offset()) <= radius * hitArea;
        if (!hit)
        {
            Say(hidden >= 0.3f ? "Ele se escondeu!" : "Errou!");
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)  // slides down the wall
            {
                Drop(land, t);
                yield return null;
            }
        }
        else
        {
            float left = awake[creature.poster] = Mathf.Max(0f, awake[creature.poster] - creature.knock * (1f + ringBonus * ringAtThrow));
            if (left <= 0f)
            {
                yield return KnockOut(land);
                yield break;
            }
            Say(ringAtThrow > 0.66f ? "Excelente!" : ringAtThrow > 0.33f ? "Ótimo!" : "Boa!");
            float side = land.x > center.x ? 1f : -1f;  // reels away from where it was hit
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.5f, reelSeconds))
            {
                float u = Mathf.Clamp01(t * Mathf.Max(0.5f, reelSeconds) / reelSeconds);
                tilt = side * Mathf.Sin(u * Mathf.PI * 3f) * 14f * (1f - u);
                squash = Mathf.Sin(u * Mathf.PI) * 0.12f;
                Drop(land, t);
                yield return null;
            }
            tilt = squash = 0f;
        }
        Destroy(rock.gameObject);
        if (rocksLeft[creature.poster] <= 0)
        {
            awayUntil[creature.poster] = Time.time + fleeSeconds;
            Say("Fugiu! Volte daqui a pouco.", fleeSeconds);
        }
        busy = false;
    }

    // Out cold: wobbles dizzy with stars over its head, falls over, and the capture screen takes over.
    IEnumerator KnockOut(Vector3 land)
    {
        knockedOut = true;
        hiddenUntil = 0f;
        Say("Nocaute!", dizzySeconds + 1f);
        for (int i = 0; i < 3; i++)
        {
            var star = new GameObject("Estrela").AddComponent<SpriteRenderer>();
            star.sprite = StarSprite();
            star.transform.SetParent(target.transform, false);
            star.transform.localScale = Vector3.one * (0.12f / star.sprite.bounds.size.x);
            stars.Add(star);
        }
        float side = land.x > center.x ? -1f : 1f;  // falls away from the hit
        for (float t = 0f; t < 1f; t += Time.deltaTime / dizzySeconds)
        {
            tilt = Mathf.Sin(t * Mathf.PI * 4f) * 12f;
            if (rock)
            {
                Drop(land, Mathf.Clamp01(t * dizzySeconds / 0.5f));
                if (t * dizzySeconds >= 0.5f)
                    Destroy(rock.gameObject);
            }
            yield return null;
        }
        float from = tilt;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
        {
            tilt = Mathf.Lerp(from, side * 80f, t * t);
            yield return null;
        }
        tilt = side * 80f;
        yield return new WaitForSeconds(0.5f);
        Caught?.Invoke(creature);  // the capture screen takes over (and stops this game)
    }

    // A rock that bounced off or missed: drops down the wall, spinning and fading.
    void Drop(Vector3 start, float t)
    {
        rock.transform.localPosition = start + new Vector3(0f, -1.5f * t * t, -0.2f * t);
        rock.transform.localRotation = Quaternion.Euler(0f, 0f, (start.x > center.x ? 1f : -1f) * t * 240f);
        rock.color = new Color(1f, 1f, 1f, 1f - t);
    }

    // ---------- helpers ----------
    float RingT() => Mathf.Repeat((Time.time - ringStart) / Mathf.Max(0.05f, ringSeconds), 1f);

    bool Away() => creature != null && awayUntil.TryGetValue(creature.poster, out var until) && Time.time < until;

    float NextHideDelay() => creature.shyness <= 0f ? float.PositiveInfinity : Random.Range(hideEvery.x, hideEvery.y) / creature.shyness;

    // Sway and a little bob, plus the way into the hole (+Z goes into the wall).
    Vector3 Offset() =>
        new Vector3(Mathf.Sin(swayTime) * sway, Mathf.Abs(Mathf.Sin(swayTime * 2f)) * sway * 0.3f, hidden * hidden * hideDepth);

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
        bottom = min.y;
        top = max.y;
    }

    // A 5x5 pixel-art star, made once.
    static Sprite StarSprite()
    {
        if (starSprite)
            return starSprite;
        const string pattern = "..#..\n.###.\n#####\n.###.\n.#.#.";
        var rows = pattern.Split('\n');
        var texture = new Texture2D(5, 5, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
            texture.SetPixel(x, 4 - y, rows[y][x] == '#' ? new Color32(0xE0, 0xB8, 0x4F, 0xFF) : Color.clear);
        texture.Apply();
        return starSprite = Sprite.Create(texture, new Rect(0, 0, 5, 5), new Vector2(0.5f, 0.5f), 100f);
    }

    static RectTransform NewImage(string name, Transform parent, Color color)
    {
        var rect = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        var image = rect.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    static void Toggle(GameObject go, bool on)
    {
        if (go.activeSelf != on)
            go.SetActive(on);
    }
}
