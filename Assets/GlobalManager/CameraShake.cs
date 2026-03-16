using System.Collections;
using UnityEngine;

namespace CameraEffects
{
    public class CameraShake : MonoBehaviour
    {
        public Camera cam;
        [Header("默认参数")]
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
            // 适用于摄像机跟随的震动：每帧以当前localPosition为基准做偏移
        public IEnumerator ShakeWithFollow(float? duration = null, float? magnitude = null)
        {
            if (_isShaking) yield break;
            _isShaking = true;
            float useDuration = duration ?? defaultDuration;
            float useMagnitude = magnitude ?? defaultMagnitude;
            float elapsed = 0.0f;
            while (elapsed < useDuration)
            {
                Vector3 basePos = transform.localPosition;
                float x = Random.Range(-1f, 1f) * useMagnitude;
                float y = Random.Range(-1f, 1f) * useMagnitude;
                transform.localPosition = basePos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
                // 恢复到跟随逻辑的位置
                transform.localPosition = basePos;
            }
            _isShaking = false;
        }
}
}