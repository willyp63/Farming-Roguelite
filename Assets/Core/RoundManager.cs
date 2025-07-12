using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : Singleton<RoundManager>
{
    [Header("Game Settings")]
    [SerializeField]
    private int requiredScore = 100;
    public int RequiredScore => requiredScore;

    [SerializeField]
    private bool isBotPlaying = false;
    public bool IsBotPlaying => isBotPlaying;

    [Header("Bot Settings")]
    [SerializeField]
    private float botMoveDelay = 1f; // Delay between bot moves in seconds

    private int numMoves = 0;
    public int NumMoves => numMoves;

    private int score = 0;
    public int Score => score;

    private bool isScoring = false;
    public bool IsScoring => isScoring;

    public bool CanMakeMove => !isScoring && !BoardManager.Instance.IsSwapping;

    [NonSerialized]
    public UnityEvent OnScoreChange = new();

    private void Start()
    {
        BoardManager.Instance.OnTileSwapped.AddListener(OnTileSwapped);
    }

    public void StartRound()
    {
        score = 0;

        EnergyManager.Instance.ResetAllEnergy();
        DeckManager.Instance.ShuffleDeck();
        BoardManager.Instance.GenerateBoard();

        // Start auto-hint functionality (only if bot is not playing)
        if (!isBotPlaying)
        {
            BoardManager.Instance.StartAutoHint();
        }
        else
        {
            // Stop auto-hint when bot is playing
            BoardManager.Instance.StopAutoHint();
            // Start bot gameplay
            MakeBotMove();
        }

        var bestSwap = BoardManager.Instance.GetBestSwap();
        Debug.Log($"Swap: {bestSwap[0].X}, {bestSwap[0].Y} -> {bestSwap[1].X}, {bestSwap[1].Y}");
    }

    public void OnTileSwapped()
    {
        // Reset auto-hint timer when a successful move is made (only if bot is not playing)
        if (!isBotPlaying)
        {
            BoardManager.Instance.ResetAutoHintTimer();
        }

        StartCoroutine(ScoreBoard());
    }

    public IEnumerator ScoreBoard()
    {
        isScoring = true;
        numMoves++;

        var matches = BoardManager.Instance.FindMatches();
        while (matches.Count > 0)
        {
            // Score matches
            foreach (var match in matches)
            {
                int matchScore = 0;
                SeasonType matchSeason = SeasonType.None;

                foreach (var tile in match)
                {
                    matchScore += tile.PointScore;
                    // All tiles in a match should have the same season, so we can use the first one
                    if (matchSeason == SeasonType.None)
                    {
                        matchSeason = tile.Season;
                    }
                }

                // Bonus for longer matches
                if (match.Count > 3)
                {
                    matchScore *= match.Count - 2; // 4 tiles = 2x, 5 tiles = 3x, etc.
                }

                AddScore(matchScore);

                // Add energy for the match (1 energy per tile)
                if (matchSeason != SeasonType.None)
                {
                    EnergyManager.Instance.AddEnergy(matchSeason, match.Count);
                    Debug.Log(
                        $"Match of {match.Count} {matchSeason} tiles scored! Added {match.Count} energy to {matchSeason}."
                    );
                    EnergyManager.Instance.LogEnergyLevels();
                }
            }

            // Flatten matches into single list
            List<BoardTile> allMatchingTiles = new List<BoardTile>();
            foreach (var match in matches)
            {
                allMatchingTiles.AddRange(match);
            }

            yield return StartCoroutine(BoardManager.Instance.RemoveTiles(allMatchingTiles));

            matches = BoardManager.Instance.FindMatches();
        }

        isScoring = false;

        // If bot is playing, continue with next move after scoring is complete
        if (isBotPlaying)
        {
            yield return new WaitForSeconds(botMoveDelay);
            MakeBotMove();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChange?.Invoke();
    }

    public void SetScore(int amount)
    {
        score = amount;
        OnScoreChange?.Invoke();
    }

    private void MakeBotMove()
    {
        if (!isBotPlaying || !CanMakeMove)
            return;

        try
        {
            var bestSwap = BoardManager.Instance.GetBestSwap();

            if (bestSwap.Count == 2)
            {
                Debug.Log(
                    $"Bot making move: {bestSwap[0].X}, {bestSwap[0].Y} -> {bestSwap[1].X}, {bestSwap[1].Y}"
                );
                BoardManager.Instance.TrySwapTiles(bestSwap[0], bestSwap[1]);
            }
            else
            {
                Debug.Log("Bot found no valid moves!");
                Debug.Log($"Bot has made {numMoves} moves.");
                // Could implement game over logic here
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in bot move: {e.Message}");
        }
    }
}
