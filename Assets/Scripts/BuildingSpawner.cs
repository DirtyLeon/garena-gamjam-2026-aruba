using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 不斷往後（Z 軸正方向）生成建築物，並回收已經離開視野的建築物。
/// 使用方式：
/// 1. 將此腳本掛在一個空物件上
/// 2. 在 Inspector 中指定 buildingPrefabs（建築物 Prefab 陣列）
/// 3. 指定 player（玩家 Transform，用來判斷回收距離）
/// 4. 調整參數即可
/// </summary>
public class BuildingSpawner : MonoBehaviour
{
    [Header("建築物設定")]
    [Tooltip("建築物 Prefab 列表，會隨機挑選生成")]
    [SerializeField] private GameObject[] buildingPrefabs;

    [Header("生成參數")]
    [Tooltip("建築物之間的間距（Z 軸）")]
    [SerializeField] private float spacing = 20f;

    [Tooltip("初始生成數量")]
    [SerializeField] private int initialCount = 10;

    [Tooltip("玩家前方多遠時預先生成")]
    [SerializeField] private float spawnAheadDistance = 100f;

    [Tooltip("玩家後方多遠時回收")]
    [SerializeField] private float despawnBehindDistance = 50f;

    [Header("位置偏移")]
    [Tooltip("生成的 X 軸位置（左側）")]
    [SerializeField] private float xOffsetLeft = -15f;

    [Tooltip("生成的 X 軸位置（右側）")]
    [SerializeField] private float xOffsetRight = 15f;

    [Tooltip("是否兩側都生成")]
    [SerializeField] private bool spawnBothSides = true;

    [Header("參考")]
    [Tooltip("玩家 Transform")]
    [SerializeField] private Transform player;

    // 目前最遠生成到的 Z 座標
    private float nextSpawnZ;

    // 活躍中的建築物
    private List<GameObject> activeBuildings = new List<GameObject>();

    // 物件池
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogError("[BuildingSpawner] buildingPrefabs 是空的！請在 Inspector 中拖入建築物 Prefab。", this);
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("[BuildingSpawner] Player 欄位未指定！初始建築會生成，但之後不會自動生成新建築。", this);
            nextSpawnZ = transform.position.z;
        }
        else
        {
            // 以玩家起始位置為中心開始生成
            nextSpawnZ = player.position.z;
        }

        // 初始生成一批建築物
        for (int i = 0; i < initialCount; i++)
        {
            SpawnBuilding();
        }

        Debug.Log($"[BuildingSpawner] 初始生成完畢，共 {activeBuildings.Count} 棟建築，起始 Z={player?.position.z ?? transform.position.z}，最遠 Z={nextSpawnZ}");
    }

    private void Update()
    {
        if (player == null) return;

        // 當玩家接近已生成的最遠處時，繼續往前生成
        while (nextSpawnZ - player.position.z < spawnAheadDistance)
        {
            SpawnBuilding();
        }

        // 回收玩家身後太遠的建築物
        for (int i = activeBuildings.Count - 1; i >= 0; i--)
        {
            if (player.position.z - activeBuildings[i].transform.position.z > despawnBehindDistance)
            {
                ReturnToPool(activeBuildings[i]);
                activeBuildings.RemoveAt(i);
            }
        }
    }

    private void SpawnBuilding()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

        // 左側生成
        PlaceBuilding(new Vector3(xOffsetLeft, 0f, nextSpawnZ));

        // 右側生成
        if (spawnBothSides)
        {
            PlaceBuilding(new Vector3(xOffsetRight, 0f, nextSpawnZ));
        }

        nextSpawnZ += spacing;
    }

    private void PlaceBuilding(Vector3 position)
    {
        GameObject building = GetFromPool();
        building.transform.position = position;
        building.transform.rotation = Quaternion.identity;
        building.SetActive(true);
        activeBuildings.Add(building);
    }

    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        // 池子為空，新建一個
        int index = Random.Range(0, buildingPrefabs.Length);
        GameObject obj = Instantiate(buildingPrefabs[index], transform);
        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
