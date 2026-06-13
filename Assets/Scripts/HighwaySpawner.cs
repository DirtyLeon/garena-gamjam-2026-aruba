using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 隨玩家移動無限生成高速道路段落。
/// 
/// 設置步驟：
/// 1. 製作一段道路 Prefab（例如一段直線公路，長度對應 segmentLength）
/// 2. 將此腳本掛在空物件上
/// 3. 拖入道路 Prefab 和玩家 Transform
/// 4. 設定 segmentLength 為單段道路在 Z 軸方向的長度
/// </summary>
public class HighwaySpawner : MonoBehaviour
{
    [Header("道路設定")]
    [Tooltip("道路段落 Prefab（可放多個隨機挑選）")]
    [SerializeField] private GameObject[] roadSegmentPrefabs;

    [Tooltip("每段道路在 Z 軸方向的長度")]
    [SerializeField] private float segmentLength = 50f;

    [Header("生成參數")]
    [Tooltip("玩家前方預先生成的段數")]
    [SerializeField] private int segmentsAhead = 5;

    [Tooltip("玩家後方保留的段數（超過就回收）")]
    [SerializeField] private int segmentsBehind = 3;

    [Header("參考")]
    [SerializeField] private Transform player;

    // 已生成的道路段落（依 Z 位置排序）
    private LinkedList<GameObject> activeSegments = new LinkedList<GameObject>();

    // 物件池
    private Queue<GameObject> pool = new Queue<GameObject>();

    // 下一段要生成的 Z 座標（前方）
    private float nextSpawnZ;

    // 最後方已生成的 Z 座標
    private float backEdgeZ;

    private void Start()
    {
        if (roadSegmentPrefabs == null || roadSegmentPrefabs.Length == 0)
        {
            Debug.LogError("[HighwaySpawner] roadSegmentPrefabs 是空的！請拖入道路 Prefab。", this);
            return;
        }

        if (player == null)
        {
            Debug.LogError("[HighwaySpawner] Player 未指定！", this);
            return;
        }

        // 以玩家位置為中心，往前後各生成
        float playerZ = player.position.z;

        // 從玩家後方開始生成
        nextSpawnZ = playerZ - segmentsBehind * segmentLength;
        backEdgeZ = nextSpawnZ;

        // 生成前方 + 後方的總段數
        int totalSegments = segmentsAhead + segmentsBehind;
        for (int i = 0; i < totalSegments; i++)
        {
            SpawnSegmentAtFront();
        }

        Debug.Log($"[HighwaySpawner] 初始生成完畢，共 {activeSegments.Count} 段道路");
    }

    private void Update()
    {
        if (player == null) return;
        if (roadSegmentPrefabs == null || roadSegmentPrefabs.Length == 0) return;

        // 玩家接近前方邊界時，生成新道路
        float aheadThreshold = player.position.z + segmentsAhead * segmentLength;
        while (nextSpawnZ < aheadThreshold)
        {
            SpawnSegmentAtFront();
        }

        // 回收玩家後方太遠的道路
        float behindThreshold = player.position.z - segmentsBehind * segmentLength;
        while (activeSegments.Count > 0 && activeSegments.First.Value.transform.position.z + segmentLength < behindThreshold)
        {
            RecycleSegmentFromBack();
        }
    }

    private void SpawnSegmentAtFront()
    {
        GameObject segment = GetFromPool();
        segment.transform.position = new Vector3(transform.position.x, transform.position.y, nextSpawnZ);
        segment.transform.rotation = Quaternion.identity;
        segment.SetActive(true);
        activeSegments.AddLast(segment);
        nextSpawnZ += segmentLength;
    }

    private void RecycleSegmentFromBack()
    {
        GameObject segment = activeSegments.First.Value;
        activeSegments.RemoveFirst();
        ReturnToPool(segment);
    }

    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        int index = Random.Range(0, roadSegmentPrefabs.Length);
        GameObject obj = Instantiate(roadSegmentPrefabs[index], transform);
        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
