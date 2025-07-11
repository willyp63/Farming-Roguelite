using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Text Elements")]
    [SerializeField]
    private TextMeshProUGUI dayText;

    [SerializeField]
    private TextMeshProUGUI requiredScoreText;

    [SerializeField]
    private TextMeshProUGUI lineLabelText;

    [SerializeField]
    private TextMeshProUGUI lineMultipliersText;

    [SerializeField]
    private TextMeshProUGUI lineScoreText;

    [SerializeField]
    private TextMeshProUGUI moneyText;

    [Header("Buttons")]
    [SerializeField]
    private Button endTurnButton;

    [SerializeField]
    private Button resetButton;

    public void Start()
    {
        // Subscribe to player manager events
        PlayerManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);

        // Subscribe to score and day events
        RoundManager.Instance.OnScoreChange.AddListener(OnScoreChanged);
        RoundManager.Instance.OnDayChange.AddListener(OnDayChanged);

        // Initialize required score
        requiredScoreText.text = RoundManager.Instance.RequiredScore.ToString();

        // Initialize Text elements
        OnMoneyChanged(PlayerManager.Instance.CurrentMoney);
        OnScoreChanged(RoundManager.Instance.PointScore, RoundManager.Instance.MultiScore);
        OnDayChanged(RoundManager.Instance.CurrentDay);

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }

    private void OnMoneyChanged(int newAmount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${newAmount}";
        }
    }

    private void OnScoreChanged(int newPoints, int newMultiplier)
    {
        lineLabelText.text =
            GridManager.Instance.NumScoringTiles > 0
                ? $"Current score ({GridManager.Instance.NumScoringTiles} tiles):"
                : "Current score:";

        UpdateScoreTextElement(lineScoreText, RoundManager.Instance.TotalScore.ToString(), "0");
        UpdateScoreTextElement(
            lineMultipliersText,
            $"<color=#048BF0>{newPoints}</color> x <color=#F54840>{newMultiplier}</color>",
            $"<color=#048BF0>0</color> x <color=#F54840>0</color>"
        );
    }

    private void UpdateScoreTextElement(
        TextMeshProUGUI textElement,
        string newText,
        string doNotShakeOn = ""
    )
    {
        if (textElement == null)
            return;

        if (textElement.text != newText && newText != doNotShakeOn)
        {
            ShakeBehavior shake = textElement.GetComponent<ShakeBehavior>();
            if (shake != null)
                shake.Shake();
        }

        textElement.text = newText;
    }

    private void OnDayChanged(int newDay)
    {
        if (dayText != null)
        {
            dayText.text = $"Week {newDay} of {RoundManager.Instance.TotalDays}";
        }
    }

    private void OnEndTurnButtonClicked()
    {
        RoundManager.Instance.GoToNextDay();
    }

    private void OnResetButtonClicked()
    {
        RoundManager.Instance.ResetRound();
    }
}
