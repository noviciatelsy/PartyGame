using UnityEngine;

public class FaceFeature : MonoBehaviour
{
    [Header("下落速度区间")]
    private float minFallSpeed = 5f;
    private float maxFallSpeed = 9f;

    [Header("目标Y位置")]
    public float targetY;

    private FaceController face;

    private bool isFalling = false;
    private bool isStopped = false;

    private float fallSpeed;
    private float startY = 15f;

    public void Init(FaceController controller)
    {
        face = controller;

    }

    public void StartFalling()
    {
        isStopped = false;
        isFalling = true;

        // 初始化位置
        Vector3 pos = transform.position;
        pos.y = startY;
        transform.position = pos;

        // 随机速度
        fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
    }

    private void Update()
    {
        if (!isFalling) return;

        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < -18.0f)
        {
            AutoFail();
        }
    }

    void AutoFail()
    {
        if (isStopped) return;

        isFalling = false;
        isStopped = true;

        // 给一个最大误差（直接使用 maxDis）
        float failDistance = face != null ? 999f : 0f;

        face.OnFeatureStopped(failDistance);
    }

    public void TryStop()
    {
        if (!isFalling || isStopped) return;

        StopFeature();
    }

    void StopFeature()
    {
        isFalling = false;
        isStopped = true;

        float currentY = transform.position.y;
        float distance = currentY - targetY;

        face.OnFeatureStopped(distance);
    }

    [Header("正确图片")]
    public SpriteRenderer correctSprite;
    private float revealDuration = 1f;

    private Coroutine revealCoroutine;
    
}