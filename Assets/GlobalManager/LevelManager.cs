using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static bool firstGameEnter = true;
    public static LevelManager Instance;
    private bool isLoadingLevel = false;
    private bool skipNextTransitionIn = true;

    [Header("全局BGM")]
    public AudioClip bgm1;
    public AudioClip bgm2;
    public AudioClip bgm3;
    [Header("ZZZCredits专属BGM")]
    public AudioClip creditsBgm;
    [Header("Game Over Scene")]
    public string gameOverSceneName = "GameEnd";
    private AudioSource bgmSource;
    public string mainMenuScene = "AAABeginScene";
    private bool bgmChosen = false;

    private List<string> levelSequence = new List<string>();
    private int sequenceIndex = 0;

    [System.Serializable]
    public class LevelData
    {
        public string sceneName;
        public int weight = 1;
    }

    [Header("LevelList")]
    public List<LevelData> levels = new List<LevelData>();
    public bool avoidRepeat = true;

    private string lastLevelLoaded = "";

    public Animator Transition;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        GenerateLevelSequence(12);

        SceneManager.sceneLoaded += OnSceneLoaded;
        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingLevel = false;
        Time.timeScale = 1f;
        StartCoroutine(HandleSceneTransition());
        // 如果进入最终结算场景，停止所有音源并返回（由结算场景自行播放音频）
        if (!string.IsNullOrEmpty(gameOverSceneName) && scene.name == gameOverSceneName)
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
                bgmChosen = false;
            }

            AudioSource[] all = FindObjectsOfType<AudioSource>();
            foreach (var a in all)
            {
                try { a.Stop(); } catch { }
            }
            return;
        }
        // 主界面：停止并重置
        if (scene.name == mainMenuScene)
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
                bgmChosen = false;
            }
            firstGameEnter = true;
            return;
        }
        if (scene.name == "ZZZCredits")
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
                if (creditsBgm != null)
                {
                    bgmSource.clip = creditsBgm;
                    bgmSource.Play();
                }
                bgmChosen = false; // 确保普通BGM下次重新选择
            }
            return;
        }
        if (bgmSource != null)
        {
            AudioClip[] bgms = new AudioClip[] { bgm1, bgm2, bgm3 };
            var validList = new System.Collections.Generic.List<AudioClip>();
            foreach (var b in bgms) if (b != null) validList.Add(b);
            if (validList.Count > 0 && (bgmSource.clip == null || bgmSource.clip == creditsBgm || !bgmChosen || !bgmSource.isPlaying))
            {
                int idx = Random.Range(0, validList.Count);
                bgmSource.clip = validList[idx];
                bgmSource.Play();
                bgmChosen = true;
                StartCoroutine(WaitForBgmEnd());
            }
            else if (!bgmSource.isPlaying && bgmSource.clip != null)
            {
                bgmSource.Play();
            }
        }
    }

    private IEnumerator WaitForBgmEnd()
    {
        if (bgmSource == null) yield break;
        while (bgmSource.isPlaying || bgmSource.clip == null)
        {
            yield return null;
        }
        AudioClip[] bgms = new AudioClip[] { bgm1, bgm2, bgm3 };
        var validList = new System.Collections.Generic.List<AudioClip>();
        foreach (var b in bgms) if (b != null) validList.Add(b);
        if (validList.Count > 0)
        {
            int idx = Random.Range(0, validList.Count);
            bgmSource.clip = validList[idx];
            bgmSource.Play();
            StartCoroutine(WaitForBgmEnd());
        }
    }

    private IEnumerator HandleSceneTransition()
    {
        yield return null;

        GameObject transitionObj = GameObject.FindGameObjectWithTag("LevelTransition");

        if (transitionObj == null)
        {
            Debug.LogWarning("LevelTransition not found. Transition animation skipped.");
            Transition = null;
            yield break;
        }

        Transition = transitionObj.GetComponentInChildren<Animator>();

        if (Transition == null)
        {
            Debug.LogWarning("LevelTransition Animator missing. Transition animation skipped.");
        }
        else if (skipNextTransitionIn)
        {
            skipNextTransitionIn = false;
            Transition.enabled = false;
            var cg = transitionObj.GetComponentInChildren<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
            Transition.enabled = true;
        }
    }

    public void NextLevel()
    {
        if (isLoadingLevel) return;   // 闃叉閲嶅鍔犺浇

        isLoadingLevel = true;

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager: 没有可用的关卡");
            return;
        }

        //string nextLevel = GetWeightedRandomLevel();
        string nextLevel = GetNextSequenceLevel();

        if (string.IsNullOrEmpty(nextLevel))
        {
            Debug.LogError("LevelManager: 没有可用的关卡");
            return;
        }

        lastLevelLoaded = nextLevel;
        //if(Transition!=null)
        //{
        //    Transition.SetTrigger("Start");
        //}
        StartCoroutine(LoadLevelAfterTransition(nextLevel));
        //SceneManager.LoadScene(nextLevel);
    }
    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LevelManager.LoadLevel: sceneName is null or empty");
            return;
        }

        lastLevelLoaded = sceneName;
        StartCoroutine(LoadLevelAfterTransition(sceneName));
    }

    private IEnumerator LoadLevelAfterTransition(string sceneName)
    {
        // 判断是否第一次从主菜单进入游戏场景
        bool skipTransition = false;
        if (firstGameEnter && SceneManager.GetActiveScene().name == mainMenuScene)
        {
            skipTransition = true;
            firstGameEnter = false;
        }

        if (!skipTransition && Transition != null)
        {
            // 如果游戏已经结束，则跳过转场动画
            if (GlobalScoreManager.Instance == null || !GlobalScoreManager.Instance.isGameOver)
            {
                var cg = Transition.GetComponentInChildren<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
                Transition.SetTrigger("Start");
                yield return new WaitForSecondsRealtime(0.6f);
            }
            else
            {
                yield return new WaitForSecondsRealtime(1.2f);
            }
        }
        SceneManager.LoadScene(sceneName);
        //可能存在暂停状态
        Time.timeScale = 1f;
    }

    // ============================
    // 权重随机关卡选择
    // ============================
    private string GetWeightedRandomLevel()
    {
        List<LevelData> validLevels = new List<LevelData>();

        foreach (var level in levels)
        {
            if (avoidRepeat && level.sceneName == lastLevelLoaded)
                continue;

            if (level.weight > 0)
                validLevels.Add(level);
        }

        if (validLevels.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (var level in validLevels)
        {
            totalWeight += level.weight;
        }

        int randomValue = Random.Range(0, totalWeight);

        int cumulative = 0;

        foreach (var level in validLevels)
        {
            cumulative += level.weight;

            if (randomValue < cumulative)
            {
                return level.sceneName;
            }
        }

        return validLevels[0].sceneName;
    }

    void GenerateLevelSequence(int count)
    {
        levelSequence.Clear();

        List<LevelData> pool = new List<LevelData>(levels);

        int limit = Mathf.Min(count, pool.Count);

        for (int i = 0; i < limit; i++)
        {
            int totalWeight = 0;

            foreach (var l in pool)
            {
                if (l.weight > 0)
                    totalWeight += l.weight;
            }

            int rand = Random.Range(0, totalWeight);

            int cumulative = 0;

            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].weight;

                if (rand < cumulative)
                {
                    levelSequence.Add(pool[j].sceneName);
                    pool.RemoveAt(j);   // 关键：移除，避免重复
                    break;
                }
            }
        }

        sequenceIndex = 0;
    }

    string GetNextSequenceLevel()
    {
        if (sequenceIndex >= levelSequence.Count)
        {
            Debug.Log("Level sequence finished.");
            return null;
        }

        string level = levelSequence[sequenceIndex];
        sequenceIndex++;

        return level;
    }
}