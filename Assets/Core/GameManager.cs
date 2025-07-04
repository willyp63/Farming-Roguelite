using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [Header("Game Settings")]
    [SerializeField]
    private int numberOfStartingCards = 8;

    [SerializeField]
    private int maxCardsPerTurn = 5;

    [SerializeField]
    private int startingScore = 0;

    [SerializeField]
    private int scoreGoal = 100;

    [SerializeField]
    private int numberOfTurns = 3;

    [Header("Events")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnCardsPlayedChanged;
    public UnityEvent OnRoundStarted;
    public UnityEvent OnTurnStarted;
    public UnityEvent OnTurnCompleted;
    public UnityEvent OnGameOver;

    // Game state
    private int currentScore;
    private int currentRound = 0;
    private int cardsPlayedThisTurn = 0;
    private bool isTurnActive = false;

    // Properties
    public int CurrentScore => currentScore;
    public int CurrentRound => currentRound;
    public int CardsPlayedThisTurn => cardsPlayedThisTurn;
    public int MaxCardsPerTurn => maxCardsPerTurn;
    public bool IsTurnActive => isTurnActive;
    public bool CanPlayMoreCards => cardsPlayedThisTurn < maxCardsPerTurn && isTurnActive;

    protected override void Awake()
    {
        base.Awake();
        currentScore = startingScore;
    }

    private void Start()
    {
        // Subscribe to grid UI events
        if (GridUIManager.Instance != null)
        {
            GridUIManager.Instance.OnCardPlayedOnTile.AddListener(OnCardPlayed);
        }

        ResetGame();
    }

    public void ResetGame()
    {
        currentScore = startingScore;
        currentRound = 0;
        cardsPlayedThisTurn = 0;
        isTurnActive = false;

        OnScoreChanged?.Invoke(currentScore);
        OnCardsPlayedChanged?.Invoke(cardsPlayedThisTurn);

        StartNewRound();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unsubscribe from events
        if (GridUIManager.Instance != null)
        {
            GridUIManager.Instance.OnCardPlayedOnTile.RemoveListener(OnCardPlayed);
        }
    }

    public void StartNewRound()
    {
        currentRound++;
        cardsPlayedThisTurn = 0;
        isTurnActive = true;

        // Draw cards for the round
        CardManager.Instance.Reset();
        CardManager.Instance.DrawCards(numberOfStartingCards);

        // Generate new grid
        GridManager.Instance.GenerateGrid();

        OnRoundStarted?.Invoke();
        OnTurnStarted?.Invoke();

        Debug.Log($"New round started.");
    }

    public void StartNewTurn()
    {
        isTurnActive = true;
        cardsPlayedThisTurn = 0;

        CardManager.Instance.DrawCards(maxCardsPerTurn);

        OnTurnStarted?.Invoke();

        Debug.Log($"New turn started.");
    }

    public void CompleteTurn()
    {
        if (!isTurnActive)
        {
            Debug.LogWarning("Cannot complete turn: no turn is active");
            return;
        }

        StartCoroutine(CompleteTurnCoroutine());
    }

    private IEnumerator CompleteTurnCoroutine()
    {
        GridManager.Instance.OnBeforeScoring();

        yield return new WaitForSeconds(2f);

        // Calculate total score from all placeables on the board
        int turnScore = GridManager.Instance.CalculateBoardScore();

        // Add to total score
        AddScore(turnScore);

        yield return new WaitForSeconds(2f);

        GridManager.Instance.OnEndOfTurn();

        yield return new WaitForSeconds(2f);

        // Clear non-permanent placeables
        GridManager.Instance.ClearNonPermanentPlaceables();

        // End the turn
        numberOfTurns--;
        isTurnActive = false;

        OnTurnCompleted?.Invoke();

        Debug.Log($"Turn completed! Turn score: {turnScore}, Total score: {currentScore}");

        yield return new WaitForSeconds(2f);

        if (currentScore > scoreGoal)
        {
            Debug.Log($"Game over! You won with {currentScore} points!");
        }
        else if (numberOfTurns == 0)
        {
            Debug.Log($"Game over! You lost with {currentScore} points!");
        }
        else
        {
            StartNewTurn();
        }
    }

    private void OnCardPlayed(Vector2Int position, Card card)
    {
        // TODO: move this so we can actually prevent cards being played
        if (!isTurnActive)
        {
            Debug.LogWarning("Cannot play card: no round is active");
            return;
        }

        if (cardsPlayedThisTurn >= maxCardsPerTurn)
        {
            Debug.LogWarning(
                $"Cannot play more cards: already played {maxCardsPerTurn} cards this turn"
            );
            return;
        }

        cardsPlayedThisTurn++;
        OnCardsPlayedChanged?.Invoke(cardsPlayedThisTurn);

        Debug.Log(
            $"Card played on tile {position}! {cardsPlayedThisTurn}/{maxCardsPerTurn} cards played this turn"
        );
    }

    private void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }
}
