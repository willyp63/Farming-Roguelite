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
        var fullDeck = DeckManager.Instance.GetFullDeck();
        if (fullDeck.Count < TotalNumTiles)
        {
            Debug.LogError(
                $"Can not generate a {BoardWidth}x{BoardHeight} board with {fullDeck.Count} tiles! (needs {TotalNumTiles})"
            );
            return;
        }

        // Clear existing board
        ClearBoard();

        // Initialize board array
        board = new BoardTile[boardWidth, boardHeight];

        // Create tiles by drawing from the deck in order
        int index = 0;
        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                DeckTile deckTile = fullDeck[index];
                index++;

                bool isInCurrentDeck = DeckManager.Instance.CurrentDeckTiles.Contains(deckTile);

                CreateTileAt(x, y, deckTile, isInCurrentDeck);
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

    private void CreateTileAt(int x, int y, DeckTile deckTile, bool isInCurrentDeck)
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

        // Apply gray color to all sprite renderers if tile is not in current deck
        if (!isInCurrentDeck)
        {
            newTile.SetInactive(true);
        }
    }

    public Vector3 GetWorldPosition(int boardXPos, int boardYPos)
    {
        float boardWidthSize = boardWidth * tileSize;
        float boardHeightSize = boardHeight * tileSize;
        return new Vector3(
            boardXPos * tileSize - boardWidthSize / 2 + tileSize / 2,
            boardYPos * tileSize - boardHeightSize / 2 + tileSize / 2,
            0f
        );
    }
}
