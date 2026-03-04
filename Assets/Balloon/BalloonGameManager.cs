using System.Collections;
using TMPro;
using UnityEngine;
public class BalloonGameManager : MonoBehaviour
{
    public static BalloonGameManager instance;

    [Header("Player Settings")]
    public BalloonEntity p1;
    public BalloonEntity p2;

    [Header("Global Settings")]
    public float continuousSpeed = 0.8f;
    public float deflateSpeed = 0.4f;
    [SerializeField] private float maxHoldTime;
    [SerializeField] private TextMeshProUGUI gameTip;

    private void Awake()
    {
        if (instance == null) instance = this;
        maxHoldTime = Random.Range(4.0f, 8.0f);

        InitPlayer(p1);
        InitPlayer(p2);
    }

    private void InitPlayer(BalloonEntity p)
    {
        p.anim = p.balloonObj.GetComponent<Animator>();
        p.minScale = p.balloonObj.transform.localScale;
    }

    private void Update()
    {
        HandlePlayerInput(p1, KeyCode.Space);
        HandlePlayerInput(p2, KeyCode.Mouse0);
    }

    private void HandlePlayerInput(BalloonEntity p, KeyCode key)
    {
        if (p.isGameOver) return;

        if (Input.GetKeyDown(key))
        {
            if (p.prepareRoutine != null) StopCoroutine(p.prepareRoutine);
            p.prepareRoutine = StartCoroutine(PrepareToInflateRoutine(p, key));
        }

        if (Input.GetKey(key) && p.canContinuousInflate)
        {
            HandleContinuousInflation(p);
        }
        else if (!Input.GetKey(key))
        {
            HandleDeflation(p);
        }

        if (Input.GetKeyUp(key))
        {
            StopInflating(p);
        }
    }

    private IEnumerator PrepareToInflateRoutine(BalloonEntity p, KeyCode key)
    {
        p.canContinuousInflate = false;
        p.anim.Play("充充爆", 0, 0f);

        float waitTime = (p.ccb != null) ? p.ccb.length : 0.3f;
        yield return new WaitForSeconds(waitTime);

        if (Input.GetKey(key))
        {
            p.canContinuousInflate = true;
        }
        else
        {
            p.anim.Play("Idle");
        }
    }

    private void HandleContinuousInflation(BalloonEntity p)
    {
        p.currentHoldTimer += Time.deltaTime;
        float growth = continuousSpeed * Time.deltaTime;
        p.balloonObj.transform.localScale += new Vector3(growth, growth, growth);

        if (p.currentHoldTimer >= maxHoldTime)
        {
            Explode(p);
        }
    }

    private void HandleDeflation(BalloonEntity p)
    {
        if (p.balloonObj.transform.localScale.x > p.minScale.x)
        {
            float shrink = deflateSpeed * Time.deltaTime;
            p.balloonObj.transform.localScale -= new Vector3(shrink, shrink, shrink);

            p.currentHoldTimer -= Time.deltaTime * (deflateSpeed / continuousSpeed);
            if (p.currentHoldTimer < 0) p.currentHoldTimer = 0;
        }
        else
        {
            p.balloonObj.transform.localScale = p.minScale;
            p.currentHoldTimer = 0;
        }
    }

    private void StopInflating(BalloonEntity p)
    {
        p.canContinuousInflate = false;
        if (p.prepareRoutine != null) StopCoroutine(p.prepareRoutine);

        if (!p.isGameOver)
        {
            if (p.balloonObj.transform.localScale.x > p.minScale.x + 0.05f)
                p.anim.Play("爆爆充");
            else
                p.anim.Play("Idle");
        }
    }

    private void Explode(BalloonEntity p)
    {
        //isGameOver = true;
        p.canContinuousInflate = false;
        gameTip.text = $"{p.playerName} Exploded!";
        p.anim.Play("爆炸");
    }
}