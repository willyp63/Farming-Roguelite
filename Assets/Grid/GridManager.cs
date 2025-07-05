using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : Singleton<GridManager>
{
    [Header("Grid Settings")]
    [SerializeField]
    private Transform gridContainer;

    [SerializeField]
    private int gridWidth = 10;

    [SerializeField]
    private int gridHeight = 10;

    [SerializeField]
    private float tileSize = 1f;

    [SerializeField]
    private float padding = 0.125f;

    [SerializeField]
    private GameObject gridTilePrefab;

    [SerializeField]
    private Tile emptyTile;

    // Grid data
    private GridTile[,] grid;
    private bool isInitialized = false;

    // Properties
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public GridTile[,] Grid => grid;
    public bool IsInitialized => isInitialized;

    // Events
    [NonSerialized]
    public UnityEvent OnGridChanged = new();

    public void InitializeGrid()
    {
        if (isInitialized)
            return;

        if (emptyTile == null)
            Debug.LogError("Empty tile is not set");

        foreach (Transform child in gridContainer.transform)
        {
            Destroy(child.gameObject);
        }

        grid = new GridTile[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateGridTile(x, y, emptyTile);
            }
        }

        isInitialized = true;
    }

    public void ResetGrid()
    {
        ClearPlaceables(true);

        foreach (GridTile tile in grid)
        {
            tile.SetTile(emptyTile);
        }
    }

    public void OnEndOfTurn()
    {
        foreach (GridTile tile in grid)
        {
            if (tile.PlacedObject != null)
                tile.PlacedObject.OnEndOfTurn();
        }

        OnGridChanged?.Invoke();
    }

    public Dictionary<Vector2Int, GridTile> GetGrid()
    {
        Dictionary<Vector2Int, GridTile> result = new Dictionary<Vector2Int, GridTile>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                result.Add(new Vector2Int(x, y), grid[x, y]);
            }
        }
        return result;
    }

    public bool HasEmptyTiles()
    {
        foreach (GridTile tile in grid)
        {
            if (tile.Tile.TileType == TileType.Empty)
                return true;
        }

        return false;
    }

    public GridTile GetTile(Vector2Int position)
    {
        if (IsValidPosition(position.x, position.y))
        {
            return grid[position.x, position.y];
        }
        return null;
    }

    public void PlaceObject(Vector2Int position, Placeable placeablePrefab, int score = 0)
    {
        if (!IsValidPosition(position.x, position.y))
        {
            Debug.LogError($"Invalid position: {position}");
            return;
        }

        GridTile tile = grid[position.x, position.y];
        GameObject placeableObject = Instantiate(
            placeablePrefab.gameObject,
            Vector3.zero,
            Quaternion.identity,
            tile.transform
        );
        Placeable placeable = placeableObject.GetComponent<Placeable>();

        if (placeable == null)
        {
            Debug.LogError(
                $"Placeable prefab {placeablePrefab.name} does not have a Placeable component"
            );
            return;
        }

        placeable.Initialize(tile, score);
        tile.SetPlacedObject(placeable);
        placeable.transform.localPosition = Vector3.zero;

        // trigger on place effects
        placeable.OnPlaced();

        // trigger on effected tile placed effects for all other placeables
        foreach (GridTile otherTile in grid)
        {
            if (otherTile == null || otherTile == tile || otherTile.PlacedObject == null)
                continue;

            otherTile.PlacedObject.OnNewPlaced(otherTile, tile);
        }

        OnGridChanged?.Invoke();
    }

    public void RemoveObject(Vector2Int position)
    {
        if (!IsValidPosition(position.x, position.y))
            return;

        GridTile tile = grid[position.x, position.y];
        if (tile.PlacedObject == null)
            return;

        tile.PlacedObject.OnRemoved();
        Destroy(tile.PlacedObject.gameObject);
        tile.ClearPlacedObject();
        OnGridChanged?.Invoke();
    }

    public List<GridTile> GetAdjacentTiles(Vector2Int position)
    {
        return GetTilesFromPositions(GetOrthogonalPositions(position));
    }

    public List<GridTile> GetDiagonalTiles(Vector2Int position)
    {
        return GetTilesFromPositions(GetDiagonalPositions(position));
    }

    public List<GridTile> GetSurroundingTiles(Vector2Int position)
    {
        return GetTilesFromPositions(
            GetDiagonalPositions(position).Concat(GetOrthogonalPositions(position))
        );
    }

    public List<GridTile> GetRowTiles(Vector2Int position)
    {
        return GetTilesFromPositions(
            Enumerable.Range(0, gridWidth).Select(x => new Vector2Int(x, position.y))
        );
    }

    public List<GridTile> GetColumnTiles(Vector2Int position)
    {
        return GetTilesFromPositions(
            Enumerable.Range(0, gridHeight).Select(y => new Vector2Int(position.x, y))
        );
    }

    public List<GridTile> GetAllTiles()
    {
        // TODO: not sure if this works
        return grid.Cast<GridTile>().ToList();
    }

    private List<GridTile> GetTilesFromPositions(IEnumerable<Vector2Int> positions)
    {
        return positions
            .Where(pos => IsValidPosition(pos.x, pos.y))
            .Select(pos => grid[pos.x, pos.y])
            .ToList();
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    private void CreateGridTile(int x, int y, Tile tile)
    {
        if (!IsValidPosition(x, y) || grid[x, y] != null)
            return;

        // Calculate world position
        Vector3 worldPosition = GetWorldPosition(x, y);

        // Instantiate the grid tile prefab
        GameObject tileObject = Instantiate(
            gridTilePrefab,
            Vector3.zero,
            Quaternion.identity,
            gridContainer
        );
        tileObject.transform.localPosition = worldPosition;
        GridTile gridTile = tileObject.GetComponent<GridTile>();

        if (gridTile != null)
        {
            gridTile.Initialize(x, y, tile);
            grid[x, y] = gridTile;
        }
        else
        {
            Debug.LogError("GridTile component not found on prefab!");
            Destroy(tileObject);
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float gridWidthSize = gridWidth * (tileSize + padding);
        float gridHeightSize = gridHeight * (tileSize + padding);
        return new Vector3(
            x * (tileSize + padding) - gridWidthSize / 2 + tileSize / 2 + padding / 2,
            y * (tileSize + padding) - gridHeightSize / 2 + tileSize / 2 + padding / 2,
            0f
        );
    }

    public int CalculateBoardScore()
    {
        int totalScore = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridTile tile = grid[x, y];
                if (tile != null && tile.PlacedObject != null && tile.PlacedObject.Score > 0)
                {
                    totalScore += tile.PlacedObject.Score;

                    FloatingTextManager.Instance.SpawnText(
                        $"+{tile.PlacedObject.Score}",
                        tile.transform.position,
                        new Color(200f / 255f, 0f / 255f, 255f / 255f, 1f)
                    );
                }
            }
        }

        return totalScore;
    }

    public void ClearPlaceables(bool clearPermanents = false)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridTile tile = grid[x, y];

                if (
                    tile.PlacedObject != null
                    && (clearPermanents || !tile.PlacedObject.IsPermanent)
                )
                {
                    RemoveObject(new Vector2Int(x, y));
                }
            }
        }
    }

    public static List<Vector2Int> GetOrthogonalPositions(Vector2Int position)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(position.x + 1, position.y),
            new Vector2Int(position.x - 1, position.y),
            new Vector2Int(position.x, position.y + 1),
            new Vector2Int(position.x, position.y - 1),
        };
    }

    public static List<Vector2Int> GetDiagonalPositions(Vector2Int position)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(position.x + 1, position.y + 1),
            new Vector2Int(position.x + 1, position.y - 1),
            new Vector2Int(position.x - 1, position.y + 1),
            new Vector2Int(position.x - 1, position.y - 1),
        };
    }
}
