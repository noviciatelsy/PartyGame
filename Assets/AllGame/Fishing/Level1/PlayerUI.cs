using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

[System.Serializable]
public class PlayerEntity
{
    [Header("References")]
    public RectTransform frameRect;
    public AudioSource audiosource;
    public Image progressImage; 
    [Header("Center Fill (Optional)")]
    public Image progressLeftImage; 
    public Image progressRightImage;
    public string playerName;
    public int playerID;
    private int score = 0;
    public int Score { get => score; set => score = value; }
    [Header("Bounds")]
    public RectTransform UpBound;
    public RectTransform DownBound;
    [HideInInspector] public float currentVelocity = 0f;
    [HideInInspector] public float progress = 0.5f;
    [HideInInspector] public bool isPressing = false;
    [HideInInspector] public bool finished = false;
    [HideInInspector] public bool hasControlled = false;

    public void ResetState(float startProgress = 0.3f)
    {
        currentVelocity = 0f;
        progress = startProgress;
        isPressing = false;
        finished = false;
        hasControlled = false;
        if (progressImage) progressImage.fillAmount = startProgress;
    }
}
public class PlayerUI : MonoBehaviour
{
    public PlayerEntity player_1;
    public PlayerEntity player_2;

    [Header("玩家1头像")] public GameObject p1NormalObj; // 常态
    public GameObject p1WinObj; // 胜利
    public GameObject p1LoseObj; // 失败
    [Header("玩家2头像")] public GameObject p2NormalObj;
    public GameObject p2WinObj;
    public GameObject p2LoseObj;

    [Header("相机震动")]
    public Transform mainCameraTransform;
    private Vector3 _originalCameraPos;

    [Header("鐗╃悊璁剧疆")]
    [Header("涓嬭惤閲嶅姏")]
    [SerializeField] private float gravity = 2500f;
    [Header("鎸変綇鏃剁殑鍗囧姏")]
    [SerializeField] private float liftForce = 3500f;
    [Header("鏈�澶хЩ鍔ㄩ�熷害闄愬埗")]
    [SerializeField] private float maxVelocity = 1200f;
    [Range(0f, 1f), Header("瑙﹀簳鍙嶅脊绯绘暟锛?0-1锛?")]
    [SerializeField] private float bounceFactor = 0.35f;
    [Header("闃诲姏锛岃秺鎺ヨ繎1瓒婂皬")]
    [SerializeField] private float drag = 0.98f;
    [Header("杩涘害鏉￠厤缃?")]
    [SerializeField, Range(0f, 1f)] private float initialProgress = 0.3f;
    [Header("鏃堕暱閰嶇疆锛堢锛?")]
    [SerializeField] private float holdTimeToWin = 3f; // 浠?0鎭㈠鍒版弧闇�瑕佺殑鏃堕棿
    [SerializeField] private float holdTimeToLose = 3f; // 浠庢弧鎺夊埌0闇�瑕佺殑鏃堕棿

    public Action<PlayerEntity, bool> OnPlayerHoldResult;

    private List<FishAI> fishAIs;
    private Canvas parentCanvas;

    public void Initialize(List<FishAI> fishes)
    {
        fishAIs = fishes;
        parentCanvas = player_1.frameRect.GetComponentInParent<Canvas>();
        player_1.ResetState(initialProgress);
        player_2.ResetState(initialProgress);

        ConfigureVerticalFill(player_1);
        ConfigureVerticalFill(player_2);

        // 头像初始化：常态显示，胜负隐藏
        if (p1NormalObj) p1NormalObj.SetActive(true);
        if (p1WinObj) p1WinObj.SetActive(false);
        if (p1LoseObj) p1LoseObj.SetActive(false);
        if (p2NormalObj) p2NormalObj.SetActive(true);
        if (p2WinObj) p2WinObj.SetActive(false);
        if (p2LoseObj) p2LoseObj.SetActive(false);

        // 相机震动初始化
        if (mainCameraTransform == null && Camera.main != null) mainCameraTransform = Camera.main.transform;
        if (mainCameraTransform != null) _originalCameraPos = mainCameraTransform.localPosition;
    }

    private void ConfigureVerticalFill(PlayerEntity p)
    {
        if (p == null) return;
        if (p.progressImage != null)
        {
            p.progressImage.type = Image.Type.Filled;
            p.progressImage.fillMethod = Image.FillMethod.Vertical;
            p.progressImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
    }

    private void SetProgressVisual(PlayerEntity p, float normalized)
    {
        float v = Mathf.Clamp01(normalized);
        if (p.progressImage != null)
        {
            p.progressImage.fillAmount = v;
        }
    }

    void Update()
    {
        UpdatePlayer(player_1);
        UpdatePlayer(player_2);
    }
    private void UpdatePlayer(PlayerEntity p)
    {
        if (p == null || p.frameRect == null || p.UpBound == null || p.DownBound == null) return;
        if (p.finished) return;

        ApplyPhysics(p);
        UpdateProgressForPlayer(p);
    }

    private void ApplyPhysics(PlayerEntity p)
    {
        float acceleration = -gravity;
        if (p.isPressing)
        {
            acceleration += liftForce;
        }
        p.currentVelocity += acceleration * Time.deltaTime;
        p.currentVelocity *= drag;
        p.currentVelocity = Mathf.Clamp(p.currentVelocity, -maxVelocity, maxVelocity);
        Vector2 pos = p.frameRect.anchoredPosition;
        pos.y += p.currentVelocity * Time.deltaTime;
        float maxY = p.UpBound.anchoredPosition.y;
        float minY = p.DownBound.anchoredPosition.y;

        if (pos.y >= maxY)
        {
            pos.y = maxY;
            if (p.currentVelocity > 0) p.currentVelocity = 0;
        }
        else if (pos.y <= minY)
        {
            pos.y = minY;
            if (p.currentVelocity < -100f)
            {
                p.currentVelocity = -p.currentVelocity * bounceFactor;
            }
            else
            {
                p.currentVelocity = 0;
            }
        }

        p.frameRect.anchoredPosition = pos;
    }

    private void UpdateProgressForPlayer(PlayerEntity p)
    {
        bool inside = false;
        if (fishAIs != null)
        {
            foreach (var f in fishAIs)
            {
                if (f == null) continue;
                Vector2 fishScreenPos = RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera, f.transform.position);
                if (RectTransformUtility.RectangleContainsScreenPoint(p.frameRect, fishScreenPos, parentCanvas.worldCamera))
                {
                    inside = true;
                    break;
                }
            }
        }
        float recoverPerSecond = holdTimeToWin > 0f ? 1f / holdTimeToWin : 0f;
        float declinePerSecond = holdTimeToLose > 0f ? 1f / holdTimeToLose : 0f;

        if (inside)
        {
            p.hasControlled = true;
            p.progress += recoverPerSecond * Time.deltaTime;
        }
        else if (p.hasControlled)
        {
            p.progress -= declinePerSecond * Time.deltaTime;
        }

        p.progress = Mathf.Clamp01(p.progress);
        SetProgressVisual(p, p.progress);

        if (p.progress >= 1f)
        {
            p.finished = true;
            OnPlayerHoldResult?.Invoke(p, true);
            return;
        }

        if (p.progress <= 0f && p.hasControlled && !inside)
        {
            p.finished = true;
            OnPlayerHoldResult?.Invoke(p, false);
        }
    }

    public void SetPress(bool pressing)
    {
        if (player_1 != null) player_1.isPressing = pressing;
        if (player_2 != null) player_2.isPressing = pressing;
    }
    public void SetPressForPlayer(int playerID, bool pressing)
    {
        if (player_1 != null && player_1.playerID == playerID) player_1.isPressing = pressing;
        if (player_2 != null && player_2.playerID == playerID) player_2.isPressing = pressing;
    }

    public void ShortRush(int playerID)
    {
        PlayerEntity p = (player_1 != null && player_1.playerID == playerID) ? player_1 : player_2;
        if (p == null || p.frameRect == null) return;
        p.currentVelocity += liftForce * 0.1f;
    }

    public void StartHoldForPlayer(int playerID)
    {
        if (player_1 != null && player_1.playerID == playerID) player_1.isPressing = true;
        if (player_2 != null && player_2.playerID == playerID) player_2.isPressing = true;
        // 播放长按音效（若配置了 AudioSource）
        PlayerEntity p = (player_1 != null && player_1.playerID == playerID) ? player_1 : player_2;
        if (p != null && p.audiosource != null)
        {
            p.audiosource.loop = true;
            if (!p.audiosource.isPlaying)
                p.audiosource.Play();
        }
    }

    public void StopHoldForPlayer(int playerID)
    {
        if (player_1 != null && player_1.playerID == playerID) player_1.isPressing = false;
        if (player_2 != null && player_2.playerID == playerID) player_2.isPressing = false;
        PlayerEntity p = (player_1 != null && player_1.playerID == playerID) ? player_1 : player_2;
        if (p != null)
        {
            p.currentVelocity -= gravity * Time.deltaTime * 0.5f;
            // 停止长按音效
            if (p.audiosource != null && p.audiosource.isPlaying)
            {
                p.audiosource.Stop();
                p.audiosource.loop = false;
            }
        }
    }

    public void ResetProgress()
    {
        if (player_1 != null) player_1.ResetState(initialProgress);
        if (player_2 != null) player_2.ResetState(initialProgress);
        // 重置头像为常态
        if (p1NormalObj) p1NormalObj.SetActive(true);
        if (p1WinObj) p1WinObj.SetActive(false);
        if (p1LoseObj) p1LoseObj.SetActive(false);
        if (p2NormalObj) p2NormalObj.SetActive(true);
        if (p2WinObj) p2WinObj.SetActive(false);
        if (p2LoseObj) p2LoseObj.SetActive(false);
        // 确保长按音效被停止
        if (player_1 != null && player_1.audiosource != null && player_1.audiosource.isPlaying)
        {
            player_1.audiosource.Stop();
            player_1.audiosource.loop = false;
        }
        if (player_2 != null && player_2.audiosource != null && player_2.audiosource.isPlaying)
        {
            player_2.audiosource.Stop();
            player_2.audiosource.loop = false;
        }
    }

    // 头像切换方法
    public void ShowPortraits(int winnerID, bool isDraw = false)
    {
        // 平局只显示常态
        if (isDraw)
        {
            if (p1NormalObj) p1NormalObj.SetActive(true);
            if (p1WinObj) p1WinObj.SetActive(false);
            if (p1LoseObj) p1LoseObj.SetActive(false);
            if (p2NormalObj) p2NormalObj.SetActive(true);
            if (p2WinObj) p2WinObj.SetActive(false);
            if (p2LoseObj) p2LoseObj.SetActive(false);
            return;
        }
        // 玩家1胜
        if (winnerID == 1)
        {
            if (p1NormalObj) p1NormalObj.SetActive(false);
            if (p1WinObj) p1WinObj.SetActive(true);
            if (p1LoseObj) p1LoseObj.SetActive(false);
            if (p2NormalObj) p2NormalObj.SetActive(false);
            if (p2WinObj) p2WinObj.SetActive(false);
            if (p2LoseObj) p2LoseObj.SetActive(true);
        }
        // 玩家2胜
        else if (winnerID == 2)
        {
            if (p1NormalObj) p1NormalObj.SetActive(false);
            if (p1WinObj) p1WinObj.SetActive(false);
            if (p1LoseObj) p1LoseObj.SetActive(true);
            if (p2NormalObj) p2NormalObj.SetActive(false);
            if (p2WinObj) p2WinObj.SetActive(true);
            if (p2LoseObj) p2LoseObj.SetActive(false);
        }
    }

    // 屏幕震动方法
    public void ShakeCamera(float duration = 0.2f, float magnitude = 0.1f)
    {
        if (mainCameraTransform != null)
            StartCoroutine(ShakeCameraCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCameraCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            mainCameraTransform.localPosition = new Vector3(
                _originalCameraPos.x + x, _originalCameraPos.y + y, _originalCameraPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCameraTransform.localPosition = _originalCameraPos;
    }
}