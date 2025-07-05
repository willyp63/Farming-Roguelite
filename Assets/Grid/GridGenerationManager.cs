using System;
using System.Collections.Generic;
using UnityEngine;

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

public class GridGenerationManager : Singleton<GridGenerationManager>
{
    [SerializeField]
    private GridManager gridManager;

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

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        // Initialize the grid first
        gridManager.InitializeGrid();

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
    }

    private void PlaceInitialTiles()
    {
        foreach (InitialTileData initialTile in initialTiles)
        {
            if (
                gridManager.IsValidPosition(initialTile.x, initialTile.y)
                && initialTile.tile != null
            )
            {
                gridManager.CreateGridTile(initialTile.x, initialTile.y, initialTile.tile);
            }
        }
    }

    private void PlaceInitialPlaceables()
    {
        foreach (InitialPlaceableData initialPlaceable in initialPlaceables)
        {
            if (
                gridManager.IsValidPosition(initialPlaceable.x, initialPlaceable.y)
                && initialPlaceable.placeable != null
            )
            {
                gridManager.PlaceObject(
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
            int maxAttempts = gridManager.GridWidth * gridManager.GridHeight * 10; // Prevent infinite loops
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
                Vector2Int position = validPositions[
                    UnityEngine.Random.Range(0, validPositions.Count)
                ];

                // Place the tile
                gridManager.CreateGridTile(position.x, position.y, step.tile);
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
            int maxAttempts = gridManager.GridWidth * gridManager.GridHeight * 10; // Prevent infinite loops
            int attempts = 0;

            while (placedCount < step.maxCount && attempts < maxAttempts)
            {
                attempts++;

                // Get all valid positions for this placeable
                List<Vector2Int> validPositions = GetValidPositionsForPlaceable(step);

                if (validPositions.Count == 0)
                    break; // No valid positions left

                // Choose a random valid position
                Vector2Int position = validPositions[
                    UnityEngine.Random.Range(0, validPositions.Count)
                ];

                // Place the placeable
                gridManager.PlaceObject(position, step.placeablePrefab);
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

        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
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
        if (gridManager.Grid[x, y] != null)
            return false;

        // If no adjacency requirements, any empty position is valid
        if (step.allowedAdjacentTiles == null || step.allowedAdjacentTiles.Count == 0)
            return true;

        // Check if position has at least one adjacent tile from the allowed list
        List<Vector2Int> adjacentPositions = GetAdjacentPositions(x, y, step.allowDiagonal);

        foreach (Vector2Int adjPos in adjacentPositions)
        {
            if (
                gridManager.IsValidPosition(adjPos.x, adjPos.y)
                && gridManager.Grid[adjPos.x, adjPos.y] != null
            )
            {
                // Check if the adjacent tile is in the allowed list
                foreach (Tile allowedTile in step.allowedAdjacentTiles)
                {
                    if (allowedTile == gridManager.Grid[adjPos.x, adjPos.y].Tile)
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

        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
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
        if (!gridManager.IsValidPosition(x, y))
            return false;

        // Position must have a tile
        if (gridManager.Grid[x, y] == null)
            return false;

        // Position must not already have a placeable
        if (gridManager.Grid[x, y].PlacedObject != null)
            return false;

        // Check if the tile is in the allowed tiles list
        if (step.allowedTiles != null && step.allowedTiles.Count > 0)
        {
            bool tileAllowed = false;
            foreach (Tile allowedTile in step.allowedTiles)
            {
                if (allowedTile == gridManager.Grid[x, y].Tile)
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
            for (int checkX = 0; checkX < gridManager.GridWidth; checkX++)
            {
                for (int checkY = 0; checkY < gridManager.GridHeight; checkY++)
                {
                    if (
                        gridManager.Grid[checkX, checkY] != null
                        && gridManager.Grid[checkX, checkY].PlacedObject != null
                    )
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
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                if (
                    gridManager.Grid[x, y] != null
                    && gridManager.Grid[x, y].Tile.TileType == tileType
                )
                    placedTiles++;
            }
        }
        return placedTiles;
    }

    private void ApplyRandomFlips()
    {
        // 50% chance to flip on X axis
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
        {
            gridManager.FlipGridX();
            Debug.Log("Grid flipped on X axis");
        }

        // 50% chance to flip on Y axis
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
        {
            gridManager.FlipGridY();
            Debug.Log("Grid flipped on Y axis");
        }
    }
}
