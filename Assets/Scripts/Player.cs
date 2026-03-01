using UnityEngine;
using UnityEngine.InputSystem;
using Wheels;

public class Player : SingletonMonoBehaviour<Player>
{
    public Wheel ActiveWheel;

    public int Money;

    private void Update()
    {
        if (!ActiveWheel)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ActiveWheel.SpinWheel(Random.Range(720f, 1440f));
        }
    }
}
