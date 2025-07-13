using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeckBoardManager : Singleton<DeckBoardManager>
{
    [Header("Board Settings")]
    [SerializeField]
    private Transform boardContainer;

    [SerializeField]
    private int boardWidth = 6;

    [SerializeField]
    private int boardHeight = 6;

    [SerializeField]
    private float tileSize = 1f;

    [SerializeField]
    private float padding = 0.125f;

    [SerializeField]
    private BoardTile boardTilePrefab;

    private BoardTile[,] board;

    private bool isSelectionMode = false;
    private UnitData selectedUnitData = null;

    [System.NonSerialized]
    public UnityEvent<BoardTile> onTileSelected = new();

    public int BoardWidth => boardWidth;
    public int BoardHeight => boardHeight;
    public int TotalNumTiles => boardWidth * boardHeight;
    public BoardTile[,] Board => board;
    public bool IsSelectionMode => isSelectionMode;

    public void ShowDeck()
    {
        GenerateBoard();

        boardContainer.gameObject.SetActive(true);
    }

    public void HideDeck()
    {
        boardContainer.gameObject.SetActive(false);

        // Exit selection mode when hiding deck
        ExitSelectionMode();
    }

    public void EnterSelectionMode(UnitData unitData)
    {
        isSelectionMode = true;
        selectedUnitData = unitData;

        // Enable click handling on all tiles
        if (board != null)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board[x, y] != null)
                    {
                        board[x, y].SetClickEnabled(true);
                    }
                }
            }
        }
    }

    public void ExitSelectionMode()
    {
        isSelectionMode = false;
        selectedUnitData = null;

        // Disable click handling on all tiles
        if (board != null)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board[x, y] != null)
                    {
                        board[x, y].SetClickEnabled(false);
                    }
                }
            }
        }
    }

    public void OnTileClicked(BoardTile tile)
    {
        if (!isSelectionMode || selectedUnitData == null)
            return;

        // Only allow placing units on empty tiles
        if (tile.DeckTile.IsEmpty)
        {
            StartCoroutine(PlaceUnitOnTile(tile));
        }
    }

    private IEnumerator PlaceUnitOnTile(BoardTile tile)
    {
        // Add the unit to the tile
        tile.DeckTile.SetUnit(selectedUnitData);
        tile.UpdateVisual();

        // Exit selection mode
        ExitSelectionMode();

        yield return new WaitForSeconds(0.5f);

        // Notify that a tile was selected
        onTileSelected?.Invoke(tile);
    }

    private void GenerateBoard()
    {
        if (DeckManager.Instance.DeckTiles.Count < TotalNumTiles)
        {
            Debug.LogError(
                $"Can not generate a {BoardWidth}x{BoardHeight} board with {DeckManager.Instance.DeckTiles.Count} tiles! (needs {TotalNumTiles})"
            );
            return;
        }

        // Clear existing board
        ClearBoard();

        // Initialize board array
        board = new BoardTile[boardWidth, boardHeight];

        // Create tiles by drawing from the deck in order
        int index = 0;
        for (int x = 0; x < boardWidth; x += 2)
        {
            for (int y = boardHeight - 1; y >= 0; y--)
            {
                for (int z = 0; z < 2; z++)
                {
                    DeckTile deckTile = DeckManager.Instance.DeckTiles[index];
                    index++;

                    CreateTileAt(x + z, y, deckTile);
                }
            }
        }
    }

    private void ClearBoard()
    {
        if (board != null)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board[x, y] != null)
                    {
                        DestroyImmediate(board[x, y].gameObject);
                    }
                }
            }
        }

        // Clear any remaining children in board container
        if (boardContainer != null)
        {
            for (int i = boardContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(boardContainer.GetChild(i).gameObject);
            }
        }
    }

    private void CreateTileAt(int x, int y, DeckTile deckTile)
    {
        Vector3 worldPos = GetWorldPosition(x, y);
        BoardTile newTile = Instantiate(
            boardTilePrefab,
            Vector3.zero,
            Quaternion.identity,
            boardContainer
        );
        newTile.transform.localPosition = worldPos;
        newTile.Initialize(x, y, deckTile, false);
        board[x, y] = newTile;
    }

    public Vector3 GetWorldPosition(int boardXPos, int boardYPos)
    {
        float boardWidthSize = boardWidth * (tileSize + padding);
        float boardHeightSize = boardHeight * (tileSize + padding);
        return new Vector3(
            boardXPos * (tileSize + padding) - boardWidthSize / 2 + tileSize / 2 + padding / 2,
            boardYPos * (tileSize + padding) - boardHeightSize / 2 + tileSize / 2 + padding / 2,
            0f
        );
    }
}
