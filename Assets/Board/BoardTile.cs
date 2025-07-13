using UnityEngine;
using UnityEngine.Rendering;

public class BoardTile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer frameImage;

    [SerializeField]
    private Transform unitContainer;

    [SerializeField]
    private DeckTile deckTile;

    [SerializeField]
    private int x;

    [SerializeField]
    private int y;

    private TooltipTrigger tooltipTrigger;
    private ShakeBehavior shakeBehavior;
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private bool isDraggable;
    private bool isClickEnabled = false;

    // Properties
    public int X => x;
    public int Y => y;
    public DeckTile DeckTile => deckTile;
    public SeasonType Season => deckTile.Season;
    public Vector2Int Position => new Vector2Int(x, y);

    public void Initialize(int xPos, int yPos, DeckTile deckTile, bool isDraggable = true)
    {
        shakeBehavior = GetComponent<ShakeBehavior>();
        tooltipTrigger = GetComponent<TooltipTrigger>();

        SetPosition(xPos, yPos);

        this.deckTile = deckTile;
        this.isDraggable = isDraggable;

        UpdateVisual();
    }

    public void SetClickEnabled(bool enabled)
    {
        isClickEnabled = enabled;
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

    public void SetTooltipEnabled(bool enabled)
    {
        if (tooltipTrigger != null)
        {
            tooltipTrigger.enabled = enabled;
        }
    }

    public void BringToFront()
    {
        GetComponent<SortingGroup>().sortingOrder = y * -10 + 1000;
    }

    void OnMouseDown()
    {
        // Handle click for deck selection mode
        if (isClickEnabled)
        {
            DeckBoardManager.Instance.OnTileClicked(this);
            return;
        }

        if (!RoundManager.Instance.CanMakeMove || !isDraggable)
            return;

        isDragging = true;
        dragStartPosition = transform.position;

        BringToFront();

        BoardManager.Instance.DisableTooltips();
        TooltipUIManager.Instance.HideTooltip();
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

        BoardManager.Instance.EnableTooltips();

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

    public void UpdateVisual()
    {
        if (tooltipTrigger != null)
        {
            tooltipTrigger.SetTooltipText(deckTile.GetTooltipText());
        }

        // Clear all objects in unitContainer
        if (unitContainer != null)
        {
            for (int i = unitContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(unitContainer.GetChild(i).gameObject);
            }
        }

        // Instantiate unit prefab
        if (deckTile.Unit != null && deckTile.Unit.Data.UnitPrefab != null && unitContainer != null)
        {
            Instantiate(deckTile.Unit.Data.UnitPrefab, unitContainer);
        }

        // Update frame sprite
        if (frameImage != null)
        {
            SeasonInfo seasonInfo = SeasonManager.GetSeasonInfo(Season);
            if (seasonInfo != null)
            {
                frameImage.color = seasonInfo.color;
            }
        }
    }
}
