using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A conversation shown over the game: one line at a time in a box, typed out letter by letter; a tap shows the
/// whole line, the next tap goes on. Dr. Beltran O.'s picture shows while he is the one talking.
/// </summary>
public class Dialogue : MonoBehaviour, IPointerClickHandler
{
    [Serializable]
    public class Line
    {
        [Tooltip("Shown over the text, e.g. \"DR. BELTRAN O.\" or \"VOCÊ\".")]
        public string speaker;
        [TextArea(2, 5)] public string text;
        [Tooltip("Show the professor while this line is said.")]
        public bool professor;
    }

    public TMP_Text speaker, text;
    public RectTransform professor;
    [Tooltip("Letters per second.")]
    public float typeSpeed = 45f;

    /// <summary>True while a conversation is on screen.</summary>
    public bool Playing => gameObject.activeSelf;

    Line[] lines;
    int index;
    float shownSince;
    Action done;
    Vector2 professorHome;

    void Awake() => professorHome = professor.anchoredPosition;

    /// <summary>Shows these lines, then calls <paramref name="finished"/>.</summary>
    public void Play(Line[] conversation, Action finished = null)
    {
        lines = conversation;
        done = finished;
        index = -1;
        gameObject.SetActive(true);
        Next();
    }

    void Next()
    {
        if (++index >= lines.Length)
        {
            gameObject.SetActive(false);
            done?.Invoke();
            return;
        }
        speaker.text = lines[index].speaker;
        text.text = KeepWhole(lines[index].text);
        text.maxVisibleCharacters = 0;
        shownSince = Time.unscaledTime;
    }

    /// <summary>The text with "G-HYDRA" never split over two lines at its hyphen.</summary>
    public static string KeepWhole(string text) => text.Replace("G-HYDRA", "<nobr>G-HYDRA</nobr>");

    public void OnPointerClick(PointerEventData _)
    {
        int length = text.textInfo.characterCount;
        if (text.maxVisibleCharacters < length)
            text.maxVisibleCharacters = length;  // show the rest of the line at once
        else
            Next();
    }

    void Update()
    {
        if (lines == null || index >= lines.Length)
            return;
        text.maxVisibleCharacters = Mathf.Max(text.maxVisibleCharacters, (int)((Time.unscaledTime - shownSince) * typeSpeed));
        // The professor slides in while he talks and out while you do.
        float t = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
        var target = lines[index].professor ? professorHome : professorHome + new Vector2(-professor.rect.width, 0f);
        professor.anchoredPosition = Vector2.Lerp(professor.anchoredPosition, target, t);
    }
}
