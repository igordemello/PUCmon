using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Shrinks a button a little while it is pressed, like a mobile game button.</summary>
public class Squish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float pressedScale = 0.93f;
    float target = 1f;

    public void OnPointerDown(PointerEventData _) => target = pressedScale;
    public void OnPointerUp(PointerEventData _) => target = 1f;
    public void OnPointerExit(PointerEventData _) => target = 1f;

    void Update()
    {
        float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-25f * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }
}
