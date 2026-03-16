using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_win : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // 尝试获取 PlayerController
        PlayerController player = other.GetComponent<PlayerController>();
        Debug.Log("碰撞到物品了");
        if (player != null)
        {
            // 调用玩家功能
            player.Towin();

            // 销毁物品（如果需要）
            Destroy(gameObject);
        }
        
    }
}
