using System.Collections;
using UnityEngine;
using TMPro;

public class VictoryAnimation : MonoBehaviour
{
    [Header("Objects")]
    public Transform leftObj;
    public Transform rightObj;

    [Header("Text")]
    public TextMeshPro text1;
    public TextMeshPro text2;
    public string wintext;

    [Header("Settings")]
    private float rotateTime = 0.25f;

    private bool isPlaying = false;

    void Start()
    {
        text1.text = "";
        text2.text = "";

        text1.transform.localScale = Vector3.zero;
        text2.transform.localScale = Vector3.zero;

        // 初始隐藏
        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(false);

    }

    // 对外调用接口
    public void PlayVictory(bool isPlayer1)
    {
        if (!isPlaying)
            StartCoroutine(VictoryRoutine(isPlayer1));
    }

    IEnumerator VictoryRoutine(bool isPlayer1)
    {
        isPlaying = true;

        float t = 0;

        if (isPlayer1)
        {
            text1.gameObject.SetActive(true);
            text1.text = "";
            text1.transform.localScale = Vector3.zero;
            // 左侧动画
            leftObj.localEulerAngles = new Vector3(0, 0, 90);

            while (t < rotateTime)
            {
                t += Time.deltaTime;
                float lerp = t / rotateTime;

                float rot = Mathf.Lerp(90, 0, lerp);
                leftObj.localEulerAngles = new Vector3(0, 0, rot);

                yield return null;
            }

            text1.text = wintext;

            yield return StartCoroutine(SpringScale(text1.transform));

            text1.transform.localScale = Vector3.one;
        }
        else
        {
            text2.gameObject.SetActive(true);
            text2.text = "";
            text2.transform.localScale = Vector3.zero;
            // 右侧动画
            rightObj.localEulerAngles = new Vector3(0, 0, -90);

            while (t < rotateTime)
            {
                t += Time.deltaTime;
                float lerp = t / rotateTime;

                float rot = Mathf.Lerp(-90, 0, lerp);
                rightObj.localEulerAngles = new Vector3(0, 0, rot);

                yield return null;
            }

            text2.text = wintext;

            yield return StartCoroutine(SpringScale(text2.transform));

            text2.transform.localScale = Vector3.one;
        }

        isPlaying = false;
    }

    IEnumerator SpringScale(Transform target)
    {
        float value = 0f;
        float velocity = 0f;
        float targetValue = 1f;

        float stiffness = 700f;   // 弹力
        float damping = 20f;      // 阻尼

        float maxTime = 1.5f;
        float time = 0f;

        while (time < maxTime)
        {
            float dt = Time.deltaTime;
            time += dt;

            float force = stiffness * (targetValue - value) - damping * velocity;
            velocity += force * dt;
            value += velocity * dt;

            target.localScale = Vector3.one * value;

            yield return null;
        }

        target.localScale = Vector3.one;
    }
}