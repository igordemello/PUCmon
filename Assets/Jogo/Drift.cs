using UnityEngine;

/// <summary>Slow floating motion for decorations (background bubbles, logo).</summary>
public class Drift : MonoBehaviour
{
    public Vector2 distance = new Vector2(40f, 60f);
    public float speed = 0.35f, tilt;
    Vector2 home;
    float phase;

    void Start()
    {
        home = ((RectTransform)transform).anchoredPosition;
        phase = transform.GetSiblingIndex() * 1.7f;
    }

    void Update()
    {
        float t = Time.unscaledTime * speed + phase;
        ((RectTransform)transform).anchoredPosition = home + new Vector2(Mathf.Sin(t * 0.8f) * distance.x, Mathf.Sin(t * 1.1f) * distance.y);
        transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.3f) * tilt);
    }
}
