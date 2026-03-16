using System;
using System.Collections;
using UnityEngine;

public class Click : MonoBehaviour
{
    [Header("手部物体")]
    [SerializeField] GameObject raiseHand;
    [SerializeField] GameObject downHand;

    [Header("按下后自动恢复时间")]
    public float recoverDelay = 0f;

    public Action OnPressed;

    private void Awake()
    {
        // 初始显示抬手，隐藏按下
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);
    }

    /// <summary>
    /// 外部调用
    /// </summary>
    //public void Press()
    //{
    //    // 切换到按下状态
    //    if (raiseHand) raiseHand.SetActive(false);
    //    if (downHand) downHand.SetActive(true);
    //    OnPressed?.Invoke();

    //    // 自动恢复
    //    StopAllCoroutines();
    //    StartCoroutine(RecoverCoroutine());
    //}

    public void Press()
    {
        StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        // raise
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);

        yield return null; // 强制一帧

        // down
        if (raiseHand) raiseHand.SetActive(false);
        if (downHand) downHand.SetActive(true);

        OnPressed?.Invoke();

        yield return new WaitForSeconds(recoverDelay);

        // raise
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);
    }

    private IEnumerator RecoverCoroutine()
    {
        yield return new WaitForSeconds(recoverDelay);
        // 恢复抬手状态
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);
    }
}