using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapFloorBuilder))]
[RequireComponent(typeof(IslandGenerator))]
public class LandFatures : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<GameObject> rockPrefabs = new List<GameObject>();
    [SerializeField][Range(0, 1)] private float rockSpawnChance = 0.1f; // Chance to spawn a rock per valid tile
    private MapFloorBuilder mapFloorBuilder;
    private IslandGenerator islandGenerator;
    private GameObject rocksParent;
    private void Awake()
    {
        mapFloorBuilder = GetComponent<MapFloorBuilder>();
        islandGenerator = GetComponent<IslandGenerator>();
        rocksParent = new GameObject("Rocks");
        rocksParent.transform.SetParent(transform);
    }

    private void Start()
    {
        mapFloorBuilder.OnMapGenerated += HandleMapGenerated;
    }

    private void OnDisable()
    {
        mapFloorBuilder.OnMapGenerated -= HandleMapGenerated;
    }

    private void HandleMapGenerated()
    {
        float width = mapFloorBuilder.Width;
        float height = mapFloorBuilder.Height;
        Debug.Log($"[LandFatures] Map generated, scattering rocks on a {width}x{height} map.");
        ScatterRocks();
    }


    private void ScatterRocks()
    {
        // get the land map from MapFloorBuilder
        var landMap = mapFloorBuilder.LandMap;
        var noiseMap = mapFloorBuilder.NoiseMap;
        float width = mapFloorBuilder.Width;
        float height = mapFloorBuilder.Height;
        // Calculate offset to center the island
        float tileSize = islandGenerator.TileSize;
        float xOffset = -width * tileSize * 0.5f;
        float zOffset = -height * tileSize * 0.5f;

        for (int x = 1; x < width - 1; x++) // avoid edges for slope check
        {
            for (int y = 1; y < height - 1; y++)
            {
                //check if its not an edge tile
                if (mapFloorBuilder.IsEdgeTile(x, y))
                    continue;
                if (landMap[x, y] && noiseMap[x, y] <= 0.5f)
                {
                    float slope = CalculateSlope(noiseMap, x, y);
                    if (slope < 0.1f && UnityEngine.Random.value < rockSpawnChance * 0.5f) // Lower chance for lower noise values
                    {
                        Vector3 position = new Vector3(
                            x * tileSize + xOffset,
                            0, // Assuming flat terrain for simplicity
                            y * tileSize + zOffset);
                        GameObject rockPrefab = rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Count)];
                        Instantiate(rockPrefab, position, Quaternion.identity, rocksParent.transform);
                    }
                }
            }
        }
    }

    private float CalculateSlope(float[,] noiseMap, int x, int y)
    {
        float dx = (noiseMap[x + 1, y] - noiseMap[x - 1, y]) * 0.5f;
        float dy = (noiseMap[x, y + 1] - noiseMap[x, y - 1]) * 0.5f;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}