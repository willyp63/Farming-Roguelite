using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Hint Animation Settings")]
    [SerializeField]
    private float hintPulseSpeed = 2f;

    [SerializeField]
    private float hintPulseIntensity = 0.3f;

    [SerializeField]
    private float hintDuration = 3f;

    [SerializeField]
    private float autoHintDelay = 5f; // Time before showing hint automatically

    [SerializeField]
    private bool enableAutoHint = true;

    private Coroutine currentHintCoroutine;
    private Coroutine autoHintCoroutine;

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
            if (!WouldCreateMatch(x, y, deckTiles[i].Season))
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

    private bool WouldCreateMatch(int x, int y, SeasonType season)
    {
        // Check horizontal matches
        int horizontalCount = 1;

        // Check left
        for (int i = x - 1; i >= 0; i--)
        {
            if (board[i, y] != null && board[i, y].Season == season)
                horizontalCount++;
            else
                break;
        }

        // Check right
        for (int i = x + 1; i < boardWidth; i++)
        {
            if (board[i, y] != null && board[i, y].Season == season)
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
            if (board[x, j] != null && board[x, j].Season == season)
                verticalCount++;
            else
                break;
        }

        // Check up
        for (int j = y + 1; j < boardHeight; j++)
        {
            if (board[x, j] != null && board[x, j].Season == season)
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
            int x = 0;
            while (x < boardWidth - 2) // Need at least 3 tiles to form a match
            {
                // Find the start of a potential match
                if (board[x, y] != null)
                {
                    SeasonType season = board[x, y].Season;
                    int matchLength = 1;

                    // Count how many consecutive tiles of the same season
                    for (int i = x + 1; i < boardWidth; i++)
                    {
                        if (board[i, y] != null && board[i, y].Season == season)
                        {
                            matchLength++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // If we found 3 or more matching tiles, add the match
                    if (matchLength >= 3)
                    {
                        List<BoardTile> match = new List<BoardTile>();
                        for (int i = 0; i < matchLength; i++)
                        {
                            match.Add(board[x + i, y]);
                        }
                        allMatches.Add(match);

                        // Skip past this match to avoid overlapping
                        x += matchLength;
                    }
                    else
                    {
                        x++;
                    }
                }
                else
                {
                    x++;
                }
            }
        }

        // Find vertical matches
        for (int x = 0; x < boardWidth; x++)
        {
            int y = 0;
            while (y < boardHeight - 2) // Need at least 3 tiles to form a match
            {
                // Find the start of a potential match
                if (board[x, y] != null)
                {
                    SeasonType season = board[x, y].Season;
                    int matchLength = 1;

                    // Count how many consecutive tiles of the same season
                    for (int j = y + 1; j < boardHeight; j++)
                    {
                        if (board[x, j] != null && board[x, j].Season == season)
                        {
                            matchLength++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // If we found 3 or more matching tiles, add the match
                    if (matchLength >= 3)
                    {
                        List<BoardTile> match = new List<BoardTile>();
                        for (int j = 0; j < matchLength; j++)
                        {
                            match.Add(board[x, y + j]);
                        }
                        allMatches.Add(match);

                        // Skip past this match to avoid overlapping
                        y += matchLength;
                    }
                    else
                    {
                        y++;
                    }
                }
                else
                {
                    y++;
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
            // Stop any current hint animation when player makes a move
            StopHintAnimation();

            // Reset auto-hint timer
            ResetAutoHintTimer();

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
                        int spawnY = boardHeight + y - shortestColumnHeight;
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

    public void ShowBestSwap()
    {
        // Stop any existing hint animation
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
        }

        var bestSwap = GetBestSwap();
        if (bestSwap.Count == 2)
        {
            currentHintCoroutine = StartCoroutine(AnimateHint(bestSwap[0], bestSwap[1]));
        }
    }

    // Public method for testing hints manually
    public void ShowHintNow()
    {
        ShowBestSwap();
    }

    public List<BoardTile> GetBestSwap()
    {
        List<(BoardTile, BoardTile, int)> validSwaps = new List<(BoardTile, BoardTile, int)>();
        HashSet<string> swapKeys = new HashSet<string>();

        // Check every tile on the board
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] == null)
                    continue;

                BoardTile currentTile = board[x, y];

                // Check all 4 adjacent positions
                CheckAdjacentSwap(currentTile, x + 1, y, validSwaps, swapKeys);
                CheckAdjacentSwap(currentTile, x - 1, y, validSwaps, swapKeys);
                CheckAdjacentSwap(currentTile, x, y + 1, validSwaps, swapKeys);
                CheckAdjacentSwap(currentTile, x, y - 1, validSwaps, swapKeys);
            }
        }

        if (validSwaps.Count == 0)
            return new List<BoardTile>();

        var bestSwap = validSwaps.OrderByDescending(swap => swap.Item3).First();
        return new List<BoardTile> { bestSwap.Item1, bestSwap.Item2 };
    }

    private void CheckAdjacentSwap(
        BoardTile tileA,
        int x,
        int y,
        List<(BoardTile, BoardTile, int)> validSwaps,
        HashSet<string> swapKeys
    )
    {
        // Check bounds
        if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight)
            return;

        BoardTile tileB = board[x, y];
        if (tileB == null)
            return;

        // Create a unique key for this swap pair to avoid duplicates
        string swapKey = GetSwapKey(tileA, tileB);
        if (swapKeys.Contains(swapKey))
            return;
        swapKeys.Add(swapKey);

        // Temporarily swap the tiles in the board array
        board[tileA.X, tileA.Y] = tileB;
        board[tileB.X, tileB.Y] = tileA;

        // Check if this swap creates any matches
        var matches = FindMatches();

        // Swap back
        board[tileA.X, tileA.Y] = tileA;
        board[tileB.X, tileB.Y] = tileB;

        // If matches were found, add this swap to the valid swaps
        if (matches.Count > 0)
        {
            validSwaps.Add((tileA, tileB, matches.Sum(match => match.Count)));
        }
    }

    private string GetSwapKey(BoardTile tileA, BoardTile tileB)
    {
        // Create a consistent key regardless of the order of tiles
        // Use the smaller coordinates first to ensure consistency
        if (tileA.X < tileB.X || (tileA.X == tileB.X && tileA.Y < tileB.Y))
        {
            return $"{tileA.X},{tileA.Y}-{tileB.X},{tileB.Y}";
        }
        else
        {
            return $"{tileB.X},{tileB.Y}-{tileA.X},{tileA.Y}";
        }
    }

    private IEnumerator AnimateHint(BoardTile tileA, BoardTile tileB)
    {
        if (tileA == null || tileB == null)
            yield break;

        // Store original scales
        Vector3 originalScaleA = tileA.transform.localScale;
        Vector3 originalScaleB = tileB.transform.localScale;

        float elapsed = 0f;
        float duration = hintDuration;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            // Create a pulsing effect using sine wave
            float pulseValue = Mathf.Sin(progress * Mathf.PI * 2 * hintPulseSpeed) * 0.5f + 0.5f;

            // Scale effect: pulse between 1.0 and 1.0 + intensity
            float scaleMultiplier = 1f + (pulseValue * hintPulseIntensity);

            // Apply scale
            tileA.transform.localScale = originalScaleA * scaleMultiplier;
            tileB.transform.localScale = originalScaleB * scaleMultiplier;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to original state
        tileA.transform.localScale = originalScaleA;
        tileB.transform.localScale = originalScaleB;

        currentHintCoroutine = null;
    }

    public void EnableTooltips()
    {
        foreach (var tile in board)
        {
            tile.SetTooltipEnabled(true);
        }
    }

    public void DisableTooltips()
    {
        foreach (var tile in board)
        {
            tile.SetTooltipEnabled(false);
        }
    }

    public void StopHintAnimation()
    {
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
            currentHintCoroutine = null;
        }
    }

    public void StartAutoHint()
    {
        if (enableAutoHint && autoHintCoroutine == null)
        {
            autoHintCoroutine = StartCoroutine(AutoHintCoroutine());
        }
    }

    public void StopAutoHint()
    {
        if (autoHintCoroutine != null)
        {
            StopCoroutine(autoHintCoroutine);
            autoHintCoroutine = null;
        }
    }

    public void ResetAutoHintTimer()
    {
        // Stop current auto-hint and restart it
        StopAutoHint();
        StartAutoHint();
    }

    private IEnumerator AutoHintCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoHintDelay);

            // Only show hint if no swap is currently happening and the game allows moves
            if (!isSwapping && RoundManager.Instance.CanMakeMove)
            {
                ShowBestSwap();
            }
        }
    }
}
