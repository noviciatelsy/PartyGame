using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//QTE：两个指针在各自区域从上到下快速移动，玩家按键尝试在随机生成的 QTE 区域内停止指针。
public class GM2 : MonoBehaviour
{
	[Header("UI Bounds & Pointers")]
	public RectTransform upBound1;
	public RectTransform downBound1;
	public RectTransform pointer1;

	public RectTransform upBound2;
	public RectTransform downBound2;
	public RectTransform pointer2;

	public RectTransform qteZonePrefab;

	[Header("游戏设置")]
	public int totalRounds = 5;
	public int roundsToWin = 3;
	public List<GameObject> player1Medals;
	public List<GameObject> player2Medals;

	[Header("初始QTE区域高度")]
	public float baseZoneHeight = 120f; 
    [Header("每轮QTE区域缩小量")]
	public float zoneShrinkPerRound = 25f;
	public float minZoneHeight = 30f;
	[Header("像素/秒，第一轮的指针移动速度")]
	public float basePointerSpeed = 600f; 
    [Header("每轮增加的指针速度")] 
	public float speedIncreasePerRound = 250f;
[Header("QTE开始前的提示延迟时间")]
	public float preQTEDelay = 1.0f; 

	[Header("Audio")]
	public AudioClip preQTESound;
	// public AudioSource audioSource; //在QTE开始前播放音效，先保留接口

	private RectTransform activeZone1;
	private RectTransform activeZone2;
	private int currentRound = 0;
	private int score1 = 0;
	private int score2 = 0;
	private bool matchActive = false;

	private Coroutine roundCoroutine;
    private Coroutine winCoroutine;

    void Start()
	{
		if (player1Medals != null) foreach (var m in player1Medals) if (m != null) m.SetActive(false);
		if (player2Medals != null) foreach (var m in player2Medals) if (m != null) m.SetActive(false);

		StartMatch();
	}

	public void StartMatch()
	{
		score1 = 0; score2 = 0; currentRound = 0; matchActive = true;
		UpdateMedalsUI();
		StartNextRound();
	}

	private void StartNextRound()
	{
		if (!matchActive) return;
		currentRound++;
		if (currentRound > totalRounds)
		{
			EndMatch();
			return;
		}
		if (roundCoroutine != null) StopCoroutine(roundCoroutine);
		roundCoroutine = StartCoroutine(RunRoundCoroutine(currentRound));
	}

	private IEnumerator RunRoundCoroutine(int roundIndex)
	{
		float zoneHeight = Mathf.Max(minZoneHeight, baseZoneHeight - (roundIndex - 1) * zoneShrinkPerRound);
		float pointerSpeed = basePointerSpeed + (roundIndex - 1) * speedIncreasePerRound;

		// if (audioSource != null && preQTESound != null) audioSource.PlayOneShot(preQTESound);
		yield return new WaitForSeconds(preQTEDelay);
		float sharedY = CalcRandomZoneY(upBound1, downBound1, zoneHeight);
		CreateOrResetZone(ref activeZone1, upBound1, pointer1, zoneHeight, sharedY);
		CreateOrResetZone(ref activeZone2, upBound2, pointer2, zoneHeight, sharedY);
		SetPointerToTop(pointer1, upBound1);
		SetPointerToTop(pointer2, upBound2);

		bool roundEnded = false;
		bool attempted1 = false;
		bool attempted2 = false;

		float topY1 = upBound1.anchoredPosition.y;
		float bottomY1 = downBound1.anchoredPosition.y;
		float topY2 = upBound2.anchoredPosition.y;
		float bottomY2 = downBound2.anchoredPosition.y;
		while (!roundEnded)
		{
			float step1 = pointerSpeed * Time.deltaTime;
			MovePointerDown(pointer1, step1, bottomY1);
			MovePointerDown(pointer2, step1, bottomY2);

			if (!attempted1 && Input.GetKeyDown(KeyCode.Space))
			{
				attempted1 = true;
				if (IsPointerInZone(pointer1, activeZone1))
				{
					score1++;
				}
				else
				{
					score2++;
				}
				UpdateMedalsUI();
				roundEnded = true;
			}
			if (!attempted2 && Input.GetMouseButtonDown(0))
			{
				attempted2 = true;
				if (IsPointerInZone(pointer2, activeZone2))
				{
					score2++;
				}
				else
				{
					score1++;
				}
				UpdateMedalsUI();
				roundEnded = true;
			}
			bool p1AtBottom = pointer1.anchoredPosition.y <= bottomY1;
			bool p2AtBottom = pointer2.anchoredPosition.y <= bottomY2;
			if ((p1AtBottom || attempted1) && (p2AtBottom || attempted2))
			{
				roundEnded = true;
			}

			if (roundEnded) break;
			yield return null;
		}
		if (activeZone1 != null) Destroy(activeZone1.gameObject);
		if (activeZone2 != null) Destroy(activeZone2.gameObject);
		if (score1 >= roundsToWin || score2 >= roundsToWin)
		{
			EndMatch();
		}
		else
		{
			yield return new WaitForSeconds(1.0f);
			StartNextRound();
		}
	}
	private float CalcRandomZoneY(RectTransform up, RectTransform down, float height)
	{
		float topY = up.anchoredPosition.y;
		float bottomY = down.anchoredPosition.y;
		float minY = Mathf.Min(topY, bottomY);
		float maxY = Mathf.Max(topY, bottomY);
		float halfH = height * 0.5f;
		return Random.Range(minY + halfH, maxY - halfH);
	}

	private void CreateOrResetZone(ref RectTransform zoneRef, RectTransform upBound, RectTransform pointer, float height, float y)
	{
		if (qteZonePrefab == null) return;
		if (zoneRef != null) Destroy(zoneRef.gameObject);
		var go = Instantiate(qteZonePrefab, upBound.parent);
		zoneRef = go;
		zoneRef.anchoredPosition = new Vector2(pointer.anchoredPosition.x, y);
		zoneRef.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

	private void SetPointerToTop(RectTransform p, RectTransform upBound)
	{
		if (p == null || upBound == null) return;
		p.anchoredPosition = new Vector2(p.anchoredPosition.x, upBound.anchoredPosition.y);
	}

	private void MovePointerDown(RectTransform p, float deltaY, float bottomY)
	{
		if (p == null) return;
		Vector2 pos = p.anchoredPosition;
		pos.y -= deltaY;
		if (pos.y < bottomY) pos.y = bottomY;
		p.anchoredPosition = pos;
	}

	private bool IsPointerInZone(RectTransform pointer, RectTransform zone)
	{
		if (pointer == null || zone == null) return false;
		float py = pointer.anchoredPosition.y;
		float zy = zone.anchoredPosition.y;
		float zh = zone.rect.height;
		float top = zy + zh * 0.5f;
		float bottom = zy - zh * 0.5f;
		return py <= top && py >= bottom;
	}

	private void UpdateMedalsUI()
	{
		if (player1Medals != null)
		{
			for (int i = 0; i < player1Medals.Count; i++)
				player1Medals[i].SetActive(i < score1);
		}
		if (player2Medals != null)
		{
			for (int i = 0; i < player2Medals.Count; i++)
				player2Medals[i].SetActive(i < score2);
		}
	}

	private void EndMatch()
	{
		matchActive = false;
		//调用LevelManager切换关卡
		Debug.Log($"Match End. Score1: {score1}, Score2: {score2}");
		//=================新增代码=======================
		if (score1 > score2)
		{
			GlobalScoreManager.Instance.AddScore(1, 1);
		}
		else if (score1 < score2)
		{
			GlobalScoreManager.Instance.AddScore(2, 1);
		}
		else Debug.Log("draw");
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}
