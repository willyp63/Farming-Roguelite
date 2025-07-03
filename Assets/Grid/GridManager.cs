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

    private bool isGridGenerated = false;

    // Grid data
    private GridTile[,] grid;

    // Properties
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public GridTile[,] Grid => grid;
    public bool IsGridGenerated => isGridGenerated;

    private void Start()
    {
        GenerateGrid();
    }

    [ContextMenu("Generate Grid")]
    public void GenerateGridFromInspector()
    {
        GenerateGrid();
    }

    [ContextMenu("Flip Grid X")]
    public void FlipGridXFromInspector()
    {
        FlipGridX();
    }

    [ContextMenu("Flip Grid Y")]
    public void FlipGridYFromInspector()
    {
        FlipGridY();
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
    }

    public void OnStartOfTurn()
    {
        foreach (GridTile tile in grid)
        {
            if (tile.PlacedObject != null)
                tile.PlacedObject.OnStartOfTurn();
        }
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

    public void SetTile(Vector2Int position, GridTile tile)
    {
        if (IsValidPosition(position.x, position.y))
        {
            grid[position.x, position.y] = tile;
        }
    }

    public void PlaceObject(Vector2Int position, Placeable placeablePrefab)
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

        placeable.Initialize(tile);
        placeable.transform.localPosition = Vector3.zero;
        placeable.OnPlaced();

        tile.SetPlacedObject(placeable);
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
            }
        }
    }

    public void MovePlaceable(Vector2Int fromPosition, Vector2Int toPosition)
    {
        if (
            !IsValidPosition(fromPosition.x, fromPosition.y)
            || !IsValidPosition(toPosition.x, toPosition.y)
        )
        {
            Debug.LogError($"Invalid positions for move: from {fromPosition} to {toPosition}");
            return;
        }

        GridTile fromTile = grid[fromPosition.x, fromPosition.y];
        GridTile toTile = grid[toPosition.x, toPosition.y];

        if (fromTile == null || fromTile.PlacedObject == null)
        {
            Debug.LogError($"No placeable found at position {fromPosition}");
            return;
        }

        if (toTile == null || toTile.PlacedObject != null)
        {
            Debug.LogError($"Target tile at {toPosition} is invalid or occupied");
            return;
        }

        // Get the placeable
        Placeable placeable = fromTile.PlacedObject;

        // Remove from old tile
        fromTile.ClearPlacedObject();

        // Place on new tile
        placeable.transform.SetParent(toTile.transform);
        placeable.transform.localPosition = Vector3.zero;
        placeable.Initialize(toTile);
        toTile.SetPlacedObject(placeable);

        // Notify the placeable that it has been moved
        placeable.OnPlaced();

        Debug.Log($"Moved placeable from {fromPosition} to {toPosition}");
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
}
