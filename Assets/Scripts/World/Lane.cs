using UnityEngine;
using System.Collections;
using System;

public class Lane : MonoBehaviour
{
    [Header("レーン設定")]
    public LaneType laneType = LaneType.Grass;

    [Header("スポナー設定")]
    public GameObject[] obstaclePrefab; // 障害物のPrefab
    public GameObject itemPrefab; // アイテムのPrefab
    public int itemCreatePercent;
    public float spawnInterval = 2f; // スポーン間隔
    public float spawnOffsetX = -10f; // スポーン開始位置
    public bool spawnFromRight = true; // 右からスポーンするか
    public int maxObstacles = 3; // 同時に存在できる最大数

    [Header("移動設定（車・電車用）")]
    public float obstacleSpeed = 3f;

    [Header("移動設定（丸太用）")]
    public float logSpeed = 2f;

    [Header("ランダム設定")]
    [Tooltip("Roadレーンの車の速度の最小値")]
    public float minCarSpeed = 3f;
    [Tooltip("Roadレーンの車の速度の最大値")]
    public float maxCarSpeed = 6f;
    [Tooltip("Road/Railレーンのスポーン間隔の最小値")]
    public float minSpawnInterval = 2f;
    [Tooltip("Road/Railレーンのスポーン間隔の最大値")]
    public float maxSpawnInterval = 5f;

    private float lastSpawnTime = 0f;
    private int currentObstacleCount = 0;

    public enum LaneType
    {
        Grass,  // 草地（障害物なし）
        Road,   // 道路（車が走る）
        River,  // 川（丸太が流れる）
        Rail    // 線路（電車が走る）
    }

    private void Start()
    {
        if (laneType == LaneType.Grass) return;

        // レーンごとのランダム設定（タイミングと速度）
        if (laneType == LaneType.Road)
        {
            obstacleSpeed = UnityEngine.Random.Range(minCarSpeed, maxCarSpeed);
            spawnInterval = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
        }
        else if (laneType == LaneType.Rail)
        {
            // 電車は速度は維持（またはInspector設定）、タイミングのみランダム
            // 電車は頻度が低めが良いので少し長めに補正しても良いが、ここでは範囲を使用
            spawnInterval = UnityEngine.Random.Range(minSpawnInterval + 2f, maxSpawnInterval + 3f);
        }

        // 初期スポーンのタイミングをずらす
        lastSpawnTime = Time.time - UnityEngine.Random.Range(0f, spawnInterval);

        int itemRand = UnityEngine.Random.Range(-10, 11);
        int itemCreatePercentRand = UnityEngine.Random.Range(0, 100);

        if (itemCreatePercentRand < itemCreatePercent)
        {
            // 障害物を生成
            GameObject item = Instantiate(itemPrefab, new Vector3(itemRand, transform.position.y, -0.5f), Quaternion.identity);
            item.transform.SetParent(transform);
        }
    }

    void Update()
    {
        // 草地は何もしない
        if (laneType == LaneType.Grass)
        {
            if (currentObstacleCount < maxObstacles)
            {
                int rand = UnityEngine.Random.Range(-10, 11);

                // 障害物を生成
                int obstacleRand = UnityEngine.Random.Range(0, 3);
                GameObject obstacle = Instantiate(obstaclePrefab[obstacleRand], new Vector3(rand, transform.position.y, -0.5f), Quaternion.identity);
                obstacle.transform.SetParent(transform);

                // カウントを増やす
                currentObstacleCount++;
            }
            return;
        }

        // スポーン間隔チェック
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            // 最大数チェック
            if (currentObstacleCount < maxObstacles)
            {
                SpawnObstacle();
                lastSpawnTime = Time.time;
            }
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;

        // スポーン位置を計算
        float spawnX = spawnFromRight ? spawnOffsetX : -spawnOffsetX;
        Vector3 spawnPos = new Vector3(spawnX, transform.position.y, -0.5f);

        // 障害物を生成
        int obstacleRand = UnityEngine.Random.Range(0, 3);
        GameObject obstacle = Instantiate(obstaclePrefab[obstacleRand], spawnPos, Quaternion.identity);
        obstacle.transform.SetParent(transform);

        // 移動スクリプトを設定
        if (laneType == LaneType.River)
        {
            // 丸太の場合
            ObstacleController logController = obstacle.GetComponent<ObstacleController>();
            if (logController == null)
            {
                logController = obstacle.AddComponent<ObstacleController>();
            }
            logController.speed = logSpeed;
            logController.moveRight = spawnFromRight;
            logController.obstacleType = ObstacleController.ObstacleType.Log;
        }
        else
        {
            // 車・電車の場合
            CarController carController = obstacle.GetComponent<CarController>();
            if (carController == null)
            {
                carController = obstacle.AddComponent<CarController>();
            }
            carController.speed = obstacleSpeed;
            carController.moveRight = spawnFromRight;
        }

        // カウントを増やす
        currentObstacleCount++;

        // 障害物が破棄されたらカウントを減らす
        StartCoroutine(WaitForDestroy(obstacle));
    }

    IEnumerator WaitForDestroy(GameObject obj)
    {
        yield return new WaitUntil(() => obj == null);
        currentObstacleCount = Mathf.Max(0, currentObstacleCount - 1);
    }

    // レーンタイプを設定（外部から変更可能）
    public void SetLaneType(LaneType type)
    {
        laneType = type;
    }
}

