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

    public static SeasonInfo GetSeasonInfo(SeasonType season)
    {
        return Instance.seasonData.GetSeasonInfo(season);
    }
}
