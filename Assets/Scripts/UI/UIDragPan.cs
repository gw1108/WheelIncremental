using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Enables panning of a RectTransform by holding left mouse button and dragging,
/// pressing the arrow keys, and zooming with the mouse scroll wheel.
/// Attach to a UI element that fills the pannable area (e.g. a background Image).
/// Assign panTarget to the RectTransform that should move and scale.
/// IDragHandler and IScrollHandler both bubble up the hierarchy, so interaction
/// works whether the pointer is over empty space or a child element.
/// </summary>
public class UIDragPan : MonoBehaviour, IDragHandler, IScrollHandler
{
    [SerializeField] private RectTransform panTarget;

    [Tooltip("Minimum pixels of panTarget that must remain visible inside the viewport on each edge.")]
    [SerializeField] private float edgePaddingX = 100f;
    [SerializeField] private float edgePaddingY = 100f;

    [Tooltip("Pixels per second the panTarget moves while an arrow key is held.")]
    [SerializeField] private float keyPanSpeed = 400f;

    [Tooltip("Scale change applied per scroll unit.")]
    [SerializeField] private float zoomSpeed = 0.02f;

    [Tooltip("Minimum and maximum uniform scale of the panTarget.")]
    [SerializeField] private float minScale = 0.35f;
    [SerializeField] private float maxScale = 2f;

    private const PointerEventData.InputButton PanButton = PointerEventData.InputButton.Left;

    private RectTransform viewport;
    private Camera canvasCamera;

    private void Awake()
    {
        viewport = (RectTransform)panTarget.parent;

        var canvas = GetComponentInParent<Canvas>();
        canvasCamera = canvas != null ? canvas.worldCamera : null;
    }

    /// <summary>
    /// Reads held arrow keys each frame and translates the panTarget accordingly,
    /// then clamps the position so the panTarget cannot be moved completely off screen.
    /// </summary>
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        var direction = Vector2.zero;

        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) direction.x += 1f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) direction.x -= 1f;
        if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) direction.y += 1f;
        if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) direction.y -= 1f;

        if (direction != Vector2.zero)
        {
            panTarget.anchoredPosition += direction * (keyPanSpeed * Time.deltaTime);
            ClampToViewport();
        }
    }

    /// <summary>
    /// Translates the panTarget by the pointer delta each frame while dragging with the left button,
    /// then clamps the position so the panTarget cannot be dragged completely off screen.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PanButton) return;

        panTarget.anchoredPosition += eventData.delta;
        ClampToViewport();
    }

    /// <summary>
    /// Scales the panTarget uniformly based on scroll wheel input, keeping the point
    /// under the cursor stationary (zoom-to-cursor). Clamps scale and position afterward.
    /// </summary>
    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        if (Mathf.Approximately(scroll, 0f)) return;

        float currentScale = panTarget.localScale.x;
        float newScale = Mathf.Clamp(currentScale + scroll * zoomSpeed, minScale, maxScale);
        float scaleFactor = newScale / currentScale;

        // Convert the screen-space cursor position to the viewport's local space so we
        // can zoom toward that exact point rather than the panTarget's pivot.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport, eventData.position, canvasCamera, out Vector2 cursorLocal);

        panTarget.localScale = new Vector3(newScale, newScale, 1f);

        // Shift anchoredPosition so the point under the cursor remains fixed.
        // anchoredPosition moves the panTarget's pivot relative to its anchor in the viewport.
        // For a center anchor, the anchor reference sits at the viewport origin, so
        // anchoredPosition directly equals the pivot position in viewport local space.
        panTarget.anchoredPosition = cursorLocal + (panTarget.anchoredPosition - cursorLocal) * scaleFactor;

        ClampToViewport();
    }

    /// <summary>
    /// Clamps panTarget.anchoredPosition so that at least edgePadding pixels of the content
    /// remain visible inside the viewport on every side. Content extents are scaled by
    /// localScale since panTarget.rect is always in unscaled local space.
    /// Works for any anchor and pivot combination on the panTarget.
    /// </summary>
    private void ClampToViewport()
    {
        Rect viewportRect = viewport.rect;
        Rect contentRect = panTarget.rect;
        float scale = panTarget.localScale.x;

        // Anchor reference point inside the viewport's local space.
        // For point anchors (anchorMin == anchorMax) this is exact; for stretch anchors it uses the midpoint.
        Vector2 anchorCenter = (panTarget.anchorMin + panTarget.anchorMax) * 0.5f;
        float anchorRefX = viewportRect.x + viewportRect.width * anchorCenter.x;
        float anchorRefY = viewportRect.y + viewportRect.height * anchorCenter.y;

        // Scaled half-extents of the content on each side, accounting for pivot.
        float contentLeft = contentRect.width * panTarget.pivot.x * scale;
        float contentRight = contentRect.width * (1f - panTarget.pivot.x) * scale;
        float contentBottom = contentRect.height * panTarget.pivot.y * scale;
        float contentTop = contentRect.height * (1f - panTarget.pivot.y) * scale;

        // Derive min/max anchoredPosition so the content edge never fully clears the viewport edge.
        // e.g. minX keeps the right side of the content at least edgePadding inside the viewport's left edge.
        float minX = viewportRect.xMin + edgePaddingX - anchorRefX - contentRight;
        float maxX = viewportRect.xMax - edgePaddingX - anchorRefX + contentLeft;
        float minY = viewportRect.yMin + edgePaddingY - anchorRefY - contentTop;
        float maxY = viewportRect.yMax - edgePaddingY - anchorRefY + contentBottom;

        Vector2 pos = panTarget.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        panTarget.anchoredPosition = pos;
    }
}

