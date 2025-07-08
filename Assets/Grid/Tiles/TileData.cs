using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty,
    Grass,
    Dirt,
    Clay,
    Sand,
    Swamp,
    Water,
}

[System.Serializable]
public class TileInfo
{
    [SerializeField]
    private TileType tileType;
    public TileType TileType => tileType;

    [SerializeField]
    private string tileName;
    public string TileName => tileName;

    [SerializeField]
    private Color tileColor;
    public Color TileColor => tileColor;

    [SerializeField]
    private Sprite tileSprite;
    public Sprite TileSprite => tileSprite;
}

[CreateAssetMenu(fileName = "TileData", menuName = "Farming Roguelike/Tile Data")]
public class TileData : ScriptableObject
{
    [SerializeField]
    private List<TileInfo> tileInfos = new();

    private Dictionary<TileType, TileInfo> tileLookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        tileLookup = new Dictionary<TileType, TileInfo>();
        foreach (var tileInfo in tileInfos)
        {
            tileLookup[tileInfo.TileType] = tileInfo;
        }
    }

    public TileInfo GetTileInfo(TileType tileType)
    {
        if (tileLookup == null)
            BuildLookup();

        tileLookup.TryGetValue(tileType, out var tileInfo);
        return tileInfo;
    }
}
