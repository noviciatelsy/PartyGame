using UnityEngine;

public class GlobalInput : MonoBehaviour
{
    public static GlobalInput Instance;

    [Header("Timing")]
    public float longPressThreshold = 0.4f;
    public float doubleClickThreshold = 0.3f;

    public enum InputType
    {
        SingleClick = 0,
        LongPress = 1,
        DoubleClick = 2
    }

    public System.Action<InputType> OnSpaceAction;
    public System.Action<InputType> OnMouseLeftAction;

    // ===== SPACE =====
    private float spacePressStart;
    private float spaceLastClickTime;
    private bool spaceIsPressing;

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceIsPressing = true;
            spacePressStart = Time.time;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            spaceIsPressing = false;

            float duration = Time.time - spacePressStart;

            // 长按优先
            if (duration >= longPressThreshold)
            {
                OnSpaceAction?.Invoke(InputType.LongPress);
                return;
            }

            // ===== 关键：立刻触发单击 =====
            OnSpaceAction?.Invoke(InputType.SingleClick);

            // 判断是否双击
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

        if (Input.GetMouseButtonUp(0))
        {
            mouseIsPressing = false;

            float duration = Time.time - mousePressStart;

            if (duration >= longPressThreshold)
            {
                OnMouseLeftAction?.Invoke(InputType.LongPress);
                return;
            }

            // ===== 立刻单击 =====
            OnMouseLeftAction?.Invoke(InputType.SingleClick);

            // 双击检测
            if (Time.time - mouseLastClickTime <= doubleClickThreshold)
            {
                OnMouseLeftAction?.Invoke(InputType.DoubleClick);
            }

            mouseLastClickTime = Time.time;
        }
    }
}