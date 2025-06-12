using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [Header("Player Spawn Settings")]
    [SerializeField] private GameObject playerPrefab;
    private List<Transform> spawnPoints = new List<Transform>();

    private IslandGenerator floorPlanBuilder;
    private MapFloorBuilder mapFloorBuilder;

    private void Awake()
    {
        floorPlanBuilder = FindFirstObjectByType<IslandGenerator>();
        mapFloorBuilder = FindFirstObjectByType<MapFloorBuilder>();
    }

    private void Start()
    {
        floorPlanBuilder.OnIslandGenerated += HandleMapGenerated;
    }

    private void OnDisable()
    {
        floorPlanBuilder.OnIslandGenerated -= HandleMapGenerated;
    }


    private void HandleMapGenerated()
    {
        spawnPoints.Clear();
        var center = FindCenters();
        if (center.Value > 0)
        {
            // Spawn player at the center of the largest land tile
            Vector3 spawnPosition = center.Key + Vector3.up * 0.5f; // Slightly above ground
            Debug.Log($"[PlayerSpawn] Spawning player at {spawnPosition}");
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.name = "Player";
        }
        else
        {
            Debug.LogWarning("No valid land tile found for player spawn.");
        }
    }

    public KeyValuePair<Vector3, float> FindCenters()
    {
        // rank all tiles in the quadtree by size where IsLand is true
        var centers = new List<KeyValuePair<Vector3, float>>();
        void Traverse(QuadtreeNode node)
        {
            if (node.IsLeaf)
            {
                if (node.IsLand.GetValueOrDefault(false))
                {
                    // Calculate center of the tile
                    float centerX = (node.Rect.x1 + node.Rect.x2) / 2f;
                    float centerY = (node.Rect.y1 + node.Rect.y2) / 2f;
                    centers.Add(new KeyValuePair<Vector3, float>(
                        new Vector3(centerX, 0, centerY),
                        (node.Rect.x2 - node.Rect.x1 + 1) * (node.Rect.y2 - node.Rect.y1 + 1) // size
                    ));
                }
            }
            else
            {
                foreach (var child in node.Children)
                {
                    Traverse(child);
                }
            }
        }

        Traverse(mapFloorBuilder.QuadtreeRoot);

        // Sort centers by size (descending)
        centers.Sort((a, b) => b.Value.CompareTo(a.Value));

        return centers.Count > 0 ? centers[0] : new KeyValuePair<Vector3, float>(Vector3.zero, 0f);
    }
}
