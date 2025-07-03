using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridUIManager : Singleton<GridUIManager>
{
    [Header("Visual Settings")]
    [SerializeField]
    private Color validPlacementColor = Color.green;

    [SerializeField]
    private Color invalidPlacementColor = Color.red;

    [SerializeField]
    private Color hoverColor = Color.yellow;

    [Header("Grid Visual Elements")]
    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private Transform gridParent;

    // Grid state
    private Dictionary<Vector2Int, GridTileUI> tileUIElements;
    private GridTileUI currentHoveredTile;
    private CardUI currentDraggedCard;
    private GridTileUI currentDraggedTile;

    // Placement state
    private bool isInPlacementMode;
    private bool isInPlaceableMoveMode;
    private HashSet<Vector2Int> validPlacementTiles;
    private HashSet<Vector2Int> validMoveTiles;

    // Events
    public UnityEvent<Vector2Int> OnTileClicked;
    public UnityEvent<Vector2Int> OnTileHovered;
    public UnityEvent<Vector2Int, Card> OnCardPlayedOnTile;
    public UnityEvent<Vector2Int, Placeable> OnPlaceableMoved;

    private void Start()
    {
        // Subscribe to card drag events
        UIManager.Instance.OnCardDragStarted.AddListener(HandleCardDragStarted);
        UIManager.Instance.OnCardDragEnded.AddListener(HandleCardDragEnded);

        if (GridManager.Instance.IsGridGenerated)
        {
            InitializeGridUI();
        }
        else
        {
            GridManager.Instance.OnGridGenerated.AddListener(InitializeGridUI);
        }
    }

    private void InitializeGridUI()
    {
        tileUIElements = new Dictionary<Vector2Int, GridTileUI>();

        // Create UI elements for each tile in the grid
        var grid = GridManager.Instance.GetGrid();
        foreach (var kvp in grid)
        {
            Vector2Int position = kvp.Key;
            GridTile tile = kvp.Value;

            GameObject tileUI = Instantiate(tilePrefab, gridParent);
            GridTileUI tileUIComponent = tileUI.GetComponent<GridTileUI>();

            tileUIComponent.Initialize(position, tile);
            tileUIComponent.transform.localPosition = GridManager.Instance.GetWorldPosition(
                position.x,
                position.y
            );

            tileUIComponent.OnTileHovered.AddListener(HandleTileHovered);
            tileUIComponent.OnTileExited.AddListener(HandleTileExited);
            tileUIComponent.OnPlaceableDragStarted.AddListener(HandlePlaceableDragStarted);
            tileUIComponent.OnPlaceableDragEnded.AddListener(HandlePlaceableDragEnded);

            tileUIElements[position] = tileUIComponent;
        }
    }

    public void OnPlaceableDragStarted(Vector2Int position, Placeable placeable)
    {
        currentDraggedTile = tileUIElements[position];
        EnterPlaceableMoveMode(placeable);
    }

    public void OnPlaceableDragEnded(Vector2Int position, Placeable placeable)
    {
        Debug.Log($"Placeable drag ended for {placeable.PlaceableName} at {position}");
        Debug.Log($"Current hovered tile: {currentHoveredTile}");

        if (currentHoveredTile == null)
        {
            Debug.Log("No hovered tile, canceling move");
            ExitPlaceableMoveMode();
            currentDraggedTile = null;
            return;
        }

        GridTile targetTile = currentHoveredTile.Tile;
        Debug.Log($"Target tile position: {targetTile.Position}");

        if (IsValidMove(targetTile.Position, placeable))
        {
            Debug.Log(
                $"Moving {placeable.PlaceableName} from {placeable.GridTile.Position} to {targetTile.Position}"
            );

            // Move the placeable
            Vector2Int oldPosition = placeable.GridTile.Position;
            Vector2Int newPosition = targetTile.Position;

            GridManager.Instance.MovePlaceable(oldPosition, newPosition);

            OnPlaceableMoved?.Invoke(newPosition, placeable);
        }
        else
        {
            Debug.Log(
                $"Invalid move: {placeable.PlaceableName} cannot be moved to {targetTile.Position}"
            );
        }

        ExitPlaceableMoveMode();
        currentDraggedTile = null;
    }

    private void EnterPlaceableMoveMode(Placeable placeable)
    {
        isInPlaceableMoveMode = true;
        validMoveTiles = GetValidMoveTiles(placeable);

        // Update visual state of all tiles
        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            GridTileUI tileUI = kvp.Value;

            if (validMoveTiles.Contains(position))
            {
                tileUI.SetHighlight(validPlacementColor, true);
            }
            else
            {
                tileUI.SetHighlight(invalidPlacementColor, true);
            }
        }
    }

    private void ExitPlaceableMoveMode()
    {
        isInPlaceableMoveMode = false;
        validMoveTiles?.Clear();

        // Clear all tile highlights
        foreach (var tileUI in tileUIElements.Values)
        {
            tileUI.SetHighlight(Color.white, false);
        }
    }

    private HashSet<Vector2Int> GetValidMoveTiles(Placeable placeable)
    {
        HashSet<Vector2Int> validTiles = new HashSet<Vector2Int>();

        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            if (IsValidMove(position, placeable))
            {
                validTiles.Add(position);
            }
        }

        return validTiles;
    }

    private bool IsValidMove(Vector2Int position, Placeable placeable)
    {
        // Check if the target position is different from current position
        if (placeable.GridTile.Position == position)
            return false;

        // Check if target tile exists and is empty
        GridTile targetTile = GridManager.Instance.GetTile(position);
        if (targetTile == null || targetTile.PlacedObject != null)
            return false;

        if (!placeable.ValidTileTypes.Contains(targetTile.Tile.TileType))
            return false;

        return true;
    }

    private void HandleCardDragStarted(CardUI cardUI)
    {
        currentDraggedCard = cardUI;
        EnterPlacementMode(cardUI.GetCard());
    }

    private void HandleCardDragEnded(CardUI cardUI)
    {
        Debug.Log("Card dragged ended");
        Debug.Log(currentHoveredTile);

        if (currentHoveredTile == null)
            return;

        GridTile tile = currentHoveredTile.Tile;

        if (
            IsValidPlacement(tile.Position, cardUI.GetCard())
            && CoinManager.Instance.HasEnoughCoins(cardUI.GetCard().Cost)
        )
        {
            Card card = cardUI.GetCard();

            // Play the card
            card.CardEffect.ApplyEffect(tile.Position, tile);
            CoinManager.Instance.SpendCoins(card.Cost);
            HandManager.Instance.RemoveCard(card);
            FloatingTextManager.Instance.SpawnText(
                $"-{card.Cost} coins",
                tile.transform.position,
                Color.yellow
            );

            OnCardPlayedOnTile?.Invoke(tile.Position, card);
        }

        ExitPlacementMode();
        currentDraggedCard = null;
    }

    private void EnterPlacementMode(Card card)
    {
        isInPlacementMode = true;
        validPlacementTiles = GetValidPlacementTiles(card);

        // Update visual state of all tiles
        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            GridTileUI tileUI = kvp.Value;

            if (validPlacementTiles.Contains(position))
            {
                tileUI.SetHighlight(validPlacementColor, true);
            }
            else
            {
                tileUI.SetHighlight(invalidPlacementColor, true);
            }
        }
    }

    private void ExitPlacementMode()
    {
        isInPlacementMode = false;
        validPlacementTiles.Clear();

        // Clear all tile highlights
        foreach (var tileUI in tileUIElements.Values)
        {
            tileUI.SetHighlight(Color.white, false);
        }
    }

    private HashSet<Vector2Int> GetValidPlacementTiles(Card card)
    {
        HashSet<Vector2Int> validTiles = new HashSet<Vector2Int>();

        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            if (IsValidPlacement(position, card))
            {
                validTiles.Add(position);
            }
        }

        return validTiles;
    }

    private bool IsValidPlacement(Vector2Int position, Card card)
    {
        // Check if player has enough coins
        if (CoinManager.Instance.GetCoins() < card.Cost)
            return false;

        // Check card-specific placement rules
        GridTile tile = GridManager.Instance.GetTile(position);
        if (tile == null)
            return false;

        // This would delegate to the card's effect to determine validity
        return card.CardEffect.IsValidPlacement(position, tile);
    }

    private void HandleTileHovered(Vector2Int position)
    {
        // Update hovered tile (only if not in placement mode or placeable move mode)
        if (!isInPlacementMode && !isInPlaceableMoveMode && currentHoveredTile != null)
            currentHoveredTile.SetHighlight(Color.white, false);

        currentHoveredTile = tileUIElements[position];
        if (!isInPlacementMode && !isInPlaceableMoveMode)
            currentHoveredTile.SetHighlight(hoverColor, true);

        OnTileHovered?.Invoke(position);
    }

    private void HandleTileExited(Vector2Int position)
    {
        if (currentHoveredTile != null)
        {
            if (!isInPlacementMode && !isInPlaceableMoveMode)
                currentHoveredTile.SetHighlight(Color.white, false);
            currentHoveredTile = null;
        }
    }

    private void HandlePlaceableDragStarted(Vector2Int position, Placeable placeable)
    {
        OnPlaceableDragStarted(position, placeable);
    }

    private void HandlePlaceableDragEnded(Vector2Int position, Placeable placeable)
    {
        OnPlaceableDragEnded(position, placeable);
    }
}
