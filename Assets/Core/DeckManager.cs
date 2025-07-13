using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InitialTileQuantity
{
    public SeasonType season;
    public int quantity;
}

[System.Serializable]
public class InitialUnitQuantity
{
    public SeasonType season;
    public UnitData unit;
    public int quantity;
}

public class DeckManager : Singleton<DeckManager>
{
    [Header("Deck Settings")]
    [SerializeField]
    private List<InitialTileQuantity> initialTileQuantities;

    [SerializeField]
    private List<InitialUnitQuantity> initialUnitQuantities;

    private List<DeckTile> deckTiles = new();
    public List<DeckTile> DeckTiles => deckTiles;

    private List<DeckTile> currentDeckTiles = new();
    public List<DeckTile> CurrentDeckTiles => currentDeckTiles;

    public void Initialize()
    {
        deckTiles.Clear();

        foreach (var tileQuantity in initialTileQuantities)
        {
            for (int i = 0; i < tileQuantity.quantity; i++)
            {
                deckTiles.Add(new DeckTile(tileQuantity.season));
            }
        }

        foreach (var unitQuantity in initialUnitQuantities)
        {
            for (int i = 0; i < unitQuantity.quantity; i++)
            {
                // Find empty tile that matches unitQuantity.season
                DeckTile matchingTile = deckTiles.Find(tile =>
                    tile.Season == unitQuantity.season && tile.Unit == null
                );

                if (matchingTile != null)
                {
                    // Add unit to tile
                    matchingTile.SetUnit(unitQuantity.unit);
                }
                else
                {
                    Debug.LogWarning(
                        $"No empty tile found for season {unitQuantity.season} to place unit {unitQuantity.unit.name}"
                    );
                    break;
                }
            }
        }

        deckTiles.Sort(
            (a, b) =>
            {
                int seasonCompare = a.Season.CompareTo(b.Season);
                if (seasonCompare != 0)
                    return seasonCompare;
                return a.IsEmpty.CompareTo(b.IsEmpty);
            }
        );

        ResetDeck();
    }

    public void ResetDeck()
    {
        currentDeckTiles.Clear();
        currentDeckTiles = new List<DeckTile>(deckTiles);
    }

    public void ShuffleDeck()
    {
        // Fisher-Yates shuffle algorithm
        for (int i = currentDeckTiles.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            DeckTile temp = currentDeckTiles[i];
            currentDeckTiles[i] = currentDeckTiles[randomIndex];
            currentDeckTiles[randomIndex] = temp;
        }
    }

    public void AddDeckTile(DeckTile deckTile)
    {
        currentDeckTiles.Add(deckTile);
    }

    public void RemoveDeckTile(DeckTile deckTile)
    {
        currentDeckTiles.Remove(deckTile);
    }

    public DeckTile DrawTileFromTop()
    {
        if (currentDeckTiles.Count == 0)
        {
            Debug.LogWarning("Cannot draw tile from empty deck!");
            return null;
        }

        DeckTile drawnTile = currentDeckTiles[0];
        currentDeckTiles.RemoveAt(0);
        return drawnTile;
    }
}
