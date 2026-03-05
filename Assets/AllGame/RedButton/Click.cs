using UnityEngine;

public class Click : MonoBehaviour
{
    [Header("固定位置")]
    public float upY = 1f;
    public float downY = -1f;

    [Header("移动速度")]
    public float moveSpeed = 25f;

    private float targetY;
    private bool moving = false;

    private void Awake()
    {
        // 初始在上方
        targetY = upY;
        Vector3 pos = transform.localPosition;
        pos.y = upY;
    }

    /// <summary>
    /// 外部调用
    /// </summary>
    public void Press()
    {
        // 每次点击 → 先下压
        targetY = downY;
        moving = true;
    }

    private void Update()
    {
        Vector3 pos = transform.localPosition;

        // 平滑移动到目标Y
        pos.y = Mathf.MoveTowards(
            pos.y,
            targetY,
            moveSpeed * Time.deltaTime
        );

        transform.localPosition = pos;

        // 如果已经到下方
        if (Mathf.Abs(pos.y - downY) < 0.01f)
        {
            // 自动回弹
            targetY = upY;
        }

        // 到达上方就停止移动
        if (Mathf.Abs(pos.y - upY) < 0.01f && targetY == upY)
        {
            moving = false;
        }
    }
}