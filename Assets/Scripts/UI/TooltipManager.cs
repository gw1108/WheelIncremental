using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public TextMeshProUGUI tooltipTextLabel;
    public TextMeshProUGUI tooltipHeaderLabel;
    public bool UseMousePosition = true;
    public float XOffset;
    public float YOffset;

    private InputAction mousePosition;
    private RectTransform rectTransform;
    private Vector2 tooltipPosition;
    private Transform tooltipTargetTransform;
    private float YOffsetTowardsCenter;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
        mousePosition = InputSystem.actions.FindAction("MousePosition");
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (UseMousePosition)
        {
            tooltipPosition = mousePosition.ReadValue<Vector2>();
        }
        else if (tooltipTargetTransform != null)
        {
            tooltipPosition = tooltipTargetTransform.position;
        }
        transform.position = tooltipPosition + new Vector2(XOffset, YOffsetTowardsCenter);
    }

    public void SetAndShowTooltip(string text, string header)
    {
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        SetAndShowTooltip(text, header, mousePos);
    }

    public void SetAndShowTooltip(string text, string header, Transform target)
    {
        tooltipTargetTransform = target;
        SetAndShowTooltip(text, header, target.position);
    }

    public void SetAndShowTooltip(string text, string header, Vector2 position)
    {
        tooltipPosition = position;
        gameObject.SetActive(true);
        tooltipTextLabel.SetText(text);
        tooltipHeaderLabel.SetText(header);
        int yValue;
        if (position.y <= Screen.height / 2f)
        {
            yValue = 0;
            YOffsetTowardsCenter = YOffset;
        }
        else
        {
            yValue = 1;
            YOffsetTowardsCenter = -YOffset;
        }
        rectTransform.pivot = new Vector2(rectTransform.pivot.x, yValue);
        transform.position = tooltipPosition + new Vector2(XOffset, YOffsetTowardsCenter);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
