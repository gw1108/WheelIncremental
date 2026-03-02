using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Wheels;

public class Player : SingletonMonoBehaviour<Player>
{
    public Wheel ActiveWheel;

    private const string HUD_MoneyLabelStringFormat = "Current: ${0}";
    private const string HUD_SpinsLeftStringFormat = "Spins Left: {0}";
    public int Money
    {
        get
        {
            return m_money;
        }
        set
        {
            m_money = value;
            HUD_MoneyLabel.SetText(string.Format(HUD_MoneyLabelStringFormat, m_money));
        }
    }
    public int CurrentSpinsLeft
    {
        get
        {
            return m_currentSpinsLeft;
        }
        set
        {
            m_currentSpinsLeft = value;
            HUD_SpinsLeftLabel.SetText(string.Format(HUD_SpinsLeftStringFormat, CurrentSpinsLeft));
        }
    }
    public TextMeshProUGUI HUD_MoneyLabel;
    public TextMeshProUGUI HUD_SpinsLeftLabel;
    public int SpinsOnNewGame = 1;

    private int m_currentSpinsLeft = 1;
    private int m_money;

    private void Start()
    {
        StartNewRound();
    }

    private void Update()
    {
        if (!ActiveWheel)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && CurrentSpinsLeft > 0)
        {
            ActiveWheel.SpinWheel(Random.Range(720f, 1440f));
            CurrentSpinsLeft--;
        }
    }

    public void StartNewRound()
    {
        // reset running multiplier to base.
        // reset current multiplier to base.
        CurrentSpinsLeft = SpinsOnNewGame;
    }


    public void OnWheelSpinComplete(WheelSegmentData wheelSegmentData)
    {
        // identify base cash
        int moneyGained = wheelSegmentData.cashPrize;
        // multiply by current multiplier
        // multiply by running multiplier
        Money += moneyGained;

        if (CurrentSpinsLeft <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        OverlayScreenManager.Instance.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
    }
}
