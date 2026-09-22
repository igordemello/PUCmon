using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Moves <see cref="target"/> onto the printed image tracked by MindAR in the page (WebGLTemplates/FreeWebAR).
/// Target space matches Zappar's: image centred on the origin, 2 units tall, content towards the camera on local -Z.
/// Only runs in WebGL builds; in the Editor the target stays where it was placed.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WebARImageTracker : MonoBehaviour
{
    public Transform target;
    public UnityEvent onTargetFound = new UnityEvent();
    public UnityEvent onTargetLost = new UnityEvent();

    public bool IsTracked { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int FreeWebAR_ReadPose(float[] pose);

    // [tracked, fovY, position xyz, forward xyz, up xyz], written by the page's tracker.
    readonly float[] pose = new float[11];
    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        // Transparent clear: the camera <video> behind the canvas shows through.
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        target.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        bool tracked = FreeWebAR_ReadPose(pose) == 1 && pose[0] > 0.5f;
        if (tracked)
        {
            cam.fieldOfView = pose[1];
            var rotation = Quaternion.LookRotation(new Vector3(pose[5], pose[6], pose[7]), new Vector3(pose[8], pose[9], pose[10]));
            target.SetPositionAndRotation(transform.TransformPoint(pose[2], pose[3], pose[4]), transform.rotation * rotation);
        }
        if (tracked == IsTracked)
            return;
        IsTracked = tracked;
        target.gameObject.SetActive(tracked);
        (tracked ? onTargetFound : onTargetLost).Invoke();
    }
#endif
}
