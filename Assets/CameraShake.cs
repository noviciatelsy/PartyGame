using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraEffects
{
public class CameraShake : MonoBehaviour
{
    public Camera cam;
   public float duration = 0.5f;
    public float magnitude = 0.1f;
    private Vector3 originalPos;
    public IEnumerator Shake()
    {
        originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
}
