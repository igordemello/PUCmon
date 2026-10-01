using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Home screen after signing in: go capture, see the collection, or sign out.</summary>
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

    [Header("Coleção")]
    public GameObject collectionPanel;
    public TMP_Text collectionProgress;
    public Button backButton;
    [Tooltip("Inactive card copied once per PUCmon of the catalog.")]
    public CreatureCard cardTemplate;

    readonly List<CreatureCard> cards = new List<CreatureCard>();

    async void Start()
    {
        if (!Account.SignedIn)
        {
            SceneManager.LoadScene(loginScene);  // it signs back in with the saved session, if there is one
            return;
        }
        captureButton.onClick.AddListener(() => SceneManager.LoadScene(captureScene));
        collectionButton.onClick.AddListener(() => Show(true));
        backButton.onClick.AddListener(() => Show(false));
        signOutButton.onClick.AddListener(() =>
        {
            Account.SignOut();
            SceneManager.LoadScene(loginScene);
        });
        foreach (var creature in catalog.creatures)
        {
            var card = Instantiate(cardTemplate, cardTemplate.transform.parent);
            card.gameObject.SetActive(true);
            cards.Add(card);
        }
        greeting.text = $"Olá, {Account.Username}!";
        Show(openCollection);
        openCollection = false;
        Refresh();

        await Account.LoadCollection();  // may have changed on another device
        if (this)
            Refresh();
    }

    void Refresh()
    {
        int caught = catalog.Caught, total = catalog.creatures.Count;
        progress.text = caught == total ? $"Você capturou todos os {total} PUCmons!" : $"{caught} de {total} PUCmons capturados";
        collectionProgress.text = $"{caught} de {total}";
        for (int i = 0; i < cards.Count; i++)
            cards[i].Show(catalog.creatures[i], Account.Collection.Contains(catalog.creatures[i].poster));
    }

    void Show(bool collection)
    {
        homePanel.SetActive(!collection);
        collectionPanel.SetActive(collection);
        StartCoroutine(Pop((collection ? collectionPanel : homePanel).transform));
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
