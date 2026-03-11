using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeData", menuName = "Skill Tree/Skill Tree Data")]
public class SkillTreeData : ScriptableObject
{
    [Tooltip("All nodes that make up the skill tree.")]
    public List<SkillTreeNodeEntry> nodes = new List<SkillTreeNodeEntry>();
}
