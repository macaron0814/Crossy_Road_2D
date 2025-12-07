using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追従設定")]
    public Transform target;
    public float followSpeed = 2f;
    public float offsetY = -2f; // プレイヤーより少し後ろの位置

    [Header("制限")]
    public bool onlyFollowUp = true; // 上方向のみ追従（下には戻らない）

    private Vector3 initialPosition;
    private float highestY = float.MinValue;

    void Start()
    {
        if (target == null)
        {
            // プレイヤーを自動検索
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        initialPosition = transform.position;
        highestY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = transform.position;
        float targetY = target.position.y + offsetY;

        // 上方向のみ追従する場合
        if (onlyFollowUp)
        {
            if (targetY > highestY)
            {
                highestY = targetY;
            }
            targetPos.y = Mathf.Lerp(transform.position.y, highestY, followSpeed * Time.deltaTime);
        }
        else
        {
            // 常に追従
            targetPos.y = Mathf.Lerp(transform.position.y, targetY, followSpeed * Time.deltaTime);
        }

        // 横方向は固定（クロッシーロードらしい固定視点）
        transform.position = targetPos;
    }
}

