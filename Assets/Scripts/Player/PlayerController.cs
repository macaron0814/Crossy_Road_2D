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

    [Header("ゲームオーバー演出")]
    [Tooltip("Hitアニメーションの1フレームの再生時間")]
    [SerializeField] private float hitFrameDuration = 0.15f;
    [Tooltip("最後から2番目(ぶつかり)フレームを保持する時間")]
    [SerializeField] private float hitHoldDuration = 0.2f;
    [Tooltip("最後のフレームでずり落ちる距離")]
    [Min(0f)]
    [SerializeField] private float hitSlideDistance = 0.4f;
    [Tooltip("最後のフレームでずり落ちる時間")]
    [SerializeField] private float hitSlideDuration = 0.35f;
    [Tooltip("ぶつかった時の最大スケール倍率")]
    [SerializeField] private float hitScaleMultiplier = 1.25f;

    private bool isMoving = false;
    private Vector3 targetPos;
    private Vector2 swipeStartPos;
    private Vector2 swipeEndPos;

    private Vector3 currentDirection = Vector3.up; // 初期は上向き
    private PlayerSpriteAnimator animator;
    private float lastMoveEndTime;
    private const float IDLE_DELAY = 0.4f;
    private bool isGameOverSequence;
    private PlayerBaseMapSwapper swapper;
    private Collider2D[] cachedColliders;
    private Rigidbody2D cachedRigidbody2D;
    private Coroutine gameOverCoroutine;

    enum SelectDir
    {
        Up,
        Down,
        Left,
        Right
    }
    SelectDir selectDir = SelectDir.Up;

    private void Start()
    {
        animator = PlayerSpriteAnimator.instance;
        swapper = GetComponent<PlayerBaseMapSwapper>();
        cachedRigidbody2D = GetComponent<Rigidbody2D>();

        // GameManagerのイベントを購読
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverPlayer += GameOverPlayer;
        }
    }

    void Update()
    {
        if (isGameOverSequence || isMoving) return;

        // 入力が無いときだけアイドルに戻す（毎フレームのリスタートを防ぐ）
        // 連続移動時のアニメーション切れを防ぐため、移動終了から少し猶予を持たせる
        bool inIdleDelay = Time.time < lastMoveEndTime + IDLE_DELAY;
        if (!IsPointerPressed() && !inIdleDelay)
        {
            switch (selectDir)
            {
                case SelectDir.Up:
                    SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.IdleUp, false);
                    break;
                case SelectDir.Down:
                    SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.IdelDown, false);
                    break;
                case SelectDir.Left:
                    SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.IdelLeft, false);
                    break;
                case SelectDir.Right:
                    SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.IdelRight, false);
                    break;
            }
        }

#if UNITY_EDITOR
        // PCデバッグ用
        HandleMouseInput();
#else
        // スマホ用
        HandleMouseInput();
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
            SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveUp, true);
            selectDir = SelectDir.Up;
            return;
        }

        // -------------------------
        // スワイプ方向取得
        // -------------------------
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // 横スワイプ
            if (delta.x > 0)
                SetDirection(Vector3.right, SelectDir.Right);
            else
                SetDirection(Vector3.left, SelectDir.Left);
        }
        else
        {
            // 縦スワイプ
            if (delta.y > 0)
                SetDirection(Vector3.up, SelectDir.Up);
            else
                SetDirection(Vector3.down, SelectDir.Down);
        }
    }

    // ================================
    // 向きを設定
    // ================================
    void SetDirection(Vector3 dir, SelectDir sd)
    {
        selectDir = sd;
        switch (selectDir)
        {
            case SelectDir.Up:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveUp, true);
                break;
            case SelectDir.Down:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveDown, true);
                break;
            case SelectDir.Left:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveLeft, true);
                break;
            case SelectDir.Right:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveRight, true);
                break;
        }

        // 向きだけ変える
        currentDirection = dir;
        MoveForward();
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

        // 同じモーションが続く場合はフレームを維持したままループする
        switch (selectDir)
        {
            case SelectDir.Up:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveUp, false);
                break;
            case SelectDir.Down:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveDown, false);
                break;
            case SelectDir.Left:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveLeft, false);
                break;
            case SelectDir.Right:
                SetAnimatorMotion(PlayerBaseMapSwapper.MotionType.MoveRight, false);
                break;
        }

        while (t < moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, targetPos, t / moveTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
        lastMoveEndTime = Time.time;

        // 到達したらスコア加算等
        // GameManager.Instance.UpdateScore(Mathf.FloorToInt(transform.position.y));
    }

    void GameOverPlayer()
    {
        if (isGameOverSequence) return;
        isGameOverSequence = true;

        // イベントから購読解除してから破棄
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverPlayer -= GameOverPlayer;
        }

        StopAllCoroutines();
        isMoving = false;
        DisableCollisionForGameOver();

        if (animator == null) animator = PlayerSpriteAnimator.instance;
        if (animator != null)
        {
            animator.Pause();
            animator.SetMotion(PlayerBaseMapSwapper.MotionType.Hit, true);
        }

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
        }
        gameOverCoroutine = StartCoroutine(PlayHitSequence());
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
        if (isGameOverSequence) return;

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

    private void SetAnimatorMotion(PlayerBaseMapSwapper.MotionType motion, bool restartOnChange)
    {
        if (animator == null) animator = PlayerSpriteAnimator.instance;
        if (animator == null) return;

        bool shouldRestart = restartOnChange && animator.CurrentMotion != motion;
        animator.Play(motion, shouldRestart);
    }

    private bool IsPointerPressed()
    {
#if UNITY_EDITOR
        return Input.GetMouseButton(0);
#else
        return Input.touchCount > 0 || Input.GetMouseButton(0);
#endif
    }

    private void DisableCollisionForGameOver()
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
        {
            cachedColliders = GetComponentsInChildren<Collider2D>();
        }
        foreach (var col in cachedColliders)
        {
            col.enabled = false;
        }

        if (cachedRigidbody2D != null)
        {
            cachedRigidbody2D.linearVelocity = Vector2.zero;
            cachedRigidbody2D.angularVelocity = 0f;
            cachedRigidbody2D.simulated = false;
        }
    }

    private IEnumerator PlayHitSequence()
    {
        if (swapper == null) swapper = GetComponent<PlayerBaseMapSwapper>();
        if (swapper == null)
        {
            Destroy(gameObject);
            yield break;
        }

        int frameCount = swapper.GetFrameCount(PlayerBaseMapSwapper.MotionType.Hit);
        if (frameCount <= 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 baseScale = transform.GetChild(0).localScale;
        Vector3 maxScale = baseScale * hitScaleMultiplier;
        int lastFrameIndex = frameCount - 1;
        int impactFrameIndex = Mathf.Max(0, frameCount - 2);
        float frameDuration = Mathf.Max(0f, hitFrameDuration);
        WaitForSeconds frameWait = frameDuration > 0f ? new WaitForSeconds(frameDuration) : null;

        for (int i = 0; i <= impactFrameIndex; i++)
        {
            float t = impactFrameIndex > 0 ? (float)i / impactFrameIndex : 1f;
            transform.GetChild(0).localScale = Vector3.Lerp(baseScale, maxScale, t);
            swapper.Apply(PlayerBaseMapSwapper.MotionType.Hit, i);
            if (frameWait != null)
            {
                yield return frameWait;
            }
            else
            {
                yield return null;
            }
        }

        if (hitHoldDuration > 0f)
        {
            float holdTimer = 0f;
            while (holdTimer < hitHoldDuration)
            {
                holdTimer += Time.deltaTime;
                yield return null;
            }
        }

        transform.GetChild(0).localScale = maxScale;
        swapper.Apply(PlayerBaseMapSwapper.MotionType.Hit, lastFrameIndex);

        Vector3 startPos = transform.GetChild(0).position;
        Vector3 endPos = startPos + Vector3.down * hitSlideDistance;
        if (hitSlideDuration <= 0f)
        {
            transform.GetChild(0).position = endPos;
        }
        else
        {
            float slideTimer = 0f;
            while (slideTimer < hitSlideDuration)
            {
                slideTimer += Time.deltaTime;
                float t = slideTimer / hitSlideDuration;
                transform.GetChild(0).position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
