using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class faceManager : MonoBehaviour
{
    private bool player1Finished = false;
    private bool player2Finished = false;

    private float player1Score;
    private float player2Score;

    private Coroutine winCoroutine;

    public void RegisterScore(FaceController.PlayerType player, float score)
    {
        if (player == FaceController.PlayerType.Player1)
        {
            player2Score = score;
            player2Finished = true;
        }
        else
        {
            player1Score = score;
            player1Finished = true;
        }

        CheckGameEnd();
    }

    public FaceController player1Controller;
    public FaceController player2Controller;
    void CheckGameEnd()
    {
        if (!player1Finished || !player2Finished)
            return;


        // œ‘ æ’˝»∑Õº∆¨
        if (player1Controller != null)
            player1Controller.ShowCorrectImage();

        if (player2Controller != null)
            player2Controller.ShowCorrectImage();


        var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
        if (camShake != null)
            StartCoroutine(camShake.Shake());
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(1.1f);
        if (player1Score > player2Score)
        {
            Debug.Log("Player 1 Wins!");
            GlobalScoreManager.Instance.AddScore(1, 1);
        }
        else if (player2Score > player1Score)
        {
            Debug.Log("Player 2 Wins!");
            GlobalScoreManager.Instance.AddScore(2, 1);
        }
        else
            Debug.Log("Draw!");
        yield return new WaitForSeconds(3f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

}
