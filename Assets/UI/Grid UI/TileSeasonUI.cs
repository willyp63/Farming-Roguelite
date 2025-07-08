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

    public void SetSeason(SeasonType season)
    {
        backgroundImage.color = SeasonManager.GetSeasonInfo(season).color;
        seasonImage.sprite = SeasonManager.GetSeasonInfo(season).symbol;

        gameObject.SetActive(season != SeasonType.Neutral);
    }
}
