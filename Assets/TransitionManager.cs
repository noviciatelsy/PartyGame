using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Grid Settings")]
    public GameObject diamondPrefab;
    public int columns = 20;
    public int rows = 12;
    public float spacing = 100f;

    [Header("Animation Settings")]
    public float expandDuration = 0.5f;
    public float shrinkDuration = 0.5f;
    public float waitTime = 0.3f;

    private List<RectTransform> diamonds = new List<RectTransform>();
    private Canvas canvas;
    private RectTransform gridRoot;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateCanvas();
        GenerateGrid();
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("TransitionCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        gridRoot = new GameObject("DiamondGrid").AddComponent<RectTransform>();
        gridRoot.SetParent(canvas.transform);
        gridRoot.anchorMin = Vector2.zero;
        gridRoot.anchorMax = Vector2.one;
        gridRoot.offsetMin = Vector2.zero;
        gridRoot.offsetMax = Vector2.zero;
    }

    void GenerateGrid()
    {
        diamonds.Clear();

        float startX = -columns / 2f * spacing;
        float startY = -rows / 2f * spacing;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject d = Instantiate(diamondPrefab, gridRoot);
                RectTransform rt = d.GetComponent<RectTransform>();

                float offset = (y % 2 == 0) ? 0 : spacing / 2f;

                rt.anchoredPosition = new Vector2(
                    startX + x * spacing + offset,
                    startY + y * spacing
                );

                rt.localScale = Vector3.zero;

                diamonds.Add(rt);
            }
        }
    }

    public void LoadSceneWithTransition(string targetScene)
    {
        StartCoroutine(TransitionRoutine(targetScene));
    }

    IEnumerator TransitionRoutine(string targetScene)
    {
        yield return StartCoroutine(ScaleDiamonds(Vector3.one, expandDuration));

        SceneManager.LoadScene(targetScene);

        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(ScaleDiamonds(Vector3.zero, shrinkDuration));
    }

    IEnumerator ScaleDiamonds(Vector3 target, float duration)
    {
        float time = 0f;

        List<Vector3> startScales = new List<Vector3>();
        foreach (var d in diamonds)
            startScales.Add(d.localScale);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            for (int i = 0; i < diamonds.Count; i++)
            {
                diamonds[i].localScale = Vector3.Lerp(startScales[i], target, t);
            }

            yield return null;
        }

        foreach (var d in diamonds)
            d.localScale = target;
    }
}
