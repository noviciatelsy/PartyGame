using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float width = 3f;
    public Transform platform;

    public float LeftEdge => transform.position.x - width * 0.5f;
    public float RightEdge => transform.position.x + width * 0.5f;

    // 给定X坐标是否落在平台上
    public bool IsPointOnPlatform(float x)
    {
        return x >= LeftEdge && x <= RightEdge;
    }

    // 返回平台边缘（玩家要移动到这里）
    public float GetEdgeX()
    {
        return RightEdge;
    }

    public void SetWidth(float width)
    {
        this.width = width;

        platform.localScale = new Vector3(
            width,
            platform.localScale.y,
            platform.localScale.z
        );
    }
}
