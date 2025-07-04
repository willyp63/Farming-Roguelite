using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Place Card Effect",
    menuName = "Farming Roguelike/Card Effects/Place Card Effect"
)]
public class PlaceCardEffect : CardEffect
{
    [SerializeField]
    private Placeable placeablePrefab;

    public override bool IsValidPlacement(Vector2Int position, GridTile tile, Card card)
    {
        return base.IsValidPlacement(position, tile, card) && tile.PlacedObject == null;
    }

    public override void ApplyEffect(Vector2Int position, GridTile tile, Card card)
    {
        GridManager.Instance.PlaceObject(position, placeablePrefab);
    }
}
