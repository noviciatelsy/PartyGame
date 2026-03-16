using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraEffects
{
public class CameraShake : MonoBehaviour
{
    public Camera cam;
    [Header("默认抖动时长")]
    public float defaultDuration = 0.2f;
    [Header("默认抖动幅度")]
    public float defaultMagnitude = 0.1f;
    private Vector3 originalPos;
    public IEnumerator Shake(float? duration = null, float? magnitude = null)
    {
        float useDuration = duration ?? defaultDuration;
        float useMagnitude = magnitude ?? defaultMagnitude;
        originalPos = transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < useDuration)
        {
            float x = Random.Range(-1f, 1f) * useMagnitude;
            float y = Random.Range(-1f, 1f) * useMagnitude;
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
}
