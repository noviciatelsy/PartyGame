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
    public Transform failTransformY;
    [Header("Dive Settings")]
    public float diveSpeed = 5f;
    public SpriteRenderer depthSprite;

    private bool isHolding = false;
    private bool isDead = false;
    public bool isfinished = false;
    public AudioSource diveAudioSource;
    public float introDuration = 2.5f;
    private bool introFinished = false;
    private float introTimer = 0f;

    private Coroutine failCoroutine;
    public DiveGameManager gameManager;

    [Header("Player Sprite")]
    public SpriteRenderer playerSpriteRenderer;

    public Sprite playerDefaultSprite;
    public Sprite playerDiveSprite;

    private bool diveSpriteSwitched = false;

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = 28f;
        transform.position = pos;
        introFinished = true;
        introTimer = 0f;

        if (playerSpriteRenderer != null && playerDefaultSprite != null)
        {
            playerSpriteRenderer.sprite = playerDefaultSprite;
        }
    }
    void Update()
    {
        if (isDead) return;
        if (!introFinished)
        {
            //PlayIntro();
            return;
        }

        if (isHolding)
        {
            if (!diveSpriteSwitched)
            {
                SwitchToDiveSprite();
            }
            transform.position += Vector3.down * diveSpeed * Time.deltaTime;

            if (transform.position.y < failTransformY.position.y)
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

    void SwitchToDiveSprite()
    {
        diveSpriteSwitched = true;

        if (playerSpriteRenderer != null && playerDiveSprite != null)
        {
            playerSpriteRenderer.sprite = playerDiveSprite;
        }
    }

    public void SetHolding(bool hold)
    {
        if (isDead) return;
        isHolding = hold;
        if (isHolding)
        {
            if (diveAudioSource != null && !diveAudioSource.isPlaying)
            {
                diveAudioSource.Play();
            }
        }
        else
        {
            if (diveAudioSource != null && diveAudioSource.isPlaying)
            {
                diveAudioSource.Stop();
            }
        }
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

        float alpha = Mathf.InverseLerp(28f, -5f, y);

        Color c = depthSprite.color;
        c.a = alpha;
        depthSprite.color = c;
    }

}
