using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// "Entrar" / "Criar conta" screen. Opens the game scene once the player is signed in, straight away when this
/// device remembers a session.
/// </summary>
public class LoginScreen : MonoBehaviour
{
    [Tooltip("Scene opened once the player is signed in.")]
    public string gameScene = "Inicio";

    [Header("Entrar")]
    public GameObject signInPanel;
    public TMP_InputField signInUser, signInPassword;
    public Button signInButton, showSignUp;
    public TMP_Text signInMessage;

    [Header("Criar conta")]
    public GameObject signUpPanel;
    public TMP_InputField signUpUser, signUpPassword;
    public Button signUpButton, showSignIn;
    public TMP_Text signUpMessage;

    [Header("Animação")]
    public CanvasGroup screen;
    public RectTransform card, logo;
    public RectTransform[] bubbles;

    bool busy;
    Vector2 cardHome, logoHome;
    Vector2[] bubbleHomes;

    async void Start()
    {
        cardHome = card.anchoredPosition;
        logoHome = logo.anchoredPosition;
        bubbleHomes = System.Array.ConvertAll(bubbles, b => b.anchoredPosition);
        signInButton.onClick.AddListener(SignIn);
        signUpButton.onClick.AddListener(SignUp);
        signInPassword.onSubmit.AddListener(_ => SignIn());
        signUpPassword.onSubmit.AddListener(_ => SignUp());
        showSignUp.onClick.AddListener(() => Show(signUpPanel));
        showSignIn.onClick.AddListener(() => Show(signInPanel));
        Show(signInPanel);
        if (Account.Notice != null)
        {
            Fail(signInMessage, Account.Notice);
            Account.Notice = null;
        }

        if (Account.HasSavedSession)
        {
            var error = await Run(signInButton, Account.Resume());
            if (error == null)
                EnterGame();
            else
                Fail(signInMessage, error);
        }
    }

    async void SignIn()
    {
        if (busy)
            return;
        string user = signInUser.text.Trim(), password = signInPassword.text;
        if (user == "") { Fail(signInMessage, "Digite seu usuário."); return; }
        if (password == "") { Fail(signInMessage, "Digite sua senha."); return; }

        var error = await Run(signInButton, Account.SignIn(user, password));
        if (error == null)
            EnterGame();
        else
            Fail(signInMessage, error);
    }

    async void SignUp()
    {
        if (busy)
            return;
        string user = Account.Normalize(signUpUser.text), password = signUpPassword.text;
        var problem = Account.CheckUsername(user) ?? (password.Length < 6 ? "A senha precisa de pelo menos 6 caracteres." : null);
        if (problem != null) { Fail(signUpMessage, problem); return; }

        var error = await Run(signUpButton, Account.SignUp(user, password));
        if (error == null)
            EnterGame();
        else
            Fail(signUpMessage, error);
    }

    // Button shows "..." and ignores taps while Supabase answers.
    async Task<string> Run(Button button, Task<string> work)
    {
        busy = true;
        var label = button.GetComponentInChildren<TMP_Text>();
        string text = label.text;
        label.text = "...";
        button.interactable = false;
        try { return await work; }
        finally
        {
            label.text = text;
            button.interactable = true;
            busy = false;
        }
    }

    void Show(GameObject panel)
    {
        signInPanel.SetActive(panel == signInPanel);
        signUpPanel.SetActive(panel == signUpPanel);
        Say(signInMessage, "");
        Say(signUpMessage, "");
        StartCoroutine(Pop());
    }

    void Fail(TMP_Text message, string text)
    {
        Say(message, text);
        StartCoroutine(Shake());
    }

    static void Say(TMP_Text message, string text)
    {
        message.text = text;
        message.gameObject.SetActive(text != "");
    }

    void EnterGame() => StartCoroutine(FadeOut());

    IEnumerator FadeOut()
    {
        busy = true;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.3f)
        {
            screen.alpha = 1f - t;
            yield return null;
        }
        SceneManager.LoadScene(gameScene);
    }

    IEnumerator Pop()
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.35f)
        {
            float u = t - 1f, back = 1f + 2.70158f * u * u * u + 1.70158f * u * u;  // ease-out back: overshoots a little
            card.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, back);
            yield return null;
        }
        card.localScale = Vector3.one;
    }

    IEnumerator Shake()
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.4f)
        {
            card.anchoredPosition = cardHome + Vector2.right * (Mathf.Sin(t * 40f) * 24f * (1f - t));
            yield return null;
        }
        card.anchoredPosition = cardHome;
    }

    // Logo bobbing and background bubbles drifting.
    void Update()
    {
        float t = Time.unscaledTime;
        logo.anchoredPosition = logoHome + Vector2.up * (Mathf.Sin(t * 2f) * 12f);
        logo.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.3f) * 2f);
        for (int i = 0; i < bubbles.Length; i++)
            bubbles[i].anchoredPosition = bubbleHomes[i] + new Vector2(Mathf.Sin(t * 0.3f + i * 1.7f) * 40f, Mathf.Sin(t * 0.4f + i * 2.3f) * 60f);
    }
}
