using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ImageChange : MonoBehaviour
{
    public List<Sprite> images = new List<Sprite>();

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 切换图片
    public void SetImageIndex(int index)
    {
        if (images == null || images.Count == 0)
            return;

        if (index < 0 || index >= images.Count)
        {
            Debug.LogWarning("Image index out of range");
            return;
        }

        spriteRenderer.sprite = images[index];
    }

    // 可选：下一张
    public void Next()
    {
        if (images.Count == 0) return;

        int currentIndex = images.IndexOf(spriteRenderer.sprite);
        currentIndex = (currentIndex + 1) % images.Count;

        spriteRenderer.sprite = images[currentIndex];
    }

    // 可选：上一张
    public void Prev()
    {
        if (images.Count == 0) return;

        int currentIndex = images.IndexOf(spriteRenderer.sprite);
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = images.Count - 1;

        spriteRenderer.sprite = images[currentIndex];
    }
}