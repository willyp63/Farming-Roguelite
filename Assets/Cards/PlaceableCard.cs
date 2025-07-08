using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Placeable Card", menuName = "Farming Roguelike/Placeable Card")]
public class PlaceableCard : Card
{
    [SerializeField]
    private Placeable placeablePrefab;
    public Placeable PlaceablePrefab => placeablePrefab;

    public override void PlayCard(GridTile tile)
    {
        GridManager.Instance.PlaceObject(tile.Position, placeablePrefab);
    }

    public override bool IsValidPlacement(GridTile tile)
    {
        bool isAllowedTile = GetAllowedTileTypes().Contains(tile.Tile.TileType);
        bool isTileEmpty = tile.PlacedObject == null;

        return isAllowedTile && isTileEmpty;
    }

    public override List<TileType> GetAllowedTileTypes()
    {
        return placeablePrefab.AllowedTileTypes;
    }

    public override SeasonType GetSeason()
    {
        return placeablePrefab.Season;
    }

    public override string GetTooltipText()
    {
        SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(GetSeason());

        List<string> lines = new List<string>
        {
            $"<size=28><color=#{ColorUtility.ToHtmlStringRGB(seasonInfo.color)}>{CardName}</color></size>",
            $"<size=20>Requires {FormatTileTypeList(GetAllowedTileTypes())}</size>",
            "",
            $"<size=20>{Text}</size>",
        };

        return string.Join("\n", lines);
    }

    private string FormatTileTypeList(List<TileType> types)
    {
        if (types == null || types.Count == 0)
            return "";
        if (types.Count == 1)
            return FormatTileTypeString(types[0]);

        var typeStrings = types.Select(FormatTileTypeString).ToList();
        return string.Join(", ", typeStrings.Take(typeStrings.Count - 1))
            + " or "
            + typeStrings.Last();
    }

    private string FormatTileTypeString(TileType tileType)
    {
        TileInfo tile = TileManager.GetTileInfo(tileType);
        return $"<color=#{ColorUtility.ToHtmlStringRGB(tile.TileColor)}>{tile.TileName}</color>";
    }
}
