using UnityEngine;
using TMPro;

namespace Wheels
{
    /// <summary>
    /// The visual representation of a wheel segment.
    /// The mesh is created at runtime since the weight of the segment can fluctuate.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class WheelSegmentVisual : MonoBehaviour
    {
        private const int ArcSegmentsPerDegree = 2;
        private const float ZPosition = 0f;

        private float _labelDistanceFromCenter = 0.6f;
        private float _labelFontSizeMultiplier = 1f;
        private Color _labelColor = Color.white;
        private TMP_FontAsset _labelFont;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private WheelSegmentData _segmentData;
        private TextMeshPro _labelText;
    
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Initialize(
            WheelSegmentData data, 
            float startAngle, 
            float sweepAngle, 
            float radius, 
            Material material,
            TMP_FontAsset labelFont
        ) {
            _segmentData = data;
            _labelFont = labelFont;
            GenerateMesh(startAngle, sweepAngle, radius);
        
            _meshRenderer.material = material;
        
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_BaseColor", data.segmentColor);
            propertyBlock.SetColor("_Color", data.segmentColor);
            _meshRenderer.SetPropertyBlock(propertyBlock);
            CreateLabel(startAngle, sweepAngle, radius);
        }

        private void CreateLabel(float startAngle, float sweepAngle, float radius)
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(transform, false);
            
            _labelText = labelObj.AddComponent<TextMeshPro>();
            
            _labelText.text = _segmentData.prizeName;
            _labelText.alignment = TextAlignmentOptions.Center;
            _labelText.fontSize = CalculateFontSize(sweepAngle, radius);
            _labelText.color = _labelColor;
            
            if (_labelFont != null)
            {
                _labelText.font = _labelFont;
            }
            
            _labelText.enableAutoSizing = true;
            _labelText.fontSizeMin = 1f;
            _labelText.fontSizeMax = _labelText.fontSize;
            
            float midAngle = startAngle + (sweepAngle * 0.5f);
            float angleRad = midAngle * Mathf.Deg2Rad;
            
            float labelDistance = radius * _labelDistanceFromCenter;
            float x = Mathf.Cos(angleRad) * labelDistance;
            float y = Mathf.Sin(angleRad) * labelDistance;
            
            labelObj.transform.localPosition = new Vector3(x, y, -0.1f);
            labelObj.transform.localRotation = Quaternion.Euler(0, 0, midAngle - 90f);
            
            float maxWidth = CalculateMaxLabelWidth(sweepAngle, labelDistance);
            float maxHeight = radius * 0.3f;
            
            _labelText.rectTransform.sizeDelta = new Vector2(maxWidth, maxHeight);
        }

        private float CalculateFontSize(float sweepAngle, float radius)
        {
            float baseFontSize = Mathf.Min(sweepAngle * 0.15f, radius * 0.4f);
            baseFontSize = Mathf.Clamp(baseFontSize, 2f, 20f);
            return baseFontSize * _labelFontSizeMultiplier;
        }

        private float CalculateMaxLabelWidth(float sweepAngle, float labelDistance)
        {
            float arcLength = (sweepAngle * Mathf.Deg2Rad) * labelDistance;
            return arcLength * 0.8f;
        }

        private void GenerateMesh(float startAngle, float sweepAngle, float radius)
        {
            int arcSegments = Mathf.Max(2, Mathf.CeilToInt(sweepAngle * ArcSegmentsPerDegree));
        
            int vertexCount = arcSegments + 2;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
        
            vertices[0] = new Vector3(0, 0, ZPosition);
            uvs[0] = new Vector2(0.5f, 0.5f);
        
            for (int i = 0; i <= arcSegments; i++)
            {
                float angle = startAngle + (sweepAngle * i / arcSegments);
                float angleRad = angle * Mathf.Deg2Rad;
            
                float x = Mathf.Cos(angleRad) * radius;
                float y = Mathf.Sin(angleRad) * radius;
            
                vertices[i + 1] = new Vector3(x, y, ZPosition);
            
                float u = (Mathf.Cos(angleRad) + 1f) * 0.5f;
                float v = (Mathf.Sin(angleRad) + 1f) * 0.5f;
                uvs[i + 1] = new Vector2(u, v);
            }
        
            int triangleCount = arcSegments;
            int[] triangles = new int[triangleCount * 3];
        
            for (int i = 0; i < arcSegments; i++)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
            }
        
            Mesh mesh = new Mesh
            {
                name = $"Mesh_{_segmentData}",
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };
        
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        
            _meshFilter.mesh = mesh;
        }
    }
}
