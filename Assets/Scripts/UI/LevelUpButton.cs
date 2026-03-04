using UnityEngine;

public class LevelUpButton : MonoBehaviour
{
    [Tooltip("The parent LevelUpButton in the skill tree hierarchy. " +
             "Leave null for the root/origin node.")]
    public LevelUpButton parentButton;

    [Tooltip("Unique identifier that matches the nodeId in SkillTreeData.")]
    public string nodeId;
}
