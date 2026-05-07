using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomTilemapGenerator))]
public class RandomTilemapGeneratorEditor : Editor
{
    private SerializedProperty targetTilemapProperty;
    private SerializedProperty fallbackTilemapNameProperty;
    private SerializedProperty generationModeProperty;
    private SerializedProperty grassTileProperty;
    private SerializedProperty dirtTileProperty;
    private SerializedProperty dirtAmountProperty;
    private SerializedProperty terrainNoiseScaleProperty;
    private SerializedProperty terrainNoiseOffsetProperty;
    private SerializedProperty tileStyleGroupsProperty;
    private SerializedProperty minCellProperty;
    private SerializedProperty maxCellProperty;
    private SerializedProperty styleNoiseScaleProperty;
    private SerializedProperty styleNoiseOffsetProperty;
    private SerializedProperty useRandomSeedProperty;
    private SerializedProperty seedProperty;
    private SerializedProperty clearBeforeGenerateProperty;

    private void OnEnable()
    {
        targetTilemapProperty = serializedObject.FindProperty("targetTilemap");
        fallbackTilemapNameProperty = serializedObject.FindProperty("fallbackTilemapName");
        generationModeProperty = serializedObject.FindProperty("generationMode");
        grassTileProperty = serializedObject.FindProperty("grassTile");
        dirtTileProperty = serializedObject.FindProperty("dirtTile");
        dirtAmountProperty = serializedObject.FindProperty("dirtAmount");
        terrainNoiseScaleProperty = serializedObject.FindProperty("terrainNoiseScale");
        terrainNoiseOffsetProperty = serializedObject.FindProperty("terrainNoiseOffset");
        tileStyleGroupsProperty = serializedObject.FindProperty("tileStyleGroups");
        minCellProperty = serializedObject.FindProperty("minCell");
        maxCellProperty = serializedObject.FindProperty("maxCell");
        styleNoiseScaleProperty = serializedObject.FindProperty("styleNoiseScale");
        styleNoiseOffsetProperty = serializedObject.FindProperty("styleNoiseOffset");
        useRandomSeedProperty = serializedObject.FindProperty("useRandomSeed");
        seedProperty = serializedObject.FindProperty("seed");
        clearBeforeGenerateProperty = serializedObject.FindProperty("clearBeforeGenerate");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "For grass/dirt maps, use Terrain Noise mode and assign Grass Tile and Dirt Tile. " +
            "Those fields can use RuleTiles, so dirt edges and corners are chosen automatically from neighbor cells. " +
            "Style Groups is only for loose variant pools.",
            MessageType.Info);

        serializedObject.Update();

        EditorGUILayout.PropertyField(targetTilemapProperty);
        EditorGUILayout.PropertyField(fallbackTilemapNameProperty);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(generationModeProperty);

        RandomTilemapGenerator.GenerationMode generationMode =
            (RandomTilemapGenerator.GenerationMode)generationModeProperty.enumValueIndex;

        if (generationMode == RandomTilemapGenerator.GenerationMode.TerrainNoise)
        {
            EditorGUILayout.PropertyField(grassTileProperty);
            EditorGUILayout.PropertyField(dirtTileProperty);
            EditorGUILayout.PropertyField(dirtAmountProperty);
            EditorGUILayout.PropertyField(terrainNoiseScaleProperty);
            EditorGUILayout.PropertyField(terrainNoiseOffsetProperty);
        }
        else
        {
            EditorGUILayout.PropertyField(tileStyleGroupsProperty, true);
            EditorGUILayout.PropertyField(styleNoiseScaleProperty);
            EditorGUILayout.PropertyField(styleNoiseOffsetProperty);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(minCellProperty);
        EditorGUILayout.PropertyField(maxCellProperty);

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
