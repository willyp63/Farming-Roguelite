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
    private bool isPermanent = false;
    public bool IsPermanent => isPermanent;

    private int score;
    public int Score => score;

    private GridTile gridtile;
    public GridTile GridTile => gridtile;

    private List<PlaceableEffect> effects = new List<PlaceableEffect>();

    public void Initialize(GridTile tile, int score)
    {
        gridtile = tile;
        this.score = score;

        effects = GetComponents<PlaceableEffect>().ToList();
    }

    public void SetScore(int newScore)
    {
        score = newScore;
    }

    public void AddScore(int addition)
    {
        score += addition;
    }

    public void MultiplyScore(int multiplier)
    {
        score *= multiplier;
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
}
