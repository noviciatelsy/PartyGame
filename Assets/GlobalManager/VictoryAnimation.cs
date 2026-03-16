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
    public Transform coinPlayer1; 
    public Transform coinPlayer2; 
    public Transform coin1Start;
    public Transform coin2Start;
    public Transform coinCenter;
    
    public Transform[] player1CoinTargets = new Transform[5];
    public Transform[] player2CoinTargets = new Transform[5];

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

    void Start()
    {
        text1.text = "";
        text2.text = "";
        text1.transform.localScale = Vector3.zero;
        text2.transform.localScale = Vector3.zero;

        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);

        if (coinPlayer1 != null) coinPlayer1.gameObject.SetActive(false);
        if (coinPlayer2 != null) coinPlayer2.gameObject.SetActive(false);
    }

    public void PlayVictory(bool isPlayer1)
    {
        if (!isPlaying)
            StartCoroutine(VictoryRoutine(isPlayer1));
    }

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

    IEnumerator PlayCoinAnimation(bool isPlayer1)
    {
        Transform coinStart;
        Transform coinTarget = null;
        Transform activeCoin;

        int playerID = isPlayer1 ? 1 : 2;
        int score = 0;
        if (GlobalScoreManager.Instance != null)
            score = GlobalScoreManager.Instance.GetScore(playerID);
        
        int targetIndex = Mathf.Clamp(score - 1, 0, 4);

        if (isPlayer1)
        {
            activeCoin = coinPlayer1;
            coinStart = coin1Start;
            if (player1CoinTargets != null && player1CoinTargets.Length > targetIndex)
                coinTarget = player1CoinTargets[targetIndex];

            if (coinPlayer1 != null) coinPlayer1.gameObject.SetActive(true);
            if (coinPlayer2 != null) coinPlayer2.gameObject.SetActive(false);
        }
        else
        {
            activeCoin = coinPlayer2;
            coinStart = coin2Start;
            if (player2CoinTargets != null && player2CoinTargets.Length > targetIndex)
                coinTarget = player2CoinTargets[targetIndex];

            if (coinPlayer2 != null) coinPlayer2.gameObject.SetActive(true);
            if (coinPlayer1 != null) coinPlayer1.gameObject.SetActive(false);
        }

        if (coinStart == null || coinTarget == null || coinCenter == null || activeCoin == null)
        {
            Debug.LogError("Coin animation references missing!");
            yield break;
        }

        activeCoin.position = coinStart.position;
        activeCoin.localScale = Vector3.one * coinStartScale;
        activeCoin.rotation = Quaternion.identity;

        float t = 0;
        while (t < coinFlyInTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / coinFlyInTime;
            float ease = 1 - Mathf.Pow(1 - lerp, 3);
            Vector3 pos = Vector3.Lerp(coinStart.position, coinCenter.position, ease);
            pos.y += Mathf.Sin(ease * Mathf.PI) * 0.5f;
            activeCoin.position = pos;
            activeCoin.localScale = Vector3.Lerp(Vector3.one * coinStartScale, Vector3.one * coinCenterScale, ease);
            yield return null;
        }

        activeCoin.position = coinCenter.position;
        activeCoin.localScale = Vector3.one * coinCenterScale;

        t = 0;
        while (t < coinFlipTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / coinFlipTime;
            float rotY = Mathf.Lerp(0, 360, lerp);
            activeCoin.rotation = Quaternion.Euler(0, rotY, 0);
            yield return null;
        }
        activeCoin.rotation = Quaternion.identity;

        yield return new WaitForSecondsRealtime(coinPauseTime);

        t = 0;
        if (audioSource != null && medalLandedSound != null)
            audioSource.PlayOneShot(medalLandedSound);
        
        Vector3 startPos = activeCoin.position;
        Vector3 startScale = activeCoin.localScale;

        while (t < coinFlyOutTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / coinFlyOutTime;
            float ease = 1 - Mathf.Pow(1 - lerp, 3);
            activeCoin.position = Vector3.Lerp(startPos, coinTarget.position, ease);
            activeCoin.localScale = Vector3.Lerp(startScale, Vector3.one * coinFinalScale, ease);
            
            var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
            if (camShake != null) StartCoroutine(camShake.ShakeWithFollow());
            yield return null;
        }
        activeCoin.position = coinTarget.position;
        activeCoin.localScale = Vector3.one * coinFinalScale;
        
        StartCoroutine(SpawnAndFlyTrophy(isPlayer1));
    }

    IEnumerator SpawnAndFlyTrophy(bool isPlayer1)
    {
        if (trophyPrefabPlayer1 == null && trophyPrefabPlayer2 == null) yield break;

        Transform activeCoin = isPlayer1 ? coinPlayer1 : coinPlayer2;
        int playerID = isPlayer1 ? 1 : 2;
        int score = 0;
        if (GlobalScoreManager.Instance != null) score = GlobalScoreManager.Instance.GetScore(playerID);
        int slotIndex = Mathf.Clamp(score - 1, 0, 4);

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

        GameObject prefabToUse = isPlayer1 ? trophyPrefabPlayer1 : trophyPrefabPlayer2;
        if (prefabToUse == null) yield break;

        // 生成奖杯并记录预制体原始缩放
        GameObject trophy = Instantiate(prefabToUse, activeCoin.position, Quaternion.identity);
        Vector3 startPos = trophy.transform.position;
        Vector3 endPos = targetSlot.position;

        // 移除飞行过程中的缩放逻辑，仅保留位置移动
        float t = 0f;
        while (t < trophyFlyTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / trophyFlyTime;
            float ease = 1 - Mathf.Pow(1 - lerp, 3);
            trophy.transform.position = Vector3.Lerp(startPos, endPos, ease);
            // 比例保持不变（不进行额外的 targetScale 插值）
            yield return null;
        }

        trophy.transform.position = endPos;

        // 清理槽位
        for (int i = targetSlot.childCount - 1; i >= 0; i--)
        {
            Transform child = targetSlot.GetChild(i);
            if (child == null || child == trophy.transform) continue;
            string lname = child.name.ToLower();
            if (lname.Contains("wincup") || child.GetComponent<SpriteRenderer>() != null || child.GetComponent<MeshRenderer>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // 设置父级，SetParent(parent, true) 会尝试保持当前的世界空间尺寸
        trophy.transform.SetParent(targetSlot, true);
        trophy.transform.localPosition = Vector3.zero;
        trophy.transform.localRotation = Quaternion.identity;
    }

    public void PopulateTrophiesFromScores(int p1Score, int p2Score)
    {
        UpdateSlotTrophies(player1TrophySlots, p1Score, trophyPrefabPlayer1);
        UpdateSlotTrophies(player2TrophySlots, p2Score, trophyPrefabPlayer2);
    }

    private void UpdateSlotTrophies(Transform[] slots, int currentScore, GameObject prefab)
    {
        if (slots == null || prefab == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            Transform slot = slots[i];
            if (slot == null) continue;
            for (int c = slot.childCount - 1; c >= 0; c--) DestroyImmediate(slot.GetChild(c).gameObject);
            
            if (i < currentScore)
            {
                GameObject trophy = Instantiate(prefab);
                trophy.transform.SetParent(slot, false); // 直接对齐父级，不再进行复杂的比例换算
                trophy.transform.localPosition = Vector3.zero;
                trophy.transform.localRotation = Quaternion.identity;
                trophy.transform.localScale = Vector3.one; 
            }
        }
    }
}