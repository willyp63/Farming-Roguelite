using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Card", menuName = "Farming Roguelike/Effect Card")]
public class EffectCard : Card
{
    [SerializeField]
    private PlaceableEffect effectPrefab;
    public PlaceableEffect EffectPrefab => effectPrefab;

    [SerializeField]
    private List<TileType> allowedTileTypes;
    public List<TileType> AllowedTileTypes => allowedTileTypes;

    [SerializeField]
    private Season season = Season.Spring;
    public Season Season => season;

    public override void PlayCard(GridTile tile)
    {
        GameObject effectObject = Instantiate(effectPrefab.gameObject);
        PlaceableEffect effect = effectObject.GetComponent<PlaceableEffect>();

        if (effect.EffectTiming != PlaceableEffectTiming.OnPlace)
        {
            Debug.LogError("Card effect must having OnPlace timing!");
        }

        effect.OnPlace(tile);

        Destroy(effect.gameObject);
    }

    public override bool IsValidPlacement(GridTile tile)
    {
        bool isAllowedTile = GetAllowedTileTypes().Contains(tile.Tile.TileType);
        bool isTileEmpty = tile.PlacedObject == null;

        return isAllowedTile && isTileEmpty;
    }

    public override List<TileType> GetAllowedTileTypes()
    {
        return allowedTileTypes;
    }

    public override Season GetSeason()
    {
        return season;
    }

    public override string GetTooltipText()
    {
        return CardName;
    }
}
