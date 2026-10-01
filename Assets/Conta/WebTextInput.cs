using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// WebGL builds: edits this TextMeshPro field through a browser input laid over it (WebTextInput.jslib), so phones
/// open their keyboard (TextMeshPro alone never does in WebGL) and browsers can autofill user name and password.
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class WebTextInput : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("Browser autofill hint: username, current-password, new-password, ...")]
    public string autocomplete = "on";

    public void OnPointerDown(PointerEventData _) => Edit();

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int PUCmon_EditText(string text, string type, string autocomplete, int maxLength,
        float[] geometry, string color, string background);
    [DllImport("__Internal")] static extern void PUCmon_PlaceText(int id, float[] geometry);
    [DllImport("__Internal")] static extern int PUCmon_TextState(int id);
    [DllImport("__Internal")] static extern string PUCmon_TakeText(int id);

    static WebTextInput current;  // the field being edited
    int edit;  // 0 = not editing
    readonly float[] geometry = new float[7], placed = new float[7];

    void Edit()
    {
        var field = GetComponent<TMP_InputField>();
        if (!field.IsInteractable())
            return;
        string type = field.contentType == TMP_InputField.ContentType.Password ? "password"
            : field.contentType == TMP_InputField.ContentType.EmailAddress ? "email" : "text";

        WebGLInput.captureAllKeyboardInput = false;  // let the keys reach the browser input
        current = this;
        Measure(field);
        geometry.CopyTo(placed, 0);
        edit = PUCmon_EditText(field.text, type, autocomplete, field.characterLimit, geometry,
            "#" + ColorUtility.ToHtmlStringRGB(field.textComponent.color), "#" + ColorUtility.ToHtmlStringRGB(field.targetGraphic.color));
    }

    // The field in screen pixels from the top left, its text size, side padding and corner radius.
    void Measure(TMP_InputField field)
    {
        var corners = new Vector3[4];  // overlay canvas: world corners are screen pixels, origin bottom left
        ((RectTransform)transform).GetWorldCorners(corners);
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        float scale = canvas.scaleFactor, radius = 0f;
        if (field.targetGraphic is Image image && image.sprite)
            radius = image.sprite.border.x / image.pixelsPerUnitMultiplier * canvas.referencePixelsPerUnit / image.sprite.pixelsPerUnit * scale;
        geometry[0] = corners[0].x;
        geometry[1] = Screen.height - corners[1].y;
        geometry[2] = corners[2].x - corners[0].x;
        geometry[3] = corners[1].y - corners[0].y;
        geometry[4] = field.pointSize * scale;
        geometry[5] = field.textViewport.offsetMin.x * scale;
        geometry[6] = radius;
    }

    void Update()
    {
        if (edit == 0)
            return;
        var field = GetComponent<TMP_InputField>();
        var text = PUCmon_TakeText(edit);
        if (text != null)
            field.text = text;

        int state = PUCmon_TextState(edit);
        if (state == 1)
        {
            // Keep the input on the field if the layout moves (screen turned, keyboard resizing the page).
            Measure(field);
            if (!System.Linq.Enumerable.SequenceEqual(geometry, placed))
            {
                geometry.CopyTo(placed, 0);
                PUCmon_PlaceText(edit, geometry);
            }
            return;
        }
        edit = 0;
        if (current == this)
        {
            current = null;
            WebGLInput.captureAllKeyboardInput = true;
        }
        if (EventSystem.current && EventSystem.current.currentSelectedGameObject == gameObject)
            EventSystem.current.SetSelectedGameObject(null);
        if (state == 3)
            field.onSubmit.Invoke(field.text);
    }
#else
    void Edit() { }  // the Editor and other platforms type into the field directly
#endif
}
