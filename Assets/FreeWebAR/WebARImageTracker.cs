using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Follows the printed images of the scene's <see cref="ImageTarget"/>s, tracked in the page (WebGLTemplates/FreeWebAR),
/// one at a time: moves the one in view onto the image, shows it, and gives this camera the page's projection so the
/// content matches the camera feed. Only runs in WebGL builds; in the Editor the targets stay where they were placed.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WebARImageTracker : MonoBehaviour
{
    /// <summary>The target in view, or null.</summary>
    public ImageTarget Current { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int FreeWebAR_ReadPose(float[] pose);
    [DllImport("__Internal")] static extern string FreeWebAR_TargetName(int index);

    // [tracked, position xyz, forward xyz, up xyz, projection 16 (column-major), target index], written by the page.
    readonly float[] pose = new float[27];
    readonly Dictionary<int, ImageTarget> byIndex = new Dictionary<int, ImageTarget>();
    ImageTarget[] targets;
    Camera cam;
    float foundAt;

    void Awake()
    {
        cam = GetComponent<Camera>();
        // Transparent clear: the camera feed canvas behind this one shows through.
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        targets = FindObjectsOfType<ImageTarget>(true);
        foreach (var target in targets)
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

        var seen = pose[0] > 0.5f ? Find((int)pose[26]) : null;
        if (seen != Current)
        {
            if (Current)
            {
                Current.IsTracked = false;
                Current.gameObject.SetActive(false);
                Current.onLost.Invoke();
            }
            Current = seen;
            foundAt = Time.unscaledTime;
            if (Current)
            {
                Current.IsTracked = true;
                Current.gameObject.SetActive(true);
                Current.onFound.Invoke();
            }
        }
        if (!Current)
            return;

        var target = Current.transform;
        var rotation = Quaternion.LookRotation(new Vector3(pose[4], pose[5], pose[6]), new Vector3(pose[7], pose[8], pose[9]));
        target.SetPositionAndRotation(transform.TransformPoint(pose[1], pose[2], pose[3]), transform.rotation * rotation);
        float t = Current.appearSeconds > 0f ? Mathf.Clamp01((Time.unscaledTime - foundAt) / Current.appearSeconds) : 1f;
        target.localScale = Vector3.one * (1f - (1f - t) * (1f - t) * (1f - t));  // ease-out cubic
    }

    // The page numbers its targets; match the number to the ImageTarget whose image has that name.
    ImageTarget Find(int index)
    {
        if (!byIndex.TryGetValue(index, out var target))
        {
            string name = FreeWebAR_TargetName(index);
            target = Array.Find(targets, t => t.image && t.image.name == name);
            if (!target)
                Debug.LogWarning($"Free WebAR: no ImageTarget in the scene uses the image \"{name}\".");
            byIndex[index] = target;
        }
        return target;
    }
#endif
}
