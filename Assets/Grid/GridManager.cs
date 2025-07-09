using System;
using System.Collections;
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

        foreach (Transform child in gridContainer.transform)
        {
            Destroy(child.gameObject);
        }

        grid = new GridTile[gridWidth, gridHeight];
        TileInfo emptyTile = TileManager.GetTileInfo(TileType.Empty);
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
        ClearScoredTiles(true);

        TileInfo emptyTile = TileManager.GetTileInfo(TileType.Empty);
        foreach (GridTile tile in grid)
        {
            tile.SetTile(emptyTile);
        }
    }

    public IEnumerator EndOfTurnEnumerator()
    {
        foreach (GridTile tile in grid)
        {
            if (tile.PlacedObject != null)
            {
                tile.PlacedObject.OnEndOfTurn();
            }
        }

        yield return new WaitForSeconds(0.66f);

        List<ScoringLine> scoringLines = GridScoringManager.Instance.GetScoringLines();
        foreach (ScoringLine scoringLine in scoringLines)
        {
            RoundManager.Instance.AddPoints(10 * scoringLine.tiles.Count);
            RoundManager.Instance.AddMulti(1 * scoringLine.tiles.Count);

            foreach (GridTile tile in scoringLine.tiles)
            {
                tile.Shake(0.33f, 0.05f);
            }

            yield return new WaitForSeconds(0.66f);

            foreach (GridTile tile in scoringLine.tiles)
            {
                if (tile.PlacedObject != null)
                {
                    tile.PlacedObject.OnTriggered();
                    tile.PlacedObject.MarkAsScored();

                    RoundManager.Instance.AddPoints(tile.PlacedObject.PointScore);
                    RoundManager.Instance.AddMulti(tile.PlacedObject.MultiScore);

                    FloatingTextManager.Instance.SpawnPointsText(
                        tile.PlacedObject.PointScore,
                        tile.transform.position
                    );
                    FloatingTextManager.Instance.SpawnMultiText(
                        tile.PlacedObject.MultiScore,
                        tile.transform.position
                    );
                }

                tile.Shake(0.33f, 0.05f);

                yield return new WaitForSeconds(0.66f);
            }

            RoundManager.Instance.CommitScore();

            yield return new WaitForSeconds(0.33f);
        }

        ClearScoredTiles();

        foreach (GridTile tile in grid)
        {
            if (tile.PlacedObject != null)
            {
                tile.PlacedObject.ResetMovementFlag();
            }
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

    public Placeable GetPlaceableAtPosition(Vector2Int position)
    {
        GridTile tile = GetTile(position);
        return tile?.PlacedObject;
    }

    public void PlaceObject(Vector2Int position, Placeable placeablePrefab)
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

        placeable.Initialize(tile);
        tile.SetPlacedObject(placeable);
        placeable.transform.localPosition = Vector3.zero;

        OnGridChanged?.Invoke();
    }

    public void RemoveObject(Vector2Int position)
    {
        if (!IsValidPosition(position.x, position.y))
            return;

        GridTile tile = grid[position.x, position.y];
        if (tile.PlacedObject == null)
            return;

        Destroy(tile.PlacedObject.gameObject);
        tile.ClearPlacedObject();
        OnGridChanged?.Invoke();
    }

    public bool MovePlaceable(Vector2Int fromPosition, Vector2Int toPosition)
    {
        if (
            !IsValidPosition(fromPosition.x, fromPosition.y)
            || !IsValidPosition(toPosition.x, toPosition.y)
        )
            return false;

        GridTile fromTile = grid[fromPosition.x, fromPosition.y];
        GridTile toTile = grid[toPosition.x, toPosition.y];

        // Check if source tile has a placeable
        if (fromTile.PlacedObject == null)
            return false;

        Placeable placeable = fromTile.PlacedObject;

        // Check if placeable is movable
        if (!placeable.IsMovable)
            return false;

        // Check if placeable has already moved today
        if (placeable.HasMovedToday)
            return false;

        // Check if destination tile is empty
        if (toTile.PlacedObject != null)
            return false;

        // Check if destination tile type is allowed
        if (
            placeable.AllowedTileTypes.Count > 0
            && !placeable.AllowedTileTypes.Contains(toTile.Tile.TileType)
        )
            return false;

        // Move the placeable
        fromTile.ClearPlacedObject();
        placeable.Initialize(toTile);
        toTile.SetPlacedObject(placeable);
        placeable.transform.SetParent(toTile.transform);
        placeable.transform.localPosition = Vector3.zero;

        // Mark as moved for today
        placeable.MarkAsMoved();

        OnGridChanged?.Invoke();
        return true;
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

    private void CreateGridTile(int x, int y, TileInfo tile)
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

    public void ClearScoredTiles(bool clearPermanents = false)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridTile tile = grid[x, y];

                if (
                    tile.PlacedObject != null
                    && tile.PlacedObject.HasBeenScored
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
