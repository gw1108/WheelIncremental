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
    public int IndexOfPurpleAccumulator = 0;
    public int IndexOfBlueAccumulator = 5;

    #region levelUpStuff
    public bool unlocksBlackRedBetting;
    public bool unlocksBlueAccumulator;
    public bool unlocksPurpleAccumulator;
    public bool unlocksRedBetMultiAlsoRedMulti;
    public bool unlocksSpinningBall;
    public bool unlocksTimeStop;

    public bool IsRedBetPoolAddedToPrize
    {
        get
        {
            return sharedRedBettingPool > 0;
        }
    }

    public int allAccumulators;
    public int allColorMulti;
    public int allColorMultiPerSpin;
    public int allColorsBetMulti;
    public int allColorsHighWedgeMulti;
    public int bankruptcyBlackBetInsurance;
    public int bankruptcyInsurance;
    public int bankruptcyInsurancePercent;
    public int blueAccumulatorBankruptcyInsurance;
    public int cashPurpleAccumulatorPerSpin;
    public int extraSpin;
    public int globalMulti;
    public int globalMultiLastSpin;
    public int globalMultiPerSpin;
    public int increaseLevelOfAllBlackWedges;
    public int increaseLevelOfAllRedWedges;
    public int increaseLevelOfAllWedges;
    public int interestGrowthBlueAccumulatorPerSpin;
    public int levelOfAllHighWedges;
    public int levelOfBlackBetPool;
    public int levelOfBlackHighWedges;
    public int levelOfRedHighWedges;
    public int multiAllAccumulators;
    public int multiBlackBets;
    public int multiBlackHighWedge;
    public int multiBlackWedge;
    public int multiBlackWedgeWhenBettingOnBlack;
    public int multiBlueAccumulator;
    public int multiPurpleAccumulator;
    public int multiRedBets;
    public int multiRedHighWedge;
    public int multiRedWedge;
    public int nonBankruptCashOut;
    public float permanentGlobalMultiBankruptcyInsurance;
    public int permanentGlobalMultiOnBlackBet;
    public int purpleAccumulatorBankruptcyInsurance;
    public int purpleAccumulatorCopyWinnings;
    public int sharedRedBettingPool;
    public int slowerWheel;
    public int spinningBallCannotBankrupt;
    public int spinningBallIsAlsoBet;
    public int timestopPriceMod;
    #endregion

    public int GetTimeStopSpinCost
    {
        get
        {
            return 3 + timestopPriceMod;
        }
    }

    private float accumulatedPurpleCash = 0f;
    private float accumulatedBlueCash = 0f;
    private float permanentGlobalMultiAccumulated = 0f;

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

    [Sirenix.OdinInspector.ReadOnly]
    public PlayerAdditionalAbilitiesUI additionalPlayerUI;

    public TextMeshProUGUI HUD_MoneyLabel;
    public TextMeshProUGUI HUD_SpinsLeftLabel;
    public int SpinsOnNewGame
    {
        get
        {
            return 1 + extraSpin;
        }
    }

    private int m_currentRoundMoney = 0;
    private int m_currentSpinsLeft = 1;
    private int m_money;
    private List<string> purchasedUpgrades = new List<string>();

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
            CurrentSpinsLeft--;
            ActiveWheel.SpinWheel(Random.Range(720f, 1440f), Random.Range(720f, 1440f));
        }
    }

    public void StartNewRound()
    {
        // reset running multiplier to base.
        // reset current multiplier to base.
        CurrentSpinsLeft = SpinsOnNewGame;
        CurrentRoundMoney = 0;
        ActiveWheel.FullRebuildWheel();
        if (additionalPlayerUI != null)
        {
            additionalPlayerUI.OnNewRound();
        }
    }

    public void OnWheelSpinComplete(WheelSegmentData wheelSegmentData)
    {
        // identify base cash
        float moneyGained = wheelSegmentData.cashPrize;
        switch (wheelSegmentData.wedgeTypeColor)
        {
            case WheelColor.green:
                // bankruptcy
                moneyGained = 0f;
                break;
            case WheelColor.purple:
                moneyGained = accumulatedPurpleCash;
                break;
            case WheelColor.blue:
                moneyGained = accumulatedBlueCash;
                break;
            case WheelColor.black:
                moneyGained = wheelSegmentData.cashPrize;
                break;
            case WheelColor.red:
                moneyGained = wheelSegmentData.cashPrize;
                break;
            default:
                Debug.LogWarning("Somehow landed on a wedge that has color: " + wheelSegmentData.wedgeTypeColor);
                break;
        }
        // multiply by current multiplier
        // multiply by running multiplier
        CurrentRoundMoney += Mathf.RoundToInt(moneyGained);

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
        int indexOfLargestRedWedges = 0, indexOfLargestBlackWedges = 0, largestRedWedge = 0, largestBlackWedge = 0;
        List<WheelSegmentData> wheel = new List<WheelSegmentData>();
        for (int i = 0; i < DefaultWheelValues.Count; i++)
        {
            wheel.Add(new WheelSegmentData());
            wheel[i].cashPrize = DefaultWheelValues[i];
            // is not accumulator slice.
            Color segmentColor;
            if (DefaultWheelValues[i] <= 0)
            {
                //if (DefaultWheelValues[i] == 0)
                {
                    // bankrupt
                    segmentColor = Color.green;
                    wheel[i].wedgeTypeColor = WheelColor.green;
                }
            }
            else
            {
                if (i % 2 == 0)
                {
                    segmentColor = Color.red;
                    wheel[i].wedgeTypeColor = WheelColor.red;
                }
                else
                {
                    segmentColor = Color.black;
                    wheel[i].wedgeTypeColor = WheelColor.black;
                }
            }
            wheel[i].segmentColor = segmentColor;
            if (wheel[i].wedgeTypeColor == WheelColor.black && wheel[i].cashPrize >= largestBlackWedge)
            {
                largestBlackWedge = wheel[i].cashPrize;
                indexOfLargestBlackWedges = i;
            }
            else if (wheel[i].wedgeTypeColor == WheelColor.red && wheel[i].cashPrize >= largestRedWedge)
            {
                largestRedWedge = wheel[i].cashPrize;
                indexOfLargestRedWedges = i;
            }
        }
        wheel[indexOfLargestBlackWedges].isHighWedge = true;
        wheel[indexOfLargestRedWedges].isHighWedge = true;

        // Level up stuff.
        for (int i = 0; i < DefaultWheelValues.Count; i++)
        {
            wheel[i].cashPrize += increaseLevelOfAllWedges;
            switch (wheel[i].wedgeTypeColor)
            {
                case WheelColor.red:
                    wheel[i].cashPrize += increaseLevelOfAllRedWedges;
                    if (wheel[i].isHighWedge)
                    {
                        wheel[i].cashPrize += levelOfRedHighWedges;
                        wheel[i].cashPrize += levelOfAllHighWedges;
                    }
                    break;
                case WheelColor.black:
                    wheel[i].cashPrize += increaseLevelOfAllBlackWedges;
                    if (wheel[i].isHighWedge)
                    {
                        wheel[i].cashPrize += levelOfBlackHighWedges;
                        wheel[i].cashPrize += levelOfAllHighWedges;
                    }
                    break;
                case WheelColor.green:
                    wheel[i].cashPrize = 0;
                    wheel[i].prizeName = "💀";
                    break;
            }

            if (wheel[i].cashPrize > 0)
            {
                //wheel[i].prizeName = $"i:{i}_" + wheel[i].cashPrize.ToString();
                wheel[i].prizeName = wheel[i].cashPrize.ToString();
            }
        }
        return wheel;
    }

    public void BuyShopNodeUpgrade(SkillTreeNodeEntry node)
    {
        purchasedUpgrades.Add(node.nodeId);
        unlocksBlackRedBetting |= node.unlocksBlackRedBetting;
        unlocksBlueAccumulator |= node.unlocksBlueAccumulator;
        unlocksPurpleAccumulator |= node.unlocksPurpleAccumulator;
        unlocksRedBetMultiAlsoRedMulti |= node.unlocksRedBetMultiAlsoRedMulti;
        unlocksSpinningBall |= node.unlocksSpinningBall;
        unlocksTimeStop |= node.unlocksTimeStop;
        allAccumulators += node.allAccumulators;
        allColorMulti += node.allColorMulti;
        allColorMultiPerSpin += node.allColorMultiPerSpin;
        allColorsBetMulti += node.allColorsBetMulti;
        allColorsHighWedgeMulti += node.allColorsHighWedgeMulti;
        bankruptcyBlackBetInsurance += node.bankruptcyBlackBetInsurance;
        bankruptcyInsurance += node.bankruptcyInsurance;
        bankruptcyInsurancePercent += node.bankruptcyInsurancePercent;
        blueAccumulatorBankruptcyInsurance += node.blueAccumulatorBankruptcyInsurance;
        cashPurpleAccumulatorPerSpin += node.cashPurpleAccumulatorPerSpin;
        extraSpin += node.extraSpin;
        globalMulti += node.globalMulti;
        globalMultiLastSpin += node.globalMultiLastSpin;
        globalMultiPerSpin += node.globalMultiPerSpin;
        increaseLevelOfAllBlackWedges += node.increaseLevelOfAllBlackWedges;
        increaseLevelOfAllRedWedges += node.increaseLevelOfAllRedWedges;
        increaseLevelOfAllWedges += node.increaseLevelOfAllWedges;
        interestGrowthBlueAccumulatorPerSpin += node.interestGrowthBlueAccumulatorPerSpin;
        levelOfAllHighWedges += node.levelOfAllHighWedges;
        levelOfBlackBetPool += node.levelOfBlackBetPool;
        levelOfBlackHighWedges += node.levelOfBlackHighWedges;
        levelOfRedHighWedges += node.levelOfRedHighWedges;
        multiAllAccumulators += node.multiAllAccumulators;
        multiBlackBets += node.multiBlackBets;
        multiBlackHighWedge += node.multiBlackHighWedge;
        multiBlackWedge += node.multiBlackWedge;
        multiBlackWedgeWhenBettingOnBlack += node.multiBlackWedgeWhenBettingOnBlack;
        multiBlueAccumulator += node.multiBlueAccumulator;
        multiPurpleAccumulator += node.multiPurpleAccumulator;
        multiRedBets += node.multiRedBets;
        multiRedHighWedge += node.multiRedHighWedge;
        multiRedWedge += node.multiRedWedge;
        nonBankruptCashOut += node.nonBankruptCashOut;
        permanentGlobalMultiBankruptcyInsurance += node.permanentGlobalMultiBankruptcyInsurance;
        permanentGlobalMultiOnBlackBet += node.permanentGlobalMultiOnBlackBet;
        purpleAccumulatorBankruptcyInsurance += node.purpleAccumulatorBankruptcyInsurance;
        purpleAccumulatorCopyWinnings += node.purpleAccumulatorCopyWinnings;
        sharedRedBettingPool += node.sharedRedBettingPool;
        slowerWheel += node.slowerWheel;
        spinningBallCannotBankrupt += node.spinningBallCannotBankrupt;
        spinningBallIsAlsoBet += node.spinningBallIsAlsoBet;
        timestopPriceMod += node.timestopPriceMod;
    }

    public float GetCurrentMulti()
    {
        float multi = 1f;
        multi += allColorMulti / 100f;
        return multi;
    }

    public float GetCurrentMulti(Color color, bool isHighWedge, bool isLastSpin, bool isBettingOnBlack)
    {
        float multi = 1f;
        multi += allColorMulti / 100f;
        if (isHighWedge)
        {
            multi += allColorsHighWedgeMulti / 100f;
        }

        if (color == Color.black)
        {
            multi += multiBlackWedge / 100f;
            if (isHighWedge)
            {
                multi += multiBlackHighWedge / 100f;
            }
            if (isBettingOnBlack)
            {
                multi += multiBlackWedgeWhenBettingOnBlack;
            }
        }

        if (color == Color.red)
        {
            multi += multiRedWedge / 100f;
            if (isHighWedge)
            {
                multi += multiRedHighWedge / 100f;
            }
        }

        if (color == Color.purple || color == Color.blue)
        {
            multi += multiAllAccumulators / 100f;
            if (color == Color.blue)
            {
                multi += multiBlueAccumulator / 100f;
            }
            if (color == Color.purple)
            {
                multi += multiPurpleAccumulator / 100f;
            }
        }

        return multi;
    }

    public float GetGlobalMulti()
    {
        float globalMulti = 1f;
        globalMulti += this.globalMulti / 100f;
        return globalMulti;
    }

    public float GetCurrentGlobalMulti(Color color, bool isHighWedge, bool isLastSpin, bool isBettingOnBlack)
    {
        float globalMulti = 1f;
        globalMulti += this.globalMulti / 100f;
        if (isLastSpin)
        {
            globalMulti += this.globalMultiLastSpin / 100f;
        }
        return globalMulti;
    }

    private void GameOver()
    {
        OverlayScreenManager.Instance.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
    }
}
