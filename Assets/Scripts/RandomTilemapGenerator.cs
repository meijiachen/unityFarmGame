using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class RandomTilemapGenerator : MonoBehaviour
{
    public enum LayerPaintTarget
    {
        GrassAndDirt,
        GrassOnly,
        DirtOnly
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

    [Header("Terrain Layers")]
    [Tooltip("Each layer writes to one Tilemap. All layers share one generated grass/dirt mask so their boundaries line up.")]
    [SerializeField] private TerrainTilemapLayer[] terrainLayers = new TerrainTilemapLayer[0];

    [Header("Generate Area")]
    [SerializeField] private Vector2Int minCell = new Vector2Int(-40, -40);
    [SerializeField] private Vector2Int maxCell = new Vector2Int(40, 40);

    [Header("Terrain Shape")]
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

    [Header("RuleTile Boundary")]
    [Tooltip("Adds neighbor tiles outside the generated area so RuleTiles at the map edge do not treat the outside as grass/Not This. Set to 1 to remove grass fringes at the generated rectangle boundary.")]
    [SerializeField, Range(0, 8)] private int ruleTileNeighborPadding;

    [Header("Random")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed;
    [SerializeField] private bool clearBeforeGenerate = true;

    [ContextMenu("Generate Random Map")]
    public void GenerateRandomMap()
    {
        List<TerrainTilemapLayer> layers = GetUsableTerrainLayers();
        if (layers.Count == 0)
        {
            Debug.LogError("RandomTilemapGenerator needs at least one enabled Terrain Layer with a Target Tilemap and a Grass or Dirt Tile.", this);
            return;
        }

        int fromX = Mathf.Min(minCell.x, maxCell.x);
        int toX = Mathf.Max(minCell.x, maxCell.x);
        int fromY = Mathf.Min(minCell.y, maxCell.y);
        int toY = Mathf.Max(minCell.y, maxCell.y);

        System.Random random = useRandomSeed ? new System.Random() : new System.Random(seed);
        bool[,] dirtMask = CreateSmoothedTerrainMask(random, fromX, toX, fromY, toY);

        int placedTileCount = 0;
        for (int i = 0; i < layers.Count; i++)
        {
            placedTileCount += PaintTerrainLayer(layers[i], random, dirtMask, fromX, fromY);
        }

        Debug.Log(string.Format(
            "Generated {0} random tiles across {1} layer(s), area x={2}..{3}, y={4}..{5}.",
            placedTileCount,
            layers.Count,
            fromX,
            toX,
            fromY,
            toY), this);
    }

    [ContextMenu("Clear Generated Area")]
    public void ClearGeneratedArea()
    {
        List<TerrainTilemapLayer> layers = GetUsableTerrainLayers();
        if (layers.Count == 0)
        {
            Debug.LogError("RandomTilemapGenerator could not find any configured Terrain Layers to clear.", this);
            return;
        }

        int padding = Mathf.Max(0, ruleTileNeighborPadding);
        int fromX = Mathf.Min(minCell.x, maxCell.x) - padding;
        int toX = Mathf.Max(minCell.x, maxCell.x) + padding;
        int fromY = Mathf.Min(minCell.y, maxCell.y) - padding;
        int toY = Mathf.Max(minCell.y, maxCell.y) + padding;

        for (int i = 0; i < layers.Count; i++)
        {
            Tilemap tilemap = ResolveLayerTilemap(layers[i]);
            if (tilemap == null)
            {
                continue;
            }

            RecordTilemapUndo(tilemap, "Clear Random Map Area");
            ClearArea(tilemap, fromX, toX, fromY, toY);
            FinalizeTilemap(tilemap);
        }
    }

    private bool[,] CreateSmoothedTerrainMask(System.Random random, int fromX, int toX, int fromY, int toY)
    {
        Vector2 noiseOffset = GetNoiseOffset(random);
        float dirtThreshold = 1f - dirtAmount;
        bool[,] dirtMask = CreateTerrainMask(fromX, toX, fromY, toY, noiseOffset, dirtThreshold);

        for (int i = 0; i < terrainSmoothingIterations; i++)
        {
            SmoothTerrainMaskOnce(dirtMask);
        }

        return dirtMask;
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

    private void SmoothTerrainMaskOnce(bool[,] dirtMask)
    {
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);
        bool[,] smoothedMask = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int dirtNeighbors = CountDirtNeighbors(dirtMask, x, y);
                smoothedMask[x, y] = dirtMask[x, y]
                    ? dirtNeighbors >= dirtSurvivalNeighbors
                    : dirtNeighbors >= dirtBirthNeighbors;
            }
        }

        CopyMask(smoothedMask, dirtMask);
    }

    private int PaintTerrainLayer(TerrainTilemapLayer layer, System.Random random, bool[,] dirtMask, int fromX, int fromY)
    {
        Tilemap tilemap = ResolveLayerTilemap(layer);
        if (tilemap == null)
        {
            return 0;
        }

        TileBase selectedDirtTile = ChooseDirtTile(layer, random);
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);
        int padding = Mathf.Max(0, ruleTileNeighborPadding);
        int toX = fromX + width - 1;
        int toY = fromY + height - 1;

        RecordTilemapUndo(tilemap, "Generate Random Map Layer");
        if (clearBeforeGenerate)
        {
            ClearArea(tilemap, fromX - padding, toX + padding, fromY - padding, toY + padding);
        }

        int placedTileCount = 0;
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

                tilemap.SetTile(new Vector3Int(fromX + localX, fromY + localY, 0), tile);
                placedTileCount++;
            }
        }

        if (ShouldPaintLayerPadding(layer))
        {
            PaintRuleTileNeighborPadding(tilemap, selectedDirtTile, dirtMask, fromX, fromY);
        }

        FinalizeTilemap(tilemap);
        return placedTileCount;
    }

    private bool ShouldPaintLayerPadding(TerrainTilemapLayer layer)
    {
        return ruleTileNeighborPadding > 0
            && layer.paintChance >= 1f
            && layer.paintOn != LayerPaintTarget.GrassOnly
            && GetValidDirtTiles(layer).Count > 0;
    }

    private void PaintRuleTileNeighborPadding(Tilemap tilemap, TileBase dirtTileForPadding, bool[,] dirtMask, int fromX, int fromY)
    {
        if (tilemap == null || dirtTileForPadding == null)
        {
            return;
        }

        int padding = Mathf.Max(0, ruleTileNeighborPadding);
        int width = dirtMask.GetLength(0);
        int height = dirtMask.GetLength(1);
        int toX = fromX + width - 1;
        int toY = fromY + height - 1;

        for (int y = fromY - padding; y <= toY + padding; y++)
        {
            for (int x = fromX - padding; x <= toX + padding; x++)
            {
                if (x >= fromX && x <= toX && y >= fromY && y <= toY)
                {
                    continue;
                }

                int nearestLocalX = Mathf.Clamp(x - fromX, 0, width - 1);
                int nearestLocalY = Mathf.Clamp(y - fromY, 0, height - 1);
                if (dirtMask[nearestLocalX, nearestLocalY])
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), dirtTileForPadding);
                }
            }
        }
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

    private static TileBase ChooseDirtTile(TerrainTilemapLayer layer, System.Random random)
    {
        List<TileBase> validDirtTiles = GetValidDirtTiles(layer);
        return validDirtTiles.Count == 0 ? null : validDirtTiles[random.Next(validDirtTiles.Count)];
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
            if (layer == null || !layer.enabled || ResolveLayerTilemap(layer) == null)
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

    private Vector2 GetNoiseOffset(System.Random random)
    {
        if (useRandomSeed)
        {
            return terrainNoiseOffset + new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
        }

        return terrainNoiseOffset + new Vector2(seed * 17.13f, seed * 31.71f);
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

    private static void FinalizeTilemap(Tilemap tilemap)
    {
        tilemap.CompressBounds();
        tilemap.RefreshAllTiles();
        MarkTilemapDirty(tilemap);
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
}
