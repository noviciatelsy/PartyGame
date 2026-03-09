using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StickHero : MonoBehaviour
{
    public Transform stick;

    private float growSpeed = 8.0f;
    private float moveSpeed = 40.0f;

    private bool growing = false;

    private float stickLength = 0;

    private bool moving = false;
    private float moveTarget;
    private float moveTargetY;
    private float currentPlatformY;

    public void InitPlatformY(float y)
    {
        currentPlatformY = y;
    }

    void Update()
    {
        if (growing)
            GrowStick();

        if (moving)
            Move();
    }

    void GrowStick()
    {
        stickLength += growSpeed * Time.deltaTime;

        stick.localScale = new Vector3(0.3f, stickLength, 1);
    }

    public void StartGrow()
    {
        if (StickHeroGameManager.Instance.gameFinished) return;
        growing = true;
    }

    public void DropStick()
    {
        if (!growing) return;

        growing = false;

        StartCoroutine(FallStickRoutine());
    }

    IEnumerator FallStickRoutine()
    {
        Platform nextP = StickHeroGameManager.Instance.GetNextPlatformForX(transform.position.x);

        float deltaY = 0f;
        float fallAngleDeg = 90f; // 默认水平

        if (nextP != null)
        {
            deltaY = nextP.transform.position.y - currentPlatformY;
            if (stickLength > Mathf.Abs(deltaY))
            {
                float cosAngle = Mathf.Clamp(deltaY / stickLength, -1f, 1f);
                fallAngleDeg = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;
            }
        }

        float duration = 0.2f;
        float timer = 0f;

        Quaternion startRot = stick.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, -fallAngleDeg);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            stick.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        stick.rotation = endRot;

        CheckPlatform(nextP, deltaY);
    }

    void CheckPlatform(Platform nextP, float deltaY)
    {
        if (nextP == null)
        {
            ResetStick();
            return;
        }

        // 棍子长度不足以桥接高度差
        if (stickLength <= Mathf.Abs(deltaY))
        {
            ResetStick();
            return;
        }

        // 三角计算：水平到达距离 = sqrt(棍子长度² - 高度差²)
        float horizontalReach = Mathf.Sqrt(stickLength * stickLength - deltaY * deltaY);
        float targetX = transform.position.x + horizontalReach;

        if (!nextP.IsPointOnPlatform(targetX))
        {
            ResetStick();
            return;
        }

        // 成功到达：更新目标位置（含Y轴）
        float playerYOffset = transform.position.y - currentPlatformY;
        currentPlatformY = nextP.transform.position.y;
        moveTarget = nextP.GetEdgeX();
        moveTargetY = currentPlatformY + playerYOffset;
        moving = true;

        if (StickHeroGameManager.Instance.IsLastPlatform(nextP))
        {
            StickHeroGameManager.Instance.PlayerWin(this);
        }
    }

    void Move()
    {
        Vector3 target = new Vector3(moveTarget, moveTargetY, transform.position.z);
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            moving = false;

            ResetStick();
        }
    }

    void ResetStick()
    {
        stickLength = 0;

        stick.localScale = new Vector3(0.2f, 0, 1);

        stick.rotation = Quaternion.identity;
    }

    public float GetX()
    {
        return transform.position.x;
    }
}