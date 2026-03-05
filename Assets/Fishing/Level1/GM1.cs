using System.Collections.Generic;
using UnityEngine;
using System;
public class GM1 : MonoBehaviour
{
    [SerializeField] private List<FishAI> fishes;
    [SerializeField] private PlayerUI playerUIComp;
    public Action<PlayerEntity> OnGameEnd;

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
        PlayerEntity winner = null;
        if (success)
        {
            winner = p;
        }
        else
        {
            if (playerUIComp.player_1 != null && playerUIComp.player_2 != null)
            {
                winner = (playerUIComp.player_1.playerID == p.playerID) ? playerUIComp.player_2 : playerUIComp.player_1;
            }
        }

        if (winner != null)
        {
            Debug.Log($"{winner.playerName} Win!");
        }

        Time.timeScale = 0f;
        OnGameEnd?.Invoke(winner);
        if (fishes != null)
        {
            foreach (var f in fishes) if (f != null) f.enabled = false;
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