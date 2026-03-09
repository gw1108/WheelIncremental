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
    public int increaseLevelOfAllWedges;
    public int increaseLevelOfAllRedWedges;
    public int increaseLevelOfAllBlackWedges;

    public int GetDefaultCost(int distanceFromOrigin)
    {
        return Mathf.CeilToInt(Mathf.Pow(1.12f, (distanceFromOrigin - 1)) * 10f);
    }
}
