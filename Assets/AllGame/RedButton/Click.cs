using System;
using System.Collections;
using UnityEngine;

public class Click : MonoBehaviour
{
    [Header("手部物体")]
    [SerializeField] GameObject raiseHand;
    [SerializeField] GameObject downHand;

    [Header("按下后自动恢复时间")]
    private float recoverDelay = 0.15f;
    public AudioSource clickSound;
    public Action OnPressed;

    private void Awake()
    {
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);
    }

    Coroutine pressCoroutine;
    public void Press()
    {
        if (pressCoroutine != null)
            StopCoroutine(pressCoroutine);

        pressCoroutine = StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        // raise
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);

        yield return null;

        // down
        if (raiseHand) raiseHand.SetActive(false);
        if (downHand) downHand.SetActive(true);

        if (clickSound && !clickSound.isPlaying)
            clickSound.Play();

        OnPressed?.Invoke();

        yield return new WaitForSeconds(recoverDelay);

        // raise
        if (raiseHand) raiseHand.SetActive(true);
        if (downHand) downHand.SetActive(false);
    }
}