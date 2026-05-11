using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomTilemapGenerator))]
public class RandomTilemapGeneratorEditor : Editor
{
    private SerializedProperty terrainLayersProperty;
    private SerializedProperty minCellProperty;
    private SerializedProperty maxCellProperty;
    private SerializedProperty dirtAmountProperty;
    private SerializedProperty terrainNoiseScaleProperty;
    private SerializedProperty terrainNoiseOffsetProperty;
    private SerializedProperty terrainSmoothingIterationsProperty;
    private SerializedProperty dirtSurvivalNeighborsProperty;
    private SerializedProperty dirtBirthNeighborsProperty;
    private SerializedProperty forceDirtBorderWidthProperty;
    private SerializedProperty ruleTileNeighborPaddingProperty;
    private SerializedProperty useRandomSeedProperty;
    private SerializedProperty seedProperty;
    private SerializedProperty clearBeforeGenerateProperty;

    private void OnEnable()
    {
        terrainLayersProperty = serializedObject.FindProperty("terrainLayers");
        minCellProperty = serializedObject.FindProperty("minCell");
        maxCellProperty = serializedObject.FindProperty("maxCell");
        dirtAmountProperty = serializedObject.FindProperty("dirtAmount");
        terrainNoiseScaleProperty = serializedObject.FindProperty("terrainNoiseScale");
        terrainNoiseOffsetProperty = serializedObject.FindProperty("terrainNoiseOffset");
        terrainSmoothingIterationsProperty = serializedObject.FindProperty("terrainSmoothingIterations");
        dirtSurvivalNeighborsProperty = serializedObject.FindProperty("dirtSurvivalNeighbors");
        dirtBirthNeighborsProperty = serializedObject.FindProperty("dirtBirthNeighbors");
        forceDirtBorderWidthProperty = serializedObject.FindProperty("forceDirtBorderWidth");
        ruleTileNeighborPaddingProperty = serializedObject.FindProperty("ruleTileNeighborPadding");
        useRandomSeedProperty = serializedObject.FindProperty("useRandomSeed");
        seedProperty = serializedObject.FindProperty("seed");
        clearBeforeGenerateProperty = serializedObject.FindProperty("clearBeforeGenerate");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Configure one Terrain Layer per Tilemap node. All layers share one generated grass/dirt mask, but each layer can use its own RuleTiles and paint chance.",
            MessageType.Info);
        EditorGUILayout.HelpBox(
            "For decoration layers such as flowers, set Paint On to Grass Only and Paint Chance to a low value such as 0.05-0.15.",
            MessageType.None);
        EditorGUILayout.HelpBox(
            "If the playable edge should not show grass, keep Force Dirt Border Width at 1. Rule Tile Neighbor Padding controls the hidden outside neighbor ring for RuleTile matching.",
            MessageType.None);

        serializedObject.Update();

        EditorGUILayout.PropertyField(terrainLayersProperty, true);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(minCellProperty);
        EditorGUILayout.PropertyField(maxCellProperty);
        EditorGUILayout.PropertyField(forceDirtBorderWidthProperty);
        EditorGUILayout.PropertyField(ruleTileNeighborPaddingProperty);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(dirtAmountProperty);
        EditorGUILayout.PropertyField(terrainNoiseScaleProperty);
        EditorGUILayout.PropertyField(terrainNoiseOffsetProperty);
        EditorGUILayout.PropertyField(terrainSmoothingIterationsProperty);
        EditorGUILayout.PropertyField(dirtSurvivalNeighborsProperty);
        EditorGUILayout.PropertyField(dirtBirthNeighborsProperty);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(useRandomSeedProperty);
        if (!useRandomSeedProperty.boolValue)
        {
            EditorGUILayout.PropertyField(seedProperty);
        }
        EditorGUILayout.PropertyField(clearBeforeGenerateProperty);

        serializedObject.ApplyModifiedProperties();

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
