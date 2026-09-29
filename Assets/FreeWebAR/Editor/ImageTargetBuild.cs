using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// WebGL builds: writes image-targets/targets.json plus one luminance image per <see cref="ImageTarget"/> image used
/// in the built scenes, in the format of 8th Wall's image-target-cli (default crop): the page loads them at start.
/// </summary>
class ImageTargetBuild : IPreprocessBuildWithReport, IProcessSceneWithReport, IPostprocessBuildWithReport
{
    const int LuminanceHeight = 640, MinWidth = 480, MinHeight = 640;  // image-target-cli constants
    static readonly List<Texture2D> images = new List<Texture2D>();

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) => images.Clear();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null || report.summary.platform != BuildTarget.WebGL)
            return;  // entering Play mode, or another platform
        foreach (var root in scene.GetRootGameObjects())
        foreach (var target in root.GetComponentsInChildren<ImageTarget>(true))
        {
            if (!target.image)
                Debug.LogWarning($"Free WebAR: ImageTarget \"{target.name}\" has no image; it can never be found.", target);
            else if (!images.Contains(target.image))
                images.Add(target.image);
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;
        if (images.Count == 0)
            throw new BuildFailedException("Free WebAR: the scene has no ImageTarget with an image.");
        var dir = Path.Combine(report.summary.outputPath, "image-targets");
        Directory.CreateDirectory(dir);
        var entries = images.Select(image => Write(image, dir)).ToArray();
        File.WriteAllText(Path.Combine(dir, "targets.json"), "[\n" + string.Join(",\n", entries) + "\n]\n");
        Debug.Log($"Free WebAR: {images.Count} image target(s): {string.Join(", ", images.Select(i => i.name))}");
        images.Clear();
    }

    // One target: luminance crop + its image-target-cli style JSON entry.
    static string Write(Texture2D image, string dir)
    {
        // The source file at full resolution (the imported texture may be downscaled or compressed).
        var source = new Texture2D(2, 2);
        source.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(image)));
        int w = source.width, h = source.height;
        var pixels = source.GetPixels32();  // bottom row first
        Object.DestroyImmediate(source);

        // Luma like the camera frames the tracker compares against (Rec. 601 on the sRGB values), top row first.
        // Landscape images are turned 90° clockwise first, as the CLI does: turned (x, y) = original (y, h - 1 - x).
        bool turned = w >= h;
        int W = turned ? h : w, H = turned ? w : h;
        var gray = new float[W * H];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            int sx = turned ? y : x, sy = turned ? h - 1 - x : y;  // source pixel, top-down
            var c = pixels[(h - 1 - sy) * w + sx];
            gray[y * W + x] = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        }

        // Default crop: the largest centred 3:4 (portrait) rectangle, as the CLI picks it.
        int left = 0, top = 0, cw = W, ch = H;
        if (W / 3f > H / 4f) { cw = Mathf.RoundToInt(H * 3f / 4f); left = Mathf.RoundToInt((W - cw) / 2f); }
        else { ch = Mathf.RoundToInt(W * 4f / 3f); top = Mathf.RoundToInt((H - ch) / 2f); }
        if (cw < MinWidth || ch < MinHeight)
            throw new BuildFailedException($"Free WebAR: image \"{image.name}\" is too small ({w}x{h}); the tracked 3:4 crop needs at least {MinWidth}x{MinHeight} px.");

        // Shrink the crop to 640 px tall with area averaging, and save it as a grey PNG.
        int ow = Mathf.RoundToInt(cw * (float)LuminanceHeight / ch), oh = LuminanceHeight;
        var output = new Color32[ow * oh];
        float sxStep = (float)cw / ow, syStep = (float)ch / oh;
        for (int y = 0; y < oh; y++)
        for (int x = 0; x < ow; x++)
        {
            int x0 = left + (int)(x * sxStep), x1 = Mathf.Max(x0 + 1, left + (int)((x + 1) * sxStep));
            int y0 = top + (int)(y * syStep), y1 = Mathf.Max(y0 + 1, top + (int)((y + 1) * syStep));
            float sum = 0f;
            for (int yy = y0; yy < y1; yy++)
            for (int xx = x0; xx < x1; xx++)
                sum += gray[yy * W + xx];
            byte v = (byte)Mathf.Clamp(Mathf.RoundToInt(sum / ((x1 - x0) * (y1 - y0))), 0, 255);
            output[(oh - 1 - y) * ow + x] = new Color32(v, v, v, 255);
        }
        var png = new Texture2D(ow, oh, TextureFormat.RGB24, false);
        png.SetPixels32(output);
        var file = Slug(image.name) + "_luminance.png";
        File.WriteAllBytes(Path.Combine(dir, file), png.EncodeToPNG());
        Object.DestroyImmediate(png);

        var inv = CultureInfo.InvariantCulture;
        return string.Format(inv,
            "  {{\"imagePath\":\"image-targets/{0}\",\"metadata\":null,\"name\":{1},\"type\":\"PLANAR\"," +
            "\"properties\":{{\"left\":{2},\"top\":{3},\"width\":{4},\"height\":{5},\"isRotated\":{6}," +
            "\"originalWidth\":{7},\"originalHeight\":{8}}}}}",
            file, Json(image.name), left, top, cw, ch, turned ? "true" : "false", W, H);
    }

    static string Slug(string name)
    {
        var sb = new StringBuilder();
        foreach (char c in name.ToLowerInvariant())
            sb.Append(char.IsLetterOrDigit(c) && c < 128 ? c : '-');
        return sb.ToString();
    }

    static string Json(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
