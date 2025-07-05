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
    private Color hoverColor = Color.yellow;

    [Header("Grid Visual Elements")]
    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private Transform gridParent;

    // Grid state
    private Dictionary<Vector2Int, GridTileUI> tileUIElements = new();
    private GridTileUI currentHoveredTile;
    private CardUI currentDraggedCard;

    // Placement state
    private bool isInPlacementMode;
    private HashSet<Vector2Int> validPlacementTiles;

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

        GridManager.Instance.OnGridChanged.AddListener(UpdateAllTileScores);

        if (GridManager.Instance.IsGridGenerated)
            InitializeGridUI();
        else
            GridManager.Instance.OnGridGenerated.AddListener(InitializeGridUI);
    }

    private void InitializeGridUI()
    {
        tileUIElements = new();

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

            tileUIElements[position] = tileUIComponent;
        }
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

        if (IsValidPlacement(tile.Position, cardUI.GetCard()))
        {
            Card card = cardUI.GetCard();

            // Play the card
            card.CardEffect.ApplyEffect(tile.Position, tile, card);

            // Update the score of all tiles
            UpdateAllTileScores();

            // Remove the card from the player's hand
            CardManager.Instance.DiscardCard(card);

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
        // Check card-specific placement rules
        GridTile tile = GridManager.Instance.GetTile(position);
        return card.CardEffect.IsValidPlacement(position, tile, card);
    }

    private void HandleTileHovered(Vector2Int position)
    {
        // Update hovered tile (only if not in placement mode or placeable move mode)
        if (!isInPlacementMode && currentHoveredTile != null)
            currentHoveredTile.SetHighlight(Color.white, false);

        currentHoveredTile = tileUIElements[position];
        if (!isInPlacementMode)
            currentHoveredTile.SetHighlight(hoverColor, true);

        OnTileHovered?.Invoke(position);
    }

    private void HandleTileExited(Vector2Int position)
    {
        if (currentHoveredTile != null)
        {
            if (!isInPlacementMode)
                currentHoveredTile.SetHighlight(Color.white, false);
            currentHoveredTile = null;
        }
    }

    private void UpdateAllTileScores()
    {
        foreach (var kvp in tileUIElements)
        {
            Vector2Int position = kvp.Key;
            GridTile tile = GridManager.Instance.GetTile(position);
            GridTileUI tileUI = kvp.Value;

            if (tile.PlacedObject != null)
            {
                tileUI.SetScore(tile.PlacedObject.Score);
            }
            else
            {
                tileUI.SetScore(0);
            }
        }
    }
}
