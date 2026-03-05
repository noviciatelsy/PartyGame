using UnityEngine;

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

    private void Update()
    {
        HandleSpace();
        HandleMouse();
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
    // MOUSE
    // =====================================================
    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseIsPressing = true;
            mousePressStart = Time.time;
        }

        // 检测 HoldStart
        if (mouseIsPressing && !MouseHolding)
        {
            if (Time.time - mousePressStart >= longPressThreshold)
            {
                MouseHolding = true;
                OnMouseHoldStart?.Invoke();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseIsPressing = false;

            if (MouseHolding)
            {
                MouseHolding = false;
                OnMouseUp?.Invoke();
            }

            float duration = Time.time - mousePressStart;

            if (duration >= longPressThreshold)
            {
                OnMouseLeftAction?.Invoke(InputType.LongPress);
                return;
            }

            OnMouseLeftAction?.Invoke(InputType.SingleClick);

            if (Time.time - mouseLastClickTime <= doubleClickThreshold)
            {
                OnMouseLeftAction?.Invoke(InputType.DoubleClick);
            }

            mouseLastClickTime = Time.time;
        }
    }
}