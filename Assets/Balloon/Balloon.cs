using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BalloonEntity
{
    public string playerName;
    public GameObject balloonObj;
    public AnimationClip ccb;
    public Sprite initialSprite;
    public Sprite inflatingSprite;
    public int score = 0;
    [HideInInspector] public Animator anim;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Vector3 minScale;
    [HideInInspector] public float currentHoldTimer = 0f;
    [HideInInspector] public bool isGameOver = false;
    [HideInInspector] public bool canContinuousInflate = false;
    [HideInInspector] public Coroutine prepareRoutine;
    [HideInInspector] public bool isWaitingToPlayDeflateAnim = false;
}
