using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spline 道路生成工具
/// 透過 Bezier Spline 控制點定義道路路徑，自動生成道路 Mesh。
/// 
/// 使用方式：
/// 1. 將此腳本掛在空物件上
/// 2. 在 Inspector 新增控制點（或在 Scene View 用 Editor 工具編輯）
/// 3. 調整道路寬度、細分數等參數
/// 4. 點擊 Generate Road 或勾選 Auto Generate
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SplineRoad : MonoBehaviour
{
    [Header("控制點")]
    public List<SplinePoint> points = new List<SplinePoint>();

    [Header("道路參數")]
    [Tooltip("道路寬度")]
    public float roadWidth = 10f;

    [Tooltip("每兩個控制點之間的細分段數")]
    [Range(2, 50)]
    public int segmentsPerCurve = 20;

    [Tooltip("UV 沿道路方向的縮放（越小紋理越密）")]
    public float uvScale = 0.1f;

    [Header("自動生成")]
    [Tooltip("修改參數時自動重新生成 Mesh")]
    public bool autoGenerate = true;

    private MeshFilter meshFilter;
    private Mesh roadMesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void OnValidate()
    {
        if (autoGenerate && points.Count >= 2)
        {
            // 延遲到下一幀生成，避免在 OnValidate 中修改 Mesh 的警告
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) GenerateRoad();
            };
        }
    }

    /// <summary>
    /// 生成道路 Mesh
    /// </summary>
    public void GenerateRoad()
    {
        if (points.Count < 2)
        {
            Debug.LogWarning("[SplineRoad] 至少需要 2 個控制點才能生成道路。");
            return;
        }

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (roadMesh == null)
        {
            roadMesh = new Mesh();
            roadMesh.name = "SplineRoad_Mesh";
        }
        else
        {
            roadMesh.Clear();
        }

        // 取得 Spline 上所有取樣點
        List<Vector3> splinePoints = GetSplinePoints();

        // 根據取樣點生成 Mesh
        BuildMesh(splinePoints);

        meshFilter.sharedMesh = roadMesh;
    }

    /// <summary>
    /// 沿 Spline 取樣所有點
    /// </summary>
    private List<Vector3> GetSplinePoints()
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            SplinePoint p0 = points[i];
            SplinePoint p1 = points[i + 1];

            int segments = segmentsPerCurve;
            for (int s = 0; s <= segments; s++)
            {
                // 避免重複加入相同的點（除了第一段的起點）
                if (i > 0 && s == 0) continue;

                float t = (float)s / segments;
                Vector3 point = EvaluateBezier(
                    p0.position,
                    p0.position + p0.handleOut,
                    p1.position + p1.handleIn,
                    p1.position,
                    t
                );
                result.Add(point);
            }
        }

        return result;
    }

    /// <summary>
    /// 三次 Bezier 曲線取值
    /// </summary>
    private Vector3 EvaluateBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return u * u * u * a
             + 3f * u * u * t * b
             + 3f * u * t * t * c
             + t * t * t * d;
    }

    /// <summary>
    /// 根據 Spline 取樣點建立道路 Mesh
    /// </summary>
    private void BuildMesh(List<Vector3> splinePoints)
    {
        int pointCount = splinePoints.Count;
        if (pointCount < 2) return;

        Vector3[] vertices = new Vector3[pointCount * 2];
        Vector2[] uvs = new Vector2[pointCount * 2];
        int[] triangles = new int[(pointCount - 1) * 6];

        float halfWidth = roadWidth * 0.5f;
        float accumulatedLength = 0f;

        for (int i = 0; i < pointCount; i++)
        {
            // 計算方向
            Vector3 forward;
            if (i < pointCount - 1)
                forward = (splinePoints[i + 1] - splinePoints[i]).normalized;
            else
                forward = (splinePoints[i] - splinePoints[i - 1]).normalized;

            // 計算右方向（在 XZ 平面）
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // 如果道路是垂直的，用另一個 up
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.Cross(Vector3.right, forward).normalized;

            // 左右頂點
            vertices[i * 2] = splinePoints[i] - right * halfWidth;
            vertices[i * 2 + 1] = splinePoints[i] + right * halfWidth;

            // UV
            if (i > 0)
                accumulatedLength += Vector3.Distance(splinePoints[i], splinePoints[i - 1]);

            float v = accumulatedLength * uvScale;
            uvs[i * 2] = new Vector2(0f, v);
            uvs[i * 2 + 1] = new Vector2(1f, v);
        }

        // 三角形
        for (int i = 0; i < pointCount - 1; i++)
        {
            int baseIndex = i * 6;
            int vertBase = i * 2;

            // 第一個三角形
            triangles[baseIndex] = vertBase;
            triangles[baseIndex + 1] = vertBase + 2;
            triangles[baseIndex + 2] = vertBase + 1;

            // 第二個三角形
            triangles[baseIndex + 3] = vertBase + 1;
            triangles[baseIndex + 4] = vertBase + 2;
            triangles[baseIndex + 5] = vertBase + 3;
        }

        roadMesh.vertices = vertices;
        roadMesh.uv = uvs;
        roadMesh.triangles = triangles;
        roadMesh.RecalculateNormals();
        roadMesh.RecalculateBounds();
    }

    /// <summary>
    /// 取得 Spline 上某個 t 值的世界座標（t: 0~1 整條路徑）
    /// </summary>
    public Vector3 GetPointOnSpline(float t)
    {
        if (points.Count < 2) return transform.position;

        t = Mathf.Clamp01(t);
        float totalT = t * (points.Count - 1);
        int segIndex = Mathf.Min((int)totalT, points.Count - 2);
        float localT = totalT - segIndex;

        SplinePoint p0 = points[segIndex];
        SplinePoint p1 = points[segIndex + 1];

        return EvaluateBezier(
            p0.position,
            p0.position + p0.handleOut,
            p1.position + p1.handleIn,
            p1.position,
            localT
        );
    }

    private void OnDrawGizmos()
    {
        if (points == null || points.Count < 2) return;

        // 畫控制點
        Gizmos.color = Color.yellow;
        foreach (var p in points)
        {
            Gizmos.DrawSphere(transform.TransformPoint(p.position), 0.5f);

            // 畫控制柄
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                transform.TransformPoint(p.position),
                transform.TransformPoint(p.position + p.handleIn)
            );
            Gizmos.DrawLine(
                transform.TransformPoint(p.position),
                transform.TransformPoint(p.position + p.handleOut)
            );
            Gizmos.color = Color.yellow;
        }

        // 畫 Spline 曲線
        Gizmos.color = Color.green;
        List<Vector3> pts = GetSplinePoints();
        for (int i = 0; i < pts.Count - 1; i++)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(pts[i]),
                transform.TransformPoint(pts[i + 1])
            );
        }
    }
}
