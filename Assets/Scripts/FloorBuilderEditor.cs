using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloorBuilder))]
public class FloorBuilderEditor : Editor
{

    readonly float SIZE = 128f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FloorBuilder builder = (FloorBuilder)target;

        if (GUILayout.Button("Generate Preview"))
        {
            builder.EditorGenerateIslandPreview();
            EditorUtility.SetDirty(builder); // Mark as dirty so Unity redraws
        }

        GUILayout.Space(10);

        if (builder.NoiseMapTexture != null)
        {
            GUILayout.Label("Noise Preview");
            GUILayout.Label(builder.NoiseMapTexture, GUILayout.Width(SIZE), GUILayout.Height(SIZE));
        }

        if (builder.TileMapTexture != null)
        {
            GUILayout.Label("Tile Preview");
            GUILayout.Label(builder.TileMapTexture, GUILayout.Width(SIZE), GUILayout.Height(SIZE));
        }
    }
}