using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FaceController : MonoBehaviour
{
    public enum PlayerType
    {
        Player1,
        Player2
    }

    [Header("归属玩家")]
    public PlayerType player;
    [SerializeField] private AudioSource audioSource;

    [Header("五官列表（仅表示数量）")]
    public List<FaceFeature> features = new List<FaceFeature>();

    [Header("匹配参数")]
    private float maxDis = 8f;
    private float effectiveMaxDis = 7f;
    public TMP_Text scoreText;

    private int currentIndex = 0;
    private float totalClampedDistance = 0f;
    private int totalFeatureCount;

    public faceManager facemanager;
    private void Start()
    {
        totalFeatureCount = features.Count;

        if (scoreText != null)
            scoreText.gameObject.SetActive(false);
        if (correctSprite != null)
        {
            Color c = correctSprite.color;
            c.a = 0f;
            correctSprite.color = c;
        }

        // 初始化五官
        foreach (var f in features)
        {
            f.Init(this);
            f.gameObject.SetActive(false);
        }

        // 打乱顺序（随机下落）
        ShuffleFeatures();

        // 订阅输入
        SubscribeInput();

        StartNextFeature();
    }

    void SubscribeInput()
    {
        if (player == PlayerType.Player1)
            GlobalInput.Instance.OnMouseLeftAction += OnInput;
        else
            GlobalInput.Instance.OnSpaceAction += OnInput;
    }

    void UnsubscribeInput()
    {
        if (GlobalInput.Instance == null) return;

        if (player == PlayerType.Player1)
            GlobalInput.Instance.OnMouseLeftAction -= OnInput;
        else
            GlobalInput.Instance.OnSpaceAction -= OnInput;
    }

    private void OnDestroy()
    {
        UnsubscribeInput();
    }

    void OnInput(GlobalInput.InputType type)
    {
        if (type != GlobalInput.InputType.SingleClick)
            return;

        if (currentIndex >= features.Count)
            return;
        audioSource.Play();
        features[currentIndex].TryStop();
    }

    void ShuffleFeatures()
    {
        for (int i = 0; i < features.Count; i++)
        {
            int rand = Random.Range(i, features.Count);
            var temp = features[i];
            features[i] = features[rand];
            features[rand] = temp;
        }
    }

    void StartNextFeature()
    {
        if (currentIndex >= features.Count)
        {
            CalculateScore();
            return;
        }

        var feature = features[currentIndex];
        feature.gameObject.SetActive(true);
        feature.StartFalling();
    }

    public void OnFeatureStopped(float distance)
    {
        float clamped = Mathf.Min(maxDis, Mathf.Abs(distance));
        if (clamped < 0.1f)
            totalClampedDistance += 0f;   // 完美不扣分
        else
            totalClampedDistance += Mathf.Pow(clamped, 1.5f);

        //totalClampedDistance += clamped;

        currentIndex++;
        StartNextFeature();
    }

    void CalculateScore()
    {
        float maxPenaltyPerFeature = Mathf.Pow(effectiveMaxDis, 1.5f);
        float maxTotalPenalty = totalFeatureCount * maxPenaltyPerFeature;

        float normalized = totalClampedDistance / maxTotalPenalty;
        float score = (1f - normalized) * 100f;
        score = Mathf.Clamp(score, 0f, 100f);

        Debug.Log(player + " 匹配度: " + score.ToString("F2") + "%");

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = "匹配度:" + score.ToString("F2") + "%";
        }

        facemanager.RegisterScore(player, score);
    }

    [Header("正确图片")]
    public SpriteRenderer correctSprite;

    private float revealDuration = 1.0f;
    public void ShowCorrectImage()
    {
        if (correctSprite != null)
            StartCoroutine(RevealRoutine());
    }

    IEnumerator RevealRoutine()
    {
        float timer = 0f;

        Color c = correctSprite.color;
        c.a = 0f;
        correctSprite.color = c;

        while (timer < revealDuration)
        {
            timer += Time.deltaTime;

            float t = timer / revealDuration;

            c.a = Mathf.Lerp(0f, 1f, t);
            correctSprite.color = c;

            yield return null;
        }

        c.a = 1f;
        correctSprite.color = c;
    }
}