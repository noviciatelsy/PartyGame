using System.Collections;
using TMPro;
using UnityEngine;

public class BreathPromptBubble : MonoBehaviour
{
    public TextMeshPro textA;
    public TextMeshPro textB;

    float offsetIn = 0.8f;
    float offsetOut = 1.0f;

    float appearTime = 0.4f;
    float stayTime = 1.6f;
    float disappearTime = 0.4f;

    Vector3 basePosA;
    Vector3 basePosB;

    bool useA = true;

    void Awake()
    {
        basePosA = textA.transform.localPosition;
        basePosB = textB.transform.localPosition;

        SetAlpha(textA, 0);
        SetAlpha(textB, 0);
    }

    public void Play(string msg)
    {
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
        c.a = a;
        t.color = c;
    }
}