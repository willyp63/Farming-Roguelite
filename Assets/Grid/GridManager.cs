using System;
using System.Collections.Generic;
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

    [NonSerialized]
    public UnityEvent OnGridGenerated = new();

    [NonSerialized]
    public UnityEvent OnGridChanged = new();

    private bool isGridGenerated = false;

    // Grid data
    private GridTile[,] grid;

    // Properties
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public GridTile[,] Grid => grid;
    public bool IsGridGenerated => isGridGenerated;

    [ContextMenu("Initialize Grid")]
    public void InitializeGridFromInspector()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        // Initialize grid
        grid = new GridTile[gridWidth, gridHeight];

        // Clear any existing grid tiles
        ClearExistingGrid();

        isGridGenerated = true;
        OnGridGenerated?.Invoke();
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

            otherTile.PlacedObject.OnEffectedTilePlaced(otherTile, tile);
        }

        OnGridChanged?.Invoke();
    }

    public void RemoveObject(Vector2Int position)
    {
        if (IsValidPosition(position.x, position.y))
        {
            GridTile tile = grid[position.x, position.y];
            if (tile != null)
            {
                tile.PlacedObject.OnRemoved();
                Destroy(tile.PlacedObject.gameObject);
                tile.ClearPlacedObject();
                OnGridChanged?.Invoke();
            }
        }
    }

    private void ClearExistingGrid()
    {
        // Find and destroy any existing grid tiles
        GridTile[] existingTiles = FindObjectsOfType<GridTile>();
        foreach (GridTile tile in existingTiles)
        {
            if (Application.isPlaying)
                Destroy(tile.gameObject);
            else
                DestroyImmediate(tile.gameObject);
        }
    }

    private List<Vector2Int> GetAdjacentPositions(int x, int y, bool includeDiagonal)
    {
        List<Vector2Int> adjacent = new List<Vector2Int>();

        // Orthogonal positions
        adjacent.Add(new Vector2Int(x + 1, y));
        adjacent.Add(new Vector2Int(x - 1, y));
        adjacent.Add(new Vector2Int(x, y + 1));
        adjacent.Add(new Vector2Int(x, y - 1));

        if (includeDiagonal)
        {
            // Diagonal positions
            adjacent.Add(new Vector2Int(x + 1, y + 1));
            adjacent.Add(new Vector2Int(x + 1, y - 1));
            adjacent.Add(new Vector2Int(x - 1, y + 1));
            adjacent.Add(new Vector2Int(x - 1, y - 1));
        }

        return adjacent;
    }

    public List<GridTile> GetAdjacentTiles(Vector2Int position)
    {
        List<GridTile> adjacentTiles = new List<GridTile>();
        List<Vector2Int> adjacentPositions = GetAdjacentPositions(position.x, position.y, false);

        foreach (Vector2Int pos in adjacentPositions)
        {
            if (IsValidPosition(pos.x, pos.y))
            {
                adjacentTiles.Add(grid[pos.x, pos.y]);
            }
        }

        return adjacentTiles;
    }

    public List<GridTile> GetDiagonalTiles(Vector2Int position)
    {
        List<GridTile> diagonalTiles = new List<GridTile>();

        // Diagonal positions
        Vector2Int[] diagonalPositions =
        {
            new Vector2Int(position.x + 1, position.y + 1),
            new Vector2Int(position.x + 1, position.y - 1),
            new Vector2Int(position.x - 1, position.y + 1),
            new Vector2Int(position.x - 1, position.y - 1),
        };

        foreach (Vector2Int pos in diagonalPositions)
        {
            if (IsValidPosition(pos.x, pos.y))
            {
                diagonalTiles.Add(grid[pos.x, pos.y]);
            }
        }

        return diagonalTiles;
    }

    public List<GridTile> GetSurroundingTiles(Vector2Int position)
    {
        List<GridTile> surroundingTiles = new List<GridTile>();
        List<Vector2Int> surroundingPositions = GetAdjacentPositions(position.x, position.y, true);

        foreach (Vector2Int pos in surroundingPositions)
        {
            if (IsValidPosition(pos.x, pos.y))
            {
                surroundingTiles.Add(grid[pos.x, pos.y]);
            }
        }

        return surroundingTiles;
    }

    public List<GridTile> GetRowTiles(Vector2Int position)
    {
        List<GridTile> rowTiles = new List<GridTile>();

        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, position.y] != null)
            {
                rowTiles.Add(grid[x, position.y]);
            }
        }

        return rowTiles;
    }

    public List<GridTile> GetColumnTiles(Vector2Int position)
    {
        List<GridTile> columnTiles = new List<GridTile>();

        for (int y = 0; y < gridHeight; y++)
        {
            if (grid[position.x, y] != null)
            {
                columnTiles.Add(grid[position.x, y]);
            }
        }

        return columnTiles;
    }

    public List<GridTile> GetAllTiles()
    {
        List<GridTile> allTiles = new List<GridTile>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    allTiles.Add(grid[x, y]);
                }
            }
        }

        return allTiles;
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public void CreateGridTile(int x, int y, Tile tile)
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

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Convert world position to local position relative to the grid
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // Calculate grid dimensions
        float gridWidthSize = gridWidth * (tileSize + padding);
        float gridHeightSize = gridHeight * (tileSize + padding);

        // Calculate the offset from the grid center
        float offsetX = localPosition.x + gridWidthSize / 2 - tileSize / 2 - padding / 2;
        float offsetY = localPosition.y + gridHeightSize / 2 - tileSize / 2 - padding / 2;

        // Convert to grid coordinates
        int x = Mathf.FloorToInt(offsetX / (tileSize + padding));
        int y = Mathf.FloorToInt(offsetY / (tileSize + padding));

        // Clamp to grid bounds
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        return new Vector2Int(x, y);
    }

    public void FlipGridX()
    {
        if (grid == null)
        {
            Debug.LogWarning("Grid is not initialized. Generate grid first.");
            return;
        }

        // Flip the grid data array
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth / 2; x++)
            {
                int oppositeX = gridWidth - 1 - x;

                // Swap the grid data
                GridTile temp = grid[x, y];
                grid[x, y] = grid[oppositeX, y];
                grid[oppositeX, y] = temp;
            }
        }

        // Update the visual positions of all tiles
        UpdateTilePositions();
    }

    public void FlipGridY()
    {
        if (grid == null)
        {
            Debug.LogWarning("Grid is not initialized. Generate grid first.");
            return;
        }

        // Flip the grid data array
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight / 2; y++)
            {
                int oppositeY = gridHeight - 1 - y;

                // Swap the grid data
                GridTile temp = grid[x, y];
                grid[x, y] = grid[x, oppositeY];
                grid[x, oppositeY] = temp;
            }
        }

        // Update the visual positions of all tiles
        UpdateTilePositions();
    }

    private void UpdateTilePositions()
    {
        // Update the world positions of all tiles to match their new grid positions
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Vector3 newWorldPosition = GetWorldPosition(x, y);

                    // Update the tile's position and grid coordinates
                    grid[x, y].transform.localPosition = newWorldPosition;
                    grid[x, y].SetPosition(x, y);
                    grid[x, y].transform.localPosition = GetWorldPosition(x, y);
                }
            }
        }
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

    public void ClearNonPermanentPlaceables()
    {
        List<Vector2Int> positionsToClear = new List<Vector2Int>();

        // Find all non-permanent placeables
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                GridTile tile = grid[x, y];

                if (tile != null && tile.PlacedObject != null && !tile.PlacedObject.IsPermanent)
                {
                    positionsToClear.Add(position);
                }
            }
        }

        // Remove them
        foreach (Vector2Int position in positionsToClear)
        {
            RemoveObject(position);
        }

        Debug.Log($"Cleared {positionsToClear.Count} non-permanent placeables from the board");
        OnGridChanged?.Invoke();
    }
}
