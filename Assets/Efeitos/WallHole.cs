using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A hole broken through the wall behind a poster's creature: chipped plaster around a jagged break, cracks over the
/// poster and a dark tunnel that seems endless. An invisible wall around the hole (DepthMask material) hides anything
/// virtual behind the poster except through the hole, so a creature can come out of it. Lies on the poster plane
/// (local XY, centred on this object); the tunnel goes into the wall (+Z, away from the viewer).
/// Use the "Buraco na parede" prefab: its renderer has the two materials (DepthMask, WallHole), in that order.
/// </summary>
[ExecuteAlways, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WallHole : MonoBehaviour
{
    [Tooltip("Width and height of the hole, in poster units (the poster is 2 tall).")]
    public Vector2 size = new Vector2(0.9f, 1.2f);
    [Range(0f, 0.4f)] public float jaggedness = 0.22f;
    [Range(0, 12)] public int cracks = 6;
    [Tooltip("How far the tunnel goes into the wall.")]
    public float depth = 3f;
    [Tooltip("Change it for a different break and different cracks.")]
    public int seed = 1;
    public Color plaster = new Color(0.86f, 0.83f, 0.78f);
    public Color deep = new Color(0.07f, 0.03f, 0.14f);

    const int Sides = 32, Rings = 14;
    const float WallThickness = 0.07f, MaskRadius = 12f;
    Mesh mesh;
    bool dirty;

    void OnEnable()
    {
        if (!mesh)
            Build();
    }

    void OnValidate() => dirty = true;

    void Update()
    {
        if (dirty)
            Build();
    }

    void OnDestroy()
    {
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
    }

    void Build()
    {
        dirty = false;
        var random = new System.Random(seed);
        float Rand() => (float)random.NextDouble();
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var depths = new List<Vector2>();  // uv.y: 0 at the wall, 1 at the end of the tunnel (drives the shader's bands)
        var wall = new List<int>();
        var hole = new List<int>();
        int Add(Vector2 p, float z, Color color, float d)
        {
            vertices.Add(new Vector3(p.x, p.y, z));
            colors.Add(color);
            depths.Add(new Vector2(0f, d));
            return vertices.Count - 1;
        }
        void Quad(List<int> to, int a, int b, int c, int d) => to.AddRange(new[] { a, b, c, b, d, c });
        Color Rough(Color c, float amount) => c * (1f - amount * Rand());

        // The break: an ellipse with every other point pulled in, like chipped masonry.
        var edge = new Vector2[Sides];
        var round = new Vector2[Sides];
        for (int i = 0; i < Sides; i++)
        {
            float a = i * Mathf.PI * 2f / Sides;
            round[i] = new Vector2(Mathf.Cos(a) * size.x, Mathf.Sin(a) * size.y) * 0.5f;
            edge[i] = round[i] * (1f - jaggedness * (i % 2 == 0 ? 0.5f * Rand() : 0.4f + 0.6f * Rand()));
        }

        // Invisible wall from the break out to MaskRadius.
        for (int i = 0; i < Sides; i++)
        {
            int j = (i + 1) % Sides;
            Quad(wall, Add(edge[i], 0f, Color.black, 0f), Add(edge[j], 0f, Color.black, 0f),
                Add(edge[i].normalized * MaskRadius, 0f, Color.black, 0f), Add(edge[j].normalized * MaskRadius, 0f, Color.black, 0f));
        }

        // Tunnel: the break, the same shape one wall thickness in (the cut through the wall), then rings going deeper,
        // rounder, narrower and darker, closed by a black cap.
        int[] previous = null;
        Vector2 drift = Vector2.zero;
        float z = 0f;
        for (int r = 0; r <= Rings; r++)
        {
            float k = r <= 1 ? 0f : (r - 1f) / (Rings - 1);
            z = r == 0 ? 0f : WallThickness + depth * Mathf.Pow(k, 1.6f);
            float shrink = Mathf.Lerp(1f, 0.12f, Mathf.Pow(k, 0.7f)), smooth = Mathf.Clamp01(k * 3f);
            drift = new Vector2(Mathf.Sin(r * 0.6f + seed), Mathf.Cos(r * 0.45f + seed)) * (0.06f * k);
            var color = r == 0 ? plaster : r == 1 ? plaster * 0.7f
                : Color.Lerp(Color.Lerp(plaster * 0.3f, deep, Mathf.Sqrt(k)), Color.black, k * k);
            var ring = new int[Sides];
            for (int i = 0; i < Sides; i++)
                ring[i] = Add(Vector2.Lerp(edge[i], round[i], smooth) * shrink + drift, z, Rough(color, 0.15f), k);
            if (previous != null)
                for (int i = 0; i < Sides; i++)
                    Quad(hole, previous[i], previous[(i + 1) % Sides], ring[i], ring[(i + 1) % Sides]);
            previous = ring;
        }
        int cap = Add(drift, z, Color.black, 1f);
        for (int i = 0; i < Sides; i++)
            hole.AddRange(new[] { previous[i], previous[(i + 1) % Sides], cap });

        // Plaster chipped off around the break, just in front of the wall.
        int[] inner = new int[Sides], outer = new int[Sides];
        for (int i = 0; i < Sides; i++)
        {
            float width = 0.025f + 0.06f * Rand();
            inner[i] = Add(edge[i], -0.003f, Rough(plaster, 0.12f), 0f);
            outer[i] = Add(edge[i] + edge[i].normalized * width, -0.003f, Rough(plaster * 0.92f, 0.12f), 0f);
        }
        for (int i = 0; i < Sides; i++)
            Quad(hole, inner[i], inner[(i + 1) % Sides], outer[i], outer[(i + 1) % Sides]);

        // Cracks running out over the poster: thin wedges that wander a little.
        var crack = plaster * 0.22f;
        for (int c = 0; c < cracks; c++)
        {
            int at = random.Next(Sides);
            Vector2 direction = edge[at].normalized, p = edge[at] + direction * 0.04f;
            float length = (0.15f + 0.35f * Rand()) * size.y;
            int left = -1, right = -1;
            for (int s = 0; s <= 4; s++)
            {
                var side = new Vector2(-direction.y, direction.x) * (0.009f * (1f - s / 4f));
                int l = Add(p + side, -0.004f, crack, 0f), rt = Add(p - side, -0.004f, crack, 0f);
                if (s > 0)
                    Quad(hole, left, right, l, rt);
                left = l;
                right = rt;
                float turn = (Rand() - 0.5f) * 0.9f;
                direction = new Vector2(direction.x * Mathf.Cos(turn) - direction.y * Mathf.Sin(turn),
                    direction.x * Mathf.Sin(turn) + direction.y * Mathf.Cos(turn));
                p += direction * (length / 4f);
            }
        }

        if (!mesh)
            mesh = new Mesh { name = "Wall hole", hideFlags = HideFlags.HideAndDontSave };
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetUVs(0, depths);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(wall, 0);
        mesh.SetTriangles(hole, 1);
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
