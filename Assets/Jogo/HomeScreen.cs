using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Home screen after signing in: go register PUCmons, open the laboratory (the collection: search it, care for a
/// PUCmon), or sign out.
/// </summary>
public class HomeScreen : MonoBehaviour
{
    /// <summary>Set before loading this scene to open straight on the collection.</summary>
    public static bool openCollection;

    public Catalog catalog;
    public string captureScene = "SampleScene", loginScene = "Login";

    [Header("Início")]
    public GameObject homePanel;
    public TMP_Text greeting, progress;
    public Button captureButton, collectionButton, signOutButton;

    [Header("Laboratório")]
    public GameObject collectionPanel;
    public TMP_Text collectionProgress;
    public Button backButton;
    [Tooltip("Inactive card copied once per PUCmon of the catalog.")]
    public CreatureCard cardTemplate;
    [Tooltip("A registered PUCmon's pen, opened by tapping its card.")]
    public CareScreen care;
    [Tooltip("Filters the cards by name or by what the PUCmon represents.")]
    public TMP_InputField search;

    readonly List<CreatureCard> cards = new List<CreatureCard>();

    async void Start()
    {
        if (!Account.SignedIn)
        {
            SceneManager.LoadScene(loginScene);  // it signs back in with the saved session, if there is one
            return;
        }
        captureButton.onClick.AddListener(() => StartCoroutine(OpenCapture()));
        collectionButton.onClick.AddListener(ShowCollection);
        backButton.onClick.AddListener(() => Show(homePanel));
        search.onValueChanged.AddListener(_ => Refresh());
        signOutButton.onClick.AddListener(() =>
        {
            Account.SignOut();
            SceneManager.LoadScene(loginScene);
        });
        foreach (var creature in catalog.creatures)
        {
            var card = Instantiate(cardTemplate, cardTemplate.transform.parent);
            card.gameObject.SetActive(true);
            card.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (Account.Collection.Contains(creature.poster))
                    care.Open(creature);
            });
            cards.Add(card);
        }
        greeting.text = $"Olá, {Account.Username}!";
        Show(openCollection ? collectionPanel : homePanel);
        openCollection = false;
        Refresh();
        Page.Ready();

        await Account.LoadCollection();  // may have changed on another device
        if (this)
            Refresh();
    }

    void Refresh()
    {
        int caught = catalog.Caught, total = catalog.creatures.Count;
        progress.text = caught == total ? $"Você registrou todos os {total} PUCmons!" : $"{caught} de {total} PUCmons registrados";
        // While searching, only registered PUCmons whose name or theme matches (the others' names are a secret).
        string query = Plain(search.text);
        int found = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            var creature = catalog.creatures[i];
            bool has = Account.Collection.Contains(creature.poster);
            bool shown = query == "" || has && (Plain(creature.displayName).Contains(query) || Plain(creature.represents).Contains(query));
            cards[i].gameObject.SetActive(shown);
            found += shown ? 1 : 0;
            cards[i].Show(creature, has, has && Account.Bonds.TryGetValue(creature.poster, out var bond) && bond.NeedsCare);
        }
        collectionProgress.text = query == "" ? $"{caught} de {total}" : found == 0 ? "Nenhum PUCmon encontrado" : found == 1 ? "1 encontrado" : $"{found} encontrados";
    }

    // Lower case without accents, so "santideo" finds "Santídeo".
    static string Plain(string text)
    {
        const string accented = "áàâãäéèêëíìîïóòôõöúùûüç", plain = "aaaaaeeeeiiiiooooouuuuc";
        var chars = (text ?? "").Trim().ToLowerInvariant().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            int k = accented.IndexOf(chars[i]);
            if (k >= 0)
                chars[i] = plain[k];
        }
        return new string(chars);
    }

    public void ShowCollection()
    {
        Refresh();
        Show(collectionPanel);
    }

    /// <summary>Shows one of the panels (home, collection, a PUCmon's pen or sheet) and hides the others.</summary>
    public void Show(GameObject panel)
    {
        foreach (var other in new[] { homePanel, collectionPanel, care.gameObject, care.sheet })
            other.SetActive(other == panel);
        StartCoroutine(Pop(panel.transform));
    }

    // The page's loading screen and the camera come first, so the tap answers at once and the camera opens while the
    // AR scene (all the PUCmons' models) loads; one frame lets the page show it before the load holds everything up.
    IEnumerator OpenCapture()
    {
        captureButton.interactable = false;
        WebARImageTracker.OpenCamera();
        yield return null;
        SceneManager.LoadScene(captureScene);
    }

    static IEnumerator Pop(Transform panel)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.3f)
        {
            float u = t - 1f;
            panel.localScale = Vector3.one * Mathf.LerpUnclamped(0.92f, 1f, 1f + 2.70158f * u * u * u + 1.70158f * u * u);
            yield return null;
        }
        panel.localScale = Vector3.one;
    }
}
