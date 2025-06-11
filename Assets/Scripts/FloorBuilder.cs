using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
}


public class FloorBuilder : MonoBehaviour
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
    private float[,] NoiseMap;

    private Texture2D previewNoise;
    public Texture2D NoiseMapTexture
    {
        get { return previewNoise; }
    }
    private Texture2D previewTile;

    public Texture2D TileMapTexture
    {
        get { return previewTile; }
    }

#if UNITY_EDITOR
    public void EditorGenerateIslandPreview()
    {
        GenerateIsland();

        previewNoise = new Texture2D(width, height);
        previewTile = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = NoiseMap[x, y];
                Color color = new Color(value, value, value);
                previewNoise.SetPixel(x, y, color);

                previewTile.SetPixel(x, y, landMap[x, y] ? Color.green : Color.blue);
            }
        }

        previewNoise.Apply();
        previewTile.Apply();
    }
#endif

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


    void GenerateIsland()
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

        NoiseMap = noiseMap;
    }

    void Start()
    {
        GenerateIsland();
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (drawPreview && previewNoise != null)
        {
            Gizmos.DrawGUITexture(new Rect(10, 10, width * 0.1f, height * 0.1f), previewNoise);
            Gizmos.DrawGUITexture(new Rect(10 + width * 0.1f + 10, 10, width * 0.1f, height * 0.1f), previewTile);
        }
    }
    void OnValidate()
    {
        if (width <= 0) width = 1;
        if (height <= 0) height = 1;
        if (noiseScale <= 0) noiseScale = 1f;
        if (islandFalloff < 0 || islandFalloff > 1) islandFalloff = Mathf.Clamp01(islandFalloff);
        if (octaves < 1) octaves = 1;

        GenerateIsland();
    }
#endif


}