using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : Singleton<RoundManager>
{
    [Header("Game Settings")]
    [SerializeField]
    private int requiredScore = 100;

    [SerializeField]
    private int numberOfDays = 3;

    [NonSerialized]
    public UnityEvent<int, int> OnScoreChange = new();

    [NonSerialized]
    public UnityEvent<int> OnDayChange = new();

    // Game state
    private int pointScore;
    private int multiScore;
    private int currentDay;

    private bool canMakeMove = false;
    public bool CanMakeMove => canMakeMove;

    public int PointScore => pointScore;
    public int MultiScore => multiScore;
    public int TotalScore => pointScore * multiScore;
    public int RequiredScore => requiredScore;
    public int CurrentDay => currentDay;
    public int TotalDays => numberOfDays;

    public void StartRound()
    {
        pointScore = 0;
        multiScore = 0;

        canMakeMove = false;
        currentDay = 1;

        // Clear grid and generate a new one
        GridGenerationManager.Instance.GenerateGrid();
        GridManager.Instance.CreateGridBackup();

        canMakeMove = true;

        OnScoreChange?.Invoke(pointScore, multiScore);
        OnDayChange?.Invoke(currentDay);

        Debug.Log($"New round started.");
    }

    public void GoToNextDay()
    {
        StartCoroutine(NextDayEnumerator());
    }

    private IEnumerator NextDayEnumerator()
    {
        canMakeMove = false;

        // Trigger end of day effects and clear non-permanent placeables
        yield return GridManager.Instance.EndOfTurnEnumerator();

        // TODO: Check if round is complete
        // TODO: Check if round is failed
        if (currentDay >= numberOfDays)
        {
            yield break;
        }

        yield return GridManager.Instance.StartOfTurnEnumerator();

        GridManager.Instance.CreateGridBackup();

        canMakeMove = true;

        // Increment day and trigger event
        currentDay++;
        OnDayChange?.Invoke(currentDay);
    }

    public void ResetRound()
    {
        // TODO
    }

    public void SetPoints(int amount)
    {
        pointScore = amount;
        OnScoreChange?.Invoke(pointScore, multiScore);
    }

    public void AddPoints(int amount, float multiplier = 1)
    {
        pointScore += amount;
        pointScore = (int)(pointScore * multiplier);

        OnScoreChange?.Invoke(pointScore, multiScore);
    }

    public void SetMulti(int amount)
    {
        multiScore = amount;
        OnScoreChange?.Invoke(pointScore, multiScore);
    }

    public void AddMulti(int amount, float multiplier = 1)
    {
        multiScore += amount;
        multiScore = (int)(multiScore * multiplier);

        OnScoreChange?.Invoke(pointScore, multiScore);
    }
}
