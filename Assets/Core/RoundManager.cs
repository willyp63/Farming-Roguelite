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

    [NonSerialized]
    public UnityEvent<int> OnCardsPlayedChange = new();

    [NonSerialized]
    public UnityEvent OnCanPlayCardsChange = new();

    // Game state
    private int roundScore;
    private int lineScore;
    private int pointScore;
    private int multiScore;

    private int numCardsPlayed = 0;
    private int currentDay;

    private bool canPlayCards = false;
    public bool CanPlayCards => canPlayCards;

    public int RoundScore => roundScore;
    public int LineScore => lineScore;
    public int PointScore => pointScore;
    public int MultiScore => multiScore;
    public int RequiredScore => requiredScore;
    public int CurrentDay => currentDay;
    public int TotalDays => numberOfDays;
    public int NumCardsPlayed => numCardsPlayed;
    public int MaxCardsPlayedPerDay => maxCardsPlayedPerDay;

    public void StartRound()
    {
        roundScore = 0;
        lineScore = 0;
        pointScore = 0;
        multiScore = 0;

        canPlayCards = false;
        numCardsPlayed = 0;
        OnCardsPlayedChange?.Invoke(numCardsPlayed);
        OnCanPlayCardsChange?.Invoke();

        currentDay = 1;

        // Clear grid and generate a new one
        GridGenerationManager.Instance.GenerateGrid();
        GridManager.Instance.CreateGridBackup();

        // Reset deck and draw initial hand
        CardManager.Instance.Reset();
        CardManager.Instance.DrawCards(cardsDrawnOnFirstDay);
        CardManager.Instance.BackupHand();

        canPlayCards = true;
        OnCanPlayCardsChange?.Invoke();

        OnScoreChange?.Invoke(pointScore, multiScore);
        OnDayChange?.Invoke(currentDay);

        Debug.Log($"New round started.");
    }

    public void ResetPlayedCards()
    {
        CardManager.Instance.RevertHand();
        GridManager.Instance.RevertToBackup();

        numCardsPlayed = 0;
        OnCardsPlayedChange?.Invoke(numCardsPlayed);
    }

    public void GoToNextDay()
    {
        StartCoroutine(NextDayEnumerator());
    }

    private IEnumerator NextDayEnumerator()
    {
        canPlayCards = false;
        OnCanPlayCardsChange?.Invoke();

        // Trigger end of day effects and clear non-permanent placeables
        yield return GridManager.Instance.EndOfTurnEnumerator();

        // TODO: Check if round is complete
        // TODO: Check if round is failed

        numCardsPlayed = 0;
        OnCardsPlayedChange?.Invoke(numCardsPlayed);

        CardManager.Instance.DrawCards(cardsDrawnPerDay);
        CardManager.Instance.BackupHand();

        GridManager.Instance.CreateGridBackup();

        canPlayCards = true;
        OnCanPlayCardsChange?.Invoke();

        // Increment day and trigger event
        currentDay++;
        OnDayChange?.Invoke(currentDay);
    }

    public void TryPlayCard(Card card, GridTile tile)
    {
        if (card.IsValidPlacement(tile) && numCardsPlayed < maxCardsPlayedPerDay && canPlayCards)
        {
            card.PlayCard(tile);
            CardManager.Instance.RemoveCardFromHand(card);

            numCardsPlayed++;
            OnCardsPlayedChange?.Invoke(numCardsPlayed);
        }
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

    public void CalculateScore()
    {
        lineScore = pointScore * multiScore;

        OnScoreChange?.Invoke(pointScore, multiScore);
    }

    public void CommitScore()
    {
        roundScore += lineScore;
        lineScore = 0;
        pointScore = 0;
        multiScore = 0;

        OnScoreChange?.Invoke(pointScore, multiScore);
    }
}
