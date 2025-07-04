using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Tile Card Effect",
    menuName = "Farming Roguelike/Card Effects/Tile Card Effect"
)]
public class TileCardEffect : CardEffect
{
    [SerializeField]
    private Tile toTile;

    public override void ApplyEffect(Vector2Int position, GridTile tile, Card card)
    {
        GridManager.Instance.GetTile(position).SetTile(toTile);
    }
}
