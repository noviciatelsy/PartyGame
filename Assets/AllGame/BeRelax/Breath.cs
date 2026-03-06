using System.Collections;
using TMPro;
using UnityEngine;

public class Breath : MonoBehaviour
{
    public BreathPromptBubble bubble;

    [Header("Game")]
    public int cycleCount = 8;
    public float phaseTime = 2f;

    //UI时间
    private bool first = true;

    private float totalGameTime;
    private bool inhalePhase = true;

    private float p1CorrectTime = 0f;
    private float p2CorrectTime = 0f;

    public TextMeshPro timerText;
    private float timeRemaining;
    private bool timerRunning = false;


    public TextMeshPro p1InhaleText;
    public TextMeshPro p1ExhaleText;
    public TextMeshPro p2InhaleText;
    public TextMeshPro p2ExhaleText;
    Vector3 p1InhaleBase;
    Vector3 p1ExhaleBase;
    Vector3 p2InhaleBase;
    Vector3 p2ExhaleBase;

    private float breathOffsetIn = 0.4f;
    private float breathOffsetOut = 0.3f;
    private float breathAppear = 0.2f;
    private float breathStay = 0.2f;
    private float breathDisappear = 0.2f;

    [Header("Match UI")]
    public TextMeshPro p1MatchText;
    public TextMeshPro p2MatchText;
    Coroutine p1Coroutine;
    Coroutine p2Coroutine;

    [Header("Alpha Control")]
    public BreathAlphaController alphaController;
    [Header("Font Fade")]
    public TextMeshPro[] fontList;
    private Coroutine fontFadeCoroutine;

    void Start()
    {
        GlobalInput.Instance.OnSpaceDown += OnPlayer1Down;
        GlobalInput.Instance.OnSpaceUp += OnPlayer1Up;

        GlobalInput.Instance.OnMouseDown += OnPlayer2Down;
        GlobalInput.Instance.OnMouseUp += OnPlayer2Up;

        totalGameTime = cycleCount * 2f * phaseTime;
        first = true;
        timeRemaining = 0;
        StartCoroutine(StartDelay());

        p1InhaleBase = p1InhaleText.transform.localPosition;
        p1ExhaleBase = p1ExhaleText.transform.localPosition;
        p2InhaleBase = p2InhaleText.transform.localPosition;
        p2ExhaleBase = p2ExhaleText.transform.localPosition;

        p1InhaleText.gameObject.SetActive(false);
        p1ExhaleText.gameObject.SetActive(false);
        p2InhaleText.gameObject.SetActive(false);
        p2ExhaleText.gameObject.SetActive(false);

        if (alphaController == null)
        {
            Debug.LogWarning("AlphaController 未赋值！");
        }
    }

    IEnumerator StartDelay()
    {
        timerRunning = true;
        yield return new WaitForSeconds(1.8f);

        StartCoroutine(GameLoop());
    }

    void Update()
    {
        if (!timerRunning) return;

        float dt = Time.deltaTime;

        timeRemaining += dt;

        UpdateTimerUI();
    }

    void OnDestroy()
    {
        if (GlobalInput.Instance == null) return;

        GlobalInput.Instance.OnSpaceDown -= OnPlayer1Down;
        GlobalInput.Instance.OnSpaceUp -= OnPlayer1Up;

        GlobalInput.Instance.OnMouseDown -= OnPlayer2Down;   // 修正
        GlobalInput.Instance.OnMouseUp -= OnPlayer2Up;
    }

    IEnumerator GameLoop()
    {
        StartFontFadeTo(0f, 12f); // 1秒内从1→0
        for (int i = 0; i < cycleCount; i++)
        {
            // ===== 吸气 =====
            inhalePhase = true;

            bubble.Play("吸气");
            alphaController?.OnBreathPhase(true);
            yield return PhaseTimer();

            // ===== 呼气 =====
            inhalePhase = false;

            bubble.Play("呼气");
            alphaController?.OnBreathPhase(false);

            yield return PhaseTimer();
        }

        EndGame();
    }

    IEnumerator PhaseTimer()
    {
        float t = 0f;

        while (t < phaseTime)
        {
            t += Time.deltaTime;

            float dt = Time.deltaTime;

            //timeRemaining += dt;
            //UpdateTimerUI();

            CheckPlayerAccuracy();

            yield return null;
        }
    }

    void CheckPlayerAccuracy()
    {
        bool p1Holding = GlobalInput.Instance.SpaceHolding;
        bool p2Holding = GlobalInput.Instance.MouseHolding;

        if (inhalePhase)
        {
            if (p1Holding) p1CorrectTime += Time.deltaTime;
            if (p2Holding) p2CorrectTime += Time.deltaTime;
        }
        else
        {
            if (!p1Holding) p1CorrectTime += Time.deltaTime;
            if (!p2Holding) p2CorrectTime += Time.deltaTime;
        }

        //UpdatePlayerTimeUI();
    }

    void EndGame()
    {
        timerRunning = false;
        alphaController?.OnGameEnd();

        float p1Score = p1CorrectTime / totalGameTime;
        float p2Score = p2CorrectTime / totalGameTime;

        // ===== 计算匹配度 =====
        UpdateMatchUI();
        StartFontFadeTo(1f, 0.1f);

        if (p1Score > p2Score)
        {
            GlobalScoreManager.Instance.AddScore(1);
        }
        else if (p2Score > p1Score)
        {
            GlobalScoreManager.Instance.AddScore(2);
        }

        StartCoroutine(NextLevelDelay());
    }

    IEnumerator NextLevelDelay()
    {
        yield return new WaitForSeconds(2f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        float t = Mathf.Max(0f, timeRemaining);

        timerText.text = t.ToString("F1") + "s";
    }

    //void UpdatePlayerTimeUI()
    //{
    //    if (p1TimeText != null)
    //    {
    //        float t1 = Mathf.Round(p1CorrectTime * 10f) / 10f;
    //        p1TimeText.text = t1.ToString("F1") + "s";
    //    }

    //    if (p2TimeText != null)
    //    {
    //        float t2 = Mathf.Round(p2CorrectTime * 10f) / 10f;
    //        p2TimeText.text = t2.ToString("F1") + "s";
    //    }
    //}
    IEnumerator ShowBreath(TextMeshPro text, Vector3 basePos)
    {
        text.gameObject.SetActive(true);

        text.transform.localPosition = basePos;

        yield return BreathAppear(text, basePos);

        yield return new WaitForSeconds(breathStay);

        yield return BreathDisappear(text, basePos);

        text.transform.localPosition = basePos;

        text.gameObject.SetActive(false);
    }

    IEnumerator BreathAppear(TextMeshPro text, Vector3 basePos)
    {
        float t = 0;

        Vector3 start = basePos + Vector3.up * breathOffsetIn;
        Vector3 end = basePos;

        text.transform.localPosition = start;
        SetAlpha(text, 0);

        while (t < breathAppear)
        {
            t += Time.deltaTime;

            float p = t / breathAppear;
            p = 1 - Mathf.Pow(1 - p, 2); // ease out

            text.transform.localPosition = Vector3.Lerp(start, end, p);
            SetAlpha(text, Mathf.Lerp(0, 1, p));

            yield return null;
        }

        text.transform.localPosition = end;
        SetAlpha(text, 1);
    }

    IEnumerator BreathDisappear(TextMeshPro text, Vector3 basePos)
    {
        float t = 0;

        Vector3 start = basePos;
        Vector3 end = basePos + Vector3.down * breathOffsetOut;

        while (t < breathDisappear)
        {
            t += Time.deltaTime;

            float p = t / breathDisappear;

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

    void OnPlayer1Down()
    {
        if (p1Coroutine != null)
            StopCoroutine(p1Coroutine);

        p1ExhaleText.gameObject.SetActive(false);
        p1Coroutine = StartCoroutine(ShowBreath(p1InhaleText, p1InhaleBase));
    }

    void OnPlayer1Up()
    {
        if (p1Coroutine != null)
            StopCoroutine(p1Coroutine);

        p1InhaleText.gameObject.SetActive(false);
        p1Coroutine = StartCoroutine(ShowBreath(p1ExhaleText, p1ExhaleBase));
    }

    void OnPlayer2Down()
    {
        if (p2Coroutine != null)
            StopCoroutine(p2Coroutine);

        p2ExhaleText.gameObject.SetActive(false);
        p2Coroutine = StartCoroutine(ShowBreath(p2InhaleText, p2InhaleBase));
    }

    void OnPlayer2Up()
    {
        if (p2Coroutine != null)
            StopCoroutine(p2Coroutine);

        p2InhaleText.gameObject.SetActive(false);
        p2Coroutine = StartCoroutine(ShowBreath(p2ExhaleText, p2ExhaleBase));
    }

    void UpdateMatchUI()
    {
        float totalTime = 30.0f;

        if (p1MatchText != null)
        {
            float rate1 = p1CorrectTime / totalTime;
            float percent1 = rate1 * 100f;

            p1MatchText.text = "匹配度:" + percent1.ToString("F1") + "%";
        }

        if (p2MatchText != null)
        {
            float rate2 = p2CorrectTime / totalTime;
            float percent2 = rate2 * 100f;

            p2MatchText.text = "匹配度:" + percent2.ToString("F1") + "%";
        }
    }

    void StartFontFadeTo(float targetAlpha, float duration)
    {
        if (fontFadeCoroutine != null)
            StopCoroutine(fontFadeCoroutine);

        fontFadeCoroutine = StartCoroutine(FontFadeRoutine(targetAlpha, duration));
    }

    IEnumerator FontFadeRoutine(float targetAlpha, float duration)
    {
        float time = 0f;

        // 先获取当前alpha
        float[] startAlphas = new float[fontList.Length];

        for (int i = 0; i < fontList.Length; i++)
        {
            if (fontList[i] != null)
                startAlphas[i] = fontList[i].color.a;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            for (int i = 0; i < fontList.Length; i++)
            {
                if (fontList[i] == null) continue;

                Color c = fontList[i].color;
                c.a = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                fontList[i].color = c;
            }

            yield return null;
        }

        // 强制修正最终值
        for (int i = 0; i < fontList.Length; i++)
        {
            if (fontList[i] == null) continue;

            Color c = fontList[i].color;
            c.a = targetAlpha;
            fontList[i].color = c;
        }
    }

}