using System.Collections;
using UnityEngine;

namespace CameraEffects
{
    public class CameraShake : MonoBehaviour
    {
        public Camera cam;
        [Header("Ä¬ÈÏ²ÎÊý")]
        public float defaultDuration = 0.2f;
        public float defaultMagnitude = 0.1f;

        private bool _isShaking = false;
        public IEnumerator ShakeWithFollow(float? duration = null, float? magnitude = null)
        {
            if (_isShaking) yield break;
            _isShaking = true;

            float useDuration = duration ?? defaultDuration;
            float useMagnitude = magnitude ?? defaultMagnitude;
            float elapsed = 0.0f;
            Vector3 originalLocalPos = transform.localPosition;

            while (elapsed < useDuration)
            {
                elapsed += Time.deltaTime;
                float damper = 1.0f - (elapsed / useDuration);
                
                float x = Random.Range(-1f, 1f) * useMagnitude * damper;
                float y = Random.Range(-1f, 1f) * useMagnitude * damper;
                transform.localPosition = originalLocalPos + new Vector3(x, y, 0);

                yield return null;
        
            }
            transform.localPosition = originalLocalPos;
            _isShaking = false;
        }
        public IEnumerator Shake(float? duration = null, float? magnitude = null)
        {
            if (_isShaking) yield break;
            _isShaking = true;
            Vector3 posBeforeShake = transform.localPosition;

            float useDuration = duration ?? defaultDuration;
            float useMagnitude = magnitude ?? defaultMagnitude;
            float elapsed = 0.0f;

            while (elapsed < useDuration)
            {
                float x = Random.Range(-1f, 1f) * useMagnitude;
                float y = Random.Range(-1f, 1f) * useMagnitude;
                transform.localPosition = posBeforeShake + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = posBeforeShake;
            _isShaking = false;
        }
    }
}