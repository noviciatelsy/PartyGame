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

    void CheckGameEnd()
    {
        if (!player1Finished || !player2Finished)
            return;

        if (player1Score > player2Score)
            Debug.Log("Player 1 Wins!");
        else if (player2Score > player1Score)
            Debug.Log("Player 2 Wins!");
        else
            Debug.Log("Draw!");

        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}
