using System.Collections;
using UnityEngine;
using TMPro;

public class VictoryAnimation : MonoBehaviour
{
    [Header("Objects")]
    public Transform leftObj;
    public Transform rightObj;
    [Header("Audio")]
    public AudioSource audioSource;
    [SerializeField] AudioClip medalFlipSound;
    [SerializeField] AudioClip medalLandedSound;
    [Header("Text")]
    public TextMeshPro text1;
    public TextMeshPro text2;
    public string wintext;

    [Header("Settings")]
    private float rotateTime = 0.4f;
    private bool isPlaying = false;

    [Header("Coin Animation")]
    public Transform coin;

    public Transform coin1Start;
    public Transform coin2Start;
    public Transform coinCenter;
    public Transform coin1Target;
    public Transform coin2Target;

    private float coinFlyInTime = 0.6f;
    private float coinFlipTime = 0.35f;
    private float coinPauseTime = 0.15f;
    private float coinFlyOutTime = 0.5f;

    private float coinStartScale = 0.8f;
    private float coinCenterScale = 3.5f;
    private float coinFinalScale = 1f;

    void Start()
    {
        text1.text = "";
        text2.text = "";
        text1.transform.localScale = Vector3.zero;
        text2.transform.localScale = Vector3.zero;

        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);
    }

    // =========================
    // 外部调用接口
    // =========================

    public void PlayVictory(bool isPlayer1)
    {
        if (!isPlaying)
            StartCoroutine(VictoryRoutine(isPlayer1));
    }

    // =========================
    // 主动画流程
    // =========================

    IEnumerator VictoryRoutine(bool isPlayer1)
    {
        isPlaying = true;
        if (audioSource != null && medalFlipSound != null)
        {
            audioSource.PlayOneShot(medalFlipSound);
        }
        float t = 0;
        bool textStarted = false;

        if (isPlayer1)
        {
            text1.gameObject.SetActive(true);
            text1.text = "";
            text1.transform.localScale = Vector3.zero;

            leftObj.localEulerAngles = new Vector3(0, 0, 90);

            while (t < rotateTime)
            {
                t += Time.deltaTime;
                float lerp = t / rotateTime;

                float rot = Mathf.Lerp(90, 0, lerp);
                leftObj.localEulerAngles = new Vector3(0, 0, rot);

                //旋转一半时开始文字动画
                if (!textStarted && lerp > 0.5f)
                {
                    textStarted = true;
                    text1.text = wintext;
                    StartCoroutine(SpringScale(text1.transform));
                }

                yield return null;
            }

            yield return StartCoroutine(PlayCoinAnimation(isPlayer1));
        }
        else
        {
            text2.gameObject.SetActive(true);
            text2.text = "";
            text2.transform.localScale = Vector3.zero;

            rightObj.localEulerAngles = new Vector3(0, 0, -90);

            while (t < rotateTime)
            {
                t += Time.deltaTime;
                float lerp = t / rotateTime;

                float rot = Mathf.Lerp(-90, 0, lerp);
                rightObj.localEulerAngles = new Vector3(0, 0, rot);

                if (!textStarted && lerp > 0.5f)
                {
                    textStarted = true;
                    text2.text = wintext;
                    StartCoroutine(SpringScale(text2.transform));
                }

                yield return null;
            }

            yield return StartCoroutine(PlayCoinAnimation(isPlayer1));
        }

        isPlaying = false;
    }

    // =========================
    // 文字弹簧动画
    // =========================
    IEnumerator SpringScale(Transform target)
    {
        float value = 0f;
        float velocity = 0f;
        float targetValue = 1f;

        float stiffness = 350f;
        float damping = 10f;

        float maxTime = 1.5f;
        float time = 0f;

        while (time < maxTime)
        {
            float dt = Time.deltaTime;
            time += dt;

            float force = stiffness * (targetValue - value) - damping * velocity;
            velocity += force * dt;
            value += velocity * dt;

            target.localScale = Vector3.one * value;

            yield return null;
        }

        target.localScale = Vector3.one;
    }

    // =========================
    // Coin 动画
    // =========================
    IEnumerator PlayCoinAnimation(bool isPlayer1)
    {
        Transform coinStart;
        Transform coinTarget;

        if (isPlayer1)
        {
            coinStart = coin1Start;
            coinTarget = coin1Target;
        }
        else
        {
            coinStart = coin2Start;
            coinTarget = coin2Target;
        }

        if (coinStart == null || coinTarget == null || coinCenter == null)
        {
            Debug.LogError("Coin animation points not assigned!");
            yield break;
        }

        coin.position = coinStart.position;
        coin.localScale = Vector3.one * coinStartScale;
        coin.rotation = Quaternion.identity;

        float t = 0;

        // ===== 飞入中心 =====
        while (t < coinFlyInTime)
        {
            t += Time.deltaTime;

            float lerp = t / coinFlyInTime;
            float ease = 1 - Mathf.Pow(1 - lerp, 3);

            Vector3 pos = Vector3.Lerp(coinStart.position, coinCenter.position, ease);

            pos.y += Mathf.Sin(ease * Mathf.PI) * 0.5f;

            coin.position = pos;

            coin.localScale = Vector3.Lerp(
                Vector3.one * coinStartScale,
                Vector3.one * coinCenterScale,
                ease
            );

            yield return null;
        }

        coin.position = coinCenter.position;
        coin.localScale = Vector3.one * coinCenterScale;

        // ===== 翻面 =====
        t = 0;
        while (t < coinFlipTime)
        {
            t += Time.deltaTime;

            float lerp = t / coinFlipTime;

            float rotY = Mathf.Lerp(0, 360, lerp);
            coin.rotation = Quaternion.Euler(0, rotY, 0);

            yield return null;
        }

        coin.rotation = Quaternion.identity;

        // ===== 停顿 =====
        yield return new WaitForSeconds(coinPauseTime);

        // ===== 飞向目标 =====
        t = 0;
        if (audioSource != null && medalLandedSound != null)
        {
            audioSource.PlayOneShot(medalLandedSound);
        }
        Vector3 startPos = coin.position;
        Vector3 startScale = coin.localScale;

        while (t < coinFlyOutTime)
        {
            t += Time.deltaTime;

            float lerp = t / coinFlyOutTime;
            float ease = 1 - Mathf.Pow(1 - lerp, 3);

            coin.position = Vector3.Lerp(startPos, coinTarget.position, ease);

            coin.localScale = Vector3.Lerp(
                startScale,
                Vector3.one * coinFinalScale,
                ease
            );
            var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
            if (camShake != null)
                StartCoroutine(camShake.Shake());
            yield return null;
        }
        coin.position = coinTarget.position;
        coin.localScale = Vector3.one * coinFinalScale;
    }
}