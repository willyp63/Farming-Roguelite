using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSeasonUI : MonoBehaviour
{
    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Image seasonImage;

    public void SetSeason(Season season)
    {
        backgroundImage.color = SeasonManager.GetSeasonColor(season);
        seasonImage.sprite = SeasonManager.GetSeasonSymbol(season);

        gameObject.SetActive(season != Season.Neutral);
    }
}
