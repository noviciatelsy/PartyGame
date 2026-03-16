using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapdayGM : MonoBehaviour
{
    public static LeapdayGM Instance;

    public PlayerController player1;
    public PlayerController player2;

    private void Awake()
    {
        Instance = this;
    }

    public void DeclareWinner(int playerIndex)
    {
        Debug.Log("Winner is Player " + playerIndex);
        GlobalScoreManager.Instance.AddScore(playerIndex, 1);

        var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
        if (camShake != null)
            StartCoroutine(camShake.Shake());
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private Coroutine winCoroutine;
    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3.0f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}
