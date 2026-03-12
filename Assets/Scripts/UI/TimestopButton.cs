using UnityEngine;
using UnityEngine.UI;

public class TimestopButton : MonoBehaviour
{
    [Sirenix.OdinInspector.ReadOnly]
    public Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnTimeStopClicked);
    }

    private void Update()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        button.interactable = CanActivateTimestop();
    }

    private void OnTimeStopClicked()
    {
        if (CanActivateTimestop())
        {
            // activate timestop
            Player.Instance.CurrentSpinsLeft -= Player.Instance.GetTimeStopSpinCost;
            Player.Instance.ActiveWheel.ForceStopWheel();
        }
    }

    private bool CanActivateTimestop()
    {
        return Player.Instance.CurrentSpinsLeft >= Player.Instance.GetTimeStopSpinCost && Player.Instance.ActiveWheel.IsSpinning();
    }
}
