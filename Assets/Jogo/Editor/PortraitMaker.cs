using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps the PUCmon catalog (Catalogo) in step with the AR scene. Every ImageTarget holding a 3D model (meshes from a
/// .glb, .fbx, ... file) is a PUCmon: when the scene is saved, new ones join the catalog (named after their poster,
/// rename them there) with a pixel-art picture of the model as it stands on its poster, seen from the viewer's side;
/// a picture is redone when its PUCmon's model or pose changed. "PUCmon > Gerar imagens 2D" redoes them all.
/// </summary>
[InitializeOnLoad]
static class PortraitMaker
{
    // Rendered at Shot x Shot, then shrunk Shot / Pixels times into hard-edged pixels with a navy outline.
    const int Shot = 480, Pixels = 96, Layer = 31;
    static readonly Color32 Outline = new Color32(0x10, 0x1F, 0x3F, 0xFF);
    const string Folder = "Assets/Jogo/Retratos";
    static readonly string[] ModelFiles = { ".glb", ".gltf", ".fbx", ".obj", ".blend", ".dae" };

    static PortraitMaker() => EditorSceneManager.sceneSaved += scene => Make(scene, onlyMissing: true);

    [MenuItem("PUCmon/Gerar imagens 2D")]
    static void MakeAll()
    {
        int made = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            made += Make(SceneManager.GetSceneAt(i), onlyMissing: false);
        EditorUtility.DisplayDialog("PUCmon", made > 0
            ? $"{made} imagens 2D geradas em {Folder}."
            : "Nenhum PUCmon nas cenas abertas. Abra a cena do AR (SampleScene) e tente de novo.", "OK");
    }

    /// <summary>Adds the scene's new PUCmons to the catalog and renders the pictures missing or out of date (or all).</summary>
    public static int Make(Scene scene, bool onlyMissing)
    {
        var catalog = AssetDatabase.FindAssets("t:Catalog").Select(g => AssetDatabase.LoadAssetAtPath<Catalog>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();
        if (!catalog || !scene.isLoaded)
            return 0;
        int made = 0;
        foreach (var target in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ImageTarget>(true)))
        {
            var model = target.image ? target.GetComponentsInChildren<Renderer>(true).Where(FromModelFile).ToArray() : null;
            if (model == null || model.Length == 0)
                continue;
            var creature = catalog.Find(target.image.name);
            var source = Source(target, model);
            if (creature == null)
                catalog.creatures.Add(creature = new Catalog.Creature { poster = target.image.name, displayName = target.image.name });
            else if (onlyMissing && creature.portrait && creature.portraitOf == source)
                continue;
            creature.portrait = Render(target, model, $"{Folder}/{target.image.name}.png");
            creature.portraitOf = source;
            EditorUtility.SetDirty(catalog);
            made++;
        }
        if (made > 0)
            AssetDatabase.SaveAssets();
        return made;
    }

    static Mesh MeshOf(Renderer renderer) => renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh
        : renderer.TryGetComponent<MeshFilter>(out var filter) ? filter.sharedMesh : null;

    static bool FromModelFile(Renderer renderer)
    {
        var mesh = MeshOf(renderer);
        return mesh && ModelFiles.Contains(Path.GetExtension(AssetDatabase.GetAssetPath(mesh)).ToLowerInvariant());
    }

    // Which meshes, where on the poster: the picture is out of date when this changes (another model, a new pose).
    static string Source(ImageTarget target, Renderer[] model) => string.Join(";", model.Select(r =>
    {
        var m = target.transform.worldToLocalMatrix * r.transform.localToWorldMatrix;
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(MeshOf(r))) + "/" + MeshOf(r).name + "@" +
            string.Join(",", Enumerable.Range(0, 16).Select(i => m[i].ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));
    }));

    // Photographs the model where it stands, alone (its own layer), from the viewer's side of the poster (-Z), a bit
    // to the side and from above, on a transparent background.
    static Sprite Render(ImageTarget target, Renderer[] model, string path)
    {
        var poster = target.transform;
        bool wasActive = target.gameObject.activeSelf;
        var layers = model.ToDictionary(r => r, r => r.gameObject.layer);
        target.gameObject.SetActive(true);
        foreach (var r in model)
            r.gameObject.layer = Layer;

        var bounds = model[0].bounds;
        foreach (var r in model)
            bounds.Encapsulate(r.bounds);
        var cam = new GameObject("Portrait camera") { hideFlags = HideFlags.HideAndDontSave }.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.cullingMask = 1 << Layer;
        cam.fieldOfView = 30f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 1000f;
        var view = poster.TransformDirection(Quaternion.Euler(12f, -22f, 0f) * Vector3.back);
        cam.transform.position = bounds.center + view * (bounds.extents.magnitude / Mathf.Sin(15f * Mathf.Deg2Rad) * 0.74f);
        cam.transform.LookAt(bounds.center, poster.up);
        var lights = new[] { AddLight(poster, new Vector3(0.45f, -0.55f, 0.7f), 1.1f), AddLight(poster, new Vector3(-0.6f, -0.1f, 0.8f), 0.45f) };

        var texture = new RenderTexture(Shot, Shot, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
        cam.targetTexture = texture;
        cam.Render();
        RenderTexture.active = texture;
        var shot = new Texture2D(Shot, Shot, TextureFormat.RGBA32, false);
        shot.ReadPixels(new Rect(0, 0, Shot, Shot), 0, 0);
        RenderTexture.active = null;
        var picture = new Texture2D(Pixels, Pixels, TextureFormat.RGBA32, false);
        picture.SetPixels32(Pixelate(shot.GetPixels32()));
        Directory.CreateDirectory(Folder);
        File.WriteAllBytes(path, picture.EncodeToPNG());

        Object.DestroyImmediate(shot);
        Object.DestroyImmediate(picture);
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(cam.gameObject);
        foreach (var light in lights)
            Object.DestroyImmediate(light.gameObject);
        foreach (var r in model)
            r.gameObject.layer = layers[r];
        target.gameObject.SetActive(wasActive);

        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite || importer.filterMode != FilterMode.Point)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Each Pixels-sized cell averages a block of the shot: solid where the model covers most of it, clear elsewhere;
    // clear cells touching the model become the outline.
    static Color32[] Pixelate(Color32[] shot)
    {
        int k = Shot / Pixels;
        var cells = new Color32[Pixels * Pixels];
        for (int y = 0; y < Pixels; y++)
        for (int x = 0; x < Pixels; x++)
        {
            float r = 0, g = 0, b = 0, a = 0;
            for (int j = 0; j < k; j++)
            for (int i = 0; i < k; i++)
            {
                var p = shot[(y * k + j) * Shot + x * k + i];
                r += p.r * p.a; g += p.g * p.a; b += p.b * p.a; a += p.a;
            }
            cells[y * Pixels + x] = a < 0.45f * 255 * k * k ? new Color32(0, 0, 0, 0)
                : new Color32((byte)(r / a), (byte)(g / a), (byte)(b / a), 255);
        }
        var outlined = (Color32[])cells.Clone();
        for (int y = 0; y < Pixels; y++)
        for (int x = 0; x < Pixels; x++)
            if (cells[y * Pixels + x].a == 0 && (Solid(cells, x - 1, y) || Solid(cells, x + 1, y) || Solid(cells, x, y - 1) || Solid(cells, x, y + 1)))
                outlined[y * Pixels + x] = Outline;
        return outlined;
    }

    static bool Solid(Color32[] cells, int x, int y) => x >= 0 && y >= 0 && x < Pixels && y < Pixels && cells[y * Pixels + x].a > 0;

    static Light AddLight(Transform poster, Vector3 direction, float intensity)
    {
        var light = new GameObject("Portrait light") { hideFlags = HideFlags.HideAndDontSave }.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = intensity;
        light.transform.rotation = Quaternion.LookRotation(poster.TransformDirection(direction));
        return light;
    }
}
