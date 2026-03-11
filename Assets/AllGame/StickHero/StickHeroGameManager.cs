using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickHeroGameManager : MonoBehaviour
{
    public static StickHeroGameManager Instance;

    public StickHero player1;
    public StickHero player2;

    public Platform platformPrefab; 
    public Platform nextPlatform;

    public Camera cam;
    public float cameraSpeed = 5f;
    public float cameraOffset = 5f;

    private List<Platform> platforms = new List<Platform>();
    private int currentPlatformIndex = 0;
    public bool gameFinished = false;

    private Coroutine winCoroutine;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GeneratePlatforms();

        float firstY = platforms[0].transform.position.y;
        player1.InitPlatformY(firstY);
        player2.InitPlatformY(firstY);

        GlobalInput.Instance.OnSpaceDown += P1Grow;
        GlobalInput.Instance.OnSpaceUp += P1Drop;

        GlobalInput.Instance.OnMouseDown += P2Grow;
        GlobalInput.Instance.OnMouseUp += P2Drop;
    }

    void LateUpdate()
    {
        FollowCamera();
        CheckPlayerScreen();
    }

    void CheckPlayerScreen()
    {
        if (gameFinished) return;

        bool p1Out = IsPlayerOutOfScreen(player1);
        bool p2Out = IsPlayerOutOfScreen(player2);

        if (p1Out && !p2Out)
        {
            PlayerWin(player2);
        }
        else if (p2Out && !p1Out)
        {
            PlayerWin(player1);
        }
    }

    void P1Grow()
    {
        player1.StartGrow();
    }

    void P1Drop()
    {
        player1.DropStick();
    }

    void P2Grow()
    {
        player2.StartGrow();
    }

    void P2Drop()
    {
        player2.DropStick();
    }


    void GeneratePlatforms()
    {
        float lastRightEdge = -12;
        float lastY = -7f;

        for (int i = 0; i < 20; i++)
        {
            float difficulty = i / 19f;

            float widthMin = Mathf.Lerp(4f, 0.5f, difficulty);
            float widthMax = Mathf.Lerp(7f, 2f, difficulty);

            float width = Random.Range(widthMin, widthMax);

            float gap = Random.Range(0.5f, 5.0f);

            float x = lastRightEdge + gap + width * 0.5f;

            // 平台高度：第一个保持不变，后续随难度增加高度差范围
            float y;
            if (i == 0)
            {
                y = -7f;
            }
            else
            {
                float heightRange = Mathf.Lerp(1f, 3f, difficulty);
                y = lastY + Random.Range(-heightRange, heightRange);
                y = Mathf.Clamp(y, -10f, -4f);
            }

            Platform p = Instantiate(
                platformPrefab,
                new Vector3(x, y, 5),
                Quaternion.identity
            );

            p.width = width;
            p.SetWidth(width);

            platforms.Add(p);

            lastRightEdge = p.RightEdge;
            lastY = y;
        }

        currentPlatformIndex = 0;
    }

    // ======================================
    // ��ȡ��һ��ƽ̨
    // ======================================

    public Platform GetNextPlatform()
    {
        if (currentPlatformIndex + 1 >= platforms.Count)
            return null;

        return platforms[currentPlatformIndex + 1];
    }

    public void ReachNextPlatform()
    {
        currentPlatformIndex++;
    }

    // ======================================
    // �����
    // ======================================

    void FollowCamera()
    {
        float rightX = Mathf.Max(player1.GetX(), player2.GetX());

        Vector3 target = new Vector3(
            rightX + cameraOffset,
            cam.transform.position.y,
            cam.transform.position.z
        );

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            target,
            cameraSpeed * Time.deltaTime
        );
    }

    public Platform GetNextPlatformForX(float x)
    {
        Platform nearest = null;
        float minDist = float.MaxValue;

        foreach (Platform p in platforms)
        {
            if (p.LeftEdge > x)
            {
                float dist = p.LeftEdge - x;

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = p;
                }
            }
        }

        return nearest;
    }

    public bool IsLastPlatform(Platform p)
    {
        if (platforms.Count == 0)
            return false;

        return p == platforms[platforms.Count - 1];
    }

    public void PlayerWin(StickHero player)
    {
        if (gameFinished) return;

        gameFinished = true;

        if (player == player1)
        {
            GlobalScoreManager.Instance.AddScore(1, 1);
            Debug.Log("Player 1 WIN!");
        }
        else
        {
            GlobalScoreManager.Instance.AddScore(2, 1);
            Debug.Log("Player 2 WIN!");
        }

        // ������Լ�UI
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }


    IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

    void OnDestroy()
    {
        if (GlobalInput.Instance != null)
        {
            GlobalInput.Instance.OnSpaceDown -= P1Grow;
            GlobalInput.Instance.OnSpaceUp -= P1Drop;
            GlobalInput.Instance.OnMouseDown -= P2Grow;
            GlobalInput.Instance.OnMouseUp -= P2Drop;
        }
    }

    bool IsPlayerOutOfScreen(StickHero player)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(player.transform.position);

        if (viewportPos.z < 0) return true;

        if (viewportPos.x < 0 || viewportPos.x > 1 ||
            viewportPos.y < 0 || viewportPos.y > 1)
        {
            return true;
        }

        return false;
    }
}
