using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor window for designing and generating a skill tree using LevelUpButton prefabs.
/// Open via Tools > Skill Tree Editor.
/// </summary>
public class SkillTreeEditorWindow : EditorWindow
{
    // ── Layout settings ─────────────────────────────────────────────────────
    private const float NodePreviewSize = 40f;
    private const float NodePreviewSpacing = 80f;
    private const string ButtonsObjectPath = "Screens/ShopScreen/Bg/Buttons";
    private const string LevelUpButtonPrefabPath = "Assets/Prefabs/UI/LevelUpButton.prefab";
    private const string LineSuffix = "_Line";
    private const string LinesContainerName = "Lines";

    // ── Serialized state ────────────────────────────────────────────────────
    private SkillTreeData skillTreeData;
    private float cellSize = 150f;
    private float lineWidth = 4f;
    private Color lineColor = new Color(0.9f, 0.8f, 0.2f, 1f);

    // ── UI scroll positions ──────────────────────────────────────────────────
    private Vector2 nodeListScroll;
    private Vector2 gridPreviewScroll;

    // ── Foldouts ────────────────────────────────────────────────────────────
    private bool showSettings = true;
    private bool showNodeList = true;
    private bool showGridPreview = true;

    // ── Node list editing ────────────────────────────────────────────────────
    private int selectedNodeIndex = -1;

    [MenuItem("Tools/Skill Tree Editor")]
    public static void ShowWindow()
    {
        GetWindow<SkillTreeEditorWindow>("Skill Tree Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Skill Tree Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        DrawDataSection();

        if (skillTreeData == null)
        {
            EditorGUILayout.HelpBox("Assign or create a SkillTreeData asset to begin.", MessageType.Info);
            return;
        }

        EditorGUI.BeginChangeCheck();

        DrawSettingsSection();
        EditorGUILayout.Space(4);
        DrawNodeListSection();
        EditorGUILayout.Space(4);
        DrawGridPreviewSection();
        EditorGUILayout.Space(8);
        DrawGenerateSection();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(skillTreeData);
    }

    // ── Data asset section ───────────────────────────────────────────────────

    private void DrawDataSection()
    {
        EditorGUILayout.BeginHorizontal();

        skillTreeData = (SkillTreeData)EditorGUILayout.ObjectField(
            "Skill Tree Data", skillTreeData, typeof(SkillTreeData), false);

        if (GUILayout.Button("New", GUILayout.Width(50)))
            CreateNewSkillTreeData();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private void CreateNewSkillTreeData()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Skill Tree Data", "SkillTreeData", "asset",
            "Choose where to save the new SkillTreeData asset.", "Assets");

        if (string.IsNullOrEmpty(path))
            return;

        SkillTreeData newData = CreateInstance<SkillTreeData>();
        AssetDatabase.CreateAsset(newData, path);
        AssetDatabase.SaveAssets();
        skillTreeData = newData;
    }

    // ── Settings section ─────────────────────────────────────────────────────

    private void DrawSettingsSection()
    {
        showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showSettings, "Layout Settings");
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            cellSize = EditorGUILayout.FloatField("Cell Size (px)", cellSize);
            lineWidth = EditorGUILayout.FloatField("Line Width (px)", lineWidth);
            lineColor = EditorGUILayout.ColorField("Line Color", lineColor);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // ── Node list section ────────────────────────────────────────────────────

    private void DrawNodeListSection()
    {
        showNodeList = EditorGUILayout.BeginFoldoutHeaderGroup(showNodeList, "Nodes");
        if (showNodeList)
        {
            EditorGUI.indentLevel++;
            nodeListScroll = EditorGUILayout.BeginScrollView(nodeListScroll, GUILayout.MaxHeight(300));

            for (int i = 0; i < skillTreeData.nodes.Count; i++)
            {
                DrawNodeRow(i);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Node"))
                AddNode();

            GUI.enabled = selectedNodeIndex >= 0 && selectedNodeIndex < skillTreeData.nodes.Count;
            if (GUILayout.Button("Remove Selected"))
                RemoveSelectedNode();
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawNodeRow(int index)
    {
        SkillTreeNodeEntry node = skillTreeData.nodes[index];
        bool isSelected = selectedNodeIndex == index;

        GUIStyle rowStyle = isSelected
            ? new GUIStyle(GUI.skin.box) { normal = { background = Texture2D.grayTexture } }
            : GUI.skin.box;

        EditorGUILayout.BeginVertical(rowStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isSelected ? "▼" : "▶", GUILayout.Width(22)))
            selectedNodeIndex = isSelected ? -1 : index;

        EditorGUILayout.LabelField($"[{index}]  {node.nodeId}  ({node.gridPosition.x}, {node.gridPosition.y})",
            EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        if (isSelected)
        {
            EditorGUI.indentLevel++;

            node.nodeId = EditorGUILayout.TextField("Node ID", node.nodeId);
            node.displayName = EditorGUILayout.TextField("Display Name", node.displayName);
            node.gridPosition = EditorGUILayout.Vector2IntField("Grid Position (col, row)", node.gridPosition);

            // Parent dropdown
            string[] parentOptions = BuildParentDropdownOptions(index);
            int currentParentIdx = GetParentDropdownIndex(node.parentNodeId, parentOptions);
            int newParentIdx = EditorGUILayout.Popup("Parent Node ID", currentParentIdx, parentOptions);
            node.parentNodeId = newParentIdx == 0 ? string.Empty : parentOptions[newParentIdx];

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(2);
    }

    private string[] BuildParentDropdownOptions(int excludeIndex)
    {
        var options = new List<string> { "(none — root)" };
        for (int i = 0; i < skillTreeData.nodes.Count; i++)
        {
            if (i != excludeIndex)
                options.Add(skillTreeData.nodes[i].nodeId);
        }
        return options.ToArray();
    }

    private int GetParentDropdownIndex(string parentId, string[] options)
    {
        if (string.IsNullOrEmpty(parentId)) return 0;
        for (int i = 1; i < options.Length; i++)
        {
            if (options[i] == parentId) return i;
        }
        return 0;
    }

    private void AddNode()
    {
        Undo.RecordObject(skillTreeData, "Add Skill Tree Node");

        skillTreeData.nodes.Add(new SkillTreeNodeEntry
        {
            nodeId = $"node_{skillTreeData.nodes.Count}",
            displayName = $"Skill {skillTreeData.nodes.Count}",
            gridPosition = Vector2Int.zero,
            parentNodeId = string.Empty
        });

        selectedNodeIndex = skillTreeData.nodes.Count - 1;
        EditorUtility.SetDirty(skillTreeData);
    }

    private void RemoveSelectedNode()
    {
        if (selectedNodeIndex < 0 || selectedNodeIndex >= skillTreeData.nodes.Count) return;

        string removedId = skillTreeData.nodes[selectedNodeIndex].nodeId;

        Undo.RecordObject(skillTreeData, "Remove Skill Tree Node");
        skillTreeData.nodes.RemoveAt(selectedNodeIndex);

        // Clear references to the removed node
        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            if (node.parentNodeId == removedId)
                node.parentNodeId = string.Empty;
        }

        selectedNodeIndex = Mathf.Clamp(selectedNodeIndex - 1, -1, skillTreeData.nodes.Count - 1);
        EditorUtility.SetDirty(skillTreeData);
    }

    // ── Grid preview section ─────────────────────────────────────────────────

    private void DrawGridPreviewSection()
    {
        showGridPreview = EditorGUILayout.BeginFoldoutHeaderGroup(showGridPreview, "Grid Preview");
        if (!showGridPreview)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return;
        }

        int maxCol = 0;
        int maxRow = 0;
        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            maxCol = Mathf.Max(maxCol, node.gridPosition.x);
            maxRow = Mathf.Max(maxRow, node.gridPosition.y);
        }

        float previewWidth = (maxCol + 1) * (NodePreviewSize + NodePreviewSpacing) + NodePreviewSpacing;
        float previewHeight = (maxRow + 1) * (NodePreviewSize + NodePreviewSpacing) + NodePreviewSpacing;

        gridPreviewScroll = EditorGUILayout.BeginScrollView(gridPreviewScroll,
            GUILayout.Height(Mathf.Clamp(previewHeight + 20, 80, 400)));

        Rect contentRect = EditorGUILayout.GetControlRect(false, previewHeight);

        // Draw connection lines first
        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            if (string.IsNullOrEmpty(node.parentNodeId)) continue;
            SkillTreeNodeEntry parent = skillTreeData.nodes.FirstOrDefault(n => n.nodeId == node.parentNodeId);
            if (parent == null) continue;

            Vector2 from = NodePreviewCenter(node.gridPosition, contentRect);
            Vector2 to = NodePreviewCenter(parent.gridPosition, contentRect);

            Handles.color = lineColor;
            Handles.DrawLine(from, to, 2f);
        }

        // Draw node circles
        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            Vector2 center = NodePreviewCenter(node.gridPosition, contentRect);
            Rect nodeRect = new Rect(center.x - NodePreviewSize * 0.5f,
                                     center.y - NodePreviewSize * 0.5f,
                                     NodePreviewSize, NodePreviewSize);

            bool isRoot = string.IsNullOrEmpty(node.parentNodeId);
            EditorGUI.DrawRect(nodeRect, isRoot ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.2f, 0.45f, 0.8f));

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            EditorGUI.LabelField(nodeRect, node.nodeId, labelStyle);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private Vector2 NodePreviewCenter(Vector2Int gridPos, Rect container)
    {
        float x = container.x + NodePreviewSpacing + gridPos.x * (NodePreviewSize + NodePreviewSpacing) + NodePreviewSize * 0.5f;
        float y = container.y + NodePreviewSpacing + gridPos.y * (NodePreviewSize + NodePreviewSpacing) + NodePreviewSize * 0.5f;
        return new Vector2(x, y);
    }

    // ── Generate section ─────────────────────────────────────────────────────

    private void DrawGenerateSection()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate in Scene", GUILayout.Height(32)))
            GenerateSkillTree();

        if (GUILayout.Button("Clear Generated", GUILayout.Height(32)))
            ClearGeneratedNodes();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "\"Generate in Scene\" clears previously generated nodes first, then instantiates fresh ones.",
            MessageType.None);
    }

    // ── Generation logic ─────────────────────────────────────────────────────

    private void GenerateSkillTree()
    {
        if (skillTreeData == null || skillTreeData.nodes.Count == 0)
        {
            EditorUtility.DisplayDialog("Skill Tree Editor", "No nodes defined in the SkillTreeData.", "OK");
            return;
        }

        ValidateUniqueIds();

        GameObject buttonsGO = FindButtonsGameObject();
        if (buttonsGO == null) return;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelUpButtonPrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Skill Tree Editor",
                $"LevelUpButton prefab not found at:\n{LevelUpButtonPrefabPath}", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(buttonsGO, "Generate Skill Tree");

        ClearGeneratedNodes(buttonsGO);

        // Ensure Lines container
        GameObject linesContainer = new GameObject(LinesContainerName);
        Undo.RegisterCreatedObjectUndo(linesContainer, "Create Lines Container");
        linesContainer.transform.SetParent(buttonsGO.transform, false);
        RectTransform linesRect = linesContainer.AddComponent<RectTransform>();
        StretchToFill(linesRect);

        // Instantiate buttons
        Dictionary<string, LevelUpButton> spawnedButtons = new Dictionary<string, LevelUpButton>();

        foreach (SkillTreeNodeEntry nodeEntry in skillTreeData.nodes)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, buttonsGO.transform);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate LevelUpButton");
            instance.name = $"LevelUpButton_{nodeEntry.nodeId}";

            RectTransform rt = instance.GetComponent<RectTransform>();
            rt.anchoredPosition = GridToCanvasPosition(nodeEntry.gridPosition);

            LevelUpButton levelUpButton = instance.GetComponent<LevelUpButton>();
            levelUpButton.nodeId = nodeEntry.nodeId;

            spawnedButtons[nodeEntry.nodeId] = levelUpButton;
        }

        // Wire parent references and create lines
        foreach (SkillTreeNodeEntry nodeEntry in skillTreeData.nodes)
        {
            if (string.IsNullOrEmpty(nodeEntry.parentNodeId)) continue;

            if (!spawnedButtons.TryGetValue(nodeEntry.nodeId, out LevelUpButton childButton)) continue;
            if (!spawnedButtons.TryGetValue(nodeEntry.parentNodeId, out LevelUpButton parentButton)) continue;

            childButton.parentButton = parentButton;
            EditorUtility.SetDirty(childButton);

            CreateLine(linesContainer, childButton, parentButton, nodeEntry.nodeId);
        }

        EditorSceneManager.MarkSceneDirty(buttonsGO.scene);
        Debug.Log($"[SkillTreeEditor] Generated {skillTreeData.nodes.Count} node(s) under '{buttonsGO.name}'.");
    }

    private void ClearGeneratedNodes()
    {
        GameObject buttonsGO = FindButtonsGameObject();
        if (buttonsGO != null)
            ClearGeneratedNodes(buttonsGO);
    }

    private void ClearGeneratedNodes(GameObject buttonsGO)
    {
        // Remove all children whose names follow our naming conventions.
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in buttonsGO.transform)
        {
            if (child.name.StartsWith("LevelUpButton_") || child.name == LinesContainerName)
                toDestroy.Add(child);
        }

        foreach (Transform t in toDestroy)
            Undo.DestroyObjectImmediate(t.gameObject);
    }

    private void CreateLine(GameObject container, LevelUpButton from, LevelUpButton to, string nodeId)
    {
        GameObject lineGO = new GameObject($"{nodeId}{LineSuffix}");
        Undo.RegisterCreatedObjectUndo(lineGO, "Create UILine");
        lineGO.transform.SetParent(container.transform, false);

        RectTransform rt = lineGO.AddComponent<RectTransform>();
        StretchToFill(rt);

        UILine line = lineGO.AddComponent<UILine>();
        line.color = lineColor;
        line.lineWidth = lineWidth;
        line.from = from.GetComponent<RectTransform>();
        line.to = to.GetComponent<RectTransform>();
        line.raycastTarget = false;
    }

    private Vector2 GridToCanvasPosition(Vector2Int gridPos)
    {
        // Grid origin is at centre; columns go right, rows go down (negative Y in UI).
        return new Vector2(gridPos.x * cellSize, -gridPos.y * cellSize);
    }

    private GameObject FindButtonsGameObject()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform found = root.transform.Find(ButtonsObjectPath);
            if (found != null) return found.gameObject;
        }

        EditorUtility.DisplayDialog("Skill Tree Editor",
            $"Could not find the 'Buttons' GameObject at path:\n{ButtonsObjectPath}\n\nMake sure the Game scene is loaded.",
            "OK");
        return null;
    }

    private void ValidateUniqueIds()
    {
        HashSet<string> seen = new HashSet<string>();
        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            if (!seen.Add(node.nodeId))
                Debug.LogWarning($"[SkillTreeEditor] Duplicate nodeId detected: '{node.nodeId}'. Results may be unpredictable.");
        }
    }

    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
