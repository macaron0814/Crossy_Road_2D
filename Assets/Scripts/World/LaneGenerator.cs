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

    // 初期生成の範囲を記録（以降の生成と完全に同じ範囲を維持するため）
    private int initialPlayerLane;
    private bool initialized = false;
    
    // 画面端の空白を防ぐための予備生成数（カメラのデッドゾーン対策）
    private const int GENERATION_MARGIN = 5;

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

        int playerLane = Mathf.RoundToInt(player.position.y / laneHeight);

        // 目標範囲を計算（InitializeLanesと完全に同じ計算）
        int targetMin = playerLane - lanesBehind;
        int targetMax = playerLane + lanesAhead + GENERATION_MARGIN; // exclusive（targetMax自体は含まない）

        // 範囲外のレーンを削除
        List<int> toRemove = new List<int>();
        foreach (var key in spawnedLanes.Keys)
        {
            if (key < targetMin || key >= targetMax)
            {
                toRemove.Add(key);
            }
        }
        foreach (var key in toRemove)
        {
            RemoveLane(key);
        }

        // 範囲内で未生成のレーンを生成
        for (int i = targetMin; i < targetMax; i++)
        {
            if (!spawnedLanes.ContainsKey(i))
            {
                SpawnLane(i);
            }
        }
    }

    void InitializeLanes()
    {
        if (player == null) return;

        initialPlayerLane = Mathf.RoundToInt(player.position.y / laneHeight);

        // 目標範囲を計算（Updateと完全に同じ計算式）
        int targetMin = initialPlayerLane - lanesBehind;
        int targetMax = initialPlayerLane + lanesAhead + GENERATION_MARGIN; // exclusive

        for (int i = targetMin; i < targetMax; i++)
        {
            StartSpawnLane(i);
        }

        initialized = true;
        Debug.Log($"[LaneGenerator] 初期生成完了: playerLane={initialPlayerLane}, 範囲=[{targetMin}, {targetMax - 1}], 生成数={spawnedLanes.Count}");
    }

    void StartSpawnLane(int laneIndex)
    {
        // 既に生成されている場合はスキップ
        if (spawnedLanes.ContainsKey(laneIndex)) return;

        // 初期レーンは最初のプレハブ（Grass）を使用
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
        initialized = false;
    }
}
