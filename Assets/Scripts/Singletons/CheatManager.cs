using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public bool SmoothWheelEnabled = false;

    private void Awake()
    {
        if (ServiceLocator.Instance.CheatManager == null)
        {
            ServiceLocator.Instance.Register(this);
        }
    }

    public void ToggleSmoothWheelSpeed()
    {
        SmoothWheelEnabled = !SmoothWheelEnabled;
    }

    public void ForceWheelStop()
    {
        Player.Instance.ActiveWheel.ForceStopWheel();
    }
}
