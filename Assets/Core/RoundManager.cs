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

    private bool canMakeMove = false;
    public bool CanMakeMove => canMakeMove;

    [NonSerialized]
    public UnityEvent OnScoreChange = new();

    public void StartRound()
    {
        score = 0;
        numMovesUsed = 0;
        canMakeMove = false;

        GridManager.Instance.GenerateGrid(PlayerManager.Instance.Tiles);

        canMakeMove = true;

        Debug.Log($"New round started.");
    }

    public IEnumerator ScoreGrid()
    {
        var matches = GridManager.Instance.FindMatches();
        while (matches.Count > 0)
        {
            // TODO: score matches

            // TODO: flatten matches
            List<GridTile> allMatchingTiles = new List<GridTile>();

            yield return GridManager.Instance.RemoveTiles(
                allMatchingTiles,
                PlayerManager.Instance.Tiles
            );

            matches = GridManager.Instance.FindMatches();
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
}
