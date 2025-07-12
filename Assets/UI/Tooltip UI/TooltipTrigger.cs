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

    [SerializeField]
    private bool useUIEvents = true; // Toggle between UI events and physics raycasting

    [SerializeField]
    private LayerMask raycastLayerMask = -1; // Default to all layers

    [SerializeField]
    private float raycastDistance = 100f;

    public float showDelay = 0.5f;

    private bool isHovering = false;
    private float hoverTimer = 0f;
    private Camera mainCamera;
    private Collider2D objectCollider;
    private Collider objectCollider3D;

    private void Start()
    {
        mainCamera = Camera.main;
        objectCollider = GetComponent<Collider2D>();
        objectCollider3D = GetComponent<Collider>();

        // If no collider is found, add a default one
        if (objectCollider == null && objectCollider3D == null)
        {
            Debug.LogWarning(
                $"TooltipTrigger on {gameObject.name} has no collider. Adding a default BoxCollider2D."
            );
            objectCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useUIEvents)
        {
            StartHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (useUIEvents)
        {
            StopHover();
        }
    }

    private void Update()
    {
        if (!useUIEvents)
        {
            CheckMouseHover();
        }

        if (isHovering)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= showDelay)
            {
                TooltipUIManager.Instance.ShowTooltip(
                    tooltipText,
                    transform.position,
                    tooltipOffset,
                    tooltipDirection,
                    isWorldPosition
                );
                hoverTimer = 0f; // Reset to prevent multiple calls
            }
        }
    }

    private void CheckMouseHover()
    {
        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        bool hitThisObject = false;

        // Check 2D collider
        if (objectCollider != null)
        {
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);
            if (objectCollider.OverlapPoint(worldPoint))
            {
                hitThisObject = true;
            }
        }
        // Check 3D collider
        else if (objectCollider3D != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance, raycastLayerMask))
            {
                if (hit.collider == objectCollider3D)
                {
                    hitThisObject = true;
                }
            }
        }

        if (hitThisObject)
        {
            if (!isHovering)
            {
                StartHover();
            }
        }
        else
        {
            if (isHovering)
            {
                StopHover();
            }
        }
    }

    private void StartHover()
    {
        isHovering = true;
        hoverTimer = 0f;
    }

    private void StopHover()
    {
        isHovering = false;
        hoverTimer = 0f;
        TooltipUIManager.Instance.HideTooltip();
    }

    // Method to update tooltip text dynamically
    public void SetTooltipText(string newText)
    {
        tooltipText = newText;
    }
}
