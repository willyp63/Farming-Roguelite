using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty,
    Grass,
    Dirt,
    Clay,
    Sand,
    Swamp,
    Water,
}

[CreateAssetMenu(fileName = "New Tile", menuName = "Farming Roguelike/Tile")]
public class Tile : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField]
    private string tileName;

    [SerializeField]
    private Sprite tileSprite;

    [SerializeField]
    private TileType tileType;

    // Properties
    public Sprite TileSprite => tileSprite;
    public TileType TileType => tileType;
    public string TileName => tileName;
}
