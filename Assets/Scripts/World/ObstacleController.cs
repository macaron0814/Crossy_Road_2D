using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2f;
    public bool moveRight = true;

    [Header("破棄設定")]
    public float destroyDistance = 20f;

    [Header("移動タイプ")]
    public ObstacleType obstacleType = ObstacleType.Log;

    public enum ObstacleType
    {
        Log,        // 丸太（川で流れる）
        MovingPlatform, // 動く足場
        Other       // その他
    }

    void Update()
    {
        // 横方向に移動
        Vector3 direction = moveRight ? Vector3.right : Vector3.left;
        transform.Translate(direction * speed * Time.deltaTime);

        // 画面外に出たら破棄
        if (Mathf.Abs(transform.position.x) > destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    // 移動方向を設定
    public void SetDirection(bool right)
    {
        moveRight = right;
    }

    // 速度を設定
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}

