using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A printed image the camera can find. Put the content for it as children of this object.
/// Space matches Zappar's: image centred on the origin, 2 units tall, content towards the camera on local -Z.
/// Its tracking data is generated from <see cref="image"/> when the WebGL build is made (ImageTargetBuild).
/// Several targets can live in one scene; the camera follows one at a time (WebARImageTracker).
/// </summary>
public class ImageTarget : MonoBehaviour
{
    [Tooltip("The printed image, e.g. from Assets/ImagesToTracker. Detailed art with lots of edges tracks best.")]
    public Texture2D image;
    [Tooltip("Seconds the content takes to grow in when this image is found (0 = appear at once).")]
    public float appearSeconds = 0.2f;
    public UnityEvent onFound = new UnityEvent();
    public UnityEvent onLost = new UnityEvent();

    /// <summary>True while the camera is tracking this image.</summary>
    public bool IsTracked { get; internal set; }

    // The printed image's outline in the Scene view, to place content on it.
    void OnDrawGizmos()
    {
        if (!image)
            return;
        float halfWidth = (float)image.width / image.height;
        Gizmos.color = IsTracked ? Color.green : new Color(1f, 0.8f, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f * halfWidth, 2f, 0f));
    }
}
