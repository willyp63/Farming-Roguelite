using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InitialTileData
{
    [SerializeField]
    public int x;

    [SerializeField]
    public int y;

    [SerializeField]
    public Tile tile;
}

[System.Serializable]
public class InitialPlaceableData
{
    [SerializeField]
    public int x;

    [SerializeField]
    public int y;

    [SerializeField]
    public Placeable placeable;
}

[System.Serializable]
public class GenerationStep
{
    [SerializeField]
    public Tile tile;

    [SerializeField]
    public int maxCount = 1;

    [SerializeField]
    public int maxTotalCount = 99;

    [SerializeField]
    public List<Tile> allowedAdjacentTiles;

    [SerializeField]
    public bool allowDiagonal = false; // Whether adjacent means diagonal as well
}

[System.Serializable]
public class PlaceableGenerationStep
{
    [SerializeField]
    public Placeable placeablePrefab;

    [SerializeField]
    public int maxCount = 1;

    [SerializeField]
    public List<Tile> allowedTiles;

    [SerializeField]
    public int minDistanceToOtherPlaceables = 1;
}

public class GridManager : Singleton<GridManager>
{
    [Header("Grid Settings")]
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

    [Header("Initial Tiles")]
    [SerializeField]
    private List<InitialTileData> initialTiles = new List<InitialTileData>();

    [Header("Initial Placeables")]
    [SerializeField]
    private List<InitialPlaceableData> initialPlaceables = new List<InitialPlaceableData>();

    [Header("Generation Steps")]
    [SerializeField]
    private List<GenerationStep> generationSteps = new List<GenerationStep>();

    [Header("Placeable Generation Steps")]
    [SerializeField]
    private List<PlaceableGenerationStep> placeableGenerationSteps =
        new List<PlaceableGenerationStep>();

    public UnityEvent OnGridGenerated;
    public UnityEvent OnGridChanged;

    private bool isGridGenerated = false;

    // Grid data
    private GridTile[,] grid;

    // Properties
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public GridTile[,] Grid => grid;
    public bool IsGridGenerated => isGridGenerated;

    [ContextMenu("Generate Grid")]
    public void GenerateGridFromInspector()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        // Initialize grid
        grid = new GridTile[gridWidth, gridHeight];

        // Clear any existing grid tiles
        ClearExistingGrid();

        // Place initial tiles
        PlaceInitialTiles();

        // Place initial placeables
        PlaceInitialPlaceables();

        // Apply generation steps
        ApplyGenerationSteps();

        // Apply placeable generation steps
        ApplyPlaceableGenerationSteps();

        // Randomly flip the grid after generation
        ApplyRandomFlips();

        // TODO: Verify grid is complete

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

    public void OnBeforeScoring()
    {
        foreach (GridTile tile in grid)
        {
            if (tile.PlacedObject != null)
                tile.PlacedObject.OnBeforeScoring();
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
        GameObject placeableObject = Instantiate(placeablePrefab.gameObject, tile.transform);
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
        placeable.OnPlaced();
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

    private void PlaceInitialTiles()
    {
        foreach (InitialTileData initialTile in initialTiles)
        {
            if (IsValidPosition(initialTile.x, initialTile.y) && initialTile.tile != null)
            {
                CreateGridTile(initialTile.x, initialTile.y, initialTile.tile);
            }
        }
    }

    private void PlaceInitialPlaceables()
    {
        foreach (InitialPlaceableData initialPlaceable in initialPlaceables)
        {
            if (
                IsValidPosition(initialPlaceable.x, initialPlaceable.y)
                && initialPlaceable.placeable != null
            )
            {
                PlaceObject(
                    new Vector2Int(initialPlaceable.x, initialPlaceable.y),
                    initialPlaceable.placeable
                );
            }
        }
    }

    private void ApplyGenerationSteps()
    {
        foreach (GenerationStep step in generationSteps)
        {
            if (step.tile == null)
                continue;

            int placedCount = 0;
            int maxAttempts = gridWidth * gridHeight * 10; // Prevent infinite loops
            int attempts = 0;

            int totalTilesPlaced = GetTotalTilesInGrid(step.tile.TileType);

            while (placedCount < step.maxCount && attempts < maxAttempts)
            {
                // Check if placing another tile would exceed maxTotalCount
                if (totalTilesPlaced >= step.maxTotalCount)
                {
                    Debug.Log(
                        $"Reached max total count ({step.maxTotalCount}) for step {step.tile.name}. Stopping placement."
                    );
                    break;
                }

                attempts++;

                // Get all valid positions for this step
                List<Vector2Int> validPositions = GetValidPositionsForStep(step);

                if (validPositions.Count == 0)
                    break; // No valid positions left

                // Choose a random valid position
                Vector2Int position = validPositions[Random.Range(0, validPositions.Count)];

                // Place the tile
                CreateGridTile(position.x, position.y, step.tile);
                placedCount++;
                totalTilesPlaced++;
            }

            if (placedCount < step.maxCount)
            {
                Debug.LogWarning(
                    $"Could only place {placedCount}/{step.maxCount} tiles for step {step.tile.name}"
                );
            }
        }
    }

    private void ApplyPlaceableGenerationSteps()
    {
        foreach (PlaceableGenerationStep step in placeableGenerationSteps)
        {
            if (step.placeablePrefab == null)
                continue;

            int placedCount = 0;
            int maxAttempts = gridWidth * gridHeight * 10; // Prevent infinite loops
            int attempts = 0;

            while (placedCount < step.maxCount && attempts < maxAttempts)
            {
                attempts++;

                // Get all valid positions for this placeable
                List<Vector2Int> validPositions = GetValidPositionsForPlaceable(step);

                if (validPositions.Count == 0)
                    break; // No valid positions left

                // Choose a random valid position
                Vector2Int position = validPositions[Random.Range(0, validPositions.Count)];

                // Place the placeable
                PlaceObject(position, step.placeablePrefab);
                placedCount++;
            }

            if (placedCount < step.maxCount)
            {
                Debug.LogWarning(
                    $"Could only place {placedCount}/{step.maxCount} placeables for step {step.placeablePrefab.name}"
                );
            }
        }
    }

    private List<Vector2Int> GetValidPositionsForStep(GenerationStep step)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPositionForStep(x, y, step))
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return validPositions;
    }

    private bool IsValidPositionForStep(int x, int y, GenerationStep step)
    {
        // Position must be empty
        if (grid[x, y] != null)
            return false;

        // If no adjacency requirements, any empty position is valid
        if (step.allowedAdjacentTiles == null || step.allowedAdjacentTiles.Count == 0)
            return true;

        // Check if position has at least one adjacent tile from the allowed list
        List<Vector2Int> adjacentPositions = GetAdjacentPositions(x, y, step.allowDiagonal);

        foreach (Vector2Int adjPos in adjacentPositions)
        {
            if (IsValidPosition(adjPos.x, adjPos.y) && grid[adjPos.x, adjPos.y] != null)
            {
                // Check if the adjacent tile is in the allowed list
                foreach (Tile allowedTile in step.allowedAdjacentTiles)
                {
                    if (allowedTile == grid[adjPos.x, adjPos.y].Tile)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
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

    private List<Vector2Int> GetValidPositionsForPlaceable(PlaceableGenerationStep step)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPositionForPlaceable(x, y, step))
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return validPositions;
    }

    private bool IsValidPositionForPlaceable(int x, int y, PlaceableGenerationStep step)
    {
        // Position must be within grid bounds
        if (!IsValidPosition(x, y))
            return false;

        // Position must have a tile
        if (grid[x, y] == null)
            return false;

        // Position must not already have a placeable
        if (grid[x, y].PlacedObject != null)
            return false;

        // Check if the tile is in the allowed tiles list
        if (step.allowedTiles != null && step.allowedTiles.Count > 0)
        {
            bool tileAllowed = false;
            foreach (Tile allowedTile in step.allowedTiles)
            {
                if (allowedTile == grid[x, y].Tile)
                {
                    tileAllowed = true;
                    break;
                }
            }
            if (!tileAllowed)
                return false;
        }

        // Check minimum distance to other placeables
        if (step.minDistanceToOtherPlaceables > 0)
        {
            for (int checkX = 0; checkX < gridWidth; checkX++)
            {
                for (int checkY = 0; checkY < gridHeight; checkY++)
                {
                    if (grid[checkX, checkY] != null && grid[checkX, checkY].PlacedObject != null)
                    {
                        int distance = Mathf.Max(Mathf.Abs(x - checkX), Mathf.Abs(y - checkY));
                        if (distance < step.minDistanceToOtherPlaceables)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    private int GetTotalTilesInGrid(TileType tileType)
    {
        int placedTiles = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null && grid[x, y].Tile.TileType == tileType)
                    placedTiles++;
            }
        }
        return placedTiles;
    }

    private bool IsValidPosition(int x, int y)
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
            transform
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

    private void ApplyRandomFlips()
    {
        // 50% chance to flip on X axis
        if (Random.Range(0f, 1f) < 0.5f)
        {
            FlipGridX();
            Debug.Log("Grid flipped on X axis");
        }

        // 50% chance to flip on Y axis
        if (Random.Range(0f, 1f) < 0.5f)
        {
            FlipGridY();
            Debug.Log("Grid flipped on Y axis");
        }
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
