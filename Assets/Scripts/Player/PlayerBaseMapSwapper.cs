using UnityEngine;

/// <summary>
/// Renderer の BaseMap/_MainTex をオブジェクトごとに差し替えるコンポーネント。
/// 「モーション 大ラフ」内の画像を Sprite 参照で割り当てれば、その Sprite の Texture をセットする。
/// </summary>
public class PlayerBaseMapSwapper : MonoBehaviour
{
    public enum MotionType
    {
        IdleUp, // 1 待機モーション(↑方向) / ↑待機*
        IdelDown,
        IdelLeft,
        IdelRight,
        MoveUp, // 2 移動モーション(↑方向) / ↑移動*
        MoveDown,
        MoveLeft,
        MoveRight,
        Turn,   // 3 振り向きモーション / 振り向き*
        Hit     // 4 ぶつかりモーション / ぶつかり*
    }

    [Header("参照")]
    [SerializeField] private Renderer targetRenderer;
    [Tooltip("true: 個別マテリアルにしてこのオブジェクトだけ差し替え")]
    [SerializeField] private bool instantiateMaterial = true;

    [Header("モーション 大ラフ から割り当て (Sprite 配列)")]
    [Tooltip("IdleUp のアニメーションフレームを順番に並べる")]
    [SerializeField] private Sprite[] idleUpSprites;
    [SerializeField] private Sprite[] idleDownSprites;
    [SerializeField] private Sprite[] idleLeftSprites;
    [SerializeField] private Sprite[] idleRightSprites;
    [Tooltip("MoveUp のアニメーションフレームを順番に並べる")]
    [SerializeField] private Sprite[] moveUpSprites;
    [SerializeField] private Sprite[] moveDownSprites;
    [SerializeField] private Sprite[] moveLeftSprites;
    [SerializeField] private Sprite[] moveRightSprites;
    [Tooltip("Turn のアニメーションフレームを順番に並べる")]
    [SerializeField] private Sprite[] turnSprites;
    [Tooltip("Hit のアニメーションフレームを順番に並べる")]
    [SerializeField] private Sprite[] hitSprites;

    private Material runtimeMat;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null) return;

        // 共有マテリアルを汚さないように必要なら複製
        runtimeMat = instantiateMaterial ? targetRenderer.material : targetRenderer.sharedMaterial;
    }

    public void Apply(MotionType motion, int frameIndex = 0)
    {
        ApplySprite(GetSpriteFor(motion, frameIndex));
    }

    private void ApplySprite(Sprite sprite)
    {
        if (runtimeMat == null || sprite == null) return;

        // Sprite から元の Texture2D を取得
        Texture2D tex = sprite.texture;
        string prop = runtimeMat.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
        runtimeMat.SetTexture(prop, tex);
    }

    private Sprite GetSpriteFor(MotionType motion, int frameIndex)
    {
        switch (motion)
        {
            case MotionType.IdleUp:
                return GetFrame(idleUpSprites, frameIndex);
            case MotionType.IdelDown:
                return GetFrame(idleDownSprites, frameIndex);
            case MotionType.IdelLeft:
                return GetFrame(idleLeftSprites, frameIndex);
            case MotionType.IdelRight:
                return GetFrame(idleRightSprites, frameIndex);
            case MotionType.MoveUp:
                return GetFrame(moveUpSprites, frameIndex);
            case MotionType.MoveDown:
                return GetFrame(moveDownSprites, frameIndex);
            case MotionType.MoveLeft:
                return GetFrame(moveLeftSprites, frameIndex);
            case MotionType.MoveRight:
                return GetFrame(moveRightSprites, frameIndex);
            case MotionType.Turn:
                return GetFrame(turnSprites, frameIndex);
            case MotionType.Hit:
                return GetFrame(hitSprites, frameIndex);
            default:
                return null;
        }
    }

    private Sprite GetFrame(Sprite[] frames, int index)
    {
        if (frames == null || frames.Length == 0) return null;
        int safeIndex = Mathf.Clamp(index, 0, frames.Length - 1);
        return frames[safeIndex];
    }

    public int GetFrameCount(MotionType motion)
    {
        switch (motion)
        {
            case MotionType.IdleUp:
                return GetLength(idleUpSprites);
            case MotionType.IdelDown:
                return GetLength(idleUpSprites);
            case MotionType.IdelLeft:
                return GetLength(idleLeftSprites);
            case MotionType.IdelRight:
                return GetLength(idleRightSprites);
            case MotionType.MoveUp:
                return GetLength(moveUpSprites);
            case MotionType.MoveDown:
                return GetLength(moveDownSprites);
            case MotionType.MoveLeft:
                return GetLength(moveLeftSprites);
            case MotionType.MoveRight:
                return GetLength(moveRightSprites);
            case MotionType.Turn:
                return GetLength(turnSprites);
            case MotionType.Hit:
                return GetLength(hitSprites);
            default:
                return 0;
        }
    }

    private int GetLength(Sprite[] frames)
    {
        return frames != null ? frames.Length : 0;
    }
}


