using System.Collections.Generic;
using UnityEngine;

public enum SeasonType
{
    Spring,
    Summer,
    Autumn,
    Winter,
    Neutral,
}

[System.Serializable]
public class SeasonInfo
{
    public SeasonType season;
    public string name;
    public Color color;
    public Sprite symbol;
}

[CreateAssetMenu(fileName = "SeasonData", menuName = "Farming Roguelike/Season Data")]
public class SeasonData : ScriptableObject
{
    [SerializeField]
    private List<SeasonInfo> seasonInfos = new();

    private Dictionary<SeasonType, SeasonInfo> seasonLookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        seasonLookup = new Dictionary<SeasonType, SeasonInfo>();
        foreach (var seasonInfo in seasonInfos)
        {
            seasonLookup[seasonInfo.season] = seasonInfo;
        }
    }

    public SeasonInfo GetSeasonInfo(SeasonType season)
    {
        if (seasonLookup == null)
            BuildLookup();

        seasonLookup.TryGetValue(season, out var seasonInfo);
        return seasonInfo;
    }
}
