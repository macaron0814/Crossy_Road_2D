using UnityEngine;

/// <summary>
/// PlayerBaseMapSwapper を一定間隔で切り替えてパラパラ漫画のように再生する。
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    public static PlayerSpriteAnimator instance;

    [SerializeField] private PlayerBaseMapSwapper swapper;
    [SerializeField] private PlayerBaseMapSwapper.MotionType currentMotion = PlayerBaseMapSwapper.MotionType.IdleUp;
    [Tooltip("1フレームあたりの時間(秒)")]
    [SerializeField] private float frameIdelDuration = 0.15f;
    [SerializeField] private float frameMoveDuration = 0.15f;
    [Tooltip("有効化時に自動再生する")]
    [SerializeField] private bool playOnEnable = true;
    [Tooltip("最後のフレームまで再生したら最初に戻る")]
    [SerializeField] private bool loop = true;

    private int frameIndex;
    private float timer;
    private bool playing;

    public PlayerBaseMapSwapper.MotionType CurrentMotion => currentMotion;

    private void Awake()
    {
        instance = this;
        if (swapper == null)
        {
            swapper = GetComponent<PlayerBaseMapSwapper>();
        }
    }

    private void OnEnable()
    {
        frameIndex = 0;
        timer = 0f;
        playing = playOnEnable;
        ApplyFrame();
    }

    private void Update()
    {
        if (!playing || swapper == null) return;

        if(currentMotion == PlayerBaseMapSwapper.MotionType.IdleUp ||
           currentMotion == PlayerBaseMapSwapper.MotionType.IdelDown ||
           currentMotion == PlayerBaseMapSwapper.MotionType.IdelLeft ||
           currentMotion == PlayerBaseMapSwapper.MotionType.IdelRight)
        {
            if (frameIdelDuration <= 0f) frameIdelDuration = 0.01f; // 安全な下限

            timer += Time.deltaTime;
            while (timer >= frameIdelDuration)
            {
                timer -= frameIdelDuration;
                StepFrame();
            }
        }
        else
        {
            if (frameMoveDuration <= 0f) frameMoveDuration = 0.01f; // 安全な下限

            timer += Time.deltaTime;
            while (timer >= frameMoveDuration)
            {
                timer -= frameMoveDuration;
                StepFrame();
            }
        }
    }

    /// <summary>再生開始（モーション変更も任意で指定）</summary>
    public void Play(PlayerBaseMapSwapper.MotionType? motion = null, bool restartFrame = false)
    {
        if (motion.HasValue) SetMotion(motion.Value, restartFrame);
        playing = true;
        ApplyFrame();
    }

    /// <summary>一時停止</summary>
    public void Pause()
    {
        playing = false;
    }

    /// <summary>停止して先頭フレームに戻す</summary>
    public void Stop()
    {
        playing = false;
        frameIndex = 0;
        ApplyFrame();
    }

    /// <summary>モーションを切り替え</summary>
    public void SetMotion(PlayerBaseMapSwapper.MotionType motion, bool restartFrame = true)
    {
        currentMotion = motion;
        if (restartFrame) frameIndex = 0;
        ApplyFrame();
    }

    private void StepFrame()
    {
        int frameCount = swapper.GetFrameCount(currentMotion);
        if (frameCount <= 0) return;

        frameIndex++;
        if (frameIndex >= frameCount)
        {
            if (loop)
            {
                frameIndex = 0;
            }
            else
            {
                frameIndex = frameCount - 1;
                playing = false;
            }
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        int frameCount = swapper != null ? swapper.GetFrameCount(currentMotion) : 0;
        if (frameCount <= 0) return;

        int clamped = Mathf.Clamp(frameIndex, 0, frameCount - 1);
        swapper.Apply(currentMotion, clamped);
    }
}


