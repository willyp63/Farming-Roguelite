using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridTileUI
    : MonoBehaviour,
        IPointerUpHandler,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
{
    [Header("Visual Components")]
    [SerializeField]
    private Image highlightImage;

    [SerializeField]
    private Image hoverImage;

    [Header("Visual Settings")]
    [SerializeField]
    private float highlightAlpha = 0.5f;

    [SerializeField]
    private float hoverAlpha = 0.5f;

    [SerializeField]
    private float dragScale = 1.2f;

    private TooltipTrigger tooltipTrigger;

    // Tile data
    private Vector2Int position;
    private GridTile tile;

    // Events
    [NonSerialized]
    public UnityEvent<Vector2Int> OnTilePointerUp = new();

    [NonSerialized]
    public UnityEvent<Vector2Int> OnTileClicked = new();

    [NonSerialized]
    public UnityEvent<Vector2Int> OnTileHovered = new();

    [NonSerialized]
    public UnityEvent<Vector2Int> OnTileExited = new();

    [NonSerialized]
    public UnityEvent<Vector2Int, Placeable> OnPlaceableDragStarted = new();

    [NonSerialized]
    public UnityEvent<Vector2Int, Placeable> OnPlaceableDragEnded = new();

    // Public properties
    public Vector2Int Position => position;
    public GridTile Tile => tile;

    // Drag state
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    public void Initialize(Vector2Int pos, GridTile gridTile)
    {
        position = pos;
        tile = gridTile;
        SetHighlight(false, Color.white);
        SetHover(false);

        tooltipTrigger = GetComponent<TooltipTrigger>();
        UpdateTooltip();
    }

    public void UpdateTooltip()
    {
        if (tile.PlacedObject != null)
        {
            tooltipTrigger.SetTooltipText(tile.PlacedObject.GetTooltipText());
            tooltipTrigger.SetPlaceableFamily(tile.PlacedObject.PlaceableFamily);
        }
        else
        {
            tooltipTrigger.SetTooltipText("");
            tooltipTrigger.SetPlaceableFamily(PlaceableFamily.None);
        }
    }

    public void SetHighlight(bool isHighlighted, Color color)
    {
        if (isHighlighted)
            highlightImage.color = new Color(color.r, color.g, color.b, highlightAlpha);
        else
            highlightImage.color = new Color(0f, 0f, 0f, 0f);
    }

    private void SetHover(bool isHovered)
    {
        if (isHovered)
            hoverImage.color = new Color(1f, 1f, 1f, hoverAlpha);
        else
            hoverImage.color = new Color(1f, 1f, 1f, 0f);
    }

    // Mouse interaction handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClicked?.Invoke(position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
        OnTileHovered?.Invoke(position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false);
        OnTileExited?.Invoke(position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnTilePointerUp?.Invoke(position);
    }

    // Drag handlers for placeable movement
    public void OnBeginDrag(PointerEventData eventData)
    {
        Placeable placedObject = tile.PlacedObject;

        // Only allow dragging if there's a movable placeable on this tile
        if (placedObject == null || !placedObject.IsMovable || placedObject.HasMovedToday)
        {
            Debug.Log(
                $"Cannot drag: placeable={placedObject}, isMovable={placedObject?.IsMovable}"
            );
            return;
        }

        Debug.Log($"Starting drag of {placedObject.PlaceableName} from position {position}");
        isDragging = true;
        originalPosition = placedObject.transform.position;
        originalScale = placedObject.transform.localScale;

        // Scale down slightly while dragging
        placedObject.transform.localScale = Vector3.one * dragScale;

        OnPlaceableDragStarted?.Invoke(position, placedObject);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Placeable placedObject = tile.PlacedObject;

        if (isDragging && placedObject != null && placedObject.IsMovable)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            Debug.Log($"Dragging {placedObject.PlaceableName} to position {worldPosition}");
            Debug.Log($"Tile position: {tile.Position}");
            Debug.Log($"Event position: {eventData.position}");
            worldPosition.z = 0;
            placedObject.transform.position = worldPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Placeable placedObject = tile.PlacedObject;

        Debug.Log($"Ending drag of {placedObject?.PlaceableName} from position {position}");
        isDragging = false;

        // Only reset position and scale if the placeable is still on this tile
        if (placedObject != null && placedObject.GridTile == tile)
        {
            placedObject.transform.position = originalPosition;
            placedObject.transform.localScale = originalScale;
        }

        OnPlaceableDragEnded?.Invoke(position, placedObject);
    }
}
