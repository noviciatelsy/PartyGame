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

        private Vector3 _initialLocalPos;
        private bool _isShaking = false;

        private void Awake()
        {
            _initialLocalPos = transform.localPosition;
        }

        public IEnumerator Shake(float? duration = null, float? magnitude = null)
        {
            if (_isShaking) yield break; 
            
            _isShaking = true;
            float useDuration = duration ?? defaultDuration;
            float useMagnitude = magnitude ?? defaultMagnitude;
            
            float elapsed = 0.0f;

            while (elapsed < useDuration)
            {
                float x = Random.Range(-1f, 1f) * useMagnitude;
                float y = Random.Range(-1f, 1f) * useMagnitude;
                transform.localPosition = _initialLocalPos + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = _initialLocalPos;
            _isShaking = false;
        }
    }
}