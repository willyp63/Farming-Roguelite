using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaceableEffectTiming
{
    OnTriggered,
    OnEndOfTurn,
    OnStartOfTurn,
    OnEndOfRound,
}

public enum TilePattern
{
    Self,
    Adjacent,
    Diagonal,
    Surrounding,
    Row,
    Column,
    All,
}

[System.Serializable]
public class GridMatch
{
    public TilePattern pattern;
    public List<TileType> tileTypes;
    public List<PlaceableType> placeableTypes;
    public bool onlyMatchNewTile = true;
}

[System.Serializable]
public class GridCondition
{
    public GridMatch match;
    public int requiredNumberOfTiles = 0;
}

public abstract class PlaceableEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField]
    protected PlaceableEffectTiming effectTiming;
    public PlaceableEffectTiming EffectTiming => effectTiming;

    [SerializeField]
    protected List<GridCondition> requiredConditions;

    [SerializeField]
    protected List<GridMatch> countMatches;

    [SerializeField]
    protected List<GridMatch> applyToMatches;

    protected abstract void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    );

    public void OnTriggered(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnTriggered);
    }

    public void OnEndOfTurn(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnEndOfTurn);
    }

    public void OnStartOfTurn(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnStartOfTurn);
    }

    public void OnEndOfRound(GridTile tile)
    {
        TryApplyEffect(tile, PlaceableEffectTiming.OnEndOfRound);
    }

    private void TryApplyEffect(
        GridTile tile,
        PlaceableEffectTiming timing,
        GridTile newTile = null
    )
    {
        if (effectTiming != timing)
            return;

        if (!AreRequiredConditionsMet(tile))
            return;

        int count = GetCount(tile, newTile);
        List<GridTile> applyToTiles = GetApplyToTiles(tile, newTile);

        ApplyEffect(tile, newTile, applyToTiles, count);
    }

    private List<GridTile> GetApplyToTiles(GridTile tile, GridTile newTile)
    {
        List<GridTile> applyToTiles = new List<GridTile>();

        foreach (GridMatch applyToMatch in applyToMatches)
        {
            List<GridTile> matchingTiles = GetMatchingTiles(tile, applyToMatch);

            if (applyToMatch.onlyMatchNewTile && newTile != null)
            {
                matchingTiles = matchingTiles.FindAll(tile => tile == newTile);
            }

            applyToTiles.AddRange(matchingTiles);
        }

        return applyToTiles;
    }

    private int GetCount(GridTile tile, GridTile newTile)
    {
        int count = 0;

        foreach (GridMatch countMatch in countMatches)
        {
            List<GridTile> matchingTiles = GetMatchingTiles(tile, countMatch);

            if (countMatch.onlyMatchNewTile && newTile != null)
            {
                matchingTiles = matchingTiles.FindAll(tile => tile == newTile);
            }

            count += matchingTiles.Count;
        }

        return count;
    }

    private bool AreRequiredConditionsMet(GridTile tile)
    {
        foreach (GridCondition requiredCondition in requiredConditions)
        {
            List<GridTile> matchingTiles = GetMatchingTiles(tile, requiredCondition.match);
            if (matchingTiles.Count < requiredCondition.requiredNumberOfTiles)
                return false;
        }

        return true;
    }

    private List<GridTile> GetMatchingTiles(GridTile tile, GridMatch match)
    {
        List<GridTile> matchingTiles = GetTilesInPattern(tile, match.pattern);
        return matchingTiles.FindAll(tile => IsMatchingTile(tile, match));
    }

    private bool IsMatchingTile(GridTile tile, GridMatch match)
    {
        if (
            match.placeableTypes.Count > 0
            && (
                tile.PlacedObject == null
                || !match.placeableTypes.Contains(tile.PlacedObject.PlaceableType)
            )
        )
        {
            return false;
        }

        if (match.tileTypes.Count > 0 && !match.tileTypes.Contains(tile.Tile.TileType))
        {
            return false;
        }

        return true;
    }

    private List<GridTile> GetTilesInPattern(GridTile tile, TilePattern pattern)
    {
        switch (pattern)
        {
            case TilePattern.Self:
                return new List<GridTile> { tile };
            case TilePattern.Adjacent:
                return GridManager.Instance.GetAdjacentTiles(tile.Position);
            case TilePattern.Diagonal:
                return GridManager.Instance.GetDiagonalTiles(tile.Position);
            case TilePattern.Surrounding:
                return GridManager.Instance.GetSurroundingTiles(tile.Position);
            case TilePattern.Row:
                return GridManager.Instance.GetRowTiles(tile.Position);
            case TilePattern.Column:
                return GridManager.Instance.GetColumnTiles(tile.Position);
            case TilePattern.All:
                return GridManager.Instance.GetAllTiles();
            default:
                return new List<GridTile>();
        }
    }
}
