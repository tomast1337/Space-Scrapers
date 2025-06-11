using UnityEngine;

public struct FloorRect
{
    public int x1, y1, x2, y2;

    public FloorRect(int x1, int y1, int x2, int y2)
    {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }

    public int Width => x2 - x1 + 1;
    public int Height => y2 - y1 + 1;


    public override string ToString()
    {
        return $"FloorRect({x1}, {y1}, {x2}, {y2})";
    }
}

public class QuadtreeNode
{
    public FloorRect Rect;
    public bool? IsLand; // true=land, false=water, null=mixed
    public QuadtreeNode[] Children;

    public bool IsLeaf => Children == null;

    public QuadtreeNode(FloorRect rect)
    {
        Rect = rect;
    }
    public override string ToString()
    {
        return $"QuadtreeNode({Rect.x1}, {Rect.y1}, {Rect.x2}, {Rect.y2}) - IsLand: {IsLand}";
    }

    static public string PrintFullTree(QuadtreeNode node, int depth = 0)
    {
        if (node == null) return "";

        string indent = new string(' ', depth * 2);
        string result = $"{indent}{node}\n";

        if (!node.IsLeaf)
        {
            foreach (var child in node.Children)
            {
                result += PrintFullTree(child, depth + 1);
            }
        }

        return result;
    }
}


public class MapFloorBuilder : MonoBehaviour
{

    [Header("Settings")]
    public int width = 100;
    public int height = 100;
    public float noiseScale = 20f;
    [Range(0, 1)] public float threshold = 0.4f;
    public float islandFalloff = 0.45f;

    [Header("Noise Settings")]
    [Range(1, 10)]
    public int octaves = 4;
    [Range(0.01f, 1f)]
    public float persistence = 0.5f;
    [Range(0.1f, 8f)]
    public float lacunarity = 2f;

    [Header("Randomization")]
    public int seed = 0;

    [Header("Editor Preview")]
    public bool drawPreview = true;

    private bool[,] landMap;
    private float[,] noiseMap;
    public float[,] NoiseMap => noiseMap;
    private QuadtreeNode quadtreeRoot;
    public QuadtreeNode QuadtreeRoot => quadtreeRoot;

    float[,] GenerateNoiseMap()
    {
        float[,] noiseMap = new float[width, height];

        System.Random pseudoRandomGenerator = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = pseudoRandomGenerator.Next(-100000, 100000);
            float offsetY = pseudoRandomGenerator.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float minNoise = float.MaxValue;
        float maxNoise = float.MinValue;

        // First pass: generate raw noise and track min/max
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / noiseScale * frequency + octaveOffsets[i].x;
                    float sampleY = y / noiseScale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight < minNoise) minNoise = noiseHeight;
                if (noiseHeight > maxNoise) maxNoise = noiseHeight;

                noiseMap[x, y] = noiseHeight; // store raw
            }
        }

        // Second pass: normalize and apply sine-based falloff
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalized = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y]);

                // 2D sine falloff (bell-shaped)
                float fx = Mathf.Sin(Mathf.PI * x / (width - 1));
                float fy = Mathf.Sin(Mathf.PI * y / (height - 1));
                float falloff = Mathf.Pow(fx * fy, islandFalloff); // use islandFalloff as exponent

                noiseMap[x, y] = normalized * falloff;
            }
        }

        return noiseMap;
    }
    private QuadtreeNode BuildQuadtree(FloorRect rect, int minSize = 2)
    {
        bool first = landMap[rect.x1, rect.y1];
        bool uniform = true;

        for (int y = rect.y1; y <= rect.y2; y++)
        {
            for (int x = rect.x1; x <= rect.x2; x++)
            {
                if (landMap[x, y] != first)
                {
                    uniform = false;
                    break;
                }
            }
            if (!uniform) break;
        }

        var node = new QuadtreeNode(rect);
        if (uniform || rect.Width <= minSize || rect.Height <= minSize)
        {
            node.IsLand = first;
            return node;
        }

        int midX = (rect.x1 + rect.x2) / 2;
        int midY = (rect.y1 + rect.y2) / 2;

        node.Children = new QuadtreeNode[4];
        node.Children[0] = BuildQuadtree(new FloorRect(rect.x1, rect.y1, midX, midY), minSize); // top-left
        node.Children[1] = BuildQuadtree(new FloorRect(midX + 1, rect.y1, rect.x2, midY), minSize); // top-right
        node.Children[2] = BuildQuadtree(new FloorRect(rect.x1, midY + 1, midX, rect.y2), minSize); // bottom-left
        node.Children[3] = BuildQuadtree(new FloorRect(midX + 1, midY + 1, rect.x2, rect.y2), minSize); // bottom-right

        return node;
    }

    public void GenerateIsland()
    {
        float[,] noiseMap = GenerateNoiseMap();
        landMap = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                landMap[x, y] = noiseMap[x, y] > threshold;
            }
        }

        this.noiseMap = noiseMap;
        this.quadtreeRoot = BuildQuadtree(new FloorRect(0, 0, width - 1, height - 1));
    }

    void Start()
    {
        GenerateIsland();
    }
}