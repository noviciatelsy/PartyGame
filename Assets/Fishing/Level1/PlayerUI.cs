using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform frameRect;
    [SerializeField] private Image progressImage;

    [Header("Bounds")]
    [SerializeField] private RectTransform UpBound;
    [SerializeField] private RectTransform DownBound;

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
    [Header("胜利需求持续时间")]
    [SerializeField] private float holdTimeToWin = 3f;
    [Header("不在区域内时进度下降速度")]
    [SerializeField] private float progressDeclineRate = 0.5f; 

    public Action OnPlayerHoldSuccess;
    public Action OnPlayerHoldFail;

    private FishAI fishAI;
    private Canvas parentCanvas;
    private float currentVelocity = 0f;
    private float holdTimer = 0f;
    private bool isPressing = false;
    private bool finished = false;

    public void Initialize(FishAI fish)
    {
        fishAI = fish;
        parentCanvas = frameRect.GetComponentInParent<Canvas>();
        ResetProgress();
    }

    void Update()
    {
        if (!enabled || finished || fishAI == null) return;

        ApplyPhysics();
        UpdateProgress();
    }

    private void ApplyPhysics()
    {
        float acceleration = -gravity;
        if (isPressing)
        {
            acceleration += liftForce;
        }
        currentVelocity += acceleration * Time.deltaTime;
        currentVelocity *= drag;
        currentVelocity = Mathf.Clamp(currentVelocity, -maxVelocity, maxVelocity);
        Vector2 pos = frameRect.anchoredPosition;
        pos.y += currentVelocity * Time.deltaTime;
        float maxY = UpBound.anchoredPosition.y;
        float minY = DownBound.anchoredPosition.y;

        if (pos.y >= maxY)
        {
            pos.y = maxY;
            if (currentVelocity > 0) currentVelocity = 0;
        }
        else if (pos.y <= minY)
        {
            pos.y = minY;
            if (currentVelocity < -100f)
            {
                currentVelocity = -currentVelocity * bounceFactor;
            }
            else
            {
                currentVelocity = 0;
            }
        }

        frameRect.anchoredPosition = pos;
    }

    private void UpdateProgress()
    {
        Vector2 fishScreenPos = RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera, fishAI.transform.position);
        bool inside = RectTransformUtility.RectangleContainsScreenPoint(frameRect, fishScreenPos, parentCanvas.worldCamera);

        if (inside)
        {
            holdTimer += Time.deltaTime;
        }
        else
        {
            holdTimer -= Time.deltaTime * progressDeclineRate;
        }

        holdTimer = Mathf.Clamp(holdTimer, 0, holdTimeToWin);
        if (progressImage) progressImage.fillAmount = holdTimer / holdTimeToWin;

        if (holdTimer >= holdTimeToWin)
        {
            finished = true;
            OnPlayerHoldSuccess?.Invoke();
        }
        else if (holdTimer <= 0 && !inside)
        {
            finished = true;
            OnPlayerHoldFail?.Invoke();
        }
    }

    public void SetPress(bool pressing)
    {
        isPressing = pressing;
    }

    public void ResetProgress()
    {
        holdTimer = 0f;
        currentVelocity = 0f;
        finished = false;
        if (progressImage) progressImage.fillAmount = 0f;
    }
}