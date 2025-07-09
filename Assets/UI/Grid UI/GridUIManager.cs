using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GridUIManager : Singleton<GridUIManager>
{
    [Header("Visual Settings")]
    [SerializeField]
    private Color validPlacementColor = Color.green;

    [SerializeField]
    private Color movablePlaceableColor = Color.magenta;

    [Header("Scoring Line Settings")]
    [SerializeField]
    private bool showScoringLines = true;

    [SerializeField]
    private Color scoringLineColor = Color.white;

    [SerializeField]
    private float scoringLineWidth = 0.05f;

    [SerializeField]
    private Sprite circleSprite;

    [SerializeField]
    private float scoringLineCircleSize = 0.1f;

    [SerializeField]
    private float scoringLineOutlineWidth = 0.02f;

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

    // Scoring line state
    private List<GameObject> currentScoringLines = new();

    // Events
    [NonSerialized]
    public UnityEvent<Vector2Int> OnTileHovered = new();

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
        GridManager.Instance.OnGridChanged.AddListener(UpdateMovableHighlights);
        GridManager.Instance.OnGridChanged.AddListener(UpdateAllSeasonUI);
        GridManager.Instance.OnGridChanged.AddListener(UpdateAllTileTooltips);
        GridManager.Instance.OnGridChanged.AddListener(UpdateScoringLines);
        RoundManager.Instance.OnCanPlayCardsChange.AddListener(UpdateMovableHighlights);
        UpdateMovableHighlights();
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

            RoundManager.Instance.TryPlayCard(card, tile);
        }

        ExitPlacementMode();
        currentDraggedCard = null;
    }

    private void HandlePlaceableDragStarted(Vector2Int position, Placeable placeable)
    {
        // Only allow dragging if placeable can move
        if (!placeable.CanMove)
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
                tileUI.SetHighlight(false, Color.white);
            }
        }
    }

    private void ExitPlacementMode()
    {
        isInPlacementMode = false;
        validPlacementTiles.Clear();

        foreach (var kvp in tileUIElements)
        {
            GridTileUI tileUI = kvp.Value;
            tileUI.SetHighlight(false, Color.white);
        }
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
                tileUI.SetHighlight(false, Color.white);
            }
        }
    }

    private void ExitPlaceableMoveMode()
    {
        isInPlaceableMoveMode = false;
        validMoveTiles.Clear();

        foreach (var kvp in tileUIElements)
        {
            GridTileUI tileUI = kvp.Value;
            tileUI.SetHighlight(false, Color.white);
        }
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

    private void UpdateMovableHighlights()
    {
        foreach (var kvp in seasonUIElements)
        {
            Vector2Int position = kvp.Key;
            TileSeasonUI seasonUI = kvp.Value;

            Placeable placeable = GridManager.Instance.GetPlaceableAtPosition(position);

            if (!RoundManager.Instance.CanPlayCards || placeable == null)
            {
                seasonUI.SetHighlight(false, Color.white);
                continue;
            }

            seasonUI.SetHighlight(placeable.CanMove, movablePlaceableColor);
        }
    }

    private void UpdateAllTileTooltips()
    {
        foreach (var kvp in tileUIElements)
        {
            GridTileUI tileUI = kvp.Value;
            tileUI.UpdateTooltip();
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
        SeasonType season = tile.PlacedObject != null ? tile.PlacedObject.Season : SeasonType.None;
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

    private void UpdateScoringLines()
    {
        if (!showScoringLines)
            return;

        // Clear all previous scoring lines
        ClearScoringLines();

        foreach (var scoringLine in GridManager.Instance.ScoringLines)
        {
            DrawScoringLine(scoringLine);
        }
    }

    private void ClearScoringLines()
    {
        foreach (var line in currentScoringLines)
        {
            if (line != null)
            {
                DestroyImmediate(line);
            }
        }
        currentScoringLines.Clear();
    }

    private void DrawScoringLine(ScoringLine scoringLine)
    {
        if (scoringLine.tiles == null || scoringLine.tiles.Count < 2)
            return;

        // Calculate line position and rotation
        Vector2 firstTilePos = scoringLine.tiles[0].transform.position;
        Vector2 lastTilePos = scoringLine.tiles[scoringLine.tiles.Count - 1].transform.position;

        // Convert world positions to canvas local positions
        Vector2 firstCanvasPos = upperCanvas.InverseTransformPoint(firstTilePos);
        Vector2 lastCanvasPos = upperCanvas.InverseTransformPoint(lastTilePos);

        // Calculate line properties
        Vector2 direction = (lastCanvasPos - firstCanvasPos).normalized;
        float distance = Vector2.Distance(firstCanvasPos, lastCanvasPos);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Create black outline line first
        GameObject outlineLineObject = new GameObject($"ScoringLine_Outline_{scoringLine.pattern}");
        outlineLineObject.transform.SetParent(upperCanvas, false);

        Image outlineLineImage = outlineLineObject.AddComponent<Image>();
        outlineLineImage.color = Color.black;
        outlineLineImage.raycastTarget = false;

        // Set outline line position and rotation
        outlineLineObject.transform.localPosition = (firstCanvasPos + lastCanvasPos) / 2f;
        outlineLineObject.transform.localRotation = Quaternion.Euler(0, 0, angle);

        // Set outline line size (slightly bigger for outline effect)
        RectTransform outlineRectTransform = outlineLineObject.GetComponent<RectTransform>();
        outlineRectTransform.sizeDelta = new Vector2(
            distance,
            scoringLineWidth + scoringLineOutlineWidth * 2
        );
        outlineRectTransform.pivot = new Vector2(0.5f, 0.5f);

        currentScoringLines.Add(outlineLineObject);

        // Create black outline circles first
        if (circleSprite != null)
        {
            // Start circle outline
            GameObject startCircleOutline = new GameObject(
                $"ScoringLine_Start_Outline_{scoringLine.pattern}"
            );
            startCircleOutline.transform.SetParent(upperCanvas, false);
            startCircleOutline.transform.localPosition = firstCanvasPos;

            Image startCircleOutlineImage = startCircleOutline.AddComponent<Image>();
            startCircleOutlineImage.sprite = circleSprite;
            startCircleOutlineImage.color = Color.black;
            startCircleOutlineImage.raycastTarget = false;

            RectTransform startCircleOutlineRect = startCircleOutline.GetComponent<RectTransform>();
            startCircleOutlineRect.sizeDelta = new Vector2(
                scoringLineCircleSize + scoringLineOutlineWidth * 2,
                scoringLineCircleSize + scoringLineOutlineWidth * 2
            );
            startCircleOutlineRect.pivot = new Vector2(0.5f, 0.5f);

            currentScoringLines.Add(startCircleOutline);

            // End circle outline
            GameObject endCircleOutline = new GameObject(
                $"ScoringLine_End_Outline_{scoringLine.pattern}"
            );
            endCircleOutline.transform.SetParent(upperCanvas, false);
            endCircleOutline.transform.localPosition = lastCanvasPos;

            Image endCircleOutlineImage = endCircleOutline.AddComponent<Image>();
            endCircleOutlineImage.sprite = circleSprite;
            endCircleOutlineImage.color = Color.black;
            endCircleOutlineImage.raycastTarget = false;

            RectTransform endCircleOutlineRect = endCircleOutline.GetComponent<RectTransform>();
            endCircleOutlineRect.sizeDelta = new Vector2(
                scoringLineCircleSize + scoringLineOutlineWidth * 2,
                scoringLineCircleSize + scoringLineOutlineWidth * 2
            );
            endCircleOutlineRect.pivot = new Vector2(0.5f, 0.5f);

            currentScoringLines.Add(endCircleOutline);
        }

        // Create colored circles on top
        if (circleSprite != null)
        {
            // Start circle
            GameObject startCircle = new GameObject($"ScoringLine_Start_{scoringLine.pattern}");
            startCircle.transform.SetParent(upperCanvas, false);
            startCircle.transform.localPosition = firstCanvasPos;

            Image startCircleImage = startCircle.AddComponent<Image>();
            startCircleImage.sprite = circleSprite;
            startCircleImage.color = scoringLineColor;
            startCircleImage.raycastTarget = false;

            RectTransform startCircleRect = startCircle.GetComponent<RectTransform>();
            startCircleRect.sizeDelta = new Vector2(scoringLineCircleSize, scoringLineCircleSize);
            startCircleRect.pivot = new Vector2(0.5f, 0.5f);

            currentScoringLines.Add(startCircle);

            // End circle
            GameObject endCircle = new GameObject($"ScoringLine_End_{scoringLine.pattern}");
            endCircle.transform.SetParent(upperCanvas, false);
            endCircle.transform.localPosition = lastCanvasPos;

            Image endCircleImage = endCircle.AddComponent<Image>();
            endCircleImage.sprite = circleSprite;
            endCircleImage.color = scoringLineColor;
            endCircleImage.raycastTarget = false;

            RectTransform endCircleRect = endCircle.GetComponent<RectTransform>();
            endCircleRect.sizeDelta = new Vector2(scoringLineCircleSize, scoringLineCircleSize);
            endCircleRect.pivot = new Vector2(0.5f, 0.5f);

            currentScoringLines.Add(endCircle);
        }

        // Create colored line on top
        GameObject lineObject = new GameObject($"ScoringLine_{scoringLine.pattern}");
        lineObject.transform.SetParent(upperCanvas, false);

        Image lineImage = lineObject.AddComponent<Image>();
        lineImage.color = scoringLineColor;
        lineImage.raycastTarget = false;

        // Set colored line position and rotation
        lineObject.transform.localPosition = (firstCanvasPos + lastCanvasPos) / 2f;
        lineObject.transform.localRotation = Quaternion.Euler(0, 0, angle);

        // Set colored line size
        RectTransform rectTransform = lineObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(distance, scoringLineWidth);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        currentScoringLines.Add(lineObject);
    }

    public TileSeasonUI GetTileSeasonUI(Vector2Int position)
    {
        return seasonUIElements[position];
    }
}
