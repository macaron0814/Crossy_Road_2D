using UnityEngine;
using System.Collections.Generic;

public class LaneGenerator : MonoBehaviour
{
    [Header("生成設定")]
    public Transform player;
    public GameObject[] lanePrefabs; // レーンのPrefab配列（Grass, Road, River, Rail）
    public int visibleLanes = 15; // プレイヤーの前後で表示するレーン数
    public float laneHeight = 1f; // レーンの高さ間隔

    [Header("生成範囲")]
    public int lanesAhead = 10; // プレイヤーの前方に生成するレーン数
    public int lanesBehind = 5; // プレイヤーの後方に保持するレーン数

    private Dictionary<int, GameObject> spawnedLanes = new Dictionary<int, GameObject>();
    private int currentMaxLane = 0;
    private int currentMinLane = 0;

    void Start()
    {
        // プレイヤーを自動検索
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // 初期レーンを生成
        InitializeLanes();
    }

    void Update()
    {
        if (player == null || lanePrefabs == null || lanePrefabs.Length == 0) return;

        int playerLane = Mathf.FloorToInt(player.position.y / laneHeight);

        // 前方にレーンを生成
        while (currentMaxLane < playerLane + lanesAhead)
        {
            SpawnLane(currentMaxLane);
            currentMaxLane++;
        }

        // 後方の古いレーンを削除
        while (currentMinLane < playerLane - lanesBehind)
        {
            RemoveLane(currentMinLane);
            currentMinLane++;
        }
    }

    void InitializeLanes()
    {
        // 初期位置から数レーン生成
        int startLane = -5;
        for (int i = startLane; i < lanesAhead; i++)
        {
            StartSpawnLane(i);
            currentMaxLane = Mathf.Max(currentMaxLane, i + 1);
        }
        currentMinLane = startLane;
    }

    void StartSpawnLane(int laneIndex)
    {
        // 既に生成されている場合はスキップ
        if (spawnedLanes.ContainsKey(laneIndex)) return;

        // ランダムにレーンプレハブを選択
        GameObject prefabToSpawn = lanePrefabs[0];

        // レーンを生成
        Vector3 spawnPos = new Vector3(0, laneIndex * laneHeight, 0);
        GameObject lane = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        lane.name = $"Lane_{laneIndex}";

        // 辞書に追加
        spawnedLanes[laneIndex] = lane;
    }

    void SpawnLane(int laneIndex)
    {
        // 既に生成されている場合はスキップ
        if (spawnedLanes.ContainsKey(laneIndex)) return;

        // ランダムにレーンプレハブを選択
        GameObject prefabToSpawn = lanePrefabs[Random.Range(0, lanePrefabs.Length)];

        // レーンを生成
        Vector3 spawnPos = new Vector3(0, laneIndex * laneHeight, 0);
        GameObject lane = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        lane.name = $"Lane_{laneIndex}";

        // 辞書に追加
        spawnedLanes[laneIndex] = lane;
    }

    void RemoveLane(int laneIndex)
    {
        if (spawnedLanes.ContainsKey(laneIndex))
        {
            Destroy(spawnedLanes[laneIndex]);
            spawnedLanes.Remove(laneIndex);
        }
    }

    // すべてのレーンをクリア（リスタート時など）
    public void ClearAllLanes()
    {
        foreach (var lane in spawnedLanes.Values)
        {
            if (lane != null)
            {
                Destroy(lane);
            }
        }
        spawnedLanes.Clear();
        currentMaxLane = 0;
        currentMinLane = 0;
    }
}

