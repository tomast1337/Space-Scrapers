#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloorBuilder))]
public class FloorBuilderEditor : Editor
{

    readonly float SIZE = 128f;

    Texture2D DrawQuadtreeTileMap(QuadtreeNode root, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        void Fill(FloorRect r, Color c)
        {
            for (int y = r.y1; y <= r.y2; y++)
            {
                for (int x = r.x1; x <= r.x2; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }
        }

        void Traverse(QuadtreeNode node)
        {
            if (node.IsLeaf)
            {
                Color c = node.IsLand == true ? Color.green : Color.blue;
                Fill(node.Rect, c);
            }
            else
            {
                foreach (var child in node.Children)
                    Traverse(child);
            }
        }

        Traverse(root);
        tex.Apply();
        return tex;
    }


    Texture2D DrawNoiseMap(float[,] noiseMap, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = noiseMap[x, y];
                Color color = new Color(value, value, value);
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return tex;
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FloorBuilder builder = (FloorBuilder)target;

        if (GUILayout.Button("Generate Preview"))
        {
            builder.GenerateIsland();
            EditorUtility.SetDirty(builder); // Mark as dirty so Unity redraws
        }

        GUILayout.Space(10);

        if (builder.NoiseMap != null)
        {
            var noiseMapTexture = DrawNoiseMap(builder.NoiseMap, builder.width, builder.height);
            GUILayout.Label("Noise Preview");
            GUILayout.Label(noiseMapTexture, GUILayout.Width(SIZE), GUILayout.Height(SIZE));
        }

        if (builder.QuadtreeRoot != null)
        {
            var tileMapTex = DrawQuadtreeTileMap(builder.QuadtreeRoot, builder.width, builder.height);
            GUILayout.Label("Quadtree Tile Map");
            GUILayout.Label(tileMapTex, GUILayout.Width(SIZE), GUILayout.Height(SIZE));
        }
    }
}
#endif