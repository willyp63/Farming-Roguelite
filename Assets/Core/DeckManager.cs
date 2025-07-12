using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DeckEntry
{
    public TileData tileData;
    public int quantity;
}

public class DeckManager : Singleton<DeckManager>
{
    [Header("Deck Settings")]
    [SerializeField]
    private List<DeckEntry> deckEntries = new();
    public List<DeckEntry> DeckEntries => deckEntries;

    private List<DeckTile> deckTiles = new();
    public List<DeckTile> DeckTiles => deckTiles;

    protected override void Awake()
    {
        base.Awake();

        ResetDeck();
    }

    public void ResetDeck()
    {
        deckTiles.Clear();
        foreach (var deckEntry in deckEntries)
        {
            for (int i = 0; i < deckEntry.quantity; i++)
            {
                deckTiles.Add(new DeckTile(deckEntry.tileData));
            }
        }
    }

    public void ShuffleDeck()
    {
        // Fisher-Yates shuffle algorithm
        for (int i = deckTiles.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            DeckTile temp = deckTiles[i];
            deckTiles[i] = deckTiles[randomIndex];
            deckTiles[randomIndex] = temp;
        }
    }

    public void AddDeckTile(DeckTile deckTile)
    {
        deckTiles.Add(deckTile);
    }

    public void RemoveDeckTile(DeckTile deckTile)
    {
        deckTiles.Remove(deckTile);
    }

    public DeckTile DrawTileFromTop()
    {
        if (deckTiles.Count == 0)
        {
            Debug.LogWarning("Cannot draw tile from empty deck!");
            return null;
        }

        DeckTile drawnTile = deckTiles[0];
        deckTiles.RemoveAt(0);
        return drawnTile;
    }
}
