using System.Collections;
using TMPro;
using UnityEngine;

public class BalloonGameManager : MonoBehaviour
{
    public static BalloonGameManager instance;
    public GameObject balloonPrefab;
    public Animator anim;
    public AnimationClip ccb;
    public float shortPressAmount = 0.2f;
    public float continuousSpeed = 0.8f;

    [Header("漏气设置")]
    public float deflateSpeed = 0.4f;
    public Vector3 minScale;

    [SerializeField] private float maxHoldTime;
    [SerializeField] private TextMeshProUGUI gameTip;
    private float currentHoldTimer = 0f;
    private bool isGameOver = false;
    private bool canContinuousInflate = false;
    private Coroutine prepareRoutine;

    private void Start()
    {
        if (instance == null) instance = this;
        maxHoldTime = Random.Range(4.0f, 8.0f);
        anim = balloonPrefab.GetComponent<Animator>();
        minScale = balloonPrefab.transform.localScale;
    }

    private void Update()
    {
        if (isGameOver) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (prepareRoutine != null) StopCoroutine(prepareRoutine);
            prepareRoutine = StartCoroutine(PrepareToInflate());
        }
        if (Input.GetKey(KeyCode.Space) && canContinuousInflate)
        {
            gameTip.text = $"持续充气中... 进度: {currentHoldTimer:F1}s";
            HandleContinuousInflation();
        }
        else if (!Input.GetKey(KeyCode.Space))
        {
            HandleDeflation();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            StopInflating();
        }
    }

    private IEnumerator PrepareToInflate()
    {
        anim.Play("充充爆", 0, 0f);
        float waitTime = (ccb != null) ? ccb.length : 0.3f;
        yield return new WaitForSeconds(waitTime);

        if (Input.GetKey(KeyCode.Space))
        {
            canContinuousInflate = true;
        }
        else
        {
            anim.Play("Idle");
        }
    }

    private void HandleContinuousInflation()
    {
        currentHoldTimer += Time.deltaTime;
        float growth = continuousSpeed * Time.deltaTime;
        balloonPrefab.transform.localScale += new Vector3(growth, growth, growth);

        if (currentHoldTimer >= maxHoldTime)
        {
            Explode();
        }
    }
    private void HandleDeflation()
    {
        if (balloonPrefab.transform.localScale.x > minScale.x)
        {
            float shrink = deflateSpeed * Time.deltaTime;
            balloonPrefab.transform.localScale -= new Vector3(shrink, shrink, shrink);
            currentHoldTimer -= Time.deltaTime * (deflateSpeed / continuousSpeed);
            if (currentHoldTimer < 0) currentHoldTimer = 0;

            gameTip.text = "气球漏气中...";
        }
        else
        {
            balloonPrefab.transform.localScale = minScale;
            currentHoldTimer = 0;
        }
    }

    private void StopInflating()
    {
        canContinuousInflate = false;
        if (prepareRoutine != null) StopCoroutine(prepareRoutine);

        if (!isGameOver)
        {
            if (balloonPrefab.transform.localScale.x >= minScale.x)
            {
                anim.Play("爆爆充");
            }
            else
            anim.Play("Idle");
        }
        

        Debug.Log("停止充气，当前计时：" + currentHoldTimer);
    }

    private void Explode()
    {
        isGameOver = true;
        canContinuousInflate = false;
        gameTip.text = "爆炸！";
        anim.Play("爆炸");
    }
}