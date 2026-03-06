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
        float duration = 0.2f;
        float timer = 0f;

        Quaternion startRot = stick.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, -90);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            stick.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        stick.rotation = endRot;

        CheckPlatform();
    }

    void CheckPlatform()
    {
        float targetX = transform.position.x + stickLength;

        Platform p = StickHeroGameManager.Instance.GetNextPlatformForX(transform.position.x);
        //Debug.Log("Checking platform" + p.name + "L:" + stickLength);

        if (p == null)
        {
            ResetStick();
            return;
        }

        if (p.IsPointOnPlatform(targetX))
        {
            moveTarget = p.GetEdgeX();
            moving = true;

            if (StickHeroGameManager.Instance.IsLastPlatform(p))
            {
                StickHeroGameManager.Instance.PlayerWin(this);
            }
        }
        else
        {
            ResetStick();
        }
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(moveTarget, transform.position.y, 0),
            moveSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - moveTarget) < 0.05f)
        {
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