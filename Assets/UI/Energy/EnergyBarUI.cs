using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [SerializeField]
    private Image symbolBackgroudImage;

    [SerializeField]
    private Image symbolImage;

    [SerializeField]
    private Image barFillImage;

    [SerializeField]
    private TextMeshProUGUI energyText;

    [SerializeField]
    private ShakeBehavior shakeBehavior;

    private int maxEnergy;

    private SeasonType seasonType;
    public SeasonType SeasonType => seasonType;

    public void Initialize(SeasonType seasonType, int maxEnergy)
    {
        this.maxEnergy = maxEnergy;
        this.seasonType = seasonType;

        SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(seasonType);
        if (seasonInfo == null)
            return;

        symbolBackgroudImage.color = seasonInfo.color;
        barFillImage.color = seasonInfo.color;
        energyText.color = seasonInfo.color;
        symbolImage.sprite = seasonInfo.symbolSprite;
    }

    public void SetEnergy(int energy)
    {
        if (energy < 0)
            energy = 0;

        float fillAmount = Mathf.Min(1f, energy / (float)maxEnergy);

        barFillImage.fillAmount = fillAmount;
        energyText.text = energy.ToString();
    }

    public void Shake()
    {
        shakeBehavior.Shake();
    }
}
