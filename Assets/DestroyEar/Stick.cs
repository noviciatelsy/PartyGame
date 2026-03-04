using UnityEngine;

public class Stick : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 2f;

    public bool isPlayerOne = true;

    private float minX = -5f;
    private float maxX = 5f;

    private bool isFail = false;

    public EarGameManager gameManager;

    private void Update()
    {
        if (gameManager == null || !gameManager.isGameStarted)
            return;

        bool isHolding = false;

        // ===== 判断输入 =====
        if (isPlayerOne)
        {
            isHolding = Input.GetMouseButton(0);
        }
        else
        {
            isHolding = Input.GetKey(KeyCode.Space);
        }

        // ===== 只有长按时才移动 =====
        if (isHolding)
        {
            Debug.Log("?");
            MoveStick();
        }
    }

    void MoveStick()
    {
        float direction = isPlayerOne ? 1f : -1f;

        // 当前局部位置
        Vector3 pos = transform.position;

        // 先计算移动
        pos.x += direction * moveSpeed * Time.deltaTime;

        // 不乘 Time.deltaTime
        // 因为你已经把 speed 调得很小

        // 先更新位置
        transform.position = pos;
        Debug.Log(transform.position);

        // 再判断是否超出
        if (pos.x <= maxX && pos.x >= minX)
        {
            if (!isFail)
            {
                isFail = true;
                gameManager.OnStickFail();
            }

            // 强制拉回边界
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }
        else
        {
            // 只要没失败才计算得分
            gameManager.OnStickSuccess(pos.x);
        }
    }
}