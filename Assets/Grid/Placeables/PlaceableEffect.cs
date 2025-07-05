using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectedTiles
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
    OnEffectedTilePlaced,
    OnRemove,
    OnEndOfTurn,
    OnEndOfRound,
}

public abstract class PlaceableEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField]
    protected PlaceableEffectTiming effectTiming;

    [SerializeField]
    protected EffectedTiles effectedTiles;

    [SerializeField]
    [Tooltip(
        "If true, this effect modifies the placeable itself rather than the placeables on the effected tiles"
    )]
    protected bool isSelfModifier = false;

    [Header("Effect Conditions")]
    [SerializeField]
    protected List<TileType> requiredTileTypes;

    [SerializeField]
    protected List<TileType> matchingTileTypes;

    [SerializeField]
    protected List<PlaceableType> matchingPlaceableTypes;

    protected abstract void ApplyEffect(GridTile tile, List<GridTile> affectedTiles);

    public void OnPlace(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnPlace);
    }

    public void OnEffectedTilePlaced(GridTile tile, GridTile placedTile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnEffectedTilePlaced, placedTile);
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

    private void TryApplyEffect(
        GridTile tile,
        PlaceableEffectTiming timing,
        GridTile placedTile = null
    )
    {
        if (requiredTileTypes.Count > 0 && !requiredTileTypes.Contains(tile.Tile.TileType))
            return;

        if (effectTiming != timing)
            return;

        List<GridTile> affectedTiles = GetAffectedTiles(tile);
        List<GridTile> matchingTiles = affectedTiles.FindAll(IsMatchingTile);

        if (placedTile == null)
        {
            ApplyEffect(tile, matchingTiles);
        }
        else if (matchingTiles.Contains(placedTile))
        {
            ApplyEffect(tile, new List<GridTile> { placedTile });
        }
    }

    private List<GridTile> GetAffectedTiles(GridTile tile)
    {
        switch (effectedTiles)
        {
            case EffectedTiles.Self:
                return new List<GridTile> { tile };
            case EffectedTiles.Adjacent:
                return GridManager.Instance.GetAdjacentTiles(tile.Position);
            case EffectedTiles.Diagonal:
                return GridManager.Instance.GetDiagonalTiles(tile.Position);
            case EffectedTiles.Surrounding:
                return GridManager.Instance.GetSurroundingTiles(tile.Position);
            case EffectedTiles.Row:
                return GridManager.Instance.GetRowTiles(tile.Position);
            case EffectedTiles.Column:
                return GridManager.Instance.GetColumnTiles(tile.Position);
            case EffectedTiles.All:
                return GridManager.Instance.GetAllTiles();
            default:
                return new List<GridTile>();
        }
    }

    private bool IsMatchingTile(GridTile tile)
    {
        if (matchingPlaceableTypes.Count > 0)
        {
            if (
                tile.PlacedObject == null
                || !matchingPlaceableTypes.Contains(tile.PlacedObject.PlaceableType)
            )
                return false;
        }

        if (matchingTileTypes.Count > 0)
        {
            if (!matchingTileTypes.Contains(tile.Tile.TileType))
                return false;
        }

        return true;
    }
}
