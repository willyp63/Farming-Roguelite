using System.Collections;
using System.Collections.Generic;
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
    private Image highlightRenderer;

    [Header("Visual Settings")]
    [SerializeField]
    private float highlightAlpha = 0.3f;

    [SerializeField]
    private float dragScale = 1.2f;

    // Tile data
    private Vector2Int position;
    private GridTile tile;

    // Drag state
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    // Events
    public UnityEvent<Vector2Int> OnTilePointerUp;
    public UnityEvent<Vector2Int> OnTileClicked;
    public UnityEvent<Vector2Int> OnTileHovered;
    public UnityEvent<Vector2Int> OnTileExited;
    public UnityEvent<Vector2Int, Placeable> OnPlaceableDragStarted;
    public UnityEvent<Vector2Int, Placeable> OnPlaceableDragEnded;

    // Public properties
    public Vector2Int Position => position;
    public GridTile Tile => tile;

    public void Initialize(Vector2Int pos, GridTile gridTile)
    {
        position = pos;
        tile = gridTile;
        SetHighlight(Color.white, false);
    }

    public void SetHighlight(Color color, bool highlight)
    {
        if (highlightRenderer != null)
        {
            Color highlightColor = color;
            highlightColor.a = highlight ? highlightAlpha : 0f;
            highlightRenderer.color = highlightColor;
        }
    }

    // Mouse interaction handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClicked?.Invoke(position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnTileHovered?.Invoke(position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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
        if (placedObject == null || !placedObject.IsMovable)
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

        // Scale down slightly while dragging and change highlight
        placedObject.transform.localScale = Vector3.one * dragScale;
        SetHighlight(Color.cyan, true); // Cyan highlight to show it's being dragged

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

        Debug.Log($"Ending drag of {placedObject.PlaceableName} from position {position}");
        isDragging = false;
        placedObject.transform.position = originalPosition;
        placedObject.transform.localScale = originalScale;

        // Reset highlight
        SetHighlight(Color.white, false);

        OnPlaceableDragEnded?.Invoke(position, placedObject);
    }
}
