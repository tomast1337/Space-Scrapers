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

    [SerializeField][Range(1, 5)] private float minEdgeScale = 1.5f;
    [SerializeField][Range(1, 5)] private float maxEdgeScale = 2; // Scale multiplier for edge tiles

    GameObject floorTilesParent;
    GameObject edgeTilesParent;


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
        Debug.Log($"[IslandGenerator] Map generated, scattering rocks on a {width}x{height} map.");

        floorTilesParent = new GameObject("FloorTiles");
        floorTilesParent.transform.SetParent(transform);
        edgeTilesParent = new GameObject("EdgeTiles");
        edgeTilesParent.transform.SetParent(transform);
        GenerateIsland();
    }

    private void GenerateIsland()
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
                    rect.x1 * tileSize + xOffset - (tileSize * 0.5f),
                    0,
                    rect.y1 * tileSize + zOffset);
                // Instantiate land tile at the calculated position
                GameObject landTileInstance = Instantiate(landTile, position, Quaternion.identity, transform);

                var xScale = rect.x2 - rect.x1 + 1;
                var yScale = rect.y2 - rect.y1 + 1;


                // Set the scale of the land tile
                landTileInstance.transform.localScale = new Vector3(xScale, 1f, yScale);
                // Set the name of the tile for easier identification
                landTileInstance.name = $"LandTile_{rect.x1}_{rect.y1}_{rect.x2}_{rect.y2}";
                // Set the parent to the floor tiles parent
                landTileInstance.transform.SetParent(floorTilesParent.transform);
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

                    if (mapFloorBuilder.IsEdgeTile(x, y))
                    {
                        // select a random edge tile from the list
                        GameObject edgeTile = edgeTiles[Random.Range(0, edgeTiles.Count)];
                        // random rotation for edge tiles
                        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        // randomly scale multiplier for edge tiles
                        float scaleMultiplierX = Random.Range(minEdgeScale, maxEdgeScale);
                        float scaleMultiplierY = Random.Range(minEdgeScale, maxEdgeScale);
                        float scaleMultiplierZ = Random.Range(minEdgeScale, maxEdgeScale);
                        // apply scale to the edge tile
                        edgeTile.transform.localScale = new Vector3(scaleMultiplierX, scaleMultiplierY, scaleMultiplierZ);
                        var spawnedEdgeTile = Instantiate(edgeTile, position, randomRotation, transform);
                        spawnedEdgeTile.name = $"EdgeTile_{x}_{y}";
                        // Set the parent to the edge tiles parent
                        spawnedEdgeTile.transform.SetParent(edgeTilesParent.transform);
                    }
                }
            }
        }
    }


}
