using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Placeable represents objects that can be placed on grid tiles
public class Placeable : MonoBehaviour
{
    [SerializeField]
    private string placeableName;
    public string PlaceableName => placeableName;

    [SerializeField]
    private bool isMovable = false;
    public bool IsMovable => isMovable;

    [SerializeField]
    private List<TileType> validTileTypes;
    public List<TileType> ValidTileTypes => validTileTypes;

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
            effect.OnPlace(this);
        }
    }

    public void OnRemoved()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnRemove(this);
        }
    }

    public void OnEndOfTurn()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnEndOfTurn(this);
        }
    }

    public void OnStartOfTurn()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnStartOfTurn(this);
        }
    }
}
