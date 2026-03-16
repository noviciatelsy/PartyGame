using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Trophy Settings")]
    public GameObject trophyPrefabPlayer1;
    public GameObject trophyPrefabPlayer2;
    public Transform[] player1TrophySlots = new Transform[5];
    public Transform[] player2TrophySlots = new Transform[5];
    public float trophyFlyTime = 0.8f;
    [Tooltip("飞行过程中相对于预制体尺寸的缩放倍率（飞行时会在 1.0 到该值之间插值），着陆时恢复为预制体原始尺寸")]
    public float trophyFlightScaleMultiplier = 1.2f;

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
                t += Time.unscaledDeltaTime;
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
            float dt = Time.unscaledDeltaTime;
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
            t += Time.unscaledDeltaTime;

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
            t += Time.unscaledDeltaTime;

            float lerp = t / coinFlipTime;

            float rotY = Mathf.Lerp(0, 360, lerp);
            coin.rotation = Quaternion.Euler(0, rotY, 0);

            yield return null;
        }

        coin.rotation = Quaternion.identity;

        // ===== 停顿（使用实时等待，不受 timeScale 影响） =====
        yield return new WaitForSecondsRealtime(coinPauseTime);

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
            t += Time.unscaledDeltaTime;

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
                StartCoroutine(camShake.ShakeWithFollow());
            yield return null;
        }
        coin.position = coinTarget.position;
        coin.localScale = Vector3.one * coinFinalScale;
        StartCoroutine(SpawnAndFlyTrophy(isPlayer1));
    }

    IEnumerator SpawnAndFlyTrophy(bool isPlayer1)
    {
        if (trophyPrefabPlayer1 == null && trophyPrefabPlayer2 == null) yield break;

        int playerID = isPlayer1 ? 1 : 2;
        int score = 0;
        if (GlobalScoreManager.Instance != null)
        {
            score = GlobalScoreManager.Instance.GetScore(playerID);
        }

        int slotIndex = Mathf.Clamp(score - 1, 0, 4); // 0-based slot

        Transform targetSlot = null;
        if (isPlayer1)
        {
            if (player1TrophySlots != null && player1TrophySlots.Length > slotIndex)
                targetSlot = player1TrophySlots[slotIndex];
        }
        else
        {
            if (player2TrophySlots != null && player2TrophySlots.Length > slotIndex)
                targetSlot = player2TrophySlots[slotIndex];
        }

        if (targetSlot == null) yield break;

        // 选择对应玩家的奖杯预制（优先使用对应玩家的预制，否则回退到通用 trophyPrefab）
        GameObject prefabToUse = null;
        if (isPlayer1)
            prefabToUse = trophyPrefabPlayer1;
        else
            prefabToUse = trophyPrefabPlayer2;

        if (prefabToUse == null) yield break;

        // 在当前 coin 位置生成奖杯（world space）
        GameObject trophy = Instantiate(prefabToUse, coin.position, Quaternion.identity);
        Vector3 startPos = trophy.transform.position;
        Vector3 endPos = targetSlot.position;
        Vector3 prefabScale = trophy.transform.localScale; // 保持预制体原始缩放

        float t = 0f;
        while (t < trophyFlyTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / trophyFlyTime;
            float ease = 1 - Mathf.Pow(1 - lerp, 3);
            trophy.transform.position = Vector3.Lerp(startPos, endPos, ease);
            // 飞行过程中插值缩放（从预制体尺寸到预制体尺寸 * multiplier）
            Vector3 targetScale = prefabScale * trophyFlightScaleMultiplier;
            trophy.transform.localScale = Vector3.Lerp(prefabScale, targetScale, ease);
            yield return null;
        }

        // 对齐到槽位位置并恢复预制体原始缩放
        trophy.transform.position = endPos;
        trophy.transform.localScale = prefabScale;

        // 删除槽位中表示占位的空奖杯（若存在）——根据常见组件或名称判断
        for (int i = targetSlot.childCount - 1; i >= 0; i--)
        {
            Transform child = targetSlot.GetChild(i);
            if (child == null) continue;
            // 如果是我们刚生成的奖杯，跳过
            if (child == trophy.transform) continue;

            string lname = child.name.ToLower();
            bool looksLikePlaceholder = lname.Contains("奖杯");
            if (looksLikePlaceholder || child.GetComponent<Image>() != null || child.GetComponent<SpriteRenderer>() != null || child.GetComponent<MeshRenderer>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // 把奖杯父级设为槽位，并确保世界空间尺寸与预制体一致（考虑槽位父级的缩放）
        trophy.transform.SetParent(targetSlot, true);
        trophy.transform.localPosition = Vector3.zero;
        trophy.transform.localRotation = Quaternion.identity;
        // 计算合适的本地缩放，使得 trophy 在世界空间的尺寸等于 prefabScale（实例化时的世界尺寸）
        Vector3 parentLossy = targetSlot.lossyScale;
        Vector3 finalLocalScale = new Vector3(
            parentLossy.x != 0f ? prefabScale.x / parentLossy.x : prefabScale.x,
            parentLossy.y != 0f ? prefabScale.y / parentLossy.y : prefabScale.y,
            parentLossy.z != 0f ? prefabScale.z / parentLossy.z : prefabScale.z
        );
        trophy.transform.localScale = finalLocalScale;
    }

    // 在场景加载时，根据当前分数即时在槽位中生成对应数量的奖杯（无动画）
    public void PopulateTrophiesFromScores(int p1Score, int p2Score)
    {
        // 清空并生成player1槽位
        if (player1TrophySlots != null)
        {
            for (int i = 0; i < player1TrophySlots.Length; i++)
            {
                Transform slot = player1TrophySlots[i];
                if (slot == null) continue;
                // 清空槽位中的占位或旧奖杯
                for (int c = slot.childCount - 1; c >= 0; c--)
                {
                    DestroyImmediate(slot.GetChild(c).gameObject);
                }

                if (i < p1Score)
                {
                    GameObject prefab = trophyPrefabPlayer1;
                    if (prefab == null) continue;
                    // Instantiate without parent, then set parent and correct localScale so world size matches prefab
                    GameObject trophy = Instantiate(prefab);
                    trophy.transform.SetParent(slot, true);
                    trophy.transform.localPosition = Vector3.zero;
                    trophy.transform.localRotation = Quaternion.identity;
                    Vector3 parentLossy = slot.lossyScale;
                    Vector3 prefabScale = prefab.transform.localScale;
                    Vector3 finalLocalScale = new Vector3(
                        parentLossy.x != 0f ? prefabScale.x / parentLossy.x : prefabScale.x,
                        parentLossy.y != 0f ? prefabScale.y / parentLossy.y : prefabScale.y,
                        parentLossy.z != 0f ? prefabScale.z / parentLossy.z : prefabScale.z
                    );
                    trophy.transform.localScale = finalLocalScale;
                }
            }
        }

        // 清空并生成player2槽位
        if (player2TrophySlots != null)
        {
            for (int i = 0; i < player2TrophySlots.Length; i++)
            {
                Transform slot = player2TrophySlots[i];
                if (slot == null) continue;
                for (int c = slot.childCount - 1; c >= 0; c--)
                {
                    DestroyImmediate(slot.GetChild(c).gameObject);
                }

                if (i < p2Score)
                {
                    GameObject prefab = trophyPrefabPlayer2;
                    if (prefab == null) continue;
                    // Instantiate without parent, then set parent and correct localScale so world size matches prefab
                    GameObject trophy = Instantiate(prefab);
                    trophy.transform.SetParent(slot, true);
                    trophy.transform.localPosition = Vector3.zero;
                    trophy.transform.localRotation = Quaternion.identity;
                    Vector3 parentLossy = slot.lossyScale;
                    Vector3 prefabScale = prefab.transform.localScale;
                    Vector3 finalLocalScale = new Vector3(
                        parentLossy.x != 0f ? prefabScale.x / parentLossy.x : prefabScale.x,
                        parentLossy.y != 0f ? prefabScale.y / parentLossy.y : prefabScale.y,
                        parentLossy.z != 0f ? prefabScale.z / parentLossy.z : prefabScale.z
                    );
                    trophy.transform.localScale = finalLocalScale;
                }
            }
        }
    }
}