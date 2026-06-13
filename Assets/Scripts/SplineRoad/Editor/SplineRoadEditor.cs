using UnityEngine;
using UnityEditor;

/// <summary>
/// SplineRoad 的自訂 Editor，提供 Scene View 中可視化編輯控制點的功能
/// </summary>
[CustomEditor(typeof(SplineRoad))]
public class SplineRoadEditor : Editor
{
    private SplineRoad road;

    private void OnEnable()
    {
        road = (SplineRoad)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("新增控制點", GUILayout.Height(30)))
        {
            Undo.RecordObject(road, "Add Spline Point");

            Vector3 newPos;
            if (road.points.Count > 0)
            {
                // 在最後一個點的前方新增
                var last = road.points[road.points.Count - 1];
                newPos = last.position + Vector3.forward * 20f;
            }
            else
            {
                newPos = Vector3.zero;
            }

            road.points.Add(new SplinePoint(newPos));
            EditorUtility.SetDirty(road);
        }

        if (GUILayout.Button("生成道路 Mesh", GUILayout.Height(30)))
        {
            Undo.RecordObject(road, "Generate Road");
            road.GenerateRoad();
            EditorUtility.SetDirty(road);
        }

        if (road.points.Count > 0 && GUILayout.Button("移除最後一個控制點"))
        {
            Undo.RecordObject(road, "Remove Last Point");
            road.points.RemoveAt(road.points.Count - 1);
            if (road.autoGenerate) road.GenerateRoad();
            EditorUtility.SetDirty(road);
        }
    }

    private void OnSceneGUI()
    {
        if (road.points == null) return;

        Transform transform = road.transform;

        for (int i = 0; i < road.points.Count; i++)
        {
            SplinePoint point = road.points[i];

            // 主控制點
            Vector3 worldPos = transform.TransformPoint(point.position);
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (newWorldPos != worldPos)
            {
                Undo.RecordObject(road, "Move Spline Point");
                point.position = transform.InverseTransformPoint(newWorldPos);
                if (road.autoGenerate) road.GenerateRoad();
                EditorUtility.SetDirty(road);
            }

            // Handle Out（離開方向）
            Vector3 handleOutWorld = transform.TransformPoint(point.position + point.handleOut);
            Handles.color = Color.red;
            Handles.DrawLine(worldPos, handleOutWorld);
            Vector3 newHandleOut = Handles.FreeMoveHandle(
                handleOutWorld, 0.5f, Vector3.zero, Handles.SphereHandleCap
            );
            if (newHandleOut != handleOutWorld)
            {
                Undo.RecordObject(road, "Move Handle Out");
                point.handleOut = transform.InverseTransformPoint(newHandleOut) - point.position;
                // 鏡像：讓 handleIn 反向對稱
                point.handleIn = -point.handleOut;
                if (road.autoGenerate) road.GenerateRoad();
                EditorUtility.SetDirty(road);
            }

            // Handle In（進入方向）
            Vector3 handleInWorld = transform.TransformPoint(point.position + point.handleIn);
            Handles.color = Color.blue;
            Handles.DrawLine(worldPos, handleInWorld);
            Vector3 newHandleIn = Handles.FreeMoveHandle(
                handleInWorld, 0.5f, Vector3.zero, Handles.SphereHandleCap
            );
            if (newHandleIn != handleInWorld)
            {
                Undo.RecordObject(road, "Move Handle In");
                point.handleIn = transform.InverseTransformPoint(newHandleIn) - point.position;
                // 鏡像：讓 handleOut 反向對稱
                point.handleOut = -point.handleIn;
                if (road.autoGenerate) road.GenerateRoad();
                EditorUtility.SetDirty(road);
            }

            // 標示序號
            Handles.color = Color.white;
            Handles.Label(worldPos + Vector3.up * 2f, $"P{i}");
        }
    }
}
