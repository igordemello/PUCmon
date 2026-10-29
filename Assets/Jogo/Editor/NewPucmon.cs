using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// "PUCmon > Novo PUCmon...": a new PUCmon from its 3D model, poster, name and description. Files get the PUCmon's name
/// (Assets/ImagesToTracker, Assets/Modelos; the poster's name is its id in the catalog, Supabase and the tracking
/// data); the catalog gets its entry; the AR scene gets its image target, laid out like the existing ones (wall hole,
/// model standing in front of the poster at the same size); saving the scene makes its 2D picture (PortraitMaker).
/// </summary>
class NewPucmon : EditorWindow
{
    const string ArScene = "Assets/Scenes/SampleScene.unity", Posters = "Assets/ImagesToTracker", Models = "Assets/Modelos";
    static readonly string[] ModelFiles = { ".glb", ".gltf", ".fbx", ".obj", ".blend", ".dae" };

    string displayName = "", represents = "", description = "";
    GameObject model;
    Texture2D poster;

    [MenuItem("PUCmon/Novo PUCmon...")]
    static void Open() => GetWindow<NewPucmon>(true, "Novo PUCmon").minSize = new Vector2(440f, 380f);

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Preencha e clique em Criar. O alvo de AR, o buraco na parede, a entrada no Catálogo e a " +
            "imagem 2D são feitos sozinhos. As ações, a passiva e o diálogo da primeira visita ao recinto (Lab Intro) você " +
            "preenche depois, no Catálogo.", MessageType.Info);
        displayName = EditorGUILayout.TextField("Nome", displayName);
        represents = EditorGUILayout.TextField(new GUIContent("Representa", "Um curso (\"Psicologia\") ou outra coisa da PUC (\"Bandejão\")."), represents);
        EditorGUILayout.LabelField("Descrição");
        description = EditorGUILayout.TextArea(description, EditorStyles.textArea, GUILayout.MinHeight(80f));
        model = (GameObject)EditorGUILayout.ObjectField("Modelo 3D (.glb)", model, typeof(GameObject), false);
        poster = (Texture2D)EditorGUILayout.ObjectField("Cartaz (imagem alvo)", poster, typeof(Texture2D), false);
        var problem = Problem();
        if (problem != null)
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
        using (new EditorGUI.DisabledScope(problem != null))
            if (GUILayout.Button("Criar PUCmon", GUILayout.Height(34f)))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;
                var target = Create(displayName.Trim(), represents.Trim(), description.Trim(), model, poster);
                Selection.activeGameObject = target.gameObject;
                EditorGUIUtility.PingObject(target);
                EditorUtility.DisplayDialog("PUCmon", $"{displayName.Trim()} foi criado! Confira a pose do modelo na cena do AR; " +
                    "se mexer nela, salve a cena e a imagem 2D é refeita.", "OK");
                Close();
            }
    }

    string Problem()
    {
        string name = displayName.Trim();
        if (name == "")
            return "Escreva o nome.";
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return "O nome não pode ter / \\ : * ? \" < > |";
        if (Catalog().Find(name) != null)
            return "Já existe um PUCmon com esse nome.";
        if (!model || !ModelFiles.Contains(Path.GetExtension(AssetDatabase.GetAssetPath(model)).ToLowerInvariant()))
            return "Arraste o modelo 3D (um arquivo .glb da pasta Assets).";
        if (!poster)
            return "Arraste a imagem do cartaz.";
        if (Taken(poster, Posters, name) || Taken(model, Models, name))
            return "Já existe um arquivo com esse nome em ImagesToTracker ou Modelos.";
        return null;
    }

    /// <summary>Creates the PUCmon; returns its image target in the (open, saved) AR scene.</summary>
    public static ImageTarget Create(string name, string represents, string description, GameObject model, Texture2D poster)
    {
        poster = (Texture2D)Move(poster, Posters, name);
        model = (GameObject)Move(model, Models, name);

        // The catalog entry first, so the picture made when the scene is saved goes into it.
        var catalog = Catalog();
        catalog.creatures.Add(new Catalog.Creature { poster = poster.name, displayName = name, represents = represents, description = description });
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        var scene = SceneManagerOpen();
        var targets = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ImageTarget>(true)).ToList();
        var template = targets.First(t => ModelOf(t) && t.GetComponentInChildren<WallHole>(true));
        var templateModel = ModelOf(template);
        var templateHole = template.GetComponentInChildren<WallHole>(true);

        // The new target, in line with the others, with the template's settings and this poster.
        var go = new GameObject($"Image Target ({poster.name})");
        go.transform.SetParent(template.transform.parent, false);
        go.transform.localRotation = template.transform.localRotation;
        go.transform.localScale = template.transform.localScale;
        go.transform.localPosition = template.transform.localPosition + Vector3.right * (targets.Max(t => t.transform.localPosition.x) + 2f - template.transform.localPosition.x);
        var target = go.AddComponent<ImageTarget>();
        EditorUtility.CopySerialized(template, target);
        target.image = poster;

        // The model stands where the template's does, scaled to its height, feet on the same spot.
        var content = templateModel.transform.parent == template.transform ? go.transform : Copy(templateModel.transform.parent, go.transform);
        var creature = (GameObject)PrefabUtility.InstantiatePrefab(model, content);
        creature.transform.localPosition = templateModel.transform.localPosition;
        creature.transform.localRotation = templateModel.transform.localRotation;
        creature.transform.localScale = templateModel.transform.localScale;
        var want = Bounds(templateModel, template.transform);  // each in its own target's space
        var have = Bounds(creature, go.transform);
        if (have.size.y > 1e-5f)
        {
            creature.transform.localScale *= want.size.y / have.size.y;
            have = Bounds(creature, go.transform);
        }
        creature.transform.position += go.transform.TransformVector(new Vector3(want.center.x - have.center.x, want.min.y - have.min.y, want.center.z - have.center.z));
        have = Bounds(creature, go.transform);

        // The wall hole behind it: the template's, as wide as this model needs, with its own cracks.
        var holeSource = PrefabUtility.GetCorrespondingObjectFromSource(PrefabUtility.GetOutermostPrefabInstanceRoot(templateHole.gameObject));
        var holeObject = holeSource ? (GameObject)PrefabUtility.InstantiatePrefab(holeSource, go.transform) : Object.Instantiate(templateHole.gameObject, go.transform);
        holeObject.transform.localPosition = templateHole.transform.localPosition;
        holeObject.transform.localRotation = templateHole.transform.localRotation;
        holeObject.transform.localScale = templateHole.transform.localScale;
        var hole = holeObject.GetComponentInChildren<WallHole>(true);
        EditorUtility.CopySerialized(templateHole, hole);
        var settings = new SerializedObject(hole);  // through properties, so the hole rebuilds (OnValidate)
        settings.FindProperty("size").vector2Value = new Vector2(hole.size.x * Mathf.Max(0.6f, have.size.x / Mathf.Max(1e-5f, want.size.x)), hole.size.y);
        settings.FindProperty("seed").intValue = Random.Range(1, 10000);
        settings.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);  // PortraitMaker makes its 2D picture now
        return target;
    }

    // ---------- helpers ----------
    static Catalog Catalog() => AssetDatabase.FindAssets("t:Catalog").Select(g => AssetDatabase.LoadAssetAtPath<Catalog>(AssetDatabase.GUIDToAssetPath(g))).First();

    static UnityEngine.SceneManagement.Scene SceneManagerOpen()
    {
        var scene = EditorSceneManager.GetSceneByPath(ArScene);
        return scene.isLoaded ? scene : EditorSceneManager.OpenScene(ArScene);
    }

    static string Destination(Object asset, string folder, string name) => $"{folder}/{name}{Path.GetExtension(AssetDatabase.GetAssetPath(asset))}";

    static bool Taken(Object asset, string folder, string name)
    {
        string to = Destination(asset, folder, name);
        return to != AssetDatabase.GetAssetPath(asset) && File.Exists(to);
    }

    // Moves and renames the file to folder/name, keeping its references.
    static Object Move(Object asset, string folder, string name)
    {
        string from = AssetDatabase.GetAssetPath(asset), to = Destination(asset, folder, name);
        if (from != to)
        {
            var error = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(error))
                throw new System.InvalidOperationException(error);
        }
        return AssetDatabase.LoadMainAssetAtPath(to);
    }

    // The outermost instance of a model file (.glb, ...) under this target.
    static GameObject ModelOf(ImageTarget target) => target.GetComponentsInChildren<Renderer>(true)
        .Select(r => PrefabUtility.GetOutermostPrefabInstanceRoot(r.gameObject))
        .FirstOrDefault(root => root && ModelFiles.Contains(Path.GetExtension(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(root))).ToLowerInvariant()));

    // An empty copy of a node (name and local transform), e.g. the "Content" holder.
    static Transform Copy(Transform source, Transform parent)
    {
        var copy = new GameObject(source.name).transform;
        copy.SetParent(parent, false);
        copy.localPosition = source.localPosition;
        copy.localRotation = source.localRotation;
        copy.localScale = source.localScale;
        return copy;
    }

    // Bounds of an object's meshes in `space`'s coordinates (from the meshes: renderer bounds lag right after creation).
    static Bounds Bounds(GameObject root, Transform space)
    {
        var bounds = new Bounds();
        bool any = false;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            var mesh = r is SkinnedMeshRenderer skinned ? skinned.sharedMesh : r.TryGetComponent<MeshFilter>(out var filter) ? filter.sharedMesh : null;
            if (!mesh)
                continue;
            var b = mesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                var corner = space.InverseTransformPoint(r.transform.TransformPoint(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) * 2 - 1, (i & 2) - 1, (i & 4) / 2 - 1))));
                if (!any)
                    bounds = new Bounds(corner, Vector3.zero);
                else
                    bounds.Encapsulate(corner);
                any = true;
            }
        }
        return bounds;
    }
}
