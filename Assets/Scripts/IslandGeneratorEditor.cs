using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IslandGenerator))]
public class IslandGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var gen = (IslandGenerator)target;
        var rect = GUILayoutUtility.GetRect(200, 200);
        if (gen.PreviewTexture != null)
            EditorGUI.DrawPreviewTexture(rect, gen.PreviewTexture);
    }
}
