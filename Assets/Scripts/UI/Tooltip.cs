using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipMessage;
    public string tooltipHeader;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TryShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnMouseEnter()
    {
        TryShowTooltip();
    }

    private void OnMouseExit()
    {
        HideTooltip();
    }

    private void TryShowTooltip()
    {
        if (!string.IsNullOrWhiteSpace(tooltipMessage) || !string.IsNullOrWhiteSpace(tooltipHeader))
        {
            ServiceLocator.Instance.TooltipManager.SetAndShowTooltip(tooltipMessage, tooltipHeader, transform);
        }
    }

    private void HideTooltip()
    {
        ServiceLocator.Instance.TooltipManager.HideTooltip();
    }
}
