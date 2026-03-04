using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [System.Serializable]
    public class LevelData
    {
        public string sceneName;
        public int weight = 1;
    }

    [Header("关卡列表（填写场景名称 + 权重）")]
    public List<LevelData> levels = new List<LevelData>();

    [Header("是否避免连续重复关卡")]
    public bool avoidRepeat = true;

    private string lastLevelLoaded = "";

    public Animator Transition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(HandleSceneTransition());
    }

    private IEnumerator HandleSceneTransition()
    {
        yield return null; 
        GameObject transitionObj = GameObject.FindGameObjectWithTag("LevelTransition");

        if (transitionObj == null)
        {
            Debug.LogWarning("没有找到 LevelTransition 物体");
            yield break;
        }
        Transition = transitionObj.GetComponentInChildren<Animator>();

        if (Transition == null)
        {
            Debug.LogWarning("LevelTransition 下没有 Animator");
            yield break;
        }

    }

    // ============================
    // 对外调用函数
    // ============================
    public void NextLevel()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager: 没有配置关卡列表！");
            return;
        }

        string nextLevel = GetWeightedRandomLevel();

        if (string.IsNullOrEmpty(nextLevel))
        {
            Debug.LogError("LevelManager: 没有可用关卡！");
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

    private IEnumerator LoadLevelAfterTransition(string sceneName)
    {
        if (Transition != null)
        {
            Transition.SetTrigger("Start");
            yield return new WaitForSeconds(0.6f);
        }
        SceneManager.LoadScene(sceneName);
    }
    // ============================
    // 加权随机核心逻辑
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