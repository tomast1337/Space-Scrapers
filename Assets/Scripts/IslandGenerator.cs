using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
[RequireComponent(typeof(MapFloorBuilder))]
public class IslandGenerator : MonoBehaviour
{
    [Header("Tiles")]
    [SerializeField] private GameObject landTile;
    [SerializeField] private List<GameObject> edgeTiles = new List<GameObject>();
    [Header("Settings")]
    [SerializeField] private float tileSize = 10f;
    private MapFloorBuilder mapFloorBuilder;




    private void Awake()
    {
        mapFloorBuilder = GetComponent<MapFloorBuilder>();
        if (mapFloorBuilder == null)
        {
            Debug.LogError("MapFloorBuilder component not found on this GameObject.");
            return;
        }
        // Validate landTile
        if (landTile == null)
        {
            Debug.LogError("Land tile prefab is not assigned.");
        }

        // Validate edgeTiles
        if (edgeTiles.Count == 0)
        {
            Debug.LogError("Edge tiles list is empty. Please assign at least one edge tile prefab.");
        }

        mapFloorBuilder.GenerateMap();
        GenerateIsland();
    }






    void GenerateIsland()
    {
        var height = mapFloorBuilder.Width;
        var width = mapFloorBuilder.Width;

        var quadtreeRoot = mapFloorBuilder.QuadtreeRoot;

        // Calculate offset to center the island
        float xOffset = -width * tileSize * 0.5f;
        float zOffset = -height * tileSize * 0.5f;

        // First pass: add land tiles , leaves of the quadtree
        void Traverse(QuadtreeNode node)
        {
            if (node.IsLeaf)
            {
                if (node.IsLand.GetValueOrDefault(false) == false)
                    return;

                var rect = node.Rect;
                Vector3 position = new Vector3(
                    rect.x1 * tileSize + xOffset,
                    -5f,
                    rect.y1 * tileSize + zOffset);
                // Instantiate land tile at the calculated position
                GameObject landTileInstance = Instantiate(landTile, position, Quaternion.identity, transform);

                var xScale = rect.x2 - rect.x1 + 1;
                var yScale = rect.y2 - rect.y1 + 1;


                // Set the scale of the land tile
                landTileInstance.transform.localScale = new Vector3(xScale, 1f, yScale);
                // Set the name of the tile for easier identification
                landTileInstance.name = $"LandTile_{rect.x1}_{rect.y1}_{rect.x2}_{rect.y2}";
            }
            else
            {
                foreach (var child in node.Children)
                {
                    Traverse(child);
                }
            }
        }
        Traverse(quadtreeRoot);

        // Second pass: add edge tiles
        var landMap = mapFloorBuilder.LandMap;

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
                        var foo = Instantiate(edgeTile, position, randomRotation, transform);
                        foo.name = $"EdgeTile_{x}_{y}";
                    }
                }
            }
        }
    }

    bool IsEdgeTile(int x, int y)
    {
        var landMap = mapFloorBuilder.LandMap;
        var width = mapFloorBuilder.Width;
        var height = mapFloorBuilder.Height;
        // Check adjacent tiles (4-directional)
        if (x > 0 && !landMap[x - 1, y]) return true;
        if (x < width - 1 && !landMap[x + 1, y]) return true;
        if (y > 0 && !landMap[x, y - 1]) return true;
        if (y < height - 1 && !landMap[x, y + 1]) return true;

        return false;
    }
}
