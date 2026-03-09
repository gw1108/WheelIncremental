using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelUpButton : MonoBehaviour
{
    [Tooltip("The parent LevelUpButton in the skill tree hierarchy. " +
             "Leave null for the root/origin node.")]
    public LevelUpButton parentButton;

    public SkillTreeNodeEntry node;

    public const float CellWidth = 100f;
    public const float CellHeight = 100f;
    public const string ButtonsObjectPath = "Screens/ShopScreen/Bg/Buttons";
    public const string LevelUpButtonPrefabPath = "Assets/Prefabs/UI/LevelUpButton.prefab";
    public const float LineWidth = 4f;

    [HorizontalGroup("Split1", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void UpLeft_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX - 1, node.gridPositionY + 1);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split1", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void Up_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX, node.gridPositionY + 1);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split1", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void UpRight_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX + 1, node.gridPositionY + 1);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split2", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void Left_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX - 1, node.gridPositionY);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split2", 0.5f)]
    [Button(ButtonSizes.Large)]
    private void Right_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX + 1, node.gridPositionY);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split3", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void DownLeft_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX - 1, node.gridPositionY - 1);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split3", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void Down_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX, node.gridPositionY - 1);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    [HorizontalGroup("Split3", 0.33f)]
    [Button(ButtonSizes.Large)]
    private void DownRight_CreateLevelUpButton()
    {
        LevelUpButton createdButton = CreateLevelUpButtonChild(node.gridPositionX + 1, node.gridPositionY - 1);
        if (createdButton != null)
        {
            Selection.activeGameObject = createdButton.gameObject;
        }
    }

    private LevelUpButton CreateLevelUpButtonChild(int newGridPosX, int newGridPosY)
    {
        GameObject parentOfButtons = FindButtonsGameObject();
        if (parentOfButtons == null)
        {
            Debug.LogWarning("Couldn't find " + ButtonsObjectPath);
            return null;
        }

        GameObject prefabToInstantiate = GetLevelUpButtonPrefab();
        if (prefabToInstantiate == null)
        {

            Debug.LogWarning("Couldnt find " + LevelUpButtonPrefabPath);
            return null;
        }

        SkillTreeNodeEntry entry = new SkillTreeNodeEntry();
        entry.gridPositionX = newGridPosX;
        entry.gridPositionY = newGridPosY;
        entry.nodeId = $"node_{entry.gridPositionX}_{entry.gridPositionY}";
        entry.parentNodeId = node.nodeId;
        entry.distanceFromOrigin = node.distanceFromOrigin + 1;
        entry.cost = entry.GetDefaultCost(entry.distanceFromOrigin);

        LevelUpButton button = CreateLevelUpButton(parentOfButtons, prefabToInstantiate, entry, null);
        CreateLine(button, this, Color.yellow);
        return button;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from">Typically yourself/param>
    /// <param name="to">Typically the parent button</param>
    /// <param name="lineColor"></param>
    public static void CreateLine(LevelUpButton from, LevelUpButton to, Color lineColor)
    {
        GameObject lineGO = new GameObject($"{from.node.nodeId}_Line");
        Undo.RegisterCreatedObjectUndo(lineGO, "Create UILine");
        lineGO.transform.SetParent(from.transform, false);

        RectTransform rt = lineGO.AddComponent<RectTransform>();
        StretchToFill(rt);

        UILine line = lineGO.AddComponent<UILine>();
        line.color = lineColor;
        line.lineWidth = LineWidth;
        line.from = from.GetComponent<RectTransform>();
        line.to = to.GetComponent<RectTransform>();
        line.raycastTarget = false;
    }

    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static LevelUpButton CreateLevelUpButton(GameObject buttonsGO, GameObject prefab, SkillTreeNodeEntry nodeEntry, Dictionary<string, LevelUpButton> spawnedButtons)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, buttonsGO.transform);
        Undo.RegisterCreatedObjectUndo(instance, "Instantiate LevelUpButton");
        instance.name = $"LevelUpButton_{nodeEntry.nodeId}";

        RectTransform rt = instance.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(nodeEntry.gridPositionX * CellWidth, nodeEntry.gridPositionY * CellHeight);

        LevelUpButton levelUpButton = instance.GetComponent<LevelUpButton>();
        levelUpButton.node = nodeEntry;

        Tooltip tooltip = instance.GetComponent<Tooltip>();
        if (tooltip != null)
        {
            tooltip.tooltipMessage = nodeEntry.displayDescription;
            tooltip.tooltipHeader = nodeEntry.displayName;
        }

        if (spawnedButtons != null)
        {
            spawnedButtons[nodeEntry.nodeId] = levelUpButton;
        }
        return levelUpButton;
    }

    public static GameObject FindButtonsGameObject()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform found = root.transform.Find(ButtonsObjectPath);
            if (found != null) return found.gameObject;
        }

        Debug.LogWarning($"Could not find the 'Buttons' GameObject at path:\n{ButtonsObjectPath}\n\nMake sure the Game scene is loaded.");
        return null;
    }

    public static GameObject GetLevelUpButtonPrefab()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(LevelUpButtonPrefabPath);
    }
}
