using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    private const string LineSuffix = "_Line";

    // ── Serialized state ────────────────────────────────────────────────────
    private SkillTreeData skillTreeData;
    private bool shouldGenerateDefaultCosts;
    private Color lineColor = new Color(0.9f, 0.8f, 0.2f, 1f);

    // ── UI scroll positions ──────────────────────────────────────────────────
    private Vector2 nodeListScroll;
    private Vector2 gridPreviewScroll;

    // ── Foldouts ────────────────────────────────────────────────────────────
    private bool showSettings = true;
    private bool showNodeList = true;
    private bool showGridPreview = true;
    private bool showCsv = true;

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
        //EditorGUILayout.Space(4);
        //DrawGridPreviewSection();
        EditorGUILayout.Space(4);
        DrawCsvSection();
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

        shouldGenerateDefaultCosts = EditorGUILayout.Toggle("Fill With Default Costs", shouldGenerateDefaultCosts);

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

        EditorGUILayout.LabelField($"[{index}]  {node.nodeId}  ({node.gridPositionX}, {node.gridPositionY})",
            EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        if (isSelected)
        {
            EditorGUI.indentLevel++;

            node.nodeId = EditorGUILayout.TextField("Node ID", node.nodeId);
            node.displayName = EditorGUILayout.TextField("Display Name", node.displayName);
            node.gridPositionX = EditorGUILayout.IntField("Grid Position X (col)", node.gridPositionX);
            node.gridPositionY = EditorGUILayout.IntField("Grid Position Y (row)", node.gridPositionY);

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
            gridPositionX = 0,
            gridPositionY = 0,
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
            maxCol = Mathf.Max(maxCol, node.gridPositionX);
            maxRow = Mathf.Max(maxRow, node.gridPositionY);
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

            Vector2 from = NodePreviewCenter(node.gridPositionX, node.gridPositionY, contentRect);
            Vector2 to = NodePreviewCenter(parent.gridPositionX, parent.gridPositionY, contentRect);

            Handles.color = lineColor;
            Handles.DrawLine(from, to, 2f);
        }

        // Draw node circles
        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            Vector2 center = NodePreviewCenter(node.gridPositionX, node.gridPositionY, contentRect);
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

    private Vector2 NodePreviewCenter(int col, int row, Rect container)
    {
        float x = container.x + NodePreviewSpacing + col * (NodePreviewSize + NodePreviewSpacing) + NodePreviewSize * 0.5f;
        float y = container.y + NodePreviewSpacing + row * (NodePreviewSize + NodePreviewSpacing) + NodePreviewSize * 0.5f;
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

        if (GUILayout.Button("Import from Scene", GUILayout.Height(32)))
            ImportFromScene();

        EditorGUILayout.HelpBox(
            "\"Generate in Scene\" clears previously generated nodes first, then instantiates fresh ones.\n" +
            "\"Import from Scene\" reads all LevelUpButton GameObjects currently in the scene and overwrites the SkillTreeData nodes.",
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
        GameObject levelUpButtonPrefab = GetLevelUpButtonPrefab();
        if (levelUpButtonPrefab == null)
        {
            EditorUtility.DisplayDialog("Skill Tree Editor",
                $"LevelUpButton prefab not found at:\n{LevelUpButton.LevelUpButtonPrefabPath}", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(buttonsGO, "Generate Skill Tree");

        ClearGeneratedNodes(buttonsGO);

        // Instantiate buttons
        Dictionary<string, LevelUpButton> spawnedButtons = new Dictionary<string, LevelUpButton>();
        foreach (SkillTreeNodeEntry nodeEntry in skillTreeData.nodes)
        {
            // Reset distance from origin for later.
            nodeEntry.distanceFromOrigin = -1;
            CreateLevelUpButton(buttonsGO, levelUpButtonPrefab, nodeEntry, spawnedButtons);
        }

        foreach (SkillTreeNodeEntry nodeEntry in skillTreeData.nodes)
        {
            CreateLevelUpButtonLine(spawnedButtons, nodeEntry, this.lineColor);
        }

        foreach (SkillTreeNodeEntry nodeEntry in skillTreeData.nodes)
        {
            // set us up the distance from origin.
            GetOrCalculateDistanceFromOrigin(spawnedButtons, nodeEntry);
            if (shouldGenerateDefaultCosts)
            {
                nodeEntry.cost = nodeEntry.GetDefaultCost(nodeEntry.distanceFromOrigin);
            }
        }

        EditorSceneManager.MarkSceneDirty(buttonsGO.scene);
        Debug.Log($"[SkillTreeEditor] Generated {skillTreeData.nodes.Count} node(s) under '{buttonsGO.name}'.");
    }

    public static int GetOrCalculateDistanceFromOrigin(Dictionary<string, LevelUpButton> spawnedButtons, SkillTreeNodeEntry nodeEntry)
    {
        if (string.IsNullOrEmpty(nodeEntry.parentNodeId))
        {
            nodeEntry.distanceFromOrigin = 0;
            return 0;
        }
        else if (spawnedButtons[nodeEntry.parentNodeId].node.distanceFromOrigin >= 0)
        {
            nodeEntry.distanceFromOrigin = 1 + spawnedButtons[nodeEntry.parentNodeId].node.distanceFromOrigin;
            return nodeEntry.distanceFromOrigin;
        }
        else
        {
            int parentDistanceFromOrigin = GetOrCalculateDistanceFromOrigin(spawnedButtons, spawnedButtons[nodeEntry.parentNodeId].node);
            nodeEntry.distanceFromOrigin = 1 + parentDistanceFromOrigin;
            return nodeEntry.distanceFromOrigin;
        }
    }

    public static bool CreateLevelUpButtonLine(Dictionary<string, LevelUpButton> spawnedButtons, SkillTreeNodeEntry nodeEntry, Color lineColor)
    {
        if (string.IsNullOrEmpty(nodeEntry.parentNodeId))
        {
            Debug.Log(nodeEntry.nodeId + "," + nodeEntry.displayName + ": has no parentId: " + nodeEntry.parentNodeId);
            return false;
        }

        if (!spawnedButtons.TryGetValue(nodeEntry.nodeId, out LevelUpButton childButton)) return false;
        if (!spawnedButtons.TryGetValue(nodeEntry.parentNodeId, out LevelUpButton parentButton)) return false;

        childButton.parentButton = parentButton;
        EditorUtility.SetDirty(childButton);

        CreateLine(childButton, parentButton, lineColor);
        return true;
    }

    public static GameObject GetLevelUpButtonPrefab()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(LevelUpButton.LevelUpButtonPrefabPath);
    }

    public static void CreateLevelUpButton(GameObject buttonsGO, GameObject prefab, SkillTreeNodeEntry nodeEntry, Dictionary<string, LevelUpButton> spawnedButtons)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, buttonsGO.transform);
        Undo.RegisterCreatedObjectUndo(instance, "Instantiate LevelUpButton");
        instance.name = $"LevelUpButton_{nodeEntry.nodeId}";

        RectTransform rt = instance.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(nodeEntry.gridPositionX * LevelUpButton.CellWidth, nodeEntry.gridPositionY * LevelUpButton.CellHeight);

        LevelUpButton levelUpButton = instance.GetComponent<LevelUpButton>();
        levelUpButton.node = nodeEntry;

        if (spawnedButtons != null)
        {
            spawnedButtons[nodeEntry.nodeId] = levelUpButton;
        }
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
            if (child.name.StartsWith("LevelUpButton_"))//|| child.name == LinesContainerName)
                toDestroy.Add(child);
        }

        foreach (Transform t in toDestroy)
            Undo.DestroyObjectImmediate(t.gameObject);
    }

    /// <summary>
    /// Reads all LevelUpButton components under the Buttons container in the active scene
    /// and overwrites the SkillTreeData nodes list with the data found on them.
    /// </summary>
    private void ImportFromScene()
    {
        GameObject buttonsGO = FindButtonsGameObject();
        if (buttonsGO == null) return;

        LevelUpButton[] sceneButtons = buttonsGO.GetComponentsInChildren<LevelUpButton>(includeInactive: true);

        if (sceneButtons.Length == 0)
        {
            EditorUtility.DisplayDialog("Import from Scene",
                $"No LevelUpButton components found under '{LevelUpButton.ButtonsObjectPath}'.", "OK");
            return;
        }

        // Build a lookup from LevelUpButton instance to nodeId so we can resolve parentNodeId.
        Dictionary<LevelUpButton, string> buttonToNodeId = new Dictionary<LevelUpButton, string>(sceneButtons.Length);
        List<SkillTreeNodeEntry> importedNodes = new List<SkillTreeNodeEntry>();

        // First pass: collect node entries, deriving nodeId from the node data or the GO name.
        foreach (LevelUpButton button in sceneButtons)
        {
            SkillTreeNodeEntry sourceNode = button.node;

            string nodeId = sourceNode != null && !string.IsNullOrEmpty(sourceNode.nodeId)
                ? sourceNode.nodeId
                : button.gameObject.name.Replace("LevelUpButton_", string.Empty);

            buttonToNodeId[button] = nodeId;
        }

        // Second pass: build entries with resolved parentNodeId.
        foreach (LevelUpButton button in sceneButtons)
        {
            SkillTreeNodeEntry sourceNode = button.node;
            string nodeId = buttonToNodeId[button];

            string parentNodeId = string.Empty;
            if (!string.IsNullOrEmpty(sourceNode.parentNodeId))
            {
                parentNodeId = sourceNode.parentNodeId;
            }
            else if (button.parentButton != null && buttonToNodeId.TryGetValue(button.parentButton, out string parentId))
            {
                parentNodeId = parentId;
            }

            RectTransform rt = button.GetComponent<RectTransform>();
            int gridX = sourceNode?.gridPositionX ?? 0;
            int gridY = sourceNode?.gridPositionY ?? 0;

            // If the node has no explicit grid position, derive it from the anchored position and cell size.
            if (sourceNode == null || (sourceNode.gridPositionX == 0 && sourceNode.gridPositionY == 0))
            {
                if (rt != null && (LevelUpButton.CellWidth > 0f && LevelUpButton.CellHeight > 0f))
                {
                    gridX = Mathf.RoundToInt(rt.anchoredPosition.x / LevelUpButton.CellWidth);
                    gridY = Mathf.RoundToInt(rt.anchoredPosition.y / LevelUpButton.CellHeight);
                }
            }

            importedNodes.Add(new SkillTreeNodeEntry
            {
                nodeId = nodeId,
                displayName = sourceNode?.displayName ?? nodeId,
                displayDescription = sourceNode?.displayDescription ?? string.Empty,
                gridPositionX = gridX,
                gridPositionY = gridY,
                parentNodeId = parentNodeId,
                cost = shouldGenerateDefaultCosts ? sourceNode.GetDefaultCost(sourceNode?.distanceFromOrigin ?? 0) : sourceNode?.cost ?? 0f,
                increaseLevelOfAllWedges = sourceNode?.increaseLevelOfAllWedges ?? 0,
                increaseLevelOfAllRedWedges = sourceNode?.increaseLevelOfAllRedWedges ?? 0,
                increaseLevelOfAllBlackWedges = sourceNode?.increaseLevelOfAllBlackWedges ?? 0,
                distanceFromOrigin = sourceNode?.distanceFromOrigin ?? 0
            });
        }

        bool confirmed = EditorUtility.DisplayDialog("Import from Scene",
            $"This will overwrite all {skillTreeData.nodes.Count} existing node(s) in '{skillTreeData.name}' " +
            $"with {importedNodes.Count} node(s) read from the scene.\n\nProceed?",
            "Import", "Cancel");

        if (!confirmed) return;

        Undo.RecordObject(skillTreeData, "Import Skill Tree from Scene");
        skillTreeData.nodes = importedNodes;
        EditorUtility.SetDirty(skillTreeData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SkillTreeEditor] Imported {importedNodes.Count} node(s) from scene into '{skillTreeData.name}'.");
    }

    public static void CreateLine(LevelUpButton from, LevelUpButton to, Color lineColor)
    {
        GameObject lineGO = new GameObject($"{from.node.nodeId}{LineSuffix}");
        Undo.RegisterCreatedObjectUndo(lineGO, "Create UILine");
        lineGO.transform.SetParent(from.transform, false);

        RectTransform rt = lineGO.AddComponent<RectTransform>();
        StretchToFill(rt);

        UILine line = lineGO.AddComponent<UILine>();
        line.color = lineColor;
        line.lineWidth = LevelUpButton.LineWidth;
        line.from = from.GetComponent<RectTransform>();
        line.to = to.GetComponent<RectTransform>();
        line.raycastTarget = false;
    }

    public static GameObject FindButtonsGameObject()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform found = root.transform.Find(LevelUpButton.ButtonsObjectPath);
            if (found != null) return found.gameObject;
        }

        EditorUtility.DisplayDialog("Skill Tree Editor",
            $"Could not find the 'Buttons' GameObject at path:\n{LevelUpButton.ButtonsObjectPath}\n\nMake sure the Game scene is loaded.",
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

    // ── CSV section ──────────────────────────────────────────────────────────

    private void DrawCsvSection()
    {
        showCsv = EditorGUILayout.BeginFoldoutHeaderGroup(showCsv, "CSV Import / Export");
        if (showCsv)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "CSV columns (order matters):\n" +
                "nodeId, displayName, displayDescription, gridPositionX, gridPositionY, parentNodeId, " +
                "cost, increaseLevelOfAllWedges, increaseLevelOfAllRedWedges, increaseLevelOfAllBlackWedges, distanceFromOrigin",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import from CSV", GUILayout.Height(28)))
                ImportFromCsv();

            if (GUILayout.Button("Export to CSV", GUILayout.Height(28)))
                ExportToCsv();

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void ImportFromCsv()
    {
        string filePath = EditorUtility.OpenFilePanel("Import Skill Tree CSV", "", "csv");
        if (string.IsNullOrEmpty(filePath)) return;

        string[] lines;
        try
        {
            lines = File.ReadAllLines(filePath, Encoding.UTF8);
        }
        catch (IOException ex)
        {
            EditorUtility.DisplayDialog("CSV Import Error", $"Could not read file:\n{ex.Message}", "OK");
            return;
        }

        if (lines.Length < 2)
        {
            EditorUtility.DisplayDialog("CSV Import Error",
                "The file must contain a header row and at least one data row.", "OK");
            return;
        }

        // Validate header
        string[] header = ParseCsvLine(lines[0]);
        string[] expectedHeader = CsvHeader();
        bool headerValid = header.Length == expectedHeader.Length &&
                           !header.Where((col, i) => col.Trim() != expectedHeader[i]).Any();

        if (!headerValid)
        {
            bool proceed = EditorUtility.DisplayDialog("CSV Import Warning",
                $"Header does not match the expected format:\n{string.Join(", ", expectedHeader)}\n\nProceed anyway?",
                "Proceed", "Cancel");
            if (!proceed) return;
        }

        List<SkillTreeNodeEntry> imported = new List<SkillTreeNodeEntry>();
        List<string> errors = new List<string>();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = ParseCsvLine(line);
            if (cols.Length < 11)
            {
                errors.Add($"Line {i + 1}: expected 11 columns, got {cols.Length}. Skipped.");
                continue;
            }

            if (!int.TryParse(cols[3].Trim(), out int gridX))
            {
                errors.Add($"Line {i + 1}: invalid gridPositionX '{cols[3]}'. Skipped.");
                continue;
            }

            if (!int.TryParse(cols[4].Trim(), out int gridY))
            {
                errors.Add($"Line {i + 1}: invalid gridPositionY '{cols[4]}'. Skipped.");
                continue;
            }

            float.TryParse(cols[6].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float cost);
            int.TryParse(cols[7].Trim(), out int increaseAll);
            int.TryParse(cols[8].Trim(), out int increaseRed);
            int.TryParse(cols[9].Trim(), out int increaseBlack);
            int.TryParse(cols[10].Trim(), out int distanceFromOrigin);

            imported.Add(new SkillTreeNodeEntry
            {
                nodeId = cols[0].Trim(),
                displayName = cols[1].Trim(),
                displayDescription = cols[2].Trim(),
                gridPositionX = gridX,
                gridPositionY = gridY,
                parentNodeId = cols[5].Trim(),
                cost = cost,
                increaseLevelOfAllWedges = increaseAll,
                increaseLevelOfAllRedWedges = increaseRed,
                increaseLevelOfAllBlackWedges = increaseBlack,
                distanceFromOrigin = distanceFromOrigin
            });
        }

        if (errors.Count > 0)
        {
            bool proceed = EditorUtility.DisplayDialog("CSV Import — Parse Errors",
                $"{errors.Count} row(s) had errors and were skipped:\n\n{string.Join("\n", errors)}\n\nImport the remaining {imported.Count} node(s)?",
                "Import", "Cancel");
            if (!proceed) return;
        }

        Undo.RecordObject(skillTreeData, "Import Skill Tree CSV");
        skillTreeData.nodes = imported;
        EditorUtility.SetDirty(skillTreeData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SkillTreeEditor] Imported {imported.Count} node(s) from '{Path.GetFileName(filePath)}'.");
    }

    private void ExportToCsv()
    {
        string defaultName = skillTreeData != null
            ? Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(skillTreeData))
            : "SkillTreeData";

        string filePath = EditorUtility.SaveFilePanel("Export Skill Tree CSV", "", defaultName, "csv");
        if (string.IsNullOrEmpty(filePath)) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Join(",", CsvHeader()));

        foreach (SkillTreeNodeEntry node in skillTreeData.nodes)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                EscapeCsvField(node.nodeId),
                EscapeCsvField(node.displayName),
                EscapeCsvField(node.displayDescription),
                node.gridPositionX.ToString(),
                node.gridPositionY.ToString(),
                EscapeCsvField(node.parentNodeId),
                node.cost.ToString(System.Globalization.CultureInfo.InvariantCulture),
                node.increaseLevelOfAllWedges.ToString(),
                node.increaseLevelOfAllRedWedges.ToString(),
                node.increaseLevelOfAllBlackWedges.ToString(),
                node.distanceFromOrigin.ToString()
            }));
        }

        try
        {
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (IOException ex)
        {
            EditorUtility.DisplayDialog("CSV Export Error", $"Could not write file:\n{ex.Message}", "OK");
            return;
        }

        Debug.Log($"[SkillTreeEditor] Exported {skillTreeData.nodes.Count} node(s) to '{Path.GetFileName(filePath)}'.");
    }

    /// <summary>Returns the canonical CSV column headers in field order.</summary>
    private static string[] CsvHeader() => new[]
    {
        "nodeId", "displayName", "displayDescription", "gridPositionX", "gridPositionY", "parentNodeId",
        "cost", "increaseLevelOfAllWedges", "increaseLevelOfAllRedWedges", "increaseLevelOfAllBlackWedges",
        "distanceFromOrigin"
    };

    /// <summary>Parses a single CSV line, respecting double-quoted fields that may contain commas.</summary>
    private static string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Escaped quote: "" inside a quoted field
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }

    /// <summary>Wraps a field in quotes and escapes any internal quotes if the field contains commas or quotes.</summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
