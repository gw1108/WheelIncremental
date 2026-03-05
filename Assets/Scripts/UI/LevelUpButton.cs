using UnityEngine;

public class LevelUpButton : MonoBehaviour
{
    [Tooltip("The parent LevelUpButton in the skill tree hierarchy. " +
             "Leave null for the root/origin node.")]
    public LevelUpButton parentButton;

    public SkillTreeNodeEntry node;
}
