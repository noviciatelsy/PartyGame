using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLinePlayer : MonoBehaviour
{
    public PlayableDirector timeline_1;
    public PlayableDirector timeline_2;
    void Awake()
    {
        GlobalScoreManager.Instance.OnGameEnd += HandleGameEnd;
    }

    private void HandleGameEnd(int winnerPlayerID)
    {
        if (winnerPlayerID == 1)
        {
            timeline_1.Play();
        }
        else if (winnerPlayerID == 2)
        {
            timeline_2.Play();
        }
    }
}
