using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapTrimmer : MonoBehaviour
{
    public Tilemap borderTilemap;
    public Tilemap gridTilemap;

    [ContextMenu("Auto Trim Grid Outside Border")]
    public void TrimGridInsideBorder()
    {
        if (borderTilemap == null || gridTilemap == null)
        {
            Debug.LogError("Tilemaps not assigned.");
            return;
        }

        BoundsInt bounds = gridTilemap.cellBounds;
        Vector3Int? start = FindValidStart(bounds);

        if (start == null)
        {
            Debug.LogError("No valid starting point found outside the border.");
            return;
        }

        Debug.Log("Flood fill starting at " + start.Value);

        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        queue.Enqueue(start.Value);
        visited.Add(start.Value);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (borderTilemap.HasTile(current)) continue;

            foreach (Vector3Int offset in new Vector3Int[] {
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0)
            })
            {
                Vector3Int next = current + offset;
                if (bounds.Contains(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        int removed = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (gridTilemap.HasTile(pos) && !visited.Contains(pos))
                {
                    gridTilemap.SetTile(pos, null);
                    removed++;
                }
            }
        }

        Debug.Log($"Trimming complete. {removed} tiles removed.");
    }

    Vector3Int? FindValidStart(BoundsInt bounds)
    {
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (gridTilemap.HasTile(pos) && !borderTilemap.HasTile(pos))
                {
                    return pos;
                }
            }
        }
        return null;
    }
}