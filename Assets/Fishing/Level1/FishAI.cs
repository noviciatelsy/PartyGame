using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAI : MonoBehaviour
{
    public RectTransform fishRect;
    [SerializeField] private RectTransform UpBound;
    [SerializeField] private RectTransform DownBound;
    [SerializeField] private float speed = 200f;

    [Header("Runtime")]
    [SerializeField] private float currentSpeed;
    private float minBoundY;
    private float maxBoundY;
    private float currentDirection;

    [SerializeField] private float minChangeInterval = 0.5f;
    [SerializeField] private float maxChangeInterval = 2.0f;

    private float changeTimer;

    public void Initialize()
    {
        if (fishRect == null) fishRect = GetComponent<RectTransform>();
        if (fishRect == null || UpBound == null || DownBound == null)
        {
            enabled = false;
            return;
        }

        minBoundY = DownBound.anchoredPosition.y;
        maxBoundY = UpBound.anchoredPosition.y;

        fishRect.anchoredPosition = new Vector2(
            fishRect.anchoredPosition.x,
            (minBoundY + maxBoundY) / 2f
        );

        GenerateNewMovement();
    }

    public void Move()
    {
        if (!enabled) return;

        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f)
        {
            GenerateNewMovement();
        }

        Vector2 newPosition = fishRect.anchoredPosition;
        newPosition.y += currentDirection * currentSpeed * Time.deltaTime;
        if (newPosition.y >= maxBoundY)
        {
            newPosition.y = maxBoundY;
            currentDirection = -1f; 
            ResetTimer();
        }
        else if (newPosition.y <= minBoundY)
        {
            newPosition.y = minBoundY;
            currentDirection = 1f;
            ResetTimer();
        }

        fishRect.anchoredPosition = newPosition;
    }

    private void GenerateNewMovement()
    {
        currentSpeed = speed * Random.Range(0.8f, 1.2f);
        float rand = Random.value;
        if (rand < 0.4f) currentDirection = 1f;      //40%向上
        else if (rand < 0.8f) currentDirection = -1f; //40%向下
        else currentDirection = 0f;                   //20%停顿

        ResetTimer();
    }

    private void ResetTimer()
    {
        changeTimer = Random.Range(minChangeInterval, maxChangeInterval);
    }
}