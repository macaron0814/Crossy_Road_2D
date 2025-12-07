using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 3f;
    public bool moveRight = true;

    [Header("破棄設定")]
    public float destroyDistance = 20f; // 画面外に出たら破棄

    float randomSpeed;

    private void Start()
    {
        randomSpeed = Random.Range(3, randomSpeed);
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

    // 移動方向を設定（Inspector またはスクリプトから）
    public void SetDirection(bool right)
    {
        moveRight = right;
    }
}

