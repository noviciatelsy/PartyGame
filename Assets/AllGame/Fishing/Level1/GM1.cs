using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
public class GM1 : MonoBehaviour
{
    [SerializeField] private List<FishAI> fishes;
    [SerializeField] private PlayerUI playerUIComp;
    public Action<PlayerEntity> OnGameEnd;
    private Coroutine winCoroutine;
    private bool mouseWasDown = false;
    private Action spaceDownHandler;
    private Action spaceUpHandler;
    private Action spaceHoldHandler;
    private Action<GlobalInput.InputType> spaceActionHandler;
    private Action<GlobalInput.InputType> mouseActionHandler;
public LevelTimer levelTimer;
    void Start()
    {
        levelTimer = GameObject.FindObjectOfType<LevelTimer>();
        levelTimer.OnTimerEndCallBack += () =>
        {
            if (winCoroutine != null) return;

            PlayerEntity winner = null;
            var p1 = playerUIComp.player_1;
            var p2 = playerUIComp.player_2;

            if (p1 != null && p2 != null)
            {
                winner = p1.progress >= p2.progress ? p1 : p2;
            }
            else
            {
                winner = p1 != null ? p1 : p2;
            }

            if (winner != null)
            {
                Debug.Log($"Time's up! {winner.playerName} wins (progress: {winner.progress:F2})");
                if (fishes != null)
                    foreach (var f in fishes) if (f != null) f.enabled = false;

                if (winner.playerID == 1)
                    GlobalScoreManager.Instance.AddScore(1, 1);
                else
                    GlobalScoreManager.Instance.AddScore(2, 1);

                // 头像切换与震动
                playerUIComp.ShowPortraits(winner.playerID);
                playerUIComp.ShakeCamera();
            }

            winCoroutine = StartCoroutine(MatchWinCoroutine(winner));
        };
        Time.timeScale = 1f; 
        foreach (var fish in fishes)
        {
            if (fish != null) fish.Initialize();
        }
        playerUIComp.Initialize(fishes);

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

        Debug.Log($"{roundWinner.playerName} Win!");

        if (fishes != null)
        {
            foreach (var f in fishes) if (f != null) f.enabled = false;
        }

        if (roundWinner.playerID == 1)
        {
            GlobalScoreManager.Instance.AddScore(1, 1);
        }
        else
        {
            GlobalScoreManager.Instance.AddScore(2, 1);
        }

        // 头像切换与震动
        playerUIComp.ShowPortraits(roundWinner.playerID);
        playerUIComp.ShakeCamera();

        if (winCoroutine == null)
            winCoroutine = StartCoroutine(MatchWinCoroutine(roundWinner));
    }

    private IEnumerator MatchWinCoroutine(PlayerEntity winner)
    {
        yield return new WaitForSeconds(2f);
        // 结算后确保头像状态正确
        if (winner != null)
        {
            playerUIComp.ShowPortraits(winner.playerID);
        }
        OnGameEnd?.Invoke(winner);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
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