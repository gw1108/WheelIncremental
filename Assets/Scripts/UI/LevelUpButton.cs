using Sirenix.OdinInspector;
using UnityEngine;

public class LevelUpButton : MonoBehaviour
{
    [Tooltip("The parent LevelUpButton in the skill tree hierarchy. " +
             "Leave null for the root/origin node.")]
    public LevelUpButton parentButton;

    public SkillTreeNodeEntry node;

    [HorizontalGroup("Split", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void UpLeft_CreateLevelUpButton()
    {
        Debug.Log("Clicked up left a buton!");
    }

    [HorizontalGroup("Split", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void Up_CreateLevelUpButton()
    {
        Debug.Log("Clicked up buton!");
    }

    [HorizontalGroup("Split", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void UpRight_CreateLevelUpButton()
    {
        Debug.Log("Clicked up right a buton!");
    }
}
