using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlaceableType
{
    None,
    Crop,
    Animal,
    Building,
    Neutral,
}

// Placeable represents objects that can be placed on grid tiles
public class Placeable : MonoBehaviour
{
    [SerializeField]
    private string placeableName;
    public string PlaceableName => placeableName;

    [SerializeField]
    private PlaceableType placeableType = PlaceableType.None;
    public PlaceableType PlaceableType => placeableType;

    [SerializeField]
    private bool isPermanent = true;
    public bool IsPermanent => isPermanent;

    [SerializeField]
    private List<TileType> allowedTileTypes;
    public List<TileType> AllowedTileTypes => allowedTileTypes;

    private GridTile gridtile;
    public GridTile GridTile => gridtile;

    private List<PlaceableEffect> effects = new List<PlaceableEffect>();

    public void Initialize(GridTile tile)
    {
        gridtile = tile;

        effects = GetComponents<PlaceableEffect>().ToList();
    }

    public void OnPlaced()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnPlace(GridTile);
        }
    }

    public void OnRemoved()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnRemove(GridTile);
        }
    }

    public void OnEndOfTurn()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnEndOfTurn(GridTile);
        }
    }

    public void OnEndOfRound()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnEndOfRound(GridTile);
        }
    }

    public void OnNewPlaced(GridTile tile, GridTile placedTile)
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnNewPlace(tile, placedTile);
        }
    }
}
