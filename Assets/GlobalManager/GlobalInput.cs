using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class GlobalInput : MonoBehaviour
{
    public static GlobalInput Instance;

    [Header("Timing")]
    public float longPressThreshold = 0.1f;
    public float doubleClickThreshold = 0.3f;

    // HOLD STATE
    public bool SpaceHolding { get; private set; }
    public bool MouseHolding { get; private set; }

    public System.Action OnMouseHoldStart;
    public System.Action OnMouseUp;
    public Action OnMouseDown;

    public enum InputType
    {
        SingleClick = 0,
        LongPress = 1,
        DoubleClick = 2
    }

    public System.Action<InputType> OnSpaceAction;
    public System.Action OnSpaceDown;
    public System.Action OnSpaceUp;
    public System.Action OnSpaceHoldStart;
    public System.Action<InputType> OnMouseLeftAction;
    public Action OnEscapeDown;
    // ===== SPACE =====
    private float spacePressStart;
    private float spaceLastClickTime;
    private bool spaceIsPressing;
    private bool spaceLongPressTriggered = false;

    // ===== MOUSE =====
    private float mousePressStart;
    private float mouseLastClickTime;
    private bool mouseIsPressing;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FlushInput();
    }

    private void Update()
    {
        HandleSpace();
        HandleMouse();
        HandleEscape();
    }

    // =====================================================
    // SPACE
    // =====================================================
    private void HandleSpace()
    {
        // detect long press while pressing
        if (spaceIsPressing && !spaceLongPressTriggered)
        {
            if (Time.time - spacePressStart >= longPressThreshold)
            {
                spaceLongPressTriggered = true;

                SpaceHolding = true;
                OnSpaceHoldStart?.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceIsPressing = true;
            spacePressStart = Time.time;

            OnSpaceDown?.Invoke();
            spaceLongPressTriggered = false;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            spaceIsPressing = false;

            SpaceHolding = false;

            OnSpaceUp?.Invoke();

            float duration = Time.time - spacePressStart;

            if (duration >= longPressThreshold)
            {
                OnSpaceAction?.Invoke(InputType.LongPress);
                return;
            }

            OnSpaceAction?.Invoke(InputType.SingleClick);

            if (Time.time - spaceLastClickTime <= doubleClickThreshold)
            {
                OnSpaceAction?.Invoke(InputType.DoubleClick);
            }

            spaceLastClickTime = Time.time;
        }
    }

    // =====================================================
    // MOUSE（完全对齐 HandleSpace 逻辑）
    // =====================================================
    private void HandleMouse()
    {
        // ===== 按下 =====
        if (Input.GetMouseButtonDown(0))
        {
            mouseIsPressing = true;
            mousePressStart = Time.unscaledTime;

            OnMouseDown?.Invoke();
        }

        // ===== 长按检测 =====
        if (mouseIsPressing && !MouseHolding)
        {
            if (Time.unscaledTime - mousePressStart >= longPressThreshold)
            {
                MouseHolding = true;
                OnMouseHoldStart?.Invoke();
            }
        }

        // ===== 松开 =====
        if (Input.GetMouseButtonUp(0))
        {
            mouseIsPressing = false;

            bool wasHolding = MouseHolding;

            MouseHolding = false;

            OnMouseUp?.Invoke(); //在真正松开时触发

            float duration = Time.unscaledTime - mousePressStart;

            if (duration >= longPressThreshold)
            {
                OnMouseLeftAction?.Invoke(InputType.LongPress);
                return;
            }

            OnMouseLeftAction?.Invoke(InputType.SingleClick);

            if (Time.unscaledTime - mouseLastClickTime <= doubleClickThreshold)
            {
                OnMouseLeftAction?.Invoke(InputType.DoubleClick);
            }

            mouseLastClickTime = Time.unscaledTime;
        }
    }

    private void HandleEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapeDown?.Invoke();
        }
    }

    //reset all state
    public void FlushInput()
    {
        //reset press
        spaceIsPressing = false;
        mouseIsPressing = false;

        //reset holding
        SpaceHolding = false;
        MouseHolding = false;
        spaceLongPressTriggered = false;

        // resettime
        spacePressStart = 0f;
        mousePressStart = 0f;

        spaceLastClickTime = 0f;
        mouseLastClickTime = 0f;
    }
}