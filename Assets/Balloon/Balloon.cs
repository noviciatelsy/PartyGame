using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BalloonEntity
{
    public string playerName;
    public GameObject balloonObj;
    public Animator anim;
    public AnimationClip ccb;
    public int id;
    [HideInInspector] public float currentHoldTimer = 0f;
    [HideInInspector] public bool isGameOver = false;
    [HideInInspector] public bool canContinuousInflate = false;
    [HideInInspector] public Coroutine prepareRoutine;
    [HideInInspector] public Vector3 minScale;
    public void Init()
    {
        anim = balloonObj.GetComponent<Animator>();
        minScale = balloonObj.transform.localScale;
    }
}
