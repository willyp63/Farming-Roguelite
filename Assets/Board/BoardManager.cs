using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoardManager : Singleton<BoardManager>
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

    [Header("Animation Settings")]
    [SerializeField]
    private float swapSpeed = 5f;

    [SerializeField]
    private float fallSpeed = 3f;

    [SerializeField]
    private float spawnSpeed = 2f;

    private BoardTile[,] board;

    private bool isSwapping = false;
    public bool IsSwapping => isSwapping;

    public int BoardWidth => boardWidth;
    public int BoardHeight => boardHeight;
    public int TotalNumTiles => boardWidth * boardHeight;
    public BoardTile[,] Board => board;

    [NonSerialized]
    public UnityEvent OnTileSwapped = new();

    public void GenerateBoard()
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

        // Create tiles by drawing from the deck, ensuring no initial matches
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                DeckTile drawnDeckTile = DrawTileAvoidingMatches(x, y);
                CreateTileAt(x, y, drawnDeckTile);
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

    private DeckTile DrawTileAvoidingMatches(int x, int y)
    {
        var deckTiles = DeckManager.Instance.DeckTiles;
        List<DeckTile> availableDeckTiles = new List<DeckTile>();

        // Check each tile in the deck to see if it would create a match
        for (int i = 0; i < deckTiles.Count; i++)
        {
            if (!WouldCreateMatch(x, y, deckTiles[i].tileData))
            {
                availableDeckTiles.Add(deckTiles[i]);
            }
        }

        // If no tiles available that avoid matches, use any tile
        if (availableDeckTiles.Count == 0)
        {
            availableDeckTiles = new List<DeckTile>(deckTiles);
        }

        // Draw the first available tile from the deck
        DeckTile drawnTile = availableDeckTiles[0];

        // Remove the drawn tile from the deck
        DeckManager.Instance.RemoveDeckTile(drawnTile);

        return drawnTile;
    }

    private bool WouldCreateMatch(int x, int y, TileData tileData)
    {
        // Check horizontal matches
        int horizontalCount = 1;

        // Check left
        for (int i = x - 1; i >= 0; i--)
        {
            if (board[i, y] != null && board[i, y].TileData.Season == tileData.Season)
                horizontalCount++;
            else
                break;
        }

        // Check right
        for (int i = x + 1; i < boardWidth; i++)
        {
            if (board[i, y] != null && board[i, y].TileData.Season == tileData.Season)
                horizontalCount++;
            else
                break;
        }

        if (horizontalCount >= 3)
            return true;

        // Check vertical matches
        int verticalCount = 1;

        // Check down
        for (int j = y - 1; j >= 0; j--)
        {
            if (board[x, j] != null && board[x, j].TileData.Season == tileData.Season)
                verticalCount++;
            else
                break;
        }

        // Check up
        for (int j = y + 1; j < boardHeight; j++)
        {
            if (board[x, j] != null && board[x, j].TileData.Season == tileData.Season)
                verticalCount++;
            else
                break;
        }

        return verticalCount >= 3;
    }

    private void CreateTileAt(int x, int y, DeckTile deckTile)
    {
        Vector3 worldPos = GetWorldPosition(x, y);
        BoardTile newTile = Instantiate(
            boardTilePrefab,
            worldPos,
            Quaternion.identity,
            boardContainer
        );
        newTile.Initialize(x, y, deckTile);
        board[x, y] = newTile;
    }

    private IEnumerator SwapTiles(BoardTile tileA, BoardTile tileB, bool animate = true)
    {
        if (tileA == null || tileB == null)
            yield break;

        // Get positions
        Vector3 posA = tileA.transform.position;
        Vector3 posB = tileB.transform.position;

        // Swap in board array
        int tempX = tileA.X;
        int tempY = tileA.Y;
        tileA.SetPosition(tileB.X, tileB.Y);
        tileB.SetPosition(tempX, tempY);

        board[tileA.X, tileA.Y] = tileA;
        board[tileB.X, tileB.Y] = tileB;

        if (animate)
        {
            // Animate the swap
            float elapsed = 0f;
            float duration = Vector3.Distance(posA, posB) / swapSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                tileA.transform.position = Vector3.Lerp(posA, posB, t);
                tileB.transform.position = Vector3.Lerp(posB, posA, t);

                yield return null;
            }
        }

        // Set final positions
        tileA.transform.position = posB;
        tileB.transform.position = posA;
    }

    public List<List<BoardTile>> FindMatches()
    {
        List<List<BoardTile>> allMatches = new List<List<BoardTile>>();

        // Find horizontal matches
        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth - 2; x++)
            {
                if (board[x, y] != null && board[x + 1, y] != null && board[x + 2, y] != null)
                {
                    SeasonType season = board[x, y].TileData.Season;
                    if (
                        board[x + 1, y].TileData.Season == season
                        && board[x + 2, y].TileData.Season == season
                    )
                    {
                        List<BoardTile> match = new List<BoardTile>();
                        match.Add(board[x, y]);
                        match.Add(board[x + 1, y]);
                        match.Add(board[x + 2, y]);

                        // Check for longer matches
                        for (int i = x + 3; i < boardWidth; i++)
                        {
                            if (board[i, y] != null && board[i, y].TileData.Season == season)
                            {
                                match.Add(board[i, y]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        allMatches.Add(match);
                    }
                }
            }
        }

        // Find vertical matches
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight - 2; y++)
            {
                if (board[x, y] != null && board[x, y + 1] != null && board[x, y + 2] != null)
                {
                    SeasonType season = board[x, y].TileData.Season;
                    if (
                        board[x, y + 1].TileData.Season == season
                        && board[x, y + 2].TileData.Season == season
                    )
                    {
                        List<BoardTile> match = new List<BoardTile>();
                        match.Add(board[x, y]);
                        match.Add(board[x, y + 1]);
                        match.Add(board[x, y + 2]);

                        // Check for longer matches
                        for (int j = y + 3; j < boardHeight; j++)
                        {
                            if (board[x, j] != null && board[x, j].TileData.Season == season)
                            {
                                match.Add(board[x, j]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        allMatches.Add(match);
                    }
                }
            }
        }

        return allMatches;
    }

    public IEnumerator RemoveTiles(List<BoardTile> tilesToRemove)
    {
        // Animate removal of tiles
        List<Coroutine> removeTilesCoroutines = new List<Coroutine>();
        foreach (var tile in tilesToRemove)
        {
            removeTilesCoroutines.Add(StartCoroutine(AnimateTileRemoval(tile)));
        }
        foreach (var coroutine in removeTilesCoroutines)
        {
            yield return coroutine;
        }

        // Destroy tiles and store the DeckTiles to be returned to the deck
        List<DeckTile> scoredTiles = new List<DeckTile>();
        foreach (var tile in tilesToRemove)
        {
            scoredTiles.Add(tile.DeckTile);
            board[tile.X, tile.Y] = null;
            Destroy(tile.gameObject);
        }

        // Apply gravity and fill empty spaces
        yield return StartCoroutine(ApplyGravity());
        yield return StartCoroutine(FillEmptySpaces());

        // Return tiles to the deck and shuffle
        foreach (var tile in scoredTiles)
        {
            DeckManager.Instance.AddDeckTile(tile);
        }
        DeckManager.Instance.ShuffleDeck();
    }

    public void TrySwapTiles(BoardTile tileA, BoardTile tileB)
    {
        StartCoroutine(TrySwapTilesCoroutine(tileA, tileB));
    }

    private IEnumerator TrySwapTilesCoroutine(BoardTile tileA, BoardTile tileB)
    {
        if (tileA == null || tileB == null)
            yield break;

        // Check if tiles are adjacent
        int deltaX = Mathf.Abs(tileA.X - tileB.X);
        int deltaY = Mathf.Abs(tileA.Y - tileB.Y);

        if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
        {
            isSwapping = true;

            // Swap tiles
            yield return StartCoroutine(SwapTiles(tileA, tileB));

            // Check for matches
            var matches = FindMatches();

            if (matches.Count > 0)
            {
                isSwapping = false;
                OnTileSwapped?.Invoke();
            }
            else
            {
                // Invalid move - swap back
                yield return StartCoroutine(SwapTiles(tileA, tileB));
                isSwapping = false;
            }
        }
    }

    private IEnumerator ApplyGravity()
    {
        List<Coroutine> fallCoroutines = new List<Coroutine>();

        // Process each column independently
        for (int x = 0; x < boardWidth; x++)
        {
            // Find the bottommost empty position in this column
            int bottomEmptyY = -1;
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] == null)
                {
                    bottomEmptyY = y;
                    break;
                }
            }

            // If there are no empty spaces in this column, skip it
            if (bottomEmptyY == -1)
                continue;

            // Move all tiles above the bottom empty position down
            for (int y = bottomEmptyY + 1; y < boardHeight; y++)
            {
                if (board[x, y] != null)
                {
                    // Calculate the new position for this tile
                    int newY = bottomEmptyY;

                    // Update the board array
                    BoardTile tile = board[x, y];
                    board[x, y] = null;
                    board[x, newY] = tile;
                    tile.SetPosition(x, newY);

                    // Start the animation coroutine
                    Vector3 targetPos = GetWorldPosition(x, newY);
                    fallCoroutines.Add(
                        StartCoroutine(MoveTileToPosition(tile, targetPos, fallSpeed))
                    );

                    // Move the bottom empty position up
                    bottomEmptyY++;
                }
            }
        }

        // Wait for all fall animations to complete
        foreach (var coroutine in fallCoroutines)
        {
            yield return coroutine;
        }
    }

    private IEnumerator FillEmptySpaces()
    {
        int shortestColumnHeight = boardHeight;
        for (int x = 0; x < boardWidth; x++)
        {
            int columnHeight = 0;
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] != null)
                {
                    columnHeight++;
                }
            }
            shortestColumnHeight = Mathf.Min(shortestColumnHeight, columnHeight);
        }

        List<Coroutine> moveTilesCoroutines = new List<Coroutine>();

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] == null)
                {
                    // Draw a new tile from the deck
                    DeckTile newDeckTile = DeckManager.Instance.DrawTileFromTop();
                    if (newDeckTile != null)
                    {
                        CreateTileAt(x, y, newDeckTile);

                        // Animate the spawn
                        int spawnY = boardHeight + y + 1 - shortestColumnHeight;
                        Vector3 spawnPos = GetWorldPosition(x, spawnY);
                        Vector3 targetPos = GetWorldPosition(x, y);
                        board[x, y].transform.position = spawnPos;
                        moveTilesCoroutines.Add(
                            StartCoroutine(MoveTileToPosition(board[x, y], targetPos, spawnSpeed))
                        );
                    }
                }
            }
        }

        foreach (var coroutine in moveTilesCoroutines)
        {
            yield return coroutine;
        }
    }

    private IEnumerator MoveTileToPosition(BoardTile tile, Vector3 targetPosition, float speed)
    {
        Vector3 startPosition = tile.transform.position;
        float elapsed = 0f;
        float duration = Vector3.Distance(startPosition, targetPosition) / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            tile.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        tile.transform.position = targetPosition;
    }

    private IEnumerator AnimateTileRemoval(BoardTile tile)
    {
        if (tile == null)
            yield break;

        tile.BringToFront();

        // Store original scale, position and get renderers
        Vector3 originalScale = tile.transform.localScale;
        SpriteRenderer[] renderers = tile.GetComponentsInChildren<SpriteRenderer>();
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Calculate scale: grow to 1.1x in first half, shrink to 0.1x in second half
            float scaleMultiplier;
            if (progress <= 0.5f)
            {
                // First half: grow from 1.0 to 1.2
                float growProgress = progress * 2f; // 0 to 1 over first half
                scaleMultiplier = Mathf.Lerp(1f, 1.2f, growProgress);
            }
            else
            {
                // Second half: shrink from 1.2 to 0.1
                float shrinkProgress = (progress - 0.5f) * 2f; // 0 to 1 over second half
                scaleMultiplier = Mathf.Lerp(1.2f, 0.1f, shrinkProgress);
            }

            // Apply scale
            tile.transform.localScale = originalScale * scaleMultiplier;

            // Fade out animation - only start fading halfway through
            float alpha = 1f;
            if (progress > 0.5f)
            {
                // Start fading from 0.5 to 1.0 progress (second half of animation)
                float fadeProgress = (progress - 0.5f) * 2f; // 0 to 1 over second half
                alpha = 1f - fadeProgress;
            }

            foreach (var renderer in renderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }

            yield return null;
        }
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
