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

    [Header("»´æ÷BGM")]
    public AudioClip bgm1;
    public AudioClip bgm2;
    public AudioClip bgm3;
    private AudioSource bgmSource;
    public string mainMenuScene = "AAABeginScene";
    private bool bgmChosen = false;

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
        if (scene.name != mainMenuScene)
        {
            if (bgmSource != null)
            {
                if (!bgmChosen)
                {
                    AudioClip[] bgms = new AudioClip[] { bgm1, bgm2, bgm3 };
                    var validList = new System.Collections.Generic.List<AudioClip>();
                    foreach (var b in bgms) if (b != null) validList.Add(b);
                    if (validList.Count > 0)
                    {
                        int idx = Random.Range(0, validList.Count);
                        bgmSource.clip = validList[idx];
                        bgmSource.Play();
                        bgmChosen = true;
                        StartCoroutine(WaitForBgmEnd());
                    }
                }
                else if (!bgmSource.isPlaying && bgmSource.clip != null)
                {
                    bgmSource.Play();
                }
            }
        }
        else
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
                bgmChosen = false;
            }
            firstGameEnter = true;
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
        if (isLoadingLevel) return;   // Èò≤Ê≠¢ÈáçÂ§çÂä†ËΩΩ

        isLoadingLevel = true;

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager: √ªÔøΩÔøΩÔøΩÔøΩÔøΩ√πÿøÔøΩÔøΩ–±ÔøΩÔøΩÔøΩ");
            return;
        }

        string nextLevel = GetWeightedRandomLevel();

        if (string.IsNullOrEmpty(nextLevel))
        {
            Debug.LogError("LevelManager: √ªÔøΩ–øÔøΩÔøΩ√πÿøÔøΩÔøΩÔøΩ");
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
        // ≈–∂œ «∑Òµ⁄“ª¥Œ¥”÷˜≤Àµ•Ω¯»Î”Œœ∑≥°æ∞
        bool skipTransition = false;
        if (firstGameEnter && SceneManager.GetActiveScene().name == mainMenuScene)
        {
            skipTransition = true;
            firstGameEnter = false;
        }

        if (!skipTransition && Transition != null)
        {
            var cg = Transition.GetComponentInChildren<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
            Transition.SetTrigger("Start");
            yield return new WaitForSecondsRealtime(0.6f);
        }
        SceneManager.LoadScene(sceneName);

        //ÂèØËÉΩ‰ºöÊúâÈî?
        Time.timeScale = 1f;
    }

    // ============================
    // ÔøΩÔøΩ»®ÔøΩÔøΩÔøΩÔøΩÔøΩÔøΩÔøΩÔøΩﬂºÔø?
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
}