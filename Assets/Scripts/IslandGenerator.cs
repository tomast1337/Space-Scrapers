using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class IslandGenerator : MonoBehaviour
{
    [Header("Island Settings")]
    public int width = 100;
    public int height = 100;
    public float tileSize = 1f;
    public float noiseScale = 20f;
    [Range(0, 1)] public float threshold = 0.4f;
    public float islandFalloff = 0.45f;

    [Header("Randomization")]
    public int seed = 0;
    public bool randomSeed = false;

    [Header("Noise Settings")]
    [Range(1, 10)]
    public int octaves = 4;
    [Range(0.01f, 1f)]
    public float persistence = 0.5f;
    [Range(0.1f, 8f)]
    public float lacunarity = 2f;

    [Header("Tiles")]
    public GameObject landTile;
    public List<GameObject> edgeTiles = new List<GameObject>();

    [Header("Editor Preview")]
    public bool drawPreview = true;
    public bool autoUpdate = true;

    private bool[,] landMap;
    private Texture2D previewTexture;
    public Texture2D PreviewTexture
    {
        get { return previewTexture; }
        set { previewTexture = value; }
    }
    private float[,] lastNoiseMap;

    private void Awake()
    {
        if (randomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        Random.InitState(seed);
    }

    void Start()
    {
        ClearPrevious();
        GenerateIsland();
    }

    private void ClearPrevious()
    {
        // Remove todos os filhos do GameObject atual
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // (Opcional) Se você instanciou meshes diretas no mesmo GameObject:
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null) meshFilter.sharedMesh = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawPreview || !Application.isEditor) return;

        if (autoUpdate || previewTexture == null || lastNoiseMap == null)
        {
            GeneratePreview();
        }

        if (previewTexture != null)
        {
            Gizmos.color = new Color(1, 1, 1, 1);
            Vector3 center = new Vector3(-width * tileSize * 0.5f, 0, -height * tileSize * 0.5f);
            Vector3 size = new Vector3(width * tileSize, 0, height * tileSize);
            Gizmos.DrawGUITexture(new Rect(center.x, center.z, size.x, size.z), previewTexture);
        }
    }

    private void OnDrawGizmosSelected() // Changed from OnDrawGizmos to Selected
    {
        if (!drawPreview || !Application.isEditor) return;

        // Force regeneration if in editor mode
        if (!EditorApplication.isPlaying)
        {
            GeneratePreview();
        }

        if (previewTexture != null)
        {
            // Calculate proper rect dimensions
            float halfWidth = width * tileSize * 0.5f;
            float halfHeight = height * tileSize * 0.5f;

            // Create a proper rectangle in world space
            Rect rect = new Rect(
                transform.position.x - halfWidth,
                transform.position.z - halfHeight,
                width * tileSize,
                height * tileSize);

            // Draw the texture
            Gizmos.color = new Color(1, 1, 1, 1);
            Gizmos.DrawGUITexture(rect, previewTexture);
        }
    }

    private void OnValidate()
    {
        if (!EditorApplication.isPlaying && autoUpdate)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) // Check if object still exists
                {
                    GeneratePreview();
                }
            };
        }
    }
#endif

    private void GeneratePreview()
    {
        lastNoiseMap = GenerateNoiseMap();

        // Create new texture if needed
        if (previewTexture == null || previewTexture.width != width || previewTexture.height != height)
        {
            previewTexture = new Texture2D(width, height);
            previewTexture.anisoLevel = 0; // Disable anisotropic filtering
            previewTexture.filterMode = FilterMode.Point; // Set filter mode
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = lastNoiseMap[x, y];
                Color color = value > threshold ?
                    new Color(0.2f, 0.8f, 0.2f, 1) : // Added alpha
                    new Color(0.1f, 0.3f, 0.8f, 1);
                previewTexture.SetPixel(x, height - 1 - y, color); // Flipped Y coordinate
            }
        }
        previewTexture.Apply();

#if UNITY_EDITOR
        // Force scene repaint
        UnityEditor.SceneView.RepaintAll();
#endif
    }

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

                // Apply circular mask
                Vector2 center = new Vector2(width / 2f, height / 2f);
                float distance = Vector2.Distance(center, new Vector2(x, y));
                float maxDistance = Mathf.Min(width, height) * islandFalloff;
                float maskValue = Mathf.Clamp01(1 - (distance / maxDistance));

                noiseMap[x, y] = Mathf.Clamp01(noiseHeight) * maskValue;
            }
        }
        return noiseMap;

    }

    void GenerateIsland()
    {
        float[,] noiseMap = GenerateNoiseMap();
        landMap = new bool[width, height];

        // Calculate offset to center the island
        float xOffset = -width * tileSize * 0.5f;
        float zOffset = -height * tileSize * 0.5f;

        // First pass: create land tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = new Vector3(
                    x * tileSize + xOffset,
                    0,
                    y * tileSize + zOffset);

                if (noiseMap[x, y] > threshold)
                {
                    Instantiate(landTile, position, Quaternion.identity, transform);
                    landMap[x, y] = true;
                }
            }
        }

        // Second pass: add edge tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (landMap[x, y])
                {
                    Vector3 position = new Vector3(
                        x * tileSize + xOffset,
                        -5f,
                        y * tileSize + zOffset);

                    if (IsEdgeTile(x, y))
                    {
                        // select a random edge tile from the list
                        GameObject edgeTile = edgeTiles[Random.Range(0, edgeTiles.Count)];
                        // random rotation for edge tiles
                        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        // randomly scale multiplier for edge tiles
                        float scaleMultiplier = Random.Range(1.5f, 2f);
                        // apply scale to the edge tile
                        edgeTile.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
                        Instantiate(edgeTile, position, randomRotation, transform);
                    }
                }
            }
        }
    }

    bool IsEdgeTile(int x, int y)
    {
        // Check adjacent tiles (4-directional)
        if (x > 0 && !landMap[x - 1, y]) return true;
        if (x < width - 1 && !landMap[x + 1, y]) return true;
        if (y > 0 && !landMap[x, y - 1]) return true;
        if (y < height - 1 && !landMap[x, y + 1]) return true;

        return false;
    }
}
