using UnityEditor;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.Editor
{
    public static class ObjectsCreator
    {
        [MenuItem("GameObject/AR-Renderer/Face Tracking", false, -1)]
        static void CreatFaceScene()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/Controllers/FaceController.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Markers Tracking", false, -1)]
        static void CreatMarkerScene()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/Controllers/MarkerController.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Geolocation Tracking", false, -1)]
        static void CreatGeolocationScene()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/Controllers/GeolocationController.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Targets Tracking", false, -1)]
        static void CreateTargetScene()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/Controllers/TargetController.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Custom Target Tracking", false, -1)]
        static void CreateCustomTargetScene()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/Controllers/CustomTargetController.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        [MenuItem("GameObject/AR-Renderer/Misc/BlendShapesSystem", false, -1)]
        static void CreateBlendShapesSystem()
        {
            GameObject target = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/BlendShapeSystem.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(target) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        [MenuItem("GameObject/AR-Renderer/Misc/Target", false, -1)]
        static void CreateTarget()
        {
            GameObject target = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/Target.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(target) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Misc/Location", false, -1)]
        static void CreateLocation()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/GeolocationLocation.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Misc/Arrow", false, -1)]
        static void CreateArrow()
        {
            GameObject arController = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/GeolocationArrow.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(arController) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/AR-Renderer/Misc/FixedEventSystem", false, -1)]
        static void CreateEventSystem()
        {
            GameObject target = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AR-Renderer/Prefabs/FixedEventSystem.prefab", typeof(GameObject));
            GameObject instance = PrefabUtility.InstantiatePrefab(target) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
    }
}