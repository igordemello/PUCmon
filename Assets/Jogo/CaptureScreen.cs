using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The AR scene's interface: while a PUCmon's poster is in view, the capture minigame (or "you already caught it"),
/// and the capture screen that saves it into the player's collection.
/// </summary>
public class CaptureScreen : MonoBehaviour
{
    public Catalog catalog;
    public WebARImageTracker tracker;
    public string homeScene = "Inicio", loginScene = "Login";

    [Header("Sobre o AR")]
    public Button homeButton;
    public TMP_Text counter;
    public CaptureMinigame minigame;
    public GameObject caughtLabel;
    public TMP_Text caughtText;

    [Header("Primeiro registro")]
    [Tooltip("Dr. Beltran O.'s talk the first time the player meets a PUCmon to capture.")]
    public Dialogue dialogue;
    public Dialogue.Line[] firstCaptureLines;

    [Header("Tela de registro")]
    public CanvasGroup capturePanel;
    public Image flash, portrait;
    public RectTransform glow;
    [Tooltip("Pictures the glow cycles through (pixel-art animation).")]
    public Sprite[] glowFrames;
    public TMP_Text title, status;
    public Button continueButton, collectionButton, retryButton;

    Catalog.Creature inView, capturing;
    float inViewSince;
    bool hintHidden;

    void Start()
    {
        if (!Account.SignedIn)
        {
            SceneManager.LoadScene(loginScene);
            return;
        }
        homeButton.onClick.AddListener(() => SceneManager.LoadScene(homeScene));
        minigame.Caught += Capture;
        continueButton.onClick.AddListener(() => capturePanel.gameObject.SetActive(false));
        collectionButton.onClick.AddListener(() =>
        {
            HomeScreen.openCollection = true;
            SceneManager.LoadScene(homeScene);
        });
        retryButton.onClick.AddListener(Save);
        capturePanel.gameObject.SetActive(false);
        Page.Ready();
    }

    void Update()
    {
        var target = tracker.Current;
        if (target && !LazyModel.IsReady(target))
            target = null;  // its PUCmon is still loading: it shows up, and the game starts, once it is there
        var creature = target && target.image ? catalog.Find(target.image.name) : null;
        if (creature != inView)
        {
            inView = creature;
            inViewSince = Time.unscaledTime;
        }
        bool panel = capturePanel.gameObject.activeSelf, caught = inView != null && Account.Collection.Contains(inView.poster);
        if (inView != null && !caught && !panel && !dialogue.Playing && FirstCapture)
            dialogue.Play(firstCaptureLines, () =>
            {
                PlayerPrefs.SetInt(IntroSeen, 1);
                PlayerPrefs.Save();
            });
        bool talking = dialogue.Playing;
        minigame.Play(inView != null && !caught && !panel && !talking ? target : null, inView);
        Toggle(caughtLabel, inView != null && caught && !panel && !talking);
        if (caught)
            caughtText.text = $"Você já registrou {inView.displayName}!";

        // Pop in when a poster shows up.
        caughtLabel.transform.localScale = Vector3.one * Overshoot(Mathf.Clamp01((Time.unscaledTime - inViewSince) / 0.35f));
        counter.text = $"{catalog.Caught}/{catalog.creatures.Count}";
        if (panel)
            glow.GetComponent<Image>().sprite = glowFrames[(int)(Time.unscaledTime * 4f) % glowFrames.Length];
        if (panel != hintHidden)
            WebARImageTracker.ShowScanHint = !(hintHidden = panel);  // no "point the camera" hint over the capture screen
    }

    // The professor explains the rocks once, to players who have not caught anything yet.
    string IntroSeen => "pucmon.beltran.captura." + Account.Username;
    bool FirstCapture => Account.Collection.Count == 0 && PlayerPrefs.GetInt(IntroSeen) == 0;

    void Capture(Catalog.Creature creature)
    {
        capturing = creature;
        portrait.sprite = capturing.portrait;
        capturePanel.gameObject.SetActive(true);
        StartCoroutine(Reveal());
        Save();
    }

    async void Save()
    {
        title.text = $"Registrando {capturing.displayName}...";
        status.text = "Levando para o laboratório...";
        retryButton.gameObject.SetActive(false);
        continueButton.interactable = collectionButton.interactable = false;

        var error = await Account.Capture(capturing.poster);
        if (!this)
            return;
        continueButton.interactable = collectionButton.interactable = true;
        if (error == null)
        {
            title.text = $"Você registrou {capturing.displayName}!";
            status.text = "Ele já está no laboratório.";
        }
        else
        {
            title.text = "Ops!";
            status.text = error;
            retryButton.gameObject.SetActive(true);
        }
    }

    // White flash, the panel fading in, the glow opening and the picture popping out of it.
    IEnumerator Reveal()
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.7f)
        {
            capturePanel.alpha = Mathf.Clamp01(t * 5f);
            flash.color = new Color(1f, 1f, 1f, 0.9f * Mathf.Clamp01(1f - t * 2.5f));
            glow.localScale = Vector3.one * Mathf.Lerp(0.3f, 1f, Mathf.Sqrt(t));
            portrait.transform.localScale = Vector3.one * Overshoot(Mathf.Clamp01((t - 0.15f) / 0.6f));
            yield return null;
        }
        capturePanel.alpha = 1f;
        flash.color = Color.clear;
        glow.localScale = portrait.transform.localScale = Vector3.one;
    }

    static float Overshoot(float t)
    {
        float u = t - 1f;
        return 1f + 2.70158f * u * u * u + 1.70158f * u * u;  // ease-out back
    }

    static void Toggle(GameObject go, bool on)
    {
        if (go.activeSelf != on)
            go.SetActive(on);
    }
}
