using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class RandomTilemapGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TileStyleGroup
    {
        public string name = "Ground";
        [Min(0f)] public float weight = 1f;
        public TileBase[] tiles = new TileBase[0];
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
        if (tilemap == null)
        {
            Debug.LogError("RandomTilemapGenerator could not find a target Tilemap. Assign Ground1 or attach this component to Ground1.", this);
            return;
        }

        List<ResolvedTileStyleGroup> validStyleGroups = GetValidStyleGroups();
        if (validStyleGroups.Count == 0)
        {
            Debug.LogError("RandomTilemapGenerator needs at least one style group with at least one Tile asset.", this);
            return;
        }

        int fromX = Mathf.Min(minCell.x, maxCell.x);
        int toX = Mathf.Max(minCell.x, maxCell.x);
        int fromY = Mathf.Min(minCell.y, maxCell.y);
        int toY = Mathf.Max(minCell.y, maxCell.y);

        if (clearBeforeGenerate)
        {
            RecordTilemapUndo(tilemap, "Generate Random Map");
            ClearArea(tilemap, fromX, toX, fromY, toY);
        }
        else
        {
            RecordTilemapUndo(tilemap, "Generate Random Map");
        }

        System.Random random = useRandomSeed ? new System.Random() : new System.Random(seed);
        Vector2 noiseOffset = useRandomSeed
            ? new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000))
            : styleNoiseOffset + new Vector2(seed * 17.13f, seed * 31.71f);
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

        tilemap.CompressBounds();
        MarkTilemapDirty(tilemap);

        Debug.Log(string.Format(
            "Generated {0} random tiles on {1}, area x={2}..{3}, y={4}..{5}.",
            placedTileCount,
            tilemap.name,
            fromX,
            toX,
            fromY,
            toY), this);
    }

    [ContextMenu("Clear Generated Area")]
    public void ClearGeneratedArea()
    {
        Tilemap tilemap = ResolveTargetTilemap();
        if (tilemap == null)
        {
            Debug.LogError("RandomTilemapGenerator could not find a target Tilemap to clear.", this);
            return;
        }

        int fromX = Mathf.Min(minCell.x, maxCell.x);
        int toX = Mathf.Max(minCell.x, maxCell.x);
        int fromY = Mathf.Min(minCell.y, maxCell.y);
        int toY = Mathf.Max(minCell.y, maxCell.y);

        RecordTilemapUndo(tilemap, "Clear Random Map Area");
        ClearArea(tilemap, fromX, toX, fromY, toY);
        tilemap.CompressBounds();
        MarkTilemapDirty(tilemap);
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
