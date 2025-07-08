using System;
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
    private Color movablePlaceableColor = Color.magenta;

    [Header("Grid Visual Elements")]
    [SerializeField]
    private GridTileUI tileUIPrefab;

    [SerializeField]
    private TileSeasonUI seasonUIPrefab;

    [SerializeField]
    private Transform lowerCanvas;

    [SerializeField]
    private Transform upperCanvas;

    // Grid state
    private Dictionary<Vector2Int, GridTileUI> tileUIElements = new();
    private Dictionary<Vector2Int, TileSeasonUI> seasonUIElements = new();
    private GridTileUI currentHoveredTile;
    private CardUI currentDraggedCard;

    // Placement state
    private bool isInPlacementMode;
    private HashSet<Vector2Int> validPlacementTiles;

    // Placeable movement state
    private bool isInPlaceableMoveMode;
    private Placeable currentDraggedPlaceable;
    private Vector2Int currentDraggedPlaceablePosition;
    private HashSet<Vector2Int> validMoveTiles;

    // Events
    [NonSerialized]
    public UnityEvent<Vector2Int> OnTileHovered = new();

    [NonSerialized]
    public UnityEvent<Vector2Int, Card> OnCardPlayedOnTile = new();

    private void Start()
    {
        // Subscribe to card drag events
        UIManager.Instance.OnCardDragStarted.AddListener(HandleCardDragStarted);
        UIManager.Instance.OnCardDragEnded.AddListener(HandleCardDragEnded);
    }

    public void InitializeGridUI()
    {
        tileUIElements = new();
        seasonUIElements = new();

        // Create UI elements for each tile in the grid
        var grid = GridManager.Instance.GetGrid();
        foreach (var kvp in grid)
        {
            Vector2Int position = kvp.Key;
            GridTile tile = kvp.Value;

            // Create tile UI
            GameObject tileUI = Instantiate(tileUIPrefab.gameObject, lowerCanvas);
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

            // Create season UI
            GameObject seasonUI = Instantiate(seasonUIPrefab.gameObject, upperCanvas);
            TileSeasonUI seasonUIComponent = seasonUI.GetComponent<TileSeasonUI>();

            seasonUI.transform.localPosition = GridManager.Instance.GetWorldPosition(
                position.x,
                position.y
            );

            seasonUIElements[position] = seasonUIComponent;
        }

        // Subscribe to grid changes to update movable placeable highlights and season UI
        GridManager.Instance.OnGridChanged.AddListener(UpdateTileHighlights);
        GridManager.Instance.OnGridChanged.AddListener(UpdateAllSeasonUI);
        UpdateTileHighlights();
        UpdateAllSeasonUI();
    }

    private void HandleCardDragStarted(CardUI cardUI)
    {
        currentDraggedCard = cardUI;
        EnterPlacementMode(cardUI.GetCard());
    }

    private void HandleCardDragEnded(CardUI cardUI)
    {
        if (currentHoveredTile != null)
        {
            GridTile tile = currentHoveredTile.Tile;
            Card card = cardUI.GetCard();

            if (
                IsValidPlacement(tile.Position, card)
                && PlayerManager.Instance.HasEnoughEnergy(card.EnergyCost)
            )
            {
                // Play card
                card.PlayCard(tile);

                if (card.EnergyCost > 0)
                    PlayerManager.Instance.SpendEnergy(card.EnergyCost);

                CardManager.Instance.DiscardCard(card);

                OnCardPlayedOnTile?.Invoke(tile.Position, card);
            }
        }

        ExitPlacementMode();
        currentDraggedCard = null;
    }

    private void HandlePlaceableDragStarted(Vector2Int position, Placeable placeable)
    {
        // Only allow dragging if placeable hasn't moved today
        if (placeable.HasMovedToday)
        {
            Debug.Log($"Cannot move {placeable.PlaceableName}: already moved today");
            return;
        }

        currentDraggedPlaceable = placeable;
        currentDraggedPlaceablePosition = position;
        EnterPlaceableMoveMode(placeable);
    }

    private void HandlePlaceableDragEnded(Vector2Int position, Placeable placeable)
    {
        if (currentHoveredTile != null && currentDraggedPlaceable != null)
        {
            Vector2Int targetPosition = currentHoveredTile.Position;

            // Attempt to move the placeable
            bool moveSuccess = GridManager.Instance.MovePlaceable(
                currentDraggedPlaceablePosition,
                targetPosition
            );

            if (moveSuccess)
            {
                Debug.Log(
                    $"Successfully moved {placeable.PlaceableName} from {currentDraggedPlaceablePosition} to {targetPosition}"
                );
            }
            else
            {
                Debug.Log(
                    $"Failed to move {placeable.PlaceableName} from {currentDraggedPlaceablePosition} to {targetPosition}"
                );
            }
        }

        ExitPlaceableMoveMode();
        currentDraggedPlaceable = null;
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
                tileUI.SetHighlight(true, validPlacementColor);
            }
            else
            {
                tileUI.SetHighlight(true, invalidPlacementColor);
            }
        }
    }

    private void ExitPlacementMode()
    {
        isInPlacementMode = false;
        validPlacementTiles.Clear();

        // Clear all tile highlights and restore movable placeable highlights
        UpdateTileHighlights();
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
                tileUI.SetHighlight(true, validPlacementColor);
            }
            else
            {
                tileUI.SetHighlight(true, invalidPlacementColor);
            }
        }
    }

    private void ExitPlaceableMoveMode()
    {
        isInPlaceableMoveMode = false;
        validMoveTiles.Clear();

        // Clear all tile highlights and restore movable placeable highlights
        UpdateTileHighlights();
    }

    private HashSet<Vector2Int> GetValidMoveTiles(Placeable placeable)
    {
        HashSet<Vector2Int> validTiles = new HashSet<Vector2Int>();

        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            if (IsValidMoveTarget(position, placeable))
            {
                validTiles.Add(position);
            }
        }

        return validTiles;
    }

    private bool IsValidMoveTarget(Vector2Int position, Placeable placeable)
    {
        GridTile tile = GridManager.Instance.GetTile(position);

        // Check if tile is empty
        if (tile.PlacedObject != null)
            return false;

        // Check if tile type is allowed
        if (
            placeable.AllowedTileTypes.Count > 0
            && !placeable.AllowedTileTypes.Contains(tile.Tile.TileType)
        )
            return false;

        return true;
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
        // Check card-specific placement rules
        GridTile tile = GridManager.Instance.GetTile(position);
        return card.IsValidPlacement(tile);
    }

    private void UpdateTileHighlights()
    {
        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            GridTileUI tileUI = kvp.Value;

            Placeable placeable = GridManager.Instance.GetPlaceableAtPosition(position);
            bool hasMovablePlaceable =
                placeable != null && placeable.IsMovable && !placeable.HasMovedToday;

            if (hasMovablePlaceable)
            {
                tileUI.SetHighlight(true, movablePlaceableColor);
            }
            else
            {
                tileUI.SetHighlight(false, Color.white);
            }
        }
    }

    private void UpdateAllSeasonUI()
    {
        foreach (var kvp in seasonUIElements)
        {
            Vector2Int position = kvp.Key;
            TileSeasonUI seasonUI = kvp.Value;

            UpdateSeasonUI(position, seasonUI);
        }
    }

    private void UpdateSeasonUI(Vector2Int position, TileSeasonUI seasonUI)
    {
        GridTile tile = GridManager.Instance.GetTile(position);
        Season season = tile.PlacedObject != null ? tile.PlacedObject.Season : Season.Neutral;
        seasonUI.SetSeason(season);
    }

    private void HandleTileHovered(Vector2Int position)
    {
        currentHoveredTile = tileUIElements[position];

        OnTileHovered?.Invoke(position);
    }

    private void HandleTileExited(Vector2Int position)
    {
        currentHoveredTile = null;
    }
}
