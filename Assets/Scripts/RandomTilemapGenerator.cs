using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class RandomTilemapGenerator : MonoBehaviour
{
    public enum GenerationMode
    {
        TerrainNoise,
        StyleGroups
    }

    public enum LayerPaintTarget
    {
        GrassAndDirt,
        GrassOnly,
        DirtOnly
    }

    [System.Serializable]
    public class TileStyleGroup
    {
        public string name = "Ground";
        [Min(0f)] public float weight = 1f;
        public TileBase[] tiles = new TileBase[0];
    }

    [System.Serializable]
    public class TerrainTilemapLayer
    {
        public bool enabled = true;
        public string name = "Terrain Layer";
        public Tilemap targetTilemap;
        public string fallbackTilemapName;
        public LayerPaintTarget paintOn = LayerPaintTarget.GrassAndDirt;
        [Range(0f, 1f)] public float paintChance = 1f;
        public TileBase grassTile;
        public TileBase dirtTile;
        [Tooltip("Optional extra Dirt RuleTiles for this Tilemap. One Dirt RuleTile is chosen for the whole layer per generation.")]
        public TileBase[] additionalDirtTiles = new TileBase[0];
    }

    private class ResolvedTileStyleGroup
    {
        public float Weight { get; private set; }
        public List<TileBase> Tiles { get; private set; }

        public ResolvedTileStyleGroup(float weight, List<TileBase> tiles)
        {
            Weight = weight;
            Tiles = tiles;
        }
    }

    [Header("Target")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private string fallbackTilemapName = "Ground1";

    [Header("Generation Mode")]
    [SerializeField] private GenerationMode generationMode = GenerationMode.TerrainNoise;

    [Header("Terrain Noise")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase dirtTile;
    [Tooltip("Optional extra Dirt RuleTiles. The generator chooses one Dirt RuleTile for the whole generated map so RuleTile neighbor checks remain correct.")]
    [SerializeField] private TileBase[] additionalDirtTiles = new TileBase[0];
    [Tooltip("Approximate amount of dirt before smoothing is applied.")]
    [SerializeField, Range(0f, 1f)] private float dirtAmount = 0.35f;
    [Tooltip("Lower values create larger, smoother dirt patches. Start around 0.015 to 0.03.")]
    [SerializeField, Min(0.001f)] private float terrainNoiseScale = 0.025f;
    [SerializeField] private Vector2 terrainNoiseOffset;
    [Tooltip("How many times to smooth the generated grass/dirt mask. Higher values remove noisy fragments.")]
    [SerializeField, Range(0, 8)] private int terrainSmoothingIterations = 3;
    [Tooltip("A dirt cell needs at least this many dirt neighbors to stay dirt during smoothing.")]
    [SerializeField, Range(0, 8)] private int dirtSurvivalNeighbors = 3;
    [Tooltip("A grass cell becomes dirt if it has at least this many dirt neighbors during smoothing.")]
    [SerializeField, Range(0, 8)] private int dirtBirthNeighbors = 5;

    [Header("Terrain Layers")]
    [Tooltip("Optional per-Tilemap settings. Use this when Ground1, Ground2, Ground3, decoration, or front layers need different RuleTiles.")]
    [SerializeField] private TerrainTilemapLayer[] terrainLayers = new TerrainTilemapLayer[0];

    [Header("Tile Palette Styles")]
    [SerializeField] private TileStyleGroup[] tileStyleGroups = new TileStyleGroup[]
    {
        new TileStyleGroup { name = "Grass", weight = 1f },
        new TileStyleGroup { name = "Dirt", weight = 1f }
    };

    [Header("Generate Area")]
    [SerializeField] private Vector2Int minCell = new Vector2Int(-40, -40);
    [SerializeField] private Vector2Int maxCell = new Vector2Int(40, 40);

    [Header("Style Layout")]
    [SerializeField, Min(0.001f)] private float styleNoiseScale = 0.05f;
    [SerializeField] private Vector2 styleNoiseOffset;

    [Header("Random")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed;
    [SerializeField] private bool clearBeforeGenerate = true;

    [ContextMenu("Generate Random Map")]
    public void GenerateRandomMap()
    {
        Tilemap tilemap = ResolveTargetTilemap();
        if (tilemap == null && !(generationMode == GenerationMode.TerrainNoise && HasUsableTerrainLayer()))
        {
            Debug.LogError("RandomTilemapGenerator could not find a target Tilemap. Assign Ground1, attach this component to Ground1, or configure Terrain Layers.", this);
            return;
        }

        int fromX = Mathf.Min(minCell.x, maxCell.x);
        int toX = Mathf.Max(minCell.x, maxCell.x);
        int fromY = Mathf.Min(minCell.y, maxCell.y);
        int toY = Mathf.Max(minCell.y, maxCell.y);

        if (!CanGenerate())
        {
            return;
        }

        bool useTerrainLayers = generationMode == GenerationMode.TerrainNoise && HasUsableTerrainLayer();
        if (clearBeforeGenerate)
        {
            if (tilemap != null && !useTerrainLayers)
            {
                RecordTilemapUndo(tilemap, "Generate Random Map");
                ClearArea(tilemap, fromX, toX, fromY, toY);
            }
        }
        else
        {
            if (tilemap != null && !useTerrainLayers)
            {
                RecordTilemapUndo(tilemap, "Generate Random Map");
            }
        }

        System.Random random = useRandomSeed ? new System.Random() : new System.Random(seed);
        int placedTileCount;

        switch (generationMode)
        {
            case GenerationMode.TerrainNoise:
                placedTileCount = GenerateTerrainNoise(tilemap, random, fromX, toX, fromY, toY);
                break;
            case GenerationMode.StyleGroups:
                placedTileCount = GenerateStyleGroups(tilemap, random, fromX, toX, fromY, toY);
                break;
            default:
                Debug.LogError("Unsupported generation mode: " + generationMode, this);
                return;
        }

        if (tilemap != null && !useTerrainLayers)
        {
            tilemap.CompressBounds();
            tilemap.RefreshAllTiles();
            MarkTilemapDirty(tilemap);
        }

        string targetName = useTerrainLayers || tilemap == null ? "configured terrain layers" : tilemap.name;
        Debug.Log(string.Format(
            "Generated {0} random tiles on {1}, area x={2}..{3}, y={4}..{5}.",
            placedTileCount,
            targetName,
            fromX,
            toX,
            fromY,
            toY), this);
    }

    private bool CanGenerate()
    {
        switch (generationMode)
        {
            case GenerationMode.TerrainNoise:
                if (HasUsableTerrainLayer())
                {
                    return true;
                }

                if (grassTile == null || GetValidDirtTiles().Count == 0)
                {
                    Debug.LogError("RandomTilemapGenerator Terrain Noise mode needs Grass Tile and at least one Dirt Tile. Dirt Tiles can be RuleTiles.", this);
                    return false;
                }

                return true;
            case GenerationMode.StyleGroups:
                if (GetValidStyleGroups().Count == 0)
                {
                    Debug.LogError("RandomTilemapGenerator Style Groups mode needs at least one style group with at least one Tile asset.", this);
                    return false;
                }

                return true;
            default:
                Debug.LogError("Unsupported generation mode: " + generationMode, this);
                return false;
        }
    }

    private int GenerateTerrainNoise(Tilemap tilemap, System.Random random, int fromX, int toX, int fromY, int toY)
    {
        Vector2 noiseOffset = GetNoiseOffset(random, terrainNoiseOffset);
        float dirtThreshold = 1f - dirtAmount;
        bool[,] dirtMask = CreateTerrainMask(fromX, toX, fromY, toY, noiseOffset, dirtThreshold);
        SmoothTerrainMask(dirtMask);

        List<TerrainTilemapLayer> usableLayers = GetUsableTerrainLayers();
        if (usableLayers.Count > 0)
        {
            int layeredTileCount = 0;
            for (int i = 0; i < usableLayers.Count; i++)
            {
                layeredTileCount += PaintTerrainLayer(usableLayers[i], random, dirtMask, fromX, fromY);
            }

            return layeredTileCount;
        }

        TileBase selectedDirtTile = ChooseDirtTile(random);
        if (selectedDirtTile == null)
        {
            Debug.LogError("RandomTilemapGenerator could not choose a Dirt Tile.", this);
            return 0;
        }

        int placedTileCount = 0;
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);

        for (int localY = 0; localY < height; localY++)
        {
            for (int localX = 0; localX < width; localX++)
            {
                int x = fromX + localX;
                int y = fromY + localY;
                TileBase tile = dirtMask[localX, localY] ? selectedDirtTile : grassTile;
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                placedTileCount++;
            }
        }

        return placedTileCount;
    }

    private int PaintTerrainLayer(TerrainTilemapLayer layer, System.Random random, bool[,] dirtMask, int fromX, int fromY)
    {
        Tilemap layerTilemap = ResolveLayerTilemap(layer);
        if (layerTilemap == null)
        {
            return 0;
        }

        TileBase selectedDirtTile = ChooseDirtTile(layer, random);
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);
        int toX = fromX + width - 1;
        int toY = fromY + height - 1;
        int placedTileCount = 0;

        if (clearBeforeGenerate)
        {
            RecordTilemapUndo(layerTilemap, "Generate Random Map Layer");
            ClearArea(layerTilemap, fromX, toX, fromY, toY);
        }
        else
        {
            RecordTilemapUndo(layerTilemap, "Generate Random Map Layer");
        }

        for (int localY = 0; localY < height; localY++)
        {
            for (int localX = 0; localX < width; localX++)
            {
                bool isDirt = dirtMask[localX, localY];
                if (!ShouldPaintLayerCell(layer, random, isDirt))
                {
                    continue;
                }

                TileBase tile = isDirt ? selectedDirtTile : layer.grassTile;
                if (tile == null)
                {
                    continue;
                }

                layerTilemap.SetTile(new Vector3Int(fromX + localX, fromY + localY, 0), tile);
                placedTileCount++;
            }
        }

        layerTilemap.CompressBounds();
        layerTilemap.RefreshAllTiles();
        MarkTilemapDirty(layerTilemap);

        return placedTileCount;
    }

    private static bool ShouldPaintLayerCell(TerrainTilemapLayer layer, System.Random random, bool isDirt)
    {
        switch (layer.paintOn)
        {
            case LayerPaintTarget.GrassOnly:
                if (isDirt)
                {
                    return false;
                }
                break;
            case LayerPaintTarget.DirtOnly:
                if (!isDirt)
                {
                    return false;
                }
                break;
        }

        if (layer.paintChance >= 1f)
        {
            return true;
        }

        if (layer.paintChance <= 0f)
        {
            return false;
        }

        return random.NextDouble() <= layer.paintChance;
    }

    private TileBase ChooseDirtTile(System.Random random)
    {
        List<TileBase> validDirtTiles = GetValidDirtTiles();
        if (validDirtTiles.Count == 0)
        {
            return null;
        }

        return validDirtTiles[random.Next(validDirtTiles.Count)];
    }

    private TileBase ChooseDirtTile(TerrainTilemapLayer layer, System.Random random)
    {
        List<TileBase> validDirtTiles = GetValidDirtTiles(layer);
        if (validDirtTiles.Count == 0)
        {
            return null;
        }

        return validDirtTiles[random.Next(validDirtTiles.Count)];
    }

    private List<TileBase> GetValidDirtTiles()
    {
        List<TileBase> validDirtTiles = new List<TileBase>();
        if (dirtTile != null)
        {
            validDirtTiles.Add(dirtTile);
        }

        if (additionalDirtTiles == null)
        {
            return validDirtTiles;
        }

        for (int i = 0; i < additionalDirtTiles.Length; i++)
        {
            if (additionalDirtTiles[i] != null)
            {
                validDirtTiles.Add(additionalDirtTiles[i]);
            }
        }

        return validDirtTiles;
    }

    private static List<TileBase> GetValidDirtTiles(TerrainTilemapLayer layer)
    {
        List<TileBase> validDirtTiles = new List<TileBase>();
        if (layer == null)
        {
            return validDirtTiles;
        }

        if (layer.dirtTile != null)
        {
            validDirtTiles.Add(layer.dirtTile);
        }

        if (layer.additionalDirtTiles == null)
        {
            return validDirtTiles;
        }

        for (int i = 0; i < layer.additionalDirtTiles.Length; i++)
        {
            if (layer.additionalDirtTiles[i] != null)
            {
                validDirtTiles.Add(layer.additionalDirtTiles[i]);
            }
        }

        return validDirtTiles;
    }

    private bool[,] CreateTerrainMask(int fromX, int toX, int fromY, int toY, Vector2 noiseOffset, float dirtThreshold)
    {
        int width = toX - fromX + 1;
        int height = toY - fromY + 1;
        bool[,] dirtMask = new bool[width, height];

        for (int localY = 0; localY < height; localY++)
        {
            for (int localX = 0; localX < width; localX++)
            {
                int x = fromX + localX;
                int y = fromY + localY;
                float noise = Mathf.PerlinNoise(
                    x * terrainNoiseScale + noiseOffset.x,
                    y * terrainNoiseScale + noiseOffset.y);
                dirtMask[localX, localY] = noise >= dirtThreshold;
            }
        }

        return dirtMask;
    }

    private void SmoothTerrainMask(bool[,] dirtMask)
    {
        for (int i = 0; i < terrainSmoothingIterations; i++)
        {
            dirtMask = SmoothTerrainMaskOnce(dirtMask);
        }
    }

    private bool[,] SmoothTerrainMaskOnce(bool[,] dirtMask)
    {
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);
        bool[,] smoothedMask = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int dirtNeighbors = CountDirtNeighbors(dirtMask, x, y);
                if (dirtMask[x, y])
                {
                    smoothedMask[x, y] = dirtNeighbors >= dirtSurvivalNeighbors;
                }
                else
                {
                    smoothedMask[x, y] = dirtNeighbors >= dirtBirthNeighbors;
                }
            }
        }

        CopyMask(smoothedMask, dirtMask);
        return dirtMask;
    }

    private static int CountDirtNeighbors(bool[,] dirtMask, int x, int y)
    {
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);
        int dirtNeighborCount = 0;

        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                int neighborX = x + offsetX;
                int neighborY = y + offsetY;
                if (neighborX < 0 || neighborY < 0 || neighborX >= width || neighborY >= height)
                {
                    continue;
                }

                if (dirtMask[neighborX, neighborY])
                {
                    dirtNeighborCount++;
                }
            }
        }

        return dirtNeighborCount;
    }

    private static void CopyMask(bool[,] source, bool[,] destination)
    {
        int width = source.GetLength(0);
        int height = source.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                destination[x, y] = source[x, y];
            }
        }
    }

    private int GenerateStyleGroups(Tilemap tilemap, System.Random random, int fromX, int toX, int fromY, int toY)
    {
        List<ResolvedTileStyleGroup> validStyleGroups = GetValidStyleGroups();
        Vector2 noiseOffset = GetNoiseOffset(random, styleNoiseOffset);
        int placedTileCount = 0;

        for (int y = fromY; y <= toY; y++)
        {
            for (int x = fromX; x <= toX; x++)
            {
                ResolvedTileStyleGroup styleGroup = ChooseStyleGroup(validStyleGroups, x, y, noiseOffset);
                TileBase tile = styleGroup.Tiles[random.Next(styleGroup.Tiles.Count)];
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                placedTileCount++;
            }
        }

        return placedTileCount;
    }

    private Vector2 GetNoiseOffset(System.Random random, Vector2 configuredOffset)
    {
        if (useRandomSeed)
        {
            return configuredOffset + new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
        }

        return configuredOffset + new Vector2(seed * 17.13f, seed * 31.71f);
    }

    [ContextMenu("Clear Generated Area")]
    public void ClearGeneratedArea()
    {
        Tilemap tilemap = ResolveTargetTilemap();
        if (tilemap == null && !(generationMode == GenerationMode.TerrainNoise && HasUsableTerrainLayer()))
        {
            Debug.LogError("RandomTilemapGenerator could not find a target Tilemap to clear.", this);
            return;
        }

        int fromX = Mathf.Min(minCell.x, maxCell.x);
        int toX = Mathf.Max(minCell.x, maxCell.x);
        int fromY = Mathf.Min(minCell.y, maxCell.y);
        int toY = Mathf.Max(minCell.y, maxCell.y);

        List<TerrainTilemapLayer> usableLayers = generationMode == GenerationMode.TerrainNoise
            ? GetUsableTerrainLayers()
            : new List<TerrainTilemapLayer>();
        if (usableLayers.Count > 0)
        {
            for (int i = 0; i < usableLayers.Count; i++)
            {
                Tilemap layerTilemap = ResolveLayerTilemap(usableLayers[i]);
                if (layerTilemap == null)
                {
                    continue;
                }

                RecordTilemapUndo(layerTilemap, "Clear Random Map Area");
                ClearArea(layerTilemap, fromX, toX, fromY, toY);
                layerTilemap.CompressBounds();
                layerTilemap.RefreshAllTiles();
                MarkTilemapDirty(layerTilemap);
            }

            return;
        }

        if (tilemap != null)
        {
            RecordTilemapUndo(tilemap, "Clear Random Map Area");
            ClearArea(tilemap, fromX, toX, fromY, toY);
            tilemap.CompressBounds();
            tilemap.RefreshAllTiles();
            MarkTilemapDirty(tilemap);
        }
    }

    private Tilemap ResolveTargetTilemap()
    {
        if (targetTilemap != null)
        {
            return targetTilemap;
        }

        Tilemap localTilemap = GetComponent<Tilemap>();
        if (localTilemap != null)
        {
            return localTilemap;
        }

        if (!string.IsNullOrEmpty(fallbackTilemapName))
        {
            GameObject targetObject = GameObject.Find(fallbackTilemapName);
            if (targetObject != null)
            {
                return targetObject.GetComponent<Tilemap>();
            }
        }

        return null;
    }

    private bool HasUsableTerrainLayer()
    {
        return GetUsableTerrainLayers().Count > 0;
    }

    private List<TerrainTilemapLayer> GetUsableTerrainLayers()
    {
        List<TerrainTilemapLayer> usableLayers = new List<TerrainTilemapLayer>();
        if (terrainLayers == null)
        {
            return usableLayers;
        }

        for (int i = 0; i < terrainLayers.Length; i++)
        {
            TerrainTilemapLayer layer = terrainLayers[i];
            if (layer == null || !layer.enabled)
            {
                continue;
            }

            if (ResolveLayerTilemap(layer) == null)
            {
                continue;
            }

            if (layer.grassTile == null && GetValidDirtTiles(layer).Count == 0)
            {
                continue;
            }

            usableLayers.Add(layer);
        }

        return usableLayers;
    }

    private static Tilemap ResolveLayerTilemap(TerrainTilemapLayer layer)
    {
        if (layer == null)
        {
            return null;
        }

        if (layer.targetTilemap != null)
        {
            return layer.targetTilemap;
        }

        if (!string.IsNullOrEmpty(layer.fallbackTilemapName))
        {
            GameObject targetObject = GameObject.Find(layer.fallbackTilemapName);
            if (targetObject != null)
            {
                return targetObject.GetComponent<Tilemap>();
            }
        }

        return null;
    }

    private List<ResolvedTileStyleGroup> GetValidStyleGroups()
    {
        List<ResolvedTileStyleGroup> validStyleGroups = new List<ResolvedTileStyleGroup>();
        if (tileStyleGroups == null)
        {
            return validStyleGroups;
        }

        for (int i = 0; i < tileStyleGroups.Length; i++)
        {
            TileStyleGroup styleGroup = tileStyleGroups[i];
            if (styleGroup == null || styleGroup.weight <= 0f || styleGroup.tiles == null)
            {
                continue;
            }

            List<TileBase> validTiles = GetValidTiles(styleGroup.tiles);
            if (validTiles.Count > 0)
            {
                validStyleGroups.Add(new ResolvedTileStyleGroup(styleGroup.weight, validTiles));
            }
        }

        return validStyleGroups;
    }

    private static List<TileBase> GetValidTiles(TileBase[] tiles)
    {
        List<TileBase> validTiles = new List<TileBase>();
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null)
            {
                validTiles.Add(tiles[i]);
            }
        }

        return validTiles;
    }

    private ResolvedTileStyleGroup ChooseStyleGroup(List<ResolvedTileStyleGroup> validStyleGroups, int x, int y, Vector2 noiseOffset)
    {
        if (validStyleGroups.Count == 1)
        {
            return validStyleGroups[0];
        }

        float totalWeight = 0f;
        for (int i = 0; i < validStyleGroups.Count; i++)
        {
            totalWeight += validStyleGroups[i].Weight;
        }

        float noise = Mathf.PerlinNoise(
            x * styleNoiseScale + noiseOffset.x,
            y * styleNoiseScale + noiseOffset.y);
        float value = noise * totalWeight;

        float cumulativeWeight = 0f;
        for (int i = 0; i < validStyleGroups.Count; i++)
        {
            cumulativeWeight += validStyleGroups[i].Weight;
            if (value <= cumulativeWeight)
            {
                return validStyleGroups[i];
            }
        }

        return validStyleGroups[validStyleGroups.Count - 1];
    }

    private static void ClearArea(Tilemap tilemap, int fromX, int toX, int fromY, int toY)
    {
        for (int y = fromY; y <= toY; y++)
        {
            for (int x = fromX; x <= toX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }

    private static void RecordTilemapUndo(Tilemap tilemap, string actionName)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCompleteObjectUndo(tilemap, actionName);
        }
#endif
    }

    private static void MarkTilemapDirty(Tilemap tilemap)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(tilemap);
            EditorSceneManager.MarkSceneDirty(tilemap.gameObject.scene);
        }
#endif
    }

    private void Reset()
    {
        targetTilemap = GetComponent<Tilemap>();
    }
}
