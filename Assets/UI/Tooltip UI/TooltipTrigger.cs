using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]
    public string tooltipText = "Default tooltip text";

    [SerializeField]
    private TooltipDirection tooltipDirection = TooltipDirection.Above;

    [SerializeField]
    private float tooltipOffset = 100f;

    [SerializeField]
    private bool isWorldPosition = false;

    public float showDelay = 0.5f;

    private bool isHovering = false;
    private float hoverTimer = 0f;

    private PlaceableFamily placeableFamily = PlaceableFamily.None;

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
                    placeableFamily,
                    transform.position,
                    tooltipOffset,
                    tooltipDirection,
                    isWorldPosition
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

    public void SetPlaceableFamily(PlaceableFamily family)
    {
        placeableFamily = family;
    }
}
