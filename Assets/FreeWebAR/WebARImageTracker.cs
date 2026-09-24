using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Moves <see cref="target"/> onto the printed image tracked by the 8th Wall engine in the page
/// (WebGLTemplates/FreeWebAR) and gives this camera the engine's projection, so the overlay matches the camera feed.
/// Target space matches Zappar's: image centred on the origin, 2 units tall, content towards the camera on local -Z.
/// Only runs in WebGL builds; in the Editor the target stays where it was placed.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WebARImageTracker : MonoBehaviour
{
    public Transform target;
    [Tooltip("Seconds the content takes to grow in when the image is found (0 = appear at once).")]
    public float appearSeconds = 0.2f;
    public UnityEvent onTargetFound = new UnityEvent();
    public UnityEvent onTargetLost = new UnityEvent();

    public bool IsTracked { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int FreeWebAR_ReadPose(float[] pose);

    // [tracked, position xyz, forward xyz, up xyz, projection 16 (column-major)], written by the page.
    readonly float[] pose = new float[26];
    Camera cam;
    float foundAt;

    void Awake()
    {
        cam = GetComponent<Camera>();
        // Transparent clear: the camera feed canvas behind this one shows through.
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        target.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (FreeWebAR_ReadPose(pose) == 0)
            return;
        if (pose[10] > 0f && !float.IsNaN(pose[15]))  // a perspective matrix has m00 > 0
        {
            var projection = new Matrix4x4();
            for (int i = 0; i < 16; i++)
                projection[i] = pose[10 + i];
            cam.projectionMatrix = projection;
        }

        bool tracked = pose[0] > 0.5f;
        if (tracked != IsTracked)
        {
            IsTracked = tracked;
            foundAt = Time.unscaledTime;
            target.gameObject.SetActive(tracked);
            (tracked ? onTargetFound : onTargetLost).Invoke();
        }
        if (!tracked)
            return;
        var rotation = Quaternion.LookRotation(new Vector3(pose[4], pose[5], pose[6]), new Vector3(pose[7], pose[8], pose[9]));
        target.SetPositionAndRotation(transform.TransformPoint(pose[1], pose[2], pose[3]), transform.rotation * rotation);
        float t = appearSeconds > 0f ? Mathf.Clamp01((Time.unscaledTime - foundAt) / appearSeconds) : 1f;
        target.localScale = Vector3.one * (1f - (1f - t) * (1f - t) * (1f - t));  // ease-out cubic
    }
#endif
}
