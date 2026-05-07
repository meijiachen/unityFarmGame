using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class RandomTilemapGenerator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private string fallbackTilemapName = "Ground1";

    [Header("Tile Palette Assets")]
    [SerializeField] private TileBase[] groundTiles = new TileBase[0];

    [Header("Generate Area")]
    [SerializeField] private Vector2Int minCell = new Vector2Int(-40, -40);
    [SerializeField] private Vector2Int maxCell = new Vector2Int(40, 40);

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

        List<TileBase> validTiles = GetValidGroundTiles();
        if (validTiles.Count == 0)
        {
            Debug.LogError("RandomTilemapGenerator needs at least one Tile asset in Ground Tiles.", this);
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
        int placedTileCount = 0;

        for (int y = fromY; y <= toY; y++)
        {
            for (int x = fromX; x <= toX; x++)
            {
                TileBase tile = validTiles[random.Next(validTiles.Count)];
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

    private List<TileBase> GetValidGroundTiles()
    {
        List<TileBase> validTiles = new List<TileBase>();
        if (groundTiles == null)
        {
            return validTiles;
        }

        for (int i = 0; i < groundTiles.Length; i++)
        {
            if (groundTiles[i] != null)
            {
                validTiles.Add(groundTiles[i]);
            }
        }

        return validTiles;
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
