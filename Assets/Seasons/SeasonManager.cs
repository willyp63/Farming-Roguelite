using UnityEngine;

public class SeasonManager : Singleton<SeasonManager>
{
    [SerializeField]
    private SeasonData seasonData;

    protected override void Awake()
    {
        base.Awake();

        if (seasonData == null)
        {
            Debug.LogError("SeasonData not assigned to SeasonManager!");
        }
    }

    public static string GetSeasonName(Season season)
    {
        return Instance.seasonData.GetSeasonName(season);
    }

    public static Color GetSeasonColor(Season season)
    {
        return Instance.seasonData.GetSeasonColor(season);
    }

    public static Sprite GetSeasonSymbol(Season season)
    {
        return Instance.seasonData.GetSeasonSymbol(season);
    }

    public static SeasonData.SeasonInfo GetSeasonInfo(Season season)
    {
        return Instance.seasonData.GetSeasonInfo(season);
    }
}
