using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Content")]
    [TextArea(3, 10)]
    public string tooltipText = "Default tooltip text";

    [SerializeField]
    private TooltipDirection tooltipDirection = TooltipDirection.Above;

    [SerializeField]
    private float tooltipOffset = 100f;

    [Header("Settings")]
    public float showDelay = 0.5f;

    private bool isHovering = false;
    private float hoverTimer = 0f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        hoverTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        hoverTimer = 0f;
        TooltipUIManager.Instance.HideTooltip();
    }

    private void Update()
    {
        if (isHovering)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= showDelay)
            {
                TooltipUIManager.Instance.ShowTooltip(
                    tooltipText,
                    transform.position,
                    tooltipOffset,
                    tooltipDirection
                );
                hoverTimer = 0f; // Reset to prevent multiple calls
            }
        }
    }

    // Method to update tooltip text dynamically
    public void SetTooltipText(string newText)
    {
        tooltipText = newText;
    }
}
