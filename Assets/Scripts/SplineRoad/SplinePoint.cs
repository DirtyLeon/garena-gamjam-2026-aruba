using UnityEngine;

/// <summary>
/// Spline 上的一個控制點資料
/// </summary>
[System.Serializable]
public class SplinePoint
{
    public Vector3 position;
    public Vector3 handleIn;   // 進入方向的控制柄（相對於 position）
    public Vector3 handleOut;  // 離開方向的控制柄（相對於 position）

    public SplinePoint(Vector3 pos)
    {
        position = pos;
        handleIn = Vector3.back * 5f;
        handleOut = Vector3.forward * 5f;
    }
}
