using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EarGameManager : MonoBehaviour
{
    [Header("游戏设置")]
    public float gameDuration = 10f;  // 总游戏时间
    public TMP_Text timerText;        // 用于显示时间的UI文本
    public GameObject earFull;        // 完整耳道显示
    public GameObject earCut;         // 显示耳道剖面

    [Header("判定区间")]
    public float successRangeMin = -5f; // 棍子的成功最小X值
    public float successRangeMax = 5f;  // 棍子的成功最大X值

    private float timer;              // 游戏倒计时
    public bool isGameStarted = false;  // 游戏是否开始

    private void Start()
    {
        timer = gameDuration;
        earFull.SetActive(true);
        earCut.SetActive(false);
        timerText.text = timer.ToString("F1");

        isGameStarted = true;
    }

    private void Update()
    {
        if (isGameStarted)
        {
            // 游戏倒计时
            timer -= Time.deltaTime;
            if (timer <= 2f && earFull.activeSelf) // 倒计时到2秒时，隐藏完整耳道显示，显示剖面图
            {
                earFull.SetActive(false);
                earCut.SetActive(true);
            }

            if (timer <= 0f)
            {
                EndGame();
            }

            timerText.text = timer.ToString("F1");
        }
    }

    public void StartGame()
    {
        isGameStarted = true;
    }

    public void EndGame()
    {
        // 结束游戏，处理胜负等逻辑
        isGameStarted = false;
        // 这里可以加入游戏结束后显示成绩等逻辑
        Debug.Log("Game Over!");
    }

    public void OnStickFail()
    {
        // 棍子失败的逻辑
        EndGame();
        Debug.Log("Player Failed!");
    }

    public void OnStickSuccess(float distance)
    {
        // 棍子成功的逻辑，接近目标区域得分高
        float score = Mathf.Clamp(1f - Mathf.Abs(distance) / (successRangeMax - successRangeMin), 0f, 1f);
        Debug.Log($"Player Success! Score: {score * 100f}%");
    }
}
