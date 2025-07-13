using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : Singleton<RoundManager>
{
    [Header("Game Settings")]
    [SerializeField]
    private bool isBotPlaying = false;
    public bool IsBotPlaying => isBotPlaying;

    [Header("Bot Settings")]
    [SerializeField]
    private float botMoveDelay = 1f; // Delay between bot moves in seconds

    private int numMoves = 0;
    public int NumMoves => numMoves;

    private int requiredScore = 100;
    public int RequiredScore => requiredScore;

    private int score = 0;
    public int Score => score;

    private bool isScoring = false;
    public bool IsScoring => isScoring;

    public bool CanMakeMove => !isScoring && !BoardManager.Instance.IsSwapping;

    [NonSerialized]
    public UnityEvent OnScoreChange = new();

    [NonSerialized]
    public UnityEvent OnRoundEnd = new();

    private void Start()
    {
        BoardManager.Instance.OnTileSwapped.AddListener(OnTileSwapped);
    }

    public void StartRound(int requiredScore)
    {
        this.requiredScore = requiredScore;
        SetScore(0);

        EnergyManager.Instance.ResetAllEnergy();
        DeckManager.Instance.ResetDeck();
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
        int cascadeBonus = 0;

        while (matches.Count > 0)
        {
            int totalScore = 0;

            // Score matches
            foreach (var match in matches)
            {
                int matchScore = 0;
                SeasonType matchSeason = SeasonType.None;

                foreach (var tile in match)
                {
                    matchScore += tile.DeckTile.PointScore;
                    // All tiles in a match should have the same season, so we can use the first one
                    if (matchSeason == SeasonType.None)
                    {
                        matchSeason = tile.Season;
                    }
                }

                // Bonus for longer matches and cascade bonuses
                int multiplier = match.Count - 2; // 4 tiles = 2x, 5 tiles = 3x, etc.
                multiplier += cascadeBonus; // 1x for each cascade bonus

                totalScore += matchScore * multiplier;

                // Show floating text for the score
                ShowMatchScore(match, matchScore * multiplier, multiplier);

                // Add energy for the match (1 energy per tile)
                if (matchSeason != SeasonType.None)
                {
                    EnergyManager.Instance.AddEnergy(matchSeason, match.Count);
                }
            }

            AddScore(totalScore);

            // Flatten matches into single list
            List<BoardTile> allMatchingTiles = new List<BoardTile>();
            foreach (var match in matches)
            {
                allMatchingTiles.AddRange(match);
            }

            yield return StartCoroutine(BoardManager.Instance.RemoveTiles(allMatchingTiles));

            matches = BoardManager.Instance.FindMatches();
            cascadeBonus += 1;
        }

        isScoring = false;

        if (score >= requiredScore)
        {
            OnRoundEnd?.Invoke();
            yield break;
        }

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
                BoardManager.Instance.TrySwapTiles(bestSwap[0], bestSwap[1]);
            }
            else
            {
                Debug.Log("Bot found no valid moves!");
                Debug.Log($"Bot has made {numMoves} moves.");
                // TODO: handle locked board state
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in bot move: {e.Message}");
        }
    }

    private void ShowMatchScore(List<BoardTile> match, int totalScore, int multiplier)
    {
        // Calculate the center position of all matched tiles
        Vector3 centerPosition = Vector3.zero;
        foreach (var tile in match)
        {
            centerPosition += tile.transform.position;
        }
        centerPosition /= match.Count;

        // Get the season color for the text
        Color textColor = Color.white; // Default color
        if (match.Count > 0 && match[0].Season != SeasonType.None)
        {
            var seasonData = Resources.Load<SeasonData>("SeasonData");
            if (seasonData != null)
            {
                var seasonInfo = seasonData.GetSeasonInfo(match[0].Season);
                if (seasonInfo != null)
                {
                    textColor = seasonInfo.color;
                }
            }
        }

        // Create the score text with rich text formatting for different colors
        string scoreText;
        if (multiplier > 1)
        {
            // Convert colors to hex format for rich text
            string pointsHex = ColorUtility.ToHtmlStringRGB(FloatingTextManager.pointsColor);
            string multiplierHex = ColorUtility.ToHtmlStringRGB(
                FloatingTextManager.multiplierColor
            );

            scoreText =
                $"<color=#{pointsHex}>{totalScore:N0}</color><size=40><color=#{multiplierHex}> (x{multiplier})</color></size>";
        }
        else
        {
            // Just points, no multiplier
            string pointsHex = ColorUtility.ToHtmlStringRGB(FloatingTextManager.pointsColor);
            scoreText = $"<color=#{pointsHex}>{totalScore:N0}</color>";
        }

        // Spawn the floating text (use white color since we're using rich text)
        FloatingTextManager.Instance.SpawnText(scoreText, centerPosition, Color.white);
    }
}
