using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds only: in the built copy of a scene, each poster's PUCmon model (a model file in a Resources folder, e.g.
/// Assets/Modelos/Resources) gives way to a <see cref="LazyModel"/> in the same place, so the AR scene opens without
/// loading every model and each one loads when its poster is first seen. The scene on disk keeps its models.
/// A model changed in the scene beyond its own position, rotation and size stays as it is (with a warning).
/// </summary>
class LazyModelBuild : IProcessSceneWithReport
{
    public int callbackOrder => 1;  // after ImageTargetBuild has listed the posters

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null)
            return;  // entering Play mode: keep the models
        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        foreach (var target in root.GetComponentsInChildren<ImageTarget>(true))
        foreach (var (model, resource) in Models(target).ToList())
        {
            var holder = new GameObject(model.name);
            holder.layer = model.gameObject.layer;
            holder.transform.SetParent(model.parent, false);
            holder.transform.SetSiblingIndex(model.GetSiblingIndex());
            holder.transform.localPosition = model.localPosition;
            holder.transform.localRotation = model.localRotation;
            holder.transform.localScale = model.localScale;
            holder.AddComponent<LazyModel>().resource = resource;
            Object.DestroyImmediate(model.gameObject);
            count++;
        }
        if (count > 0)
            Debug.Log($"LazyModel: {count} model(s) of {scene.name} load when their poster is seen.");
    }

    // The instances of model files from Resources under this poster, with each one's Resources path. Prefab links are
    // gone in a build, so a model is found by its meshes (sub-assets of the file) and its instance root is the
    // nearest ancestor shaped exactly like the file's (which also tells it was not changed in the scene).
    static IEnumerable<(Transform model, string resource)> Models(ImageTarget target)
    {
        var found = new HashSet<Transform>();
        foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            var mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : renderer.TryGetComponent<MeshFilter>(out var filter) ? filter.sharedMesh : null;
            string path = mesh ? AssetDatabase.GetAssetPath(mesh) : "";
            int at = path.IndexOf("/Resources/");
            if (at < 0 || !AssetDatabase.IsSubAsset(mesh))
                continue;
            var file = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!file)
                continue;
            var model = renderer.transform;
            while (model && model != target.transform && !SameAs(model, file.transform, true))
                model = model.parent;
            if (!model || model == target.transform)
            {
                Debug.LogWarning($"LazyModel: \"{renderer.name}\" ({path}) was changed in the scene, so it loads with the scene.", renderer);
                continue;
            }
            if (found.Add(model))
                yield return (model, Path.ChangeExtension(path.Substring(at + "/Resources/".Length), null));
        }
    }

    // Same hierarchy, components, meshes, materials and (below the root) placement as the model file.
    static bool SameAs(Transform scene, Transform file, bool root)
    {
        if (!root && (scene.localPosition != file.localPosition || scene.localRotation != file.localRotation || scene.localScale != file.localScale))
            return false;
        if (scene.childCount != file.childCount || scene.gameObject.activeSelf != file.gameObject.activeSelf)
            return false;
        var a = scene.GetComponents<Component>().Select(c => c.GetType());
        var b = file.GetComponents<Component>().Select(c => c.GetType());
        if (!a.SequenceEqual(b))
            return false;
        if (scene.TryGetComponent<Renderer>(out var r) && !r.sharedMaterials.SequenceEqual(file.GetComponent<Renderer>().sharedMaterials))
            return false;
        if (scene.TryGetComponent<MeshFilter>(out var f) && f.sharedMesh != file.GetComponent<MeshFilter>().sharedMesh)
            return false;
        if (scene.TryGetComponent<SkinnedMeshRenderer>(out var s) && s.sharedMesh != file.GetComponent<SkinnedMeshRenderer>().sharedMesh)
            return false;
        for (int i = 0; i < scene.childCount; i++)
            if (!SameAs(scene.GetChild(i), file.GetChild(i), false))
                return false;
        return true;
    }
}
