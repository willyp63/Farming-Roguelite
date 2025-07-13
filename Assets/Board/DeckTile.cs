using UnityEngine;

public class DeckTile
{
    private readonly SeasonType season;
    public SeasonType Season => season;

    private DeckUnit unit = null;
    public DeckUnit Unit => unit;
    public bool IsEmpty => unit == null;

    public int PointScore => unit == null ? UnitData.DEFAULT_POINT_SCORE : unit.PointScore;

    public DeckTile(SeasonType seasonType)
    {
        season = seasonType;
    }

    public void ClearUnit()
    {
        unit = null;
    }

    public void SetUnit(UnitData unitData)
    {
        unit = new DeckUnit(unitData);
    }

    public string GetTooltipText()
    {
        SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(season);

        string nameText = unit != null ? unit.Data.UnitName : "Empty";
        string text =
            $"<size=28><color=#{ColorUtility.ToHtmlStringRGB(seasonInfo.color)}>{nameText}</color></size>";

        text +=
            $"\n<size=24><color=#{ColorUtility.ToHtmlStringRGB(FloatingTextManager.pointsColor)}>+{PointScore} points</color></size>";

        if (unit != null && unit.Data.Text != null && unit.Data.Text != "")
        {
            text += $"\n\n<size=20>{unit.Data.Text}</size>";
        }

        return text.Trim();
    }
}
