using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Wheels;

public class Player : SingletonMonoBehaviour<Player>
{
    public Wheel ActiveWheel;
    public int WheelSize = 6;
    public int GlobalWheelLevel = 0;
    public List<int> DefaultWheelValues;

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
    public int CurrentRoundMoney
    {
        get
        {
            return m_currentRoundMoney;
        }
        set
        {
            m_currentRoundMoney = value;
            HUD_MoneyLabel.SetText(string.Format(HUD_MoneyLabelStringFormat, m_currentRoundMoney));
        }
    }

    public TextMeshProUGUI HUD_MoneyLabel;
    public TextMeshProUGUI HUD_SpinsLeftLabel;
    public int SpinsOnNewGame = 1;

    private int m_currentRoundMoney = 0;
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
        CurrentRoundMoney = 0;
    }


    public void OnWheelSpinComplete(WheelSegmentData wheelSegmentData)
    {
        // identify base cash
        int moneyGained = wheelSegmentData.cashPrize;
        // multiply by current multiplier
        // multiply by running multiplier
        CurrentRoundMoney += moneyGained;

        if (CurrentSpinsLeft <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Get the wheel segments based off the player's wheel level.
    /// </summary>
    /// <returns></returns>
    public List<WheelSegmentData> GetWheelSegmentData()
    {
        List<WheelSegmentData> wheel = new List<WheelSegmentData>();
        //foreach(int defaultWheelValue in DefaultWheelValues)
        for (int i = 0; i < DefaultWheelValues.Count; i++)
        {
            wheel.Add(new WheelSegmentData());
            wheel[i].cashPrize = DefaultWheelValues[i];
            wheel[i].prizeName = wheel[i].cashPrize.ToString();
            // is not accumulator slice.
            Color segmentColor;
            if (i % 2 == 0)
            {
                segmentColor = Color.red;
            }
            else
            {
                segmentColor = Color.black;
            }
            wheel[i].segmentColor = segmentColor;
        }
        return wheel;
    }

    private void GameOver()
    {
        OverlayScreenManager.Instance.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
    }
}
