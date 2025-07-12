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
        RoundManager.Instance.OnScoreChange.AddListener(OnScoreChanged);

        OnScoreChanged();
    }

    private void OnScoreChanged()
    {
        requiredScoreText.text = RoundManager.Instance.RequiredScore.ToString();
        scoreText.text = RoundManager.Instance.Score.ToString();
    }
}
