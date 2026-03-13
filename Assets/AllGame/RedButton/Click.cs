using System;
using UnityEngine;

public class Click : MonoBehaviour
{
    [Header("�̶�λ��")]
    public float upY = 1f;
    public float downY = -1f;

    [Header("�ƶ��ٶ�")]
    public float moveSpeed = 25f;
public Action OnPressed;
    private float targetY;
    private bool moving = false;

    private void Awake()
    {
        // ��ʼ���Ϸ�
        targetY = upY;
        Vector3 pos = transform.localPosition;
        pos.y = upY;
    }

    /// <summary>
    /// �ⲿ����
    /// </summary>
    public void Press()
    {
        // ÿ�ε�� �� ����ѹ
        targetY = downY;
        moving = true;
        OnPressed?.Invoke();
    }

    private void Update()
    {
        Vector3 pos = transform.localPosition;

        // ƽ���ƶ���Ŀ��Y
        pos.y = Mathf.MoveTowards(
            pos.y,
            targetY,
            moveSpeed * Time.deltaTime
        );

        transform.localPosition = pos;

        // ����Ѿ����·�
        if (Mathf.Abs(pos.y - downY) < 0.01f)
        {
            // �Զ��ص�
            targetY = upY;
        }

        // �����Ϸ���ֹͣ�ƶ�
        if (Mathf.Abs(pos.y - upY) < 0.01f && targetY == upY)
        {
            moving = false;
        }
    }
}