using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowHeight : MonoBehaviour
{
    public float followSpeed = 5f;

    [Header("Top Trigger Offset")]
    private float topOffset = 5f; // 玩家超过这个高度才移动Camera

    private Transform target;
    PlayerController[] players;
    bool gameEnded = false;

    private void Start()
    {
        players = FindObjectsOfType<PlayerController>();
    }

    void LateUpdate()
    {
        FindHighestPlayer();

        if (target == null) return;

        float triggerY = transform.position.y + topOffset;

        // 只有超过触发线才移动
        if (target.position.y > triggerY)
        {
            float targetY = target.position.y - topOffset;

            float newY = Mathf.Lerp(
                transform.position.y,
                targetY,
                followSpeed * Time.deltaTime
            );

            // 保证Camera只往上移动
            if (newY > transform.position.y)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    newY,
                    transform.position.z
                );
            }
        }

        CheckPlayerOutOfScreen();
    }

    void FindHighestPlayer()
    {
        if (players.Length == 0) return;

        Transform highest = players[0].transform;

        foreach (PlayerController p in players)
        {
            if (p.transform.position.y > highest.position.y)
            {
                highest = p.transform;
            }
        }

        target = highest;
    }

    // Scene里显示触发线（调试用）
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        float y = transform.position.y + topOffset;

        Gizmos.DrawLine(
            new Vector3(-100, y, 0),
            new Vector3(100, y, 0)
        );
    }

    void CheckPlayerOutOfScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        foreach (PlayerController p in players)
        {
            if (p == null) continue;

            Vector3 viewPos = cam.WorldToViewportPoint(p.transform.position);

            bool outOfScreen =
                viewPos.y < -0.1f ||   // 掉到屏幕下
                viewPos.y > 1.1f ||    // 飞出屏幕上
                viewPos.x < -0.1f ||
                viewPos.x > 1.1f;

            if (outOfScreen && !gameEnded)
            {
                gameEnded = true;
                DeclareOtherPlayerWin(p);
                return;
            }
        }
    }

    void DeclareOtherPlayerWin(PlayerController loser)
    {
        foreach (PlayerController p in players)
        {
            if (p == null) continue;

            if (p != loser)
            {
                p.Towin();
                break;
            }
        }
    }
}