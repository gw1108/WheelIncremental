using UnityEngine;

[System.Serializable]
public class SkillTreeNodeEntry
{
    [Tooltip("Unique identifier for this node.")]
    public string nodeId;

    [Tooltip("Display name shown to the player.")]
    public string displayName;
    public string displayDescription;

    [Tooltip("Column position in the 2D grid (zero-based).")]
    public int gridPositionX;

    [Tooltip("Row position in the 2D grid (zero-based).")]
    public int gridPositionY;

    [Tooltip("nodeId of the parent node. Leave empty for the root/origin node.")]
    public string parentNodeId;

    [Tooltip("Cost to purchase this upgrade")]
    public float cost;

    public int distanceFromOrigin = 0;

    // Level up effects
    public bool unlocksBlackRedBetting;
    public bool unlocksBlueAccumulator;
    public bool unlocksPurpleAccumulator;
    public bool unlocksRedBetMultiAlsoRedMulti;
    public bool unlocksSpinningBall;
    public bool unlocksTimeStop;

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

    public int GetDefaultCost(int distanceFromOrigin)
    {
        return Mathf.CeilToInt(Mathf.Pow(1.12f, (distanceFromOrigin - 1)) * 10f);
    }

    /// <summary>
    /// For debugging purposes.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"nodeId={nodeId},displayName={displayName},displayDescription={displayDescription},gridPositionX={gridPositionX},gridPositionY={gridPositionY},parentNodeId={parentNodeId},cost={cost},distanceFromOrigin={distanceFromOrigin},unlocksBlackRedBetting={unlocksBlackRedBetting},unlocksBlueAccumulator={unlocksBlueAccumulator},unlocksPurpleAccumulator={unlocksPurpleAccumulator},unlocksRedBetMultiAlsoRedMulti={unlocksRedBetMultiAlsoRedMulti},unlocksSpinningBall={unlocksSpinningBall},unlocksTimeStop={unlocksTimeStop},allAccumulators={allAccumulators},allColorMulti={allColorMulti},allColorMultiPerSpin={allColorMultiPerSpin},allColorsBetMulti={allColorsBetMulti},allColorsHighWedgeMulti={allColorsHighWedgeMulti},bankruptcyBlackBetInsurance={bankruptcyBlackBetInsurance},bankruptcyInsurance={bankruptcyInsurance},bankruptcyInsurancePercent={bankruptcyInsurancePercent},blueAccumulatorBankruptcyInsurance={blueAccumulatorBankruptcyInsurance},cashPurpleAccumulatorPerSpin={cashPurpleAccumulatorPerSpin},extraSpin={extraSpin},globalMulti={globalMulti},globalMultiLastSpin={globalMultiLastSpin},globalMultiPerSpin={globalMultiPerSpin},increaseLevelOfAllBlackWedges={increaseLevelOfAllBlackWedges},increaseLevelOfAllRedWedges={increaseLevelOfAllRedWedges},increaseLevelOfAllWedges={increaseLevelOfAllWedges},interestGrowthBlueAccumulatorPerSpin={interestGrowthBlueAccumulatorPerSpin},levelOfAllHighWedges={levelOfAllHighWedges},levelOfBlackBetPool={levelOfBlackBetPool},levelOfBlackHighWedges={levelOfBlackHighWedges},levelOfRedHighWedges={levelOfRedHighWedges},multiAllAccumulators={multiAllAccumulators},multiBlackBets={multiBlackBets},multiBlackHighWedge={multiBlackHighWedge},multiBlackWedge={multiBlackWedge},multiBlackWedgeWhenBettingOnBlack={multiBlackWedgeWhenBettingOnBlack},multiBlueAccumulator ={multiBlueAccumulator},multiPurpleAccumulator={multiPurpleAccumulator},multiRedBets={multiRedBets},multiRedHighWedge={multiRedHighWedge},multiRedWedge={multiRedWedge},nonBankruptCashOut={nonBankruptCashOut},permanentGlobalMultiBankruptcyInsurance={permanentGlobalMultiBankruptcyInsurance},permanentGlobalMultiOnBlackBet={permanentGlobalMultiOnBlackBet},purpleAccumulatorBankruptcyInsurance={purpleAccumulatorBankruptcyInsurance},purpleAccumulatorCopyWinnings={purpleAccumulatorCopyWinnings},sharedRedBettingPool={sharedRedBettingPool},slowerWheel={slowerWheel},spinningBallCannotBankrupt={spinningBallCannotBankrupt},spinningBallIsAlsoBet={spinningBallIsAlsoBet},timestopPriceMod={timestopPriceMod}";
    }
}
