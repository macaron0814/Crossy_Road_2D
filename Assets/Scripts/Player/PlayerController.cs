using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveStep = 1f;
    public float moveTime = 0.1f;

    [Header("衝突判定設定")]
    public float collisionCheckRadius = 0.4f; // 衝突チェックの半径
    public LayerMask obstacleLayer; // 障害物のレイヤー（オプション）
    public string obstacleTag = "Obstacle"; // 障害物のタグ

    private bool isMoving = false;
    private Vector3 targetPos;
    private Vector2 swipeStartPos;
    private Vector2 swipeEndPos;

    private Vector3 currentDirection = Vector3.up; // 初期は上向き

    private void Start()
    {
        // GameManagerのイベントを購読
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverPlayer += GameOverPlayer;
        }
    }

    void Update()
    {
        if (isMoving) return;

#if UNITY_EDITOR
        // PCデバッグ用
        HandleMouseInput();
#else
        // スマホ用
        HandleTouchInput();
#endif
    }

    // ================================
    //  スマホ（タッチ）
    // ================================
    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                swipeStartPos = touch.position;
                break;

            case TouchPhase.Ended:
                swipeEndPos = touch.position;
                DetectInput(swipeEndPos - swipeStartPos);
                break;
        }
    }

    // ================================
    // PCエディタ用（クリック＆ドラッグ）
    // ================================
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            swipeEndPos = Input.mousePosition;
            DetectInput(swipeEndPos - swipeStartPos);
        }
    }

    // ================================
    // 入力判定（タップ or スワイプ）
    // ================================
    void DetectInput(Vector2 delta)
    {
        float distance = delta.magnitude;

        // -------------------------
        // タップ判定（小さい移動）
        // -------------------------
        if (distance < 20f)
        {
            // タップ時は常に上方向に前進
            MoveInDirection(Vector3.up);
            return;
        }

        // -------------------------
        // スワイプ方向取得
        // -------------------------
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // 横スワイプ
            if (delta.x > 0)
                SetDirection(Vector3.right);
            else
                SetDirection(Vector3.left);
        }
        else
        {
            // 縦スワイプ
            if (delta.y > 0)
                SetDirection(Vector3.up);
            else
                SetDirection(Vector3.down);
        }
    }

    // ================================
    // 向きを設定
    // ================================
    void SetDirection(Vector3 dir)
    {
        // 既に同じ向きなら前進
        if (currentDirection == dir)
        {
            MoveForward();
            return;
        }

        // 向きだけ変える
        currentDirection = dir;

        // キャラクターの見た目方向を変えたいならここでRotate
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); // 上をデフォルトに合わせた補正
    }

    // ================================
    // 前へ1マス移動
    // ================================
    void MoveForward()
    {
        MoveInDirection(currentDirection);
    }

    // ================================
    // 指定方向に1マス移動
    // ================================
    void MoveInDirection(Vector3 direction)
    {
        targetPos = transform.position + direction * moveStep;

        // 移動先に障害物があるかチェック
        if (CanMoveTo(targetPos))
        {
            StartCoroutine(Move());
        }
        else
        {
            // 障害物があるため移動しない
            // 必要に応じて効果音やアニメーションを再生
        }
    }

    // ================================
    // 移動先に障害物があるかチェック
    // ================================
    bool CanMoveTo(Vector3 position)
    {
        // 方法1: OverlapCircleで障害物を検出
        Collider2D hit = Physics2D.OverlapCircle(position, collisionCheckRadius);

        if (hit != null)
        {
            // 自分自身のColliderは無視
            if (hit.gameObject == gameObject)
            {
                return true;
            }

            // 障害物タグまたはレイヤーで判定
            if (!string.IsNullOrEmpty(obstacleTag) && hit.CompareTag(obstacleTag))
            {
                return false; // 障害物がある
            }

            // レイヤーマスクで判定（設定されている場合）
            if (obstacleLayer.value != 0 && ((1 << hit.gameObject.layer) & obstacleLayer.value) != 0)
            {
                return false; // 障害物がある
            }

            // 車や電車は移動を妨げない（OnTriggerEnter2Dで処理）
            if (hit.CompareTag("Car") || hit.CompareTag("Train"))
            {
                return true; // 車・電車は移動を妨げない（衝突で死亡するため）
            }
        }

        // 方法2: Raycastで移動経路をチェック（より正確）
        Vector2 direction = (position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, position);
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, direction, distance);

        if (raycastHit.collider != null)
        {
            // 自分自身のColliderは無視
            if (raycastHit.collider.gameObject == gameObject)
            {
                return true;
            }

            // 障害物タグまたはレイヤーで判定
            if (!string.IsNullOrEmpty(obstacleTag) && raycastHit.collider.CompareTag(obstacleTag))
            {
                return false; // 障害物がある
            }

            // レイヤーマスクで判定
            if (obstacleLayer.value != 0 && ((1 << raycastHit.collider.gameObject.layer) & obstacleLayer.value) != 0)
            {
                return false; // 障害物がある
            }

            // 車や電車は移動を妨げない
            if (raycastHit.collider.CompareTag("Car") || raycastHit.collider.CompareTag("Train"))
            {
                return true;
            }
        }

        return true; // 障害物がないので移動可能
    }

    // ================================
    // 滑らかな移動
    // ================================
    IEnumerator Move()
    {
        isMoving = true;
        float t = 0;
        Vector3 start = transform.position;

        while (t < moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, targetPos, t / moveTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        // 到達したらスコア加算等
        // GameManager.Instance.UpdateScore(Mathf.FloorToInt(transform.position.y));
    }

    void GameOverPlayer()
    {
        // イベントから購読解除してから破棄
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverPlayer -= GameOverPlayer;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // オブジェクトが破棄される際にイベントから購読解除
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverPlayer -= GameOverPlayer;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            GameManager.Instance.GameOver();
        }

        if (other.CompareTag("Item"))
        {
            GameManager.Instance.AddStamina(30);
            Destroy(other.gameObject);
        }
    }
}
