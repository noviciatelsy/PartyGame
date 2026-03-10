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

    public void ResetState()
    {
        currentVelocity = 0f;
        progress = 0.5f;
        isPressing = false;
        finished = false;
        hasControlled = true;
        if (progressImage) progressImage.fillAmount = 0.5f;
        if (progressLeftImage) progressLeftImage.fillAmount = 0.5f;
        if (progressRightImage) progressRightImage.fillAmount = 0.5f;
    }
}
public class PlayerUI : MonoBehaviour
{
    public PlayerEntity player_1;
    public PlayerEntity player_2;

    [Header("物理设置")]
    [Header("下落重力")]
    [SerializeField] private float gravity = 2500f;
    [Header("按住时的升力")]
    [SerializeField] private float liftForce = 3500f;
    [Header("最大移动速度限制")]
    [SerializeField] private float maxVelocity = 1200f;
    [Range(0f, 1f), Header("触底反弹系数（0-1）")]
    [SerializeField] private float bounceFactor = 0.35f;
    [Header("阻力，越接近1越小")]
    [SerializeField] private float drag = 0.98f;
    [Header("时长配置（秒）")]
    [SerializeField] private float holdTimeToWin = 3f; // 从0恢复到满需要的时间
    [SerializeField] private float holdTimeToLose = 3f; // 从满掉到0需要的时间

    public Action<PlayerEntity, bool> OnPlayerHoldResult;

    private List<FishAI> fishAIs;
    private Canvas parentCanvas;

    public void Initialize(List<FishAI> fishes)
    {
        fishAIs = fishes;
        parentCanvas = player_1.frameRect.GetComponentInParent<Canvas>();
        player_1.ResetState();
        player_2.ResetState();

        ConfigureCenterFill(player_1);
        ConfigureCenterFill(player_2);
    }

    private void ConfigureCenterFill(PlayerEntity p)
    {
        if (p == null) return;
        if (p.progressLeftImage != null)
        {
            p.progressLeftImage.type = Image.Type.Filled;
            p.progressLeftImage.fillMethod = Image.FillMethod.Horizontal;
            p.progressLeftImage.fillOrigin = (int)Image.OriginHorizontal.Right;
        }
        if (p.progressRightImage != null)
        {
            p.progressRightImage.type = Image.Type.Filled;
            p.progressRightImage.fillMethod = Image.FillMethod.Horizontal;
            p.progressRightImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }

    private void SetProgressVisual(PlayerEntity p, float normalized)
    {
        float v = Mathf.Clamp01(normalized);
        if (p.progressLeftImage != null && p.progressRightImage != null)
        {
            p.progressLeftImage.fillAmount = v;
            p.progressRightImage.fillAmount = v;
            return;
        }
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
    }

    public void StopHoldForPlayer(int playerID)
    {
        if (player_1 != null && player_1.playerID == playerID) player_1.isPressing = false;
        if (player_2 != null && player_2.playerID == playerID) player_2.isPressing = false;
        PlayerEntity p = (player_1 != null && player_1.playerID == playerID) ? player_1 : player_2;
        if (p != null)
        {
            p.currentVelocity -= gravity * Time.deltaTime * 0.5f;
        }
    }

    public void ResetProgress()
    {
        if (player_1 != null) player_1.ResetState();
        if (player_2 != null) player_2.ResetState();
    }
}