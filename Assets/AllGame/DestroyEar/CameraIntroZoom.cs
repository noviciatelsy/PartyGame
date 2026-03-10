using UnityEngine;

public class CameraIntroZoom : MonoBehaviour
{
    public float introDelay = 1f;     // 前1秒不动
    public float introDuration = 1f;  // 后1秒播放动画

    public Vector3 startPos = new Vector3(0, 0, -10);
    public Vector3 endPos = new Vector3(0, 20, -10);

    public float startSize = 30f;
    public float endSize = 6f;

    [Header("Follow Target")]
    public Transform player;
    public float triggerOffset = 1f;

    private float timer = 0f;
    private bool introFinished = false;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        transform.position = startPos;
        cam.orthographicSize = startSize;
    }

    void Update()
    {
        if (!introFinished)
        {
            PlayIntro();
        }
        else
        {
            FollowPlayer();
        }
    }

    void PlayIntro()
    {
        timer += Time.deltaTime;

        // 第一阶段：停留
        if (timer <= introDelay)
        {
            return;
        }

        // 第二阶段：播放动画
        float t = (timer - introDelay) / introDuration;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(startPos, endPos, t);
        cam.orthographicSize = Mathf.Lerp(startSize, endSize, t);

        if (t >= 1f)
        {
            introFinished = true;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

        float cameraY = transform.position.y;

        float bottom = cameraY - cam.orthographicSize;

        if (player.position.y <= bottom + triggerOffset)
        {
            float newY = player.position.y + cam.orthographicSize - triggerOffset;

            transform.position = new Vector3(
                transform.position.x,
                newY,
                transform.position.z
            );
        }
    }
}