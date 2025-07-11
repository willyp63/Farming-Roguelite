using System;
using System.Collections;
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
    private GridTile gridTilePrefab;

    private GridTile[,] grid;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public int TotalNumTiles => gridWidth * gridHeight;
    public GridTile[,] Grid => grid;

    [NonSerialized]
    public UnityEvent OnGridChanged = new();

    public void GenerateGrid(List<GridTileData> tiles)
    {
        if (tiles.Count < TotalNumTiles)
        {
            Debug.LogError(
                $"Can not generate a {GridWidth}x{GridHeight} grid with {tiles.Count} tiles! (needs {TotalNumTiles})"
            );
            return;
        }

        // TODO: destroy all GridTiles
        // TODO: fill the grid with a random selection of TotalNumTiles from tiles
    }

    public void SwapTiles(GridTile tileA, GridTile tileB)
    {
        // TODO: swap the tiles
    }

    public List<List<GridTile>> FindMatches()
    {
        // TODO: look for all sequences of 3+ tiles in a row/column with the same SeasonType
        return new List<List<GridTile>>();
    }

    public IEnumerator RemoveTiles(List<GridTile> tilesToRemove, List<GridTileData> newTiles)
    {
        // TODO: destroy all GridTiles in tilesToRemove (no animation for now)
        // TODO: shift all tiles down so that all the missing spaces are at the top of each column (animate them moving)
        // TODO: fill the missing spaces with newTiles. Choose tiles in order from the list, but fill the spaces in a random order (new tiles should animate in from the top of the screen)

        yield return new WaitForEndOfFrame();
    }

    public Vector3 GetWorldPosition(int gridXPos, int gridYPos)
    {
        float gridWidthSize = gridWidth * (tileSize + padding);
        float gridHeightSize = gridHeight * (tileSize + padding);
        return new Vector3(
            gridXPos * (tileSize + padding) - gridWidthSize / 2 + tileSize / 2 + padding / 2,
            gridYPos * (tileSize + padding) - gridHeightSize / 2 + tileSize / 2 + padding / 2,
            0f
        );
    }
}
