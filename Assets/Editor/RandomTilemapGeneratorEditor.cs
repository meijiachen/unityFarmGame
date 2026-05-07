using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomTilemapGenerator))]
public class RandomTilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
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
