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

                // 记录调用时的位置，非跟随模式下结束后恢复到此位置
                Vector3 originalPos = transform.localPosition;

                float elapsed = 0.0f;

                while (elapsed < useDuration)
                {
                    float x = Random.Range(-1f, 1f) * useMagnitude;
                    float y = Random.Range(-1f, 1f) * useMagnitude;
                    transform.localPosition = originalPos + new Vector3(x, y, 0);

                    elapsed += Time.deltaTime;
                    yield return null;
                }
                // 恢复为调用时的位置
                transform.localPosition = _initialLocalPos;
                _isShaking = false;
        }
        //适用于摄像机跟随的震动：每帧以当前localPosition为基准做偏移
        public IEnumerator ShakeWithFollow(float? duration = null, float? magnitude = null)
        {
            if (_isShaking) yield break;
            _isShaking = true;
            float useDuration = duration ?? defaultDuration;
            float useMagnitude = magnitude ?? defaultMagnitude;

            float elapsed = 0.0f;
            Vector3 lastBasePos = transform.localPosition;
            while (elapsed < useDuration)
            {
                // 等待一帧以让跟随逻辑完成位置更新，再读取基准位置
                yield return null;
                Vector3 basePos = transform.localPosition;
                float x = Random.Range(-1f, 1f) * useMagnitude;
                float y = Random.Range(-1f, 1f) * useMagnitude;
                Vector3 offset = new Vector3(x, y, 0);
                transform.localPosition = basePos + offset;
                lastBasePos = basePos;

                elapsed += Time.deltaTime;
            }
            // 结束时恢复到跟随逻辑的最后位置，避免回到 Awake 时的初始位置
            transform.localPosition = lastBasePos;
            _isShaking = false;
        }
    }
}