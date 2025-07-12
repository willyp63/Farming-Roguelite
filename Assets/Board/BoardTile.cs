using UnityEngine;
using UnityEngine.Rendering;

public class BoardTile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer frameImage;

    [SerializeField]
    private Transform objectContainer;

    [SerializeField]
    private DeckTile deckTile;

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
    private bool isDragging = false;
    private Vector3 dragStartPosition;

    // Properties
    public int X => x;
    public int Y => y;
    public DeckTile DeckTile => deckTile;
    public TileData TileData => deckTile.tileData;
    public Vector2Int Position => new Vector2Int(x, y);

    public void Start()
    {
        shakeBehavior = GetComponent<ShakeBehavior>();

        // Ensure we have a collider for drag detection
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f); // Match tile size
            collider.isTrigger = false;
        }
    }

    public void Initialize(int xPos, int yPos, DeckTile deckTile)
    {
        SetPosition(xPos, yPos);

        this.deckTile = deckTile;
        pointScore = deckTile.tileData.PointScore;
        multiScore = deckTile.tileData.MultiScore;

        UpdateVisual();
    }

    public void SetPosition(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
        GetComponent<SortingGroup>().sortingOrder = yPos * -10;
    }

    public void Shake()
    {
        if (shakeBehavior != null)
        {
            shakeBehavior.Shake();
        }
    }

    public void BringToFront()
    {
        GetComponent<SortingGroup>().sortingOrder = y * -10 + 1000;
    }

    void OnMouseDown()
    {
        if (!RoundManager.Instance.CanMakeMove)
            return;

        isDragging = true;
        dragStartPosition = transform.position;

        BringToFront();
    }

    void OnMouseDrag()
    {
        if (!isDragging)
            return;

        // Move the tile with the mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        transform.position = mouseWorldPos;
    }

    void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        // Find the tile we're hovering over
        BoardTile targetTile = FindTileUnderMouse();

        // Return to original position
        transform.position = dragStartPosition;

        // Reset sorting order
        SetPosition(x, y);

        if (targetTile != null && targetTile != this && IsAdjacent(targetTile))
        {
            // Try to swap with adjacent tile
            BoardManager.Instance.TrySwapTiles(this, targetTile);
        }
    }

    private BoardTile FindTileUnderMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
        foreach (Collider2D collider in colliders)
        {
            BoardTile tile = collider.GetComponent<BoardTile>();
            if (tile != null && tile != this)
            {
                return tile;
            }
        }

        return null;
    }

    private bool IsAdjacent(BoardTile other)
    {
        int deltaX = Mathf.Abs(x - other.X);
        int deltaY = Mathf.Abs(y - other.Y);

        // Adjacent means sharing an edge (not diagonal)
        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
    }

    private void UpdateVisual()
    {
        // Clear all objects in objectContainer
        if (objectContainer != null)
        {
            for (int i = objectContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(objectContainer.GetChild(i).gameObject);
            }
        }

        TileData tileData = deckTile.tileData;

        // Instantiate tileData.objectPrefab into objectContainer
        if (tileData != null && tileData.ObjectPrefab != null && objectContainer != null)
        {
            Instantiate(tileData.ObjectPrefab, objectContainer);
        }

        // Update frame sprite
        if (frameImage != null && tileData != null)
        {
            SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(tileData.Season);
            if (seasonInfo != null)
            {
                frameImage.color = seasonInfo.color;
            }
        }
    }
}
