using UnityEngine;

public class GM1 : MonoBehaviour
{
    [SerializeField] private FishAI fishAI;
    [SerializeField] private PlayerUI playerUIComp;

    void Start()
    {
        fishAI.Initialize();
        playerUIComp.Initialize(fishAI);
        GlobalInput.Instance.OnSpaceDown += () => playerUIComp.SetPress(true);
        GlobalInput.Instance.OnSpaceUp += () => playerUIComp.SetPress(false);
        playerUIComp.OnPlayerHoldSuccess += HandleSuccess;
        playerUIComp.OnPlayerHoldFail += HandleFail;
    }

    void Update()
    {
        if (fishAI != null && fishAI.enabled) fishAI.Move();
    }
    private void HandleSuccess()
    {
        Debug.Log("Hold Success!");
        fishAI.enabled = false;
    }
    private void HandleFail()
    {
        Debug.Log("Hold Failed!");
        fishAI.enabled = false;
    }   
}