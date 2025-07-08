using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : Singleton<RoundManager>
{
    [Header("Game Settings")]
    [SerializeField]
    private int cardsDrawnOnFirstDay = 8;

    [SerializeField]
    private int cardsDrawnPerDay = 5;

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
        currentDay = 1;

        // Clear grid and generate a new one
        GridGenerationManager.Instance.GenerateGrid();

        // Reset deck and draw initial hand
        CardManager.Instance.Reset();
        CardManager.Instance.DrawCards(cardsDrawnOnFirstDay);

        OnScoreChange?.Invoke(pointScore, multiScore);
        OnDayChange?.Invoke(currentDay);

        Debug.Log($"New round started.");
    }

    public bool IsRoundComplete()
    {
        return TotalScore >= requiredScore;
    }

    public void GoToNextDay()
    {
        StartCoroutine(NextDayEnumerator());
    }

    private IEnumerator NextDayEnumerator()
    {
        // Trigger end of day effects and clear non-permanent placeables
        GridManager.Instance.OnEndOfTurn();
        yield return new WaitForSeconds(1f);

        // Check if round is complete
        if (IsRoundComplete())
        {
            Debug.Log("Round complete!");
        }
        // Check if round is failed
        else if (currentDay >= numberOfDays)
        {
            Debug.Log("Round failed!");
        }

        // Reset energy & draw cards for the next day
        PlayerManager.Instance.ResetEnergy();
        CardManager.Instance.DrawCards(cardsDrawnPerDay);

        // Increment day and trigger event
        currentDay++;
        OnDayChange?.Invoke(currentDay);
    }

    public void AddPoints(int amount, float multiplier = 1)
    {
        pointScore += amount;
        pointScore = (int)(pointScore * multiplier);

        OnScoreChange?.Invoke(pointScore, multiScore);
    }

    public void AddMulti(int amount, float multiplier = 1)
    {
        multiScore += amount;
        multiScore = (int)(multiScore * multiplier);

        OnScoreChange?.Invoke(pointScore, multiScore);
    }
}
