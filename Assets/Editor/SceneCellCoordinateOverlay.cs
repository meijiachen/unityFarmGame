using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneCellCoordinateOverlay
{
    private const string EnabledPrefKey = "SceneCellCoordinateOverlay.Enabled";
    private static readonly GUIContent CellLabel = new GUIContent();

    private static GUIStyle labelStyle;
    private static bool isEnabled;
    private static Vector2 lastMousePosition;
    private static bool hasMousePosition;

    static SceneCellCoordinateOverlay()
    {
        isEnabled = EditorPrefs.GetBool(EnabledPrefKey, true);

        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Scene Cell Coordinates/Enabled")]
    private static void ToggleEnabled()
    {
        isEnabled = !isEnabled;
        EditorPrefs.SetBool(EnabledPrefKey, isEnabled);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Scene Cell Coordinates/Enabled", true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked("Tools/Scene Cell Coordinates/Enabled", isEnabled);
        return true;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!isEnabled || sceneView == null || Event.current == null)
        {
            return;
        }

        Event currentEvent = Event.current;
        sceneView.wantsMouseMove = true;
        UpdateMousePosition(currentEvent, sceneView);

        if (currentEvent.type != EventType.Repaint || !hasMousePosition)
        {
            return;
        }

        UnityEngine.Grid grid = ResolveGrid();
        if (grid == null)
        {
            return;
        }

        Vector3 worldPosition;
        if (!TryGetWorldPositionOnGridPlane(grid, lastMousePosition, out worldPosition))
        {
            return;
        }

        Vector3Int cellPosition = grid.WorldToCell(worldPosition);
        CellLabel.text = string.Format("Cell: ({0}, {1}, {2})", cellPosition.x, cellPosition.y, cellPosition.z);

        DrawLabel(sceneView, lastMousePosition, CellLabel);
    }

    private static void UpdateMousePosition(Event currentEvent, SceneView sceneView)
    {
        if (currentEvent.type == EventType.MouseLeaveWindow)
        {
            hasMousePosition = false;
            sceneView.Repaint();
            return;
        }

        if (currentEvent.isMouse || currentEvent.type == EventType.MouseMove)
        {
            lastMousePosition = currentEvent.mousePosition;
            hasMousePosition = true;

            if (currentEvent.type == EventType.MouseMove || currentEvent.type == EventType.MouseDrag)
            {
                sceneView.Repaint();
            }
        }
    }

    private static UnityEngine.Grid ResolveGrid()
    {
        if (Selection.activeGameObject != null)
        {
            UnityEngine.Grid selectedGrid = Selection.activeGameObject.GetComponentInParent<UnityEngine.Grid>();
            if (selectedGrid != null)
            {
                return selectedGrid;
            }
        }

        return Object.FindObjectOfType<UnityEngine.Grid>();
    }

    private static bool TryGetWorldPositionOnGridPlane(UnityEngine.Grid grid, Vector2 mousePosition, out Vector3 worldPosition)
    {
        Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
        Plane gridPlane = new Plane(grid.transform.forward, grid.transform.position);

        float distance;
        if (gridPlane.Raycast(worldRay, out distance))
        {
            worldPosition = worldRay.GetPoint(distance);
            return true;
        }

        worldPosition = Vector3.zero;
        return false;
    }

    private static void DrawLabel(SceneView sceneView, Vector2 mousePosition, GUIContent content)
    {
        GUIStyle style = GetLabelStyle();
        Vector2 labelSize = style.CalcSize(content);
        const float padding = 8f;
        const float offset = 14f;

        float width = labelSize.x + padding * 2f;
        float height = labelSize.y + padding;
        float x = Mathf.Min(mousePosition.x + offset, sceneView.position.width - width - padding);
        float y = Mathf.Min(mousePosition.y + offset, sceneView.position.height - height - padding);

        x = Mathf.Max(padding, x);
        y = Mathf.Max(padding, y);

        Handles.BeginGUI();
        GUI.Label(new Rect(x, y, width, height), content, style);
        Handles.EndGUI();
    }

    private static GUIStyle GetLabelStyle()
    {
        if (labelStyle != null)
        {
            return labelStyle;
        }

        labelStyle = new GUIStyle(EditorStyles.helpBox)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12
        };
        labelStyle.normal.textColor = Color.white;

        return labelStyle;
    }
}
