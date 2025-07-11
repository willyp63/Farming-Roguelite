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
    public TileType tileType;
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
    public TileType tileType;

    [SerializeField]
    public int maxCount = 1;

    [SerializeField]
    public int maxTotalCount = 99;

    [SerializeField]
    public List<TileType> allowedAdjacentTileTypes;

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
    public List<TileType> allowedTileTypes;

    [SerializeField]
    public int minDistanceToOtherPlaceables = 1;
}

public class GridGenerationManager : Singleton<GridGenerationManager>
{
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

    [SerializeField]
    private bool makeAllLinesScorable = false;

    public void GenerateGrid()
    {
        GridManager.Instance.ResetGrid();

        // Place initial tiles
        PlaceInitialTiles();

        // Place initial placeables
        PlaceInitialPlaceables();

        // Apply generation steps
        ApplyGenerationSteps();

        // Apply placeable generation steps
        ApplyPlaceableGenerationSteps();

        // Verify that generation is complete
        if (GridManager.Instance.HasEmptyTiles())
            Debug.LogError("Grid generation incomplete! Found empty tiles");
    }

    private void PlaceInitialTiles()
    {
        foreach (InitialTileData initialTile in initialTiles)
        {
            if (GridManager.Instance.IsValidPosition(initialTile.x, initialTile.y))
            {
                TileInfo tile = TileManager.GetTileInfo(initialTile.tileType);
                GridManager.Instance.Grid[initialTile.x, initialTile.y].SetTile(tile);
            }
        }
    }

    private void PlaceInitialPlaceables()
    {
        foreach (InitialPlaceableData initialPlaceable in initialPlaceables)
        {
            if (
                GridManager.Instance.IsValidPosition(initialPlaceable.x, initialPlaceable.y)
                && initialPlaceable.placeable != null
            )
            {
                GridManager.Instance.PlaceObject(
                    new Vector2Int(initialPlaceable.x, initialPlaceable.y),
                    initialPlaceable.placeable,
                    true
                );
            }
        }
    }

    private void ApplyGenerationSteps()
    {
        foreach (GenerationStep step in generationSteps)
        {
            int placedCount = 0;
            int maxAttempts = GridManager.Instance.GridWidth * GridManager.Instance.GridHeight * 10; // Prevent infinite loops
            int attempts = 0;

            int totalTilesPlaced = GetTotalTilesInGrid(step.tileType);

            while (placedCount < step.maxCount && attempts < maxAttempts)
            {
                // Check if placing another tile would exceed maxTotalCount
                if (totalTilesPlaced >= step.maxTotalCount)
                {
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
                TileInfo tile = TileManager.GetTileInfo(step.tileType);
                GridManager.Instance.Grid[position.x, position.y].SetTile(tile);
                placedCount++;
                totalTilesPlaced++;
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
            int maxAttempts = GridManager.Instance.GridWidth * GridManager.Instance.GridHeight * 10; // Prevent infinite loops
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
                GridManager.Instance.PlaceObject(position, step.placeablePrefab, true);

                if (makeAllLinesScorable)
                {
                    var placeableScoringLines = GridScoringManager.Instance.GetAllLinesForPosition(
                        position
                    );
                    bool areAllLinesScorable = true;
                    foreach (var line in placeableScoringLines)
                    {
                        if (!GridScoringManager.Instance.IsLineScorable(line))
                        {
                            areAllLinesScorable = false;
                            break;
                        }
                    }
                    if (!areAllLinesScorable)
                    {
                        GridManager.Instance.RemoveObject(position);
                        continue;
                    }
                }

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

        for (int x = 0; x < GridManager.Instance.GridWidth; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridHeight; y++)
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
        if (GridManager.Instance.Grid[x, y].Tile.TileType != TileType.Empty)
            return false;

        // If no adjacency requirements, any empty position is valid
        if (step.allowedAdjacentTileTypes == null || step.allowedAdjacentTileTypes.Count == 0)
            return true;

        // Check if position has at least one adjacent tile from the allowed list
        Vector2Int position = new Vector2Int(x, y);
        List<Vector2Int> adjacentPositions = GridManager.GetOrthogonalPositions(position);
        if (step.allowDiagonal)
        {
            adjacentPositions.AddRange(GridManager.GetDiagonalPositions(position));
        }

        foreach (Vector2Int adjPos in adjacentPositions)
        {
            if (
                GridManager.Instance.IsValidPosition(adjPos.x, adjPos.y)
                && GridManager.Instance.Grid[adjPos.x, adjPos.y] != null
            )
            {
                // Check if the adjacent tile is in the allowed list
                foreach (TileType allowedTileType in step.allowedAdjacentTileTypes)
                {
                    if (
                        allowedTileType
                        == GridManager.Instance.Grid[adjPos.x, adjPos.y].Tile.TileType
                    )
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private List<Vector2Int> GetValidPositionsForPlaceable(PlaceableGenerationStep step)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = 0; x < GridManager.Instance.GridWidth; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridHeight; y++)
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
        if (!GridManager.Instance.IsValidPosition(x, y))
            return false;

        // Position must have a tile
        if (GridManager.Instance.Grid[x, y] == null)
            return false;

        // Position must not already have a placeable
        if (GridManager.Instance.Grid[x, y].PlacedObject != null)
            return false;

        // Check if the tile is in the allowed tiles list
        if (step.allowedTileTypes != null && step.allowedTileTypes.Count > 0)
        {
            bool tileAllowed = false;
            foreach (TileType allowedTileType in step.allowedTileTypes)
            {
                if (allowedTileType == GridManager.Instance.Grid[x, y].Tile.TileType)
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
            for (int checkX = 0; checkX < GridManager.Instance.GridWidth; checkX++)
            {
                for (int checkY = 0; checkY < GridManager.Instance.GridHeight; checkY++)
                {
                    if (
                        GridManager.Instance.Grid[checkX, checkY] != null
                        && GridManager.Instance.Grid[checkX, checkY].PlacedObject != null
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
        for (int x = 0; x < GridManager.Instance.GridWidth; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridHeight; y++)
            {
                if (
                    GridManager.Instance.Grid[x, y] != null
                    && GridManager.Instance.Grid[x, y].Tile.TileType == tileType
                )
                    placedTiles++;
            }
        }
        return placedTiles;
    }
}
