using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer frameImage;

    [SerializeField]
    private Transform objectContainer;

    [SerializeField]
    private GridTileData tileData;

    [SerializeField]
    private int x;

    [SerializeField]
    private int y;

    [SerializeField]
    private int pointScore = 0;
    public int PointScore => pointScore;

    [SerializeField]
    private int multiScore = 0;
    public int MultiScore => multiScore;

    private ShakeBehavior shakeBehavior;

    // Properties
    public int X => x;
    public int Y => y;
    public GridTileData TileData => tileData;
    public Vector2Int Position => new Vector2Int(x, y);

    public void Start()
    {
        shakeBehavior = GetComponent<ShakeBehavior>();
    }

    public void Initialize(int xPos, int yPos, GridTileData data)
    {
        x = xPos;
        y = yPos;

        pointScore = data.PointScore;
        multiScore = data.MultiScore;

        SetTileData(data);
    }

    public void SetTileData(GridTileData data)
    {
        tileData = data;

        UpdateVisual();
    }

    public void SetPosition(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
    }

    public void Shake()
    {
        if (shakeBehavior != null)
        {
            shakeBehavior.Shake();
        }
    }

    private void UpdateVisual()
    {
        // TODO: clear all objects in objectContainer
        // TODO: Instantiate tileData.objectPrefab into objectContainer

        SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(tileData.Season);
        frameImage.sprite = seasonInfo.tileFrameSprite;
    }
}
