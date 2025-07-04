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
        IPointerExitHandler
{
    [Header("Visual Components")]
    [SerializeField]
    private Image highlightRenderer;

    [Header("Visual Settings")]
    [SerializeField]
    private float highlightAlpha = 0.3f;

    // Tile data
    private Vector2Int position;
    private GridTile tile;

    // Events
    public UnityEvent<Vector2Int> OnTilePointerUp;
    public UnityEvent<Vector2Int> OnTileClicked;
    public UnityEvent<Vector2Int> OnTileHovered;
    public UnityEvent<Vector2Int> OnTileExited;

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
}
