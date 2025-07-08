using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SeasonData", menuName = "Farming Roguelike/Season Data")]
public class SeasonData : ScriptableObject
{
    [System.Serializable]
    public class SeasonInfo
    {
        public Season season;
        public string name;
        public Color color;
        public Sprite symbol;
    }

    [SerializeField]
    private List<SeasonInfo> seasonInfos = new List<SeasonInfo>();

    private Dictionary<Season, SeasonInfo> seasonLookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        seasonLookup = new Dictionary<Season, SeasonInfo>();
        foreach (var seasonInfo in seasonInfos)
        {
            seasonLookup[seasonInfo.season] = seasonInfo;
        }
    }

    public string GetSeasonName(Season season)
    {
        if (seasonLookup == null)
            BuildLookup();

        if (seasonLookup.TryGetValue(season, out var seasonInfo))
        {
            return seasonInfo.name;
        }

        Debug.LogWarning($"No name found for season {season}");
        return season.ToString();
    }

    public Color GetSeasonColor(Season season)
    {
        if (seasonLookup == null)
            BuildLookup();

        if (seasonLookup.TryGetValue(season, out var seasonInfo))
        {
            return seasonInfo.color;
        }

        Debug.LogWarning($"No color found for season {season}, returning white");
        return Color.white;
    }

    public Sprite GetSeasonSymbol(Season season)
    {
        if (seasonLookup == null)
            BuildLookup();

        if (seasonLookup.TryGetValue(season, out var seasonInfo))
        {
            return seasonInfo.symbol;
        }

        Debug.LogWarning($"No symbol found for season {season}");
        return null;
    }

    public SeasonInfo GetSeasonInfo(Season season)
    {
        if (seasonLookup == null)
            BuildLookup();

        seasonLookup.TryGetValue(season, out var seasonInfo);
        return seasonInfo;
    }
}
