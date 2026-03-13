using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [Header("TimeofLevel")]
    public float duration = 10f;
    public Transform targetScaleObject;

    [Header("timebar")]
    public float startScaleX = 10f;
    public float endScaleX = 0f;
    public Action OnTimerEndCallBack;
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

        // ���㵱ǰ scale.x
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
        if (OnTimerEndCallBack != null)
        {
            OnTimerEndCallBack.Invoke();
        }
        else if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}
