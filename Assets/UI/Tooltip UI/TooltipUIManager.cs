using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TooltipDirection
{
    Right,
    Left,
    Above,
    Below,
}

public class TooltipUIManager : Singleton<TooltipUIManager>
{
    [Header("Tooltip UI References")]
    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    public Canvas canvas;

    protected override void Awake()
    {
        base.Awake();

        HideTooltip();
    }

    public void HideTooltip()
    {
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    public void ShowTooltip(
        string content,
        Vector3 position,
        float offset,
        TooltipDirection direction,
        bool isWorldPosition = false
    )
    {
        if (string.IsNullOrEmpty(content))
            return;

        if (tooltipObject == null || tooltipText == null || canvas == null)
            return;

        // Set pivot based on direction
        RectTransform rectTransform = tooltipObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            switch (direction)
            {
                case TooltipDirection.Above:
                    rectTransform.pivot = new Vector2(0.5f, 0f); // top-center
                    break;
                case TooltipDirection.Below:
                    rectTransform.pivot = new Vector2(0.5f, 1f); // bottom-center
                    break;
                case TooltipDirection.Right:
                    rectTransform.pivot = new Vector2(0f, 0.5f); // left-center
                    break;
                case TooltipDirection.Left:
                    rectTransform.pivot = new Vector2(1f, 0.5f); // right-center
                    break;
            }
        }

        // Calculate position based on direction and offset
        Vector3 offsetVector = Vector3.zero;
        switch (direction)
        {
            case TooltipDirection.Above:
                offsetVector = Vector3.up * offset;
                break;
            case TooltipDirection.Below:
                offsetVector = Vector3.down * offset;
                break;
            case TooltipDirection.Right:
                offsetVector = Vector3.right * offset;
                break;
            case TooltipDirection.Left:
                offsetVector = Vector3.left * offset;
                break;
        }

        Vector3 targetPosition = position + offsetVector;

        // Convert world position to screen position if needed
        if (isWorldPosition)
        {
            // Convert world position to screen position
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetPosition);

            // Convert screen position to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPoint
            );

            tooltipObject.transform.localPosition = localPoint;
        }
        else
        {
            // Set the tooltip position directly (existing behavior)
            tooltipObject.transform.position = targetPosition;
        }

        // Set tooltip text content
        tooltipText.text = content;

        // Activate the tooltip
        tooltipObject.SetActive(true);
    }
}
