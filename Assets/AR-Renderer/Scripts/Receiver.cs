// #define VERBOSE

using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public static class Receiver
    {
        private static bool isActivated = false;

        [DllImport("__Internal")]
        private static extern void set_callbacks(delegate_TargetTransform vtt,
                                                    delegate_FaceTransform vft,
                                                    delegate_MarkerTransform vmt,
                                                    delegate_GeolocationTransform vgt,
                                                    delegate_ScanningFinished vsf,
                                                    delegate_SceneLoaded vsl);

        delegate void delegate_TargetTransform(Vector3 position, Quaternion rotation, Vector3 scale, float cameraFov, int targetIndex);
        delegate void delegate_FaceTransform(Vector3 position, Quaternion rotation, Vector3 scale, float cameraFov, string faceMeshData, int status);
        delegate void delegate_MarkerTransform(Vector3 position, Quaternion rotation, float distanceToHitPoint, float cameraFov);
        delegate void delegate_GeolocationTransform(Vector3 position, Quaternion rotation, float cameraFov, GeolocationCoordinates geoPos);
        delegate void delegate_ScanningFinished();
        delegate void delegate_SceneLoaded(int sceneIndex);

        [RuntimeInitializeOnLoadMethod]
        public static void Activate()
        {
            if (!isActivated)
            {
                set_callbacks(
                    p_TargetTransform,
                    p_FaceTransform,
                    p_MarkerTransform,
                    p_GeolocationTransform,
                    p_ScanningFinished,
                    p_SceneLoaded);
                isActivated = true;
            }
        }

        [MonoPInvokeCallback(typeof(delegate_TargetTransform))]
        private static void p_TargetTransform(Vector3 position, Quaternion rotation, Vector3 scale, float cameraFov, int targetIndex)
        {
            EventBus.onTargetTracking?.Invoke(position, rotation, scale, cameraFov, targetIndex);
        }

        [MonoPInvokeCallback(typeof(delegate_FaceTransform))]
        private static void p_FaceTransform(Vector3 position, Quaternion rotation, Vector3 scale, float cameraFov, string faceMeshData, int status)
        {
            Vector3[] facemeshVertices = Utils.FaceVerticies(faceMeshData);
            EventBus.onFaceTracking?.Invoke(position, rotation, scale, cameraFov, facemeshVertices, status);
        }

        [MonoPInvokeCallback(typeof(delegate_MarkerTransform))]
        private static void p_MarkerTransform(Vector3 position, Quaternion rotation, float distanceToHitPoint, float cameraFov)
        {
            EventBus.onMarkerTracking?.Invoke(position, rotation, distanceToHitPoint, cameraFov);
        }

        [MonoPInvokeCallback(typeof(delegate_GeolocationTransform))]
        private static void p_GeolocationTransform(Vector3 position, Quaternion rotation, float cameraFov, GeolocationCoordinates geoPos)
        {
            EventBus.onGeolocationTracking?.Invoke(position, rotation, cameraFov, geoPos);
        }

        [MonoPInvokeCallback(typeof(delegate_ScanningFinished))]
        private static void p_ScanningFinished()
        {
            EventBus.onScanningFinished?.Invoke();
        }

        [MonoPInvokeCallback(typeof(delegate_SceneLoaded))]
        private static void p_SceneLoaded(int sceneIndex)
        {
            EventBus.onSceneLoaded?.Invoke(sceneIndex);
        }
    }
}