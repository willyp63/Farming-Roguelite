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

public enum PlaceableFamily
{
    None,
    Wheat,
    Corn,
    Carrot,
    Potato,
    Cabbage,
    Tomato,
    Onion,
    Cow,
    Chicken,
    Sheep,
    Pig,
    Goat,
    Rabbit,
    Fish,
    Pumpkin,
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
    private PlaceableFamily placeableFamily = PlaceableFamily.None;
    public PlaceableFamily PlaceableFamily => placeableFamily;

    [SerializeField]
    private SeasonType season = SeasonType.Spring;
    public SeasonType Season => season;

    [SerializeField]
    private bool isPermanent = true;
    public bool IsPermanent => isPermanent;

    [SerializeField]
    private bool isMovable = false;
    public bool IsMovable => isMovable;

    [SerializeField]
    private List<TileType> allowedTileTypes;
    public List<TileType> AllowedTileTypes => allowedTileTypes;

    [SerializeField]
    [TextArea(4, 8)]
    private string text;
    public string Text => text;

    [SerializeField]
    private int pointScore = 0;
    public int PointScore => pointScore + pointScoreAddition;

    private int pointScoreAddition = 0;
    public int PointScoreAddition => pointScoreAddition;

    [SerializeField]
    private int multiScore = 0;
    public int MultiScore => multiScore + multiScoreAddition;

    private int multiScoreAddition = 0;
    public int MultiScoreAddition => multiScoreAddition;

    private GridTile gridtile;
    public GridTile GridTile => gridtile;

    private GridTile startOfDayGridTile;
    public GridTile StartOfDayGridTile => startOfDayGridTile;

    private List<PlaceableEffect> effects = new List<PlaceableEffect>();

    private Card card;
    public Card Card => card;

    private Placeable spawnPlaceableOnMove;
    public Placeable SpawnPlaceableOnMove => spawnPlaceableOnMove;

    private bool hasSpawnedOnMove = false;
    public bool HasSpawnedOnMove => hasSpawnedOnMove;

    private bool isCommitted = false;
    public bool IsCommitted => isCommitted;
    public bool CanMove => !isCommitted || isMovable;

    private bool hasBeenScored = false;
    public bool HasBeenScored => hasBeenScored;

    public void Initialize(GridTile tile)
    {
        gridtile = tile;

        effects = GetComponents<PlaceableEffect>().ToList();
    }

    public void OnTriggered()
    {
        foreach (PlaceableEffect effect in effects)
        {
            effect.OnTriggered(GridTile);
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

    public void SetCard(Card card)
    {
        this.card = card;
    }

    public void MarkAsCommitted()
    {
        startOfDayGridTile = gridtile;
        isCommitted = true;
    }

    public void MarkAsScored()
    {
        hasBeenScored = true;
    }

    public void ResetScoredFlag()
    {
        hasBeenScored = false;
    }

    public void AddPoints(int amount)
    {
        pointScoreAddition += amount;
    }

    public void AddMulti(int amount)
    {
        multiScoreAddition += amount;
    }

    public void SetSpawnPlaceableOnMove(Placeable placeable)
    {
        spawnPlaceableOnMove = placeable;
    }

    public void SetHasSpawnedOnMove(bool hasSpawned)
    {
        hasSpawnedOnMove = hasSpawned;
    }

    public string GetTooltipText()
    {
        SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(season);

        List<string> lines = new List<string>
        {
            $"<size=28><color=#{ColorUtility.ToHtmlStringRGB(seasonInfo.color)}>{placeableName}</color></size>",
            allowedTileTypes == null || allowedTileTypes.Count == 0
                ? ""
                : $"<size=20>{FormatTileTypeList(allowedTileTypes)}</size>",
            string.IsNullOrEmpty(text) ? "" : $"\n<size=20>{text}</size>",
        };

        return string.Join("\n", lines.Where(line => !string.IsNullOrEmpty(line)));
    }

    private string FormatTileTypeList(List<TileType> types)
    {
        if (types == null || types.Count == 0)
            return "";
        if (types.Count == 1)
            return FormatTileTypeString(types[0]);

        var typeStrings = types.Select(FormatTileTypeString).ToList();
        return string.Join(", ", typeStrings.Take(typeStrings.Count - 1))
            + " or "
            + typeStrings.Last();
    }

    private string FormatTileTypeString(TileType tileType)
    {
        TileInfo tile = TileManager.GetTileInfo(tileType);
        return $"<color=#{ColorUtility.ToHtmlStringRGB(tile.TileColor)}>{tile.TileName}</color>";
    }
}
