using UnityEngine;

/// <summary>
/// Stands in for a PUCmon's 3D model in builds (put there by LazyModelBuild): the model loads, from Resources, the
/// first time its poster is seen, instead of all of them with the AR scene. Same place, same pose as in the scene.
/// </summary>
public class LazyModel : MonoBehaviour
{
    [Tooltip("The model's path in a Resources folder, without extension, e.g. \"Psoneco\".")]
    public string resource;

    GameObject model;
    ResourceRequest request;

    /// <summary>Whether the model is in place.</summary>
    public bool Ready => model;

    /// <summary>Whether everything on this poster has loaded (a poster with no lazy model always has).</summary>
    public static bool IsReady(Component target)
    {
        var lazy = target.GetComponentInChildren<LazyModel>(true);
        return !lazy || lazy.Ready;
    }

    void OnEnable()
    {
        if (model || request != null)
            return;
        request = Resources.LoadAsync<GameObject>(resource);
        request.completed += _ =>
        {
            if (!this)
                return;
            if (request.asset is GameObject prefab)
            {
                model = Instantiate(prefab, transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
            }
            else
                Debug.LogError($"LazyModel: no model at Resources/{resource}.", this);
            request = null;
        };
    }
}
