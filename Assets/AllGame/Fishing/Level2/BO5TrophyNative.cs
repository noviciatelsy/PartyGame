using System.Collections;
using UnityEngine;

public class BO5TrophyNative : MonoBehaviour
{
    [Header("奖杯 UI")]
    public RectTransform centerPoint;      // 屏幕中心参考点
    public RectTransform[] fragments;      // 3个碎片（摆好槽位位置，初始隐藏）
    public RectTransform fullTrophy;       // 完整奖杯

    [Header("碎片掉落")]
    public float fragmentDropHeight = 500f;
    public AudioClip fragmentDropSound;
    public AudioSource audioSource;
    [Header("最终奖杯缩放")]
    public float finalTrophyScale = 3f;

    [Header("玩家头像")]
    public Transform winnerPortrait;       // 胜者头像 Transform
    public Transform loserPortrait;        // 败者头像 Transform
    public GameObject winnerNormalObj;     // 胜者常态图
    public GameObject winnerWinObj;        // 胜者胜利图
    public GameObject loserNormalObj;      // 败者常态图
    public GameObject loserLoseObj;        // 败者失败图

    [Header("相机震动")]
    public Transform mainCameraTransform;

    private Vector3 _originalCameraPos;

    void Start()
    {
        if (mainCameraTransform == null) mainCameraTransform = Camera.main.transform;
        _originalCameraPos = mainCameraTransform.localPosition;

        foreach (var f in fragments) f.gameObject.SetActive(false);
        fullTrophy.gameObject.SetActive(false);

        // 头像初始：常态显示，胜负隐藏
        if (winnerNormalObj) winnerNormalObj.SetActive(true);
        if (winnerWinObj) winnerWinObj.SetActive(false);
        if (loserNormalObj) loserNormalObj.SetActive(true);
        if (loserLoseObj) loserLoseObj.SetActive(false);
    }

    /// <summary>
    /// 外部调用入口：currentWinCount 取值 1, 2, 3
    /// </summary>
    public void PlayWinStep(int currentWinCount)
    {
        StartCoroutine(FragmentRoutine(currentWinCount - 1));
    }

    IEnumerator FragmentRoutine(int index)
    {
        RectTransform frag = fragments[index];
        Vector3 targetPos = frag.localPosition;

        // 从槽位正上方掉落
        frag.gameObject.SetActive(true);
        Vector3 startPos = targetPos + Vector3.up * fragmentDropHeight;
        frag.localPosition = startPos;
        frag.localScale = Vector3.one;

        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeIn = t * t;
            frag.localPosition = Vector3.Lerp(startPos, targetPos, easeIn);
            yield return null;
        }
        frag.localPosition = targetPos;

        if (fragmentDropSound)
        {
            audioSource.PlayOneShot(fragmentDropSound);
        }

        // 入槽轻震
        StartCoroutine(ShakeCamera(0.15f, 0.1f));

        // 第三局：触发合体 + 头像切换
        if (index == 2)
        {
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(VictoryRoutine());
        }
    }

   IEnumerator VictoryRoutine()
    {
        foreach (var f in fragments) f.gameObject.SetActive(false);  
        fullTrophy.gameObject.SetActive(true);
        fullTrophy.localPosition = fragments[0].parent.localPosition;
        fullTrophy.localScale = Vector3.one;
        fullTrophy.localRotation = Quaternion.identity;
        StartCoroutine(ShakeCamera(0.5f, 0.3f));
        SwitchPortraits();
        if (winnerPortrait) StartCoroutine(WinnerPortraitAnim());
        if (loserPortrait) StartCoroutine(LoserPortraitAnim());
        yield return null;
        gameObject.SetActive(false);
        fullTrophy.gameObject.SetActive(false);
    }
    void SwitchPortraits()
    {
        // 只显示胜者胜利图，隐藏胜者常态图
        if (winnerNormalObj) winnerNormalObj.SetActive(false);
        if (winnerWinObj) winnerWinObj.SetActive(true);
        // 只显示败者失败图，隐藏败者常态图
        if (loserNormalObj) loserNormalObj.SetActive(false);
        if (loserLoseObj) loserLoseObj.SetActive(true);
    }

    /// <summary>
    /// 胜者头像：爆裂缩放 0.8 -> 1.3 -> 1.0
    /// </summary>
    IEnumerator WinnerPortraitAnim()
    {
        Vector3 orig = winnerPortrait.localScale;

        // 阶段1：缩小到 0.8
        yield return StartCoroutine(ScaleTo(winnerPortrait, orig * 0.8f, 0.1f));
        // 阶段2：弹出到 1.3
        yield return StartCoroutine(ScaleTo(winnerPortrait, orig * 1.3f, 0.2f));
        // 阶段3：回弹到 1.0
        yield return StartCoroutine(ScaleTo(winnerPortrait, orig, 0.15f));
    }

    /// <summary>
    /// 败者头像：缩小到0.85并变暗
    /// </summary>
    IEnumerator LoserPortraitAnim()
    {
        Vector3 orig = loserPortrait.localScale;
        Vector3 target = orig * 0.85f;

        float elapsed = 0f;
        float duration = 0.4f;
        SpriteRenderer sr = loserPortrait.GetComponent<SpriteRenderer>();
        Color origColor = sr ? sr.color : Color.white;
        Color darkColor = origColor * 0.5f;
        darkColor.a = origColor.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float s = Mathf.SmoothStep(0, 1, t);
            loserPortrait.localScale = Vector3.Lerp(orig, target, s);
            if (sr) sr.color = Color.Lerp(origColor, darkColor, s);
            yield return null;
        }
    }

    IEnumerator ScaleTo(Transform rt, Vector3 target, float duration)
    {
        Vector3 start = rt.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        rt.localScale = target;
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCameraTransform.localPosition = new Vector3(
                _originalCameraPos.x + x, _originalCameraPos.y + y, _originalCameraPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCameraTransform.localPosition = _originalCameraPos;
    }
}
