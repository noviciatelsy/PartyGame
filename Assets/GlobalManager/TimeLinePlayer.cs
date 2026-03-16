using UnityEngine;
using UnityEngine.Playables;

public class TimeLinePlayer : MonoBehaviour
{
    public AudioSource aS;
    public PlayableDirector timeline_1;
    public PlayableDirector timeline_2;

    void Start()
    {
        if (GlobalScoreManager.Instance == null)
        {
            Debug.LogError("TimeLinePlayer: GlobalScoreManager ¶ªÊ§£¡");
            return;
        }
        int p1Score = GlobalScoreManager.Instance.GetScore(1);
        int p2Score = GlobalScoreManager.Instance.GetScore(2);

        if (p1Score > p2Score)
        {
            if (timeline_1 != null) timeline_1.Play();
        }
        else if (p2Score > p1Score)
        {
            if (timeline_2 != null) timeline_2.Play();
        }

        if (aS != null)
        {
            aS.Play();
        }
    }
    public void ExitGame()
    {
        LevelManager.Instance.LoadLevel(LevelManager.Instance.mainMenuScene);
    }
}