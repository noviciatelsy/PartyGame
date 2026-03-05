using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDive : MonoBehaviour
{
    public enum PlayerType
    {
        Player1,
        Player2
    }

    public PlayerType playerType;

    [Header("Dive Settings")]
    public float diveSpeed = 5f;
    public SpriteRenderer depthSprite;

    private bool isHolding = false;
    private bool isDead = false;
    public bool isfinished = false;

    public float introDuration = 2.5f;
    private bool introFinished = false;
    private float introTimer = 0f;

    private Coroutine failCoroutine;
    public DiveGameManager gameManager;

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = -25f;
        transform.position = pos;
        introFinished = false;
        introTimer = 0f;

        PlayIntro();
    }

    void PlayIntro()
    {
        introTimer += Time.deltaTime;

        float y;

        // 前0.5秒
        if (introTimer <= 1.0f)
        {
            float t = introTimer / 0.5f;
            y = Mathf.Lerp(-25f, -20f, t);
        }
        // 后2秒
        else
        {
            float t = Mathf.Clamp01((introTimer - 1.0f) / 2f);
            y = Mathf.Lerp(-25f, 20f, t);

            if (t >= 1f)
            {
                introFinished = true;
            }
        }

        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    void Update()
    {
        if (isDead) return;
        if (!introFinished)
        {
            PlayIntro();
            return;
        }

        if (isHolding)
        {
            transform.position += Vector3.down * diveSpeed * Time.deltaTime;

            if (transform.position.y < -20f)
            {
                if (failCoroutine == null)
                {
                    failCoroutine = StartCoroutine(FailRoutine());
                }
            }
        }

        if (!isDead && !isfinished)
        {
            UpdateAlpha();
        }
    }

    public void SetHolding(bool hold)
    {
        if (isDead) return;
        isHolding = hold;
    }

    public float GetDepth()
    {
        return transform.position.y;
    }

    public bool IsDead()
    {
        Debug.Log("遇到鲨鱼了!");
        return isDead;
    }

    IEnumerator FailRoutine()
    {
        isDead = true;

        float duration = 0.1f;
        float timer = 0f;

        Color startColor = depthSprite.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            depthSprite.color = c;

            yield return null;
        }

        // 确保最终透明
        Color final = depthSprite.color;
        final.a = 0f;
        depthSprite.color = final;

        gameManager.PlayerFailed(this);
    }

    void UpdateAlpha()
    {
        if (depthSprite == null) return;

        float y = transform.position.y;

        float alpha;

        if (y >= 0f)
        {
            alpha = Mathf.Clamp01((20f - y) / 20f);
        }
        else
        {
            alpha = 1f;
        }

        Color c = depthSprite.color;
        c.a = alpha;
        depthSprite.color = c;
    }

}
