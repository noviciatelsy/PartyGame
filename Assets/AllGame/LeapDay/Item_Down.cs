using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Dwon : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // 尝试获取 PlayerController
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // 调用玩家功能
            player.ToDown();

            // 销毁物品（如果需要）
            Destroy(gameObject);
        }
    }
}
