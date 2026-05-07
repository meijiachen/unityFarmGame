using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomTilemapGenerator))]
public class RandomTilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Put tiles with the same visual style into the same Tile Style Group. " +
            "The generator uses noise to pick a style per area, then randomly picks a tile inside that style.",
            MessageType.Info);

        DrawDefaultInspector();

        RandomTilemapGenerator generator = (RandomTilemapGenerator)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Random Map"))
        {
            generator.GenerateRandomMap();
        }

        if (GUILayout.Button("Clear Generated Area"))
        {
            generator.ClearGeneratedArea();
        }
    }
}
