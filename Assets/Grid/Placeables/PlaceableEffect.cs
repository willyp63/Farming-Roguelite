using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectedTilesType
{
    Self,
    Adjacent,
    Diagonal,
    Surrounding,
    Row,
    Column,
    All,
}

public enum PlaceableEffectTiming
{
    OnPlace,
    OnRemove,
    OnBeforeScoring,
    OnEndOfTurn,
    OnEndOfRound,
}

public abstract class PlaceableEffect : MonoBehaviour
{
    [SerializeField]
    private PlaceableEffectTiming effectTiming;

    [SerializeField]
    private EffectedTilesType effectedTilesType;

    [SerializeField]
    private List<TileType> allowedTileTypes;

    protected abstract void ApplyEffect(GridTile tile, List<GridTile> affectedTiles);

    public void OnPlace(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnPlace);
    }

    public void OnRemove(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnRemove);
    }

    public void OnEndOfTurn(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnEndOfTurn);
    }

    public void OnEndOfRound(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnEndOfRound);
    }

    public void OnBeforeScoring(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnBeforeScoring);
    }

    private void TryApplyEffect(GridTile tile, PlaceableEffectTiming timing)
    {
        if (allowedTileTypes.Count > 0 && !allowedTileTypes.Contains(tile.Tile.TileType))
            return;

        if (effectTiming == timing)
            ApplyEffect(tile, GetAffectedTiles(tile));
    }

    private List<GridTile> GetAffectedTiles(GridTile tile)
    {
        switch (effectedTilesType)
        {
            case EffectedTilesType.Self:
                return new List<GridTile> { tile };
            case EffectedTilesType.Adjacent:
                return GridManager.Instance.GetAdjacentTiles(tile.Position);
            case EffectedTilesType.Diagonal:
                return GridManager.Instance.GetDiagonalTiles(tile.Position);
            case EffectedTilesType.Surrounding:
                return GridManager.Instance.GetSurroundingTiles(tile.Position);
            case EffectedTilesType.Row:
                return GridManager.Instance.GetRowTiles(tile.Position);
            case EffectedTilesType.Column:
                return GridManager.Instance.GetColumnTiles(tile.Position);
            case EffectedTilesType.All:
                return GridManager.Instance.GetAllTiles();
            default:
                return new List<GridTile>();
        }
    }
}
