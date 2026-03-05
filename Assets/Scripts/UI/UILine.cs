using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a UI line between two RectTransforms using Unity's Graphic system.
/// Attach to a UI GameObject that sits under the same Canvas as the targets.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UILine : Graphic
{
    [Tooltip("Start point of the line (typically the outer node button).")]
    public RectTransform from;

    [Tooltip("End point of the line (typically the parent node button).")]
    public RectTransform to;

    [Tooltip("Thickness of the rendered line in pixels.")]
    public float lineWidth = 4f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (from == null || to == null)
            return;

        Vector2 fromLocal = WorldToLocal(from.TransformPoint(Vector2.zero));
        Vector2 toLocal = WorldToLocal(to.TransformPoint(Vector2.zero));

        Vector2 dir = (toLocal - fromLocal).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * (lineWidth * 0.5f);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = fromLocal + perp;
        vh.AddVert(vertex);

        vertex.position = fromLocal - perp;
        vh.AddVert(vertex);

        vertex.position = toLocal - perp;
        vh.AddVert(vertex);

        vertex.position = toLocal + perp;
        vh.AddVert(vertex);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    private Vector2 WorldToLocal(Vector3 worldPosition)
    {
        return rectTransform.InverseTransformPoint(worldPosition);
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Keep the mesh refreshed in the editor when nodes are moved.
        SetVerticesDirty();
    }
#endif

    private void OnDrawGizmos()
    {
        if (from == null || to == null)
            return;

        Vector3 fromWorld = from.position;//from.TransformPoint(from.localPosition);
        Vector3 toWorld = to.position;//to.TransformPoint(to.localPosition);
        Vector3 midpoint = (fromWorld + toWorld) * 0.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(midpoint, Vector3.one * 10f);
    }
}
