using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSeasonUI : MonoBehaviour
{
    [SerializeField]
    private Image highlightBorderImage;

    [SerializeField]
    private Image highlightImage;

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Image seasonImage;

    public void SetSeason(SeasonType season)
    {
        backgroundImage.color = SeasonManager.GetSeasonInfo(season).color;
        seasonImage.sprite = SeasonManager.GetSeasonInfo(season).symbol;

        gameObject.SetActive(season != SeasonType.None);
    }

    public void SetHighlight(bool isHighlighted, Color color)
    {
        highlightImage.gameObject.SetActive(isHighlighted);
        highlightBorderImage.gameObject.SetActive(isHighlighted);
        highlightImage.color = color;
    }
}
