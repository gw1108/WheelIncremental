using UnityEngine;

[System.Serializable]
public class SkillTreeNodeEntry
{
    [Tooltip("Unique identifier for this node.")]
    public string nodeId;

    [Tooltip("Display name shown to the player.")]
    public string displayName;

    [Tooltip("Column and row position in the 2D grid (zero-based).")]
    public Vector2Int gridPosition;

    [Tooltip("nodeId of the parent node. Leave empty for the root/origin node.")]
    public string parentNodeId;

    [Tooltip("Cost to purchase this upgrade")]
    public float cost;

    // Level up effects
    public int IncreaseLevelOfAllWedges;
    public int IncreaseLevelOfAllRedWedges;
    public int IncreaseLevelOfAllBlackWedges;
}
