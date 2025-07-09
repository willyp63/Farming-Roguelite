using System;
using System.Collections;
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

    [SerializeField]
    private int maxCardsPlayedPerDay = 5;

    [NonSerialized]
    public UnityEvent<int, int> OnScoreChange = new();

    [NonSerialized]
    public UnityEvent<int> OnDayChange = new();

    // Game state
    private int roundScore;
    private int pointScore;
    private int multiScore;

    private int numCardsPlayed = 0;
    private int currentDay;

    public int RoundScore => roundScore;
    public int PointScore => pointScore;
    public int MultiScore => multiScore;
    public int TotalScore => pointScore * multiScore;
    public int RequiredScore => requiredScore;
    public int CurrentDay => currentDay;
    public int TotalDays => numberOfDays;

    public void StartRound()
    {
        roundScore = 0;
        pointScore = 0;
        multiScore = 0;

        numCardsPlayed = 0;
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

    public void ResetPlayedCards()
    {
        numCardsPlayed = 0;
        GridManager.Instance.ResetUncommittedPlaceables();
    }

    public void GoToNextDay()
    {
        StartCoroutine(NextDayEnumerator());
    }

    private IEnumerator NextDayEnumerator()
    {
        // Trigger end of day effects and clear non-permanent placeables
        yield return GridManager.Instance.EndOfTurnEnumerator();

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

        numCardsPlayed = 0;
        CardManager.Instance.DrawCards(cardsDrawnPerDay);

        // Increment day and trigger event
        currentDay++;
        OnDayChange?.Invoke(currentDay);
    }

    public void TryPlayCard(Card card, GridTile tile)
    {
        if (card.IsValidPlacement(tile) && numCardsPlayed < maxCardsPlayedPerDay)
        {
            card.PlayCard(tile);
            CardManager.Instance.RemoveCardFromHand(card);
            numCardsPlayed++;
        }
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

    public void CommitScore()
    {
        roundScore += TotalScore;
        pointScore = 0;
        multiScore = 0;

        OnScoreChange?.Invoke(pointScore, multiScore);
    }
}
