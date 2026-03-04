using System.Collections;
using UnityEngine;

public class BalloonGameManager : MonoBehaviour
{
    public static BalloonGameManager instance;
    public GameObject balloonPrefab;
    public Animator anim;
    public AnimationClip ccb;
    [Header("Settings")]
    public float shortPressAmount = 0.2f;
    public float continuousSpeed = 0.8f;
    [SerializeField] private float maxHoldTime;

    private float currentHoldTimer = 0f;
    private bool isGameOver = false;
    private bool canContinuousInflate = false;
    private Coroutine prepareRoutine;

    private void Start()
    {
        if (instance == null) instance = this;
        maxHoldTime = Random.Range(4.0f, 8.0f);
        anim = balloonPrefab.GetComponent<Animator>();
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
            HandleContinuousInflation();
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

    private void StopInflating()
    {
        canContinuousInflate = false;
        if (prepareRoutine != null) StopCoroutine(prepareRoutine);
        Debug.Log("停止充气，当前计时：" + currentHoldTimer);
    }

    private void Explode()
    {
        isGameOver = true;
        canContinuousInflate = false;
        Debug.Log("爆炸！");
        anim.Play("爆炸");
    }
}