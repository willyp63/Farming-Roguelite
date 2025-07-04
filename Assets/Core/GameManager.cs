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

        // Calculate total score from all placeables on the board
        int turnScore = CalculateBoardScore();

        // Add to total score
        AddScore(turnScore);

        // Clear non-permanent placeables
        ClearNonPermanentPlaceables();

        // End the turn
        numberOfTurns--;
        isTurnActive = false;

        OnTurnCompleted?.Invoke();

        Debug.Log($"Turn completed! Turn score: {turnScore}, Total score: {currentScore}");

        StartCoroutine(StartNextTurnAfterDelay(2f));
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

    private int CalculateBoardScore()
    {
        int totalScore = 0;
        Dictionary<Vector2Int, GridTile> grid = GridManager.Instance.GetGrid();

        foreach (var kvp in grid)
        {
            GridTile tile = kvp.Value;
            if (tile != null && tile.PlacedObject != null)
            {
                totalScore += tile.PlacedObject.Score;
            }
        }

        return totalScore;
    }

    private void ClearNonPermanentPlaceables()
    {
        Dictionary<Vector2Int, GridTile> grid = GridManager.Instance.GetGrid();
        List<Vector2Int> positionsToClear = new List<Vector2Int>();

        // Find all non-permanent placeables
        foreach (var kvp in grid)
        {
            Vector2Int position = kvp.Key;
            GridTile tile = kvp.Value;

            if (tile != null && tile.PlacedObject != null && !tile.PlacedObject.IsPermanent)
            {
                positionsToClear.Add(position);
            }
        }

        // Remove them
        foreach (Vector2Int position in positionsToClear)
        {
            GridManager.Instance.RemoveObject(position);
        }

        Debug.Log($"Cleared {positionsToClear.Count} non-permanent placeables from the board");
    }

    private void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    private IEnumerator StartNextTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

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
}
