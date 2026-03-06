using System.Collections;
using UnityEngine;

public class BreathAlphaController : MonoBehaviour
{
    [Header("Target")]
    public SpriteRenderer targetImage;

    [Header("Settings")]
    private float inhaleRise = 0.4f;
    private float exhaleDrop = 0.2f;

    [Tooltip("一次呼吸的平滑时间（吸气 or 呼气）")]
    private float phaseAnimTime = 0.6f;

    public float endFadeTime = 0.1f;

    private int breathIndex = 0;

    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    private Coroutine animCoroutine;

    // ==================================================
    // 呼吸阶段
    // ==================================================
    public void OnBreathPhase(bool isInhale)
    {
        breathIndex++;

        if (breathIndex == 1)
            return; // 前1次不动

        float baseValue = (breathIndex - 2) * 0.1f;
        //baseValue = Mathf.Clamp(baseValue, 0f, 1f);

        float nextTarget = currentAlpha;

        if (isInhale)
        {
            nextTarget = Mathf.Min(currentAlpha + inhaleRise, 1.05f);
        }
        else
        {
            nextTarget = Mathf.Max(currentAlpha - exhaleDrop, 0f);
        }

        StartSmoothTransition(nextTarget);
    }

    // ==================================================
    // 平滑过渡
    // ==================================================
    void StartSmoothTransition(float newTarget)
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        targetAlpha = newTarget;
        animCoroutine = StartCoroutine(AnimateToTarget());
    }

    IEnumerator AnimateToTarget()
    {
        float start = currentAlpha;
        float t = 0f;

        while (t < phaseAnimTime)
        {
            t += Time.deltaTime;

            float p = t / phaseAnimTime;
            currentAlpha = Mathf.Lerp(start, targetAlpha, p);

            SetAlpha(currentAlpha);

            yield return null;
        }

        currentAlpha = targetAlpha;
        SetAlpha(currentAlpha);
    }

    // ==================================================
    // 结束
    // ==================================================
    public void OnGameEnd()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        StartCoroutine(FadeToZero());
    }

    IEnumerator FadeToZero()
    {
        float start = currentAlpha;
        float t = 0f;

        while (t < endFadeTime)
        {
            t += Time.deltaTime;

            float p = t / endFadeTime;
            SetAlpha(Mathf.Lerp(start, 0f, p));

            yield return null;
        }

        SetAlpha(0f);
    }

    void SetAlpha(float alpha)
    {
        if (targetImage == null) return;

        Color c = targetImage.color;
        c.a = Mathf.Clamp01(alpha);
        targetImage.color = c;
    }
}