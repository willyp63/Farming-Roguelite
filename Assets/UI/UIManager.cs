using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Text Elements")]
    [SerializeField]
    private TextMeshProUGUI requiredScoreText;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI moneyText;

    public void Start()
    {
        PlayerManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
        RoundManager.Instance.OnScoreChange.AddListener(OnScoreChanged);

        OnMoneyChanged();
        OnScoreChanged();
    }

    private void OnMoneyChanged()
    {
        moneyText.text = $"${PlayerManager.Instance.CurrentMoney}";
    }

    private void OnScoreChanged()
    {
        requiredScoreText.text = RoundManager.Instance.RequiredScore.ToString();
        scoreText.text = RoundManager.Instance.Score.ToString();
    }
}
