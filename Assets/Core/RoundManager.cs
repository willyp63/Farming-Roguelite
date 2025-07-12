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
    private int maxNumMoves = 3;
    public int MaxNumMoves => maxNumMoves;

    private int score = 0;
    public int Score => score;

    private int numMovesUsed = 0;
    public int NumMovesUsed => numMovesUsed;
    public int NumMovesLeft => maxNumMoves - numMovesUsed;

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
        numMovesUsed = 0;

        DeckManager.Instance.ShuffleDeck();
        BoardManager.Instance.GenerateBoard();

        // Start auto-hint functionality
        BoardManager.Instance.StartAutoHint();

        var bestSwap = BoardManager.Instance.GetBestSwap();
        Debug.Log($"Swap: {bestSwap[0].X}, {bestSwap[0].Y} -> {bestSwap[1].X}, {bestSwap[1].Y}");
    }

    public void OnTileSwapped()
    {
        // Reset auto-hint timer when a successful move is made
        BoardManager.Instance.ResetAutoHintTimer();

        StartCoroutine(ScoreBoard());
    }

    public IEnumerator ScoreBoard()
    {
        isScoring = true;

        var matches = BoardManager.Instance.FindMatches();
        while (matches.Count > 0)
        {
            // Score matches
            foreach (var match in matches)
            {
                int matchScore = 0;
                foreach (var tile in match)
                {
                    matchScore += tile.PointScore;
                }

                // Bonus for longer matches
                if (match.Count > 3)
                {
                    matchScore *= match.Count - 2; // 4 tiles = 2x, 5 tiles = 3x, etc.
                }

                AddScore(matchScore);
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
}
