using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
public class GM1 : MonoBehaviour
{
    [SerializeField] private List<FishAI> fishes;
    [SerializeField] private PlayerUI playerUIComp;
    public Action<PlayerEntity> OnGameEnd;
    [Header("Match UI")]
    [SerializeField] private List<GameObject> player1Medals;
    [SerializeField] private List<GameObject> player2Medals;
    [SerializeField] private int roundsToWin = 2;
    private Coroutine winCoroutine;

    private bool mouseWasDown = false;
    private Action spaceDownHandler;
    private Action spaceUpHandler;
    private Action spaceHoldHandler;
    private Action<GlobalInput.InputType> spaceActionHandler;
    private Action<GlobalInput.InputType> mouseActionHandler;

    void Start()
    {
        Time.timeScale = 1f; 
        if (fishes == null || fishes.Count == 0)
        {
            Debug.LogWarning("GM1.Start: no fishes assigned.");
            return;
        }

        foreach (var fish in fishes)
        {
            if (fish != null) fish.Initialize();
        }
        playerUIComp.Initialize(fishes);
        // 初始化分数并隐藏所有奖牌，确保一开始显示为空
        if (playerUIComp != null)
        {
            if (playerUIComp.player_1 != null) playerUIComp.player_1.Score = 0;
            if (playerUIComp.player_2 != null) playerUIComp.player_2.Score = 0;
        }
        if (player1Medals != null)
        {
            foreach (var m in player1Medals) if (m != null) m.SetActive(false);
        }
        if (player2Medals != null)
        {
            foreach (var m in player2Medals) if (m != null) m.SetActive(false);
        }
        UpdateMedalsUI();
        spaceDownHandler = () => playerUIComp.SetPressForPlayer(1, true);
        spaceUpHandler = () => { playerUIComp.SetPressForPlayer(1, false); playerUIComp.StopHoldForPlayer(1); };
        spaceHoldHandler = () => playerUIComp.StartHoldForPlayer(1);
        spaceActionHandler = (t) =>
        {
            if (t == GlobalInput.InputType.SingleClick || t == GlobalInput.InputType.DoubleClick)
            {
                playerUIComp.ShortRush(1);
            }
        };

        GlobalInput.Instance.OnSpaceDown += spaceDownHandler;
        GlobalInput.Instance.OnSpaceUp += spaceUpHandler;
        GlobalInput.Instance.OnSpaceHoldStart += spaceHoldHandler;
        GlobalInput.Instance.OnSpaceAction += spaceActionHandler;
        mouseActionHandler = (t) =>
        {
            if (t == GlobalInput.InputType.SingleClick || t == GlobalInput.InputType.DoubleClick)
            {
                playerUIComp.ShortRush(2);
            }
            else if (t == GlobalInput.InputType.LongPress)
            {
                playerUIComp.StartHoldForPlayer(2);
                playerUIComp.StopHoldForPlayer(2);
            }
        };
        GlobalInput.Instance.OnMouseLeftAction += mouseActionHandler;

        playerUIComp.OnPlayerHoldResult += HandleResult;
    }

    void Update()
    {
        if (fishes != null)
        {
            for (int i = 0; i < fishes.Count; i++)
            {
                var f = fishes[i];
                if (f != null && f.enabled) f.Move();
            }
        }
        bool mouseDown = Input.GetMouseButton(0);
        if (mouseDown && !mouseWasDown)
        {
            playerUIComp.SetPressForPlayer(2, true);
        }
        else if (!mouseDown && mouseWasDown)
        {
            playerUIComp.SetPressForPlayer(2, false);
            playerUIComp.StopHoldForPlayer(2);
        }
        mouseWasDown = mouseDown;
    }
    private void HandleResult(PlayerEntity p, bool success)
    {
        PlayerEntity roundWinner = null;
        if (success)
        {
            roundWinner = p;
        }
        else
        {
            if (playerUIComp.player_1 != null && playerUIComp.player_2 != null)
            {
                roundWinner = (playerUIComp.player_1.playerID == p.playerID) ? playerUIComp.player_2 : playerUIComp.player_1;
            }
        }

        if (roundWinner == null) return;

        roundWinner.Score += 1;
        UpdateMedalsUI();

        Debug.Log($"{roundWinner.playerName} Win round! Score: {roundWinner.Score}");

        if (fishes != null)
        {
            foreach (var f in fishes) if (f != null) f.enabled = false;
        }

        if (roundWinner.Score >= roundsToWin)
        {
            Debug.Log($"{roundWinner.playerName} is match winner!");
            //========================新增代码================
            if (roundWinner.playerID == 1)
            {
                GlobalScoreManager.Instance.AddScore(1, 1);
            }
            else
            {
                GlobalScoreManager.Instance.AddScore(2, 1);
            }

            if (winCoroutine == null)
                winCoroutine = StartCoroutine(MatchWinCoroutine(roundWinner));
        }
        else
        {
            StartCoroutine(RoundEndCoroutine());
        }
    }

    private void UpdateMedalsUI()
    {
        if (playerUIComp == null) return;
        if (player1Medals != null && playerUIComp.player_1 != null)
        {
            for (int i = 0; i < player1Medals.Count; i++)
            {
                player1Medals[i].SetActive(i < playerUIComp.player_1.Score);
            }
        }
        if (player2Medals != null && playerUIComp.player_2 != null)
        {
            for (int i = 0; i < player2Medals.Count; i++)
            {
                player2Medals[i].SetActive(i < playerUIComp.player_2.Score);
            }
        }
    }

    private IEnumerator RoundEndCoroutine()
    {
        yield return new WaitForSeconds(2f);
        ResetForNextRound();
    }

    private IEnumerator MatchWinCoroutine(PlayerEntity winner)
    {
        yield return new WaitForSeconds(3f);
        OnGameEnd?.Invoke(winner);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

    private void ResetForNextRound()
    {
        if (playerUIComp != null)
        {
            playerUIComp.ResetProgress();
        }
        if (fishes != null)
        {
            foreach (var f in fishes)
            {
                if (f != null)
                {
                    f.enabled = true;
                    f.Initialize();
                }
            }
        }
    }

    void OnDestroy()
    {
        if (GlobalInput.Instance != null)
        {
            if (spaceDownHandler != null) GlobalInput.Instance.OnSpaceDown -= spaceDownHandler;
            if (spaceUpHandler != null) GlobalInput.Instance.OnSpaceUp -= spaceUpHandler;
            if (spaceHoldHandler != null) GlobalInput.Instance.OnSpaceHoldStart -= spaceHoldHandler;
            if (spaceActionHandler != null) GlobalInput.Instance.OnSpaceAction -= spaceActionHandler;
            if (mouseActionHandler != null) GlobalInput.Instance.OnMouseLeftAction -= mouseActionHandler;
        }
    }
}