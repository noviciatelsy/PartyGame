using System.Collections;
using TMPro;
using UnityEngine;

public class BreathPromptBubble : MonoBehaviour
{
    public TextMeshPro textA;
    public TextMeshPro textB;
    public float fadeDuration = 20f;

    float offsetIn = 0.8f;
    float offsetOut = 1.0f;

    float appearTime = 0.4f;
    float stayTime = 1.6f;
    float disappearTime = 0.4f;

    Vector3 basePosA;
    Vector3 basePosB;

    bool useA = true;
    bool fadeStarted = false;

    float fadeAlpha = 1f;
    
    void Awake()
    {
        if (textA == null || textB == null)
        {
            Debug.LogError("BreathPromptBubble: textA 或 textB 没有赋值", this);
            return;
        }

        basePosA = new Vector3 (0, 0, 0);
        basePosB = new Vector3(0, 0, 0);

        SetAlpha(textA, 0);
        SetAlpha(textB, 0);
    }

    public void Play(string msg)
    {
        if (!fadeStarted)
        {
            fadeStarted = true;
            StartCoroutine(FadeRoutine());
        }


        TextMeshPro text = useA ? textA : textB;
        Vector3 basePos = useA ? basePosA : basePosB;

        text.text = msg;

        StopCoroutine("PlayRoutine");
        StartCoroutine(PlayRoutine(text, basePos));

        useA = !useA;
    }

    IEnumerator PlayRoutine(TextMeshPro text, Vector3 basePos)
    {
        yield return Appear(text, basePos);

        yield return new WaitForSeconds(stayTime);

        yield return Disappear(text, basePos);
    }

    IEnumerator Appear(TextMeshPro text, Vector3 basePos)
    {
        float t = 0;

        Vector3 start = basePos + Vector3.up * offsetIn;
        Vector3 end = basePos;

        text.transform.localPosition = start;
        SetAlpha(text, 0);

        while (t < appearTime)
        {
            t += Time.deltaTime;

            float p = t / appearTime;
            p = 1 - Mathf.Pow(1 - p, 2);

            text.transform.localPosition = Vector3.Lerp(start, end, p);

            SetAlpha(text, Mathf.Lerp(0, 1, p));

            yield return null;
        }

        text.transform.localPosition = end;
        SetAlpha(text, 1);
    }

    IEnumerator Disappear(TextMeshPro text, Vector3 basePos)
    {
        float t = 0;

        Vector3 start = basePos;
        Vector3 end = basePos + Vector3.down * offsetOut;

        while (t < disappearTime)
        {
            t += Time.deltaTime;

            float p = t / disappearTime;

            text.transform.localPosition = Vector3.Lerp(start, end, p);

            SetAlpha(text, Mathf.Lerp(1, 0, p));

            yield return null;
        }

        text.transform.localPosition = end;
        SetAlpha(text, 0);
    }

    void SetAlpha(TextMeshPro t, float a)
    {
        Color c = t.color;
        c.a = a * fadeAlpha;
        t.color = c;
    }

    IEnumerator FadeRoutine()
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            fadeAlpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            yield return null;
        }

        fadeAlpha = 0;
    }
}