using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapFloorBuilder))]
public class LandFatures : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<GameObject> rockPrefabs = new List<GameObject>();
    [SerializeField][Range(0, 1)] private float rockSpawnChance = 0.1f; // Chance to spawn a rock per valid tile
    [SerializeField][Range(0, 1)] private float slopeThreshold = 0.5f; // Slope sensitivity for rock placement
    [SerializeField][Range(0, 1)] private float rockHeightThreshold = 0.6f; // Only spawn rocks above this normalized height


    private MapFloorBuilder mapFloorBuilder;
    private void Awake()
    {
        mapFloorBuilder = GetComponent<MapFloorBuilder>();

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

        for (int x = 1; x < width - 1; x++) // avoid edges for slope check
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (!landMap[x, y]) continue;

                float heightValue = noiseMap[x, y];
                if (heightValue < rockHeightThreshold) continue;

                float slope = CalculateSlope(noiseMap, x, y);
                if (slope < slopeThreshold) continue;

                if (UnityEngine.Random.value < rockSpawnChance)
                {
                    Vector3 worldPos = new Vector3(x, 0, y); // Adjust Y if elevation is visualized
                    var rockPrefab = rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Count)];
                    Instantiate(rockPrefab, worldPos, Quaternion.identity, transform);
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