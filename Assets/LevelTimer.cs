using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [Header("倒计时时长（秒）")]
    public float duration = 10f;

    [Header("需要缩放的物体")]
    public Transform targetScaleObject;

    [Header("初始 X 值")]
    public float startScaleX = 10f;

    [Header("结束 X 值")]
    public float endScaleX = 0f;

    private float timer;
    private bool isRunning = false;

    void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        timer = duration;
        isRunning = true;

        if (targetScaleObject != null)
        {
            Vector3 scale = targetScaleObject.localScale;
            scale.x = startScaleX;
            targetScaleObject.localScale = scale;
        }
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;

        float progress = Mathf.Clamp01(1f - (timer / duration));

        // 计算当前 scale.x
        float currentX = Mathf.Lerp(startScaleX, endScaleX, progress);

        if (targetScaleObject != null)
        {
            Vector3 scale = targetScaleObject.localScale;
            scale.x = currentX;
            targetScaleObject.localScale = scale;
        }

        if (timer <= 0f)
        {
            isRunning = false;
            OnTimerEnd();
        }
    }

    private void OnTimerEnd()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}
