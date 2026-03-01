using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("移动设置")]
    public float speed = 5f;
    [Tooltip("输入判定阈值，避免手柄摇杆轻微漂移导致原地播放跑步动画")]
    [SerializeField] private float moveThreshold = 0.01f;

    // 性能优化：提前缓存 Animator 参数的 Hash 值
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Rigidbody2D rb;
    private Animator animator;
    private InputAction moveAction;
    private Vector2 inputDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // 全局获取 Input System 的 Move 动作 (Unity 1.7.0+ 最新语法)
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void OnEnable()
    {
        moveAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
    }

    void Update()
    {
        if (moveAction == null || animator == null) return;

        // 1. 读取输入向量
        inputDir = moveAction.ReadValue<Vector2>();
        
        // 2. 计算当前输入的速度大小 (magnitude 通常在 0 到 1 之间)
        float currentSpeed = inputDir.magnitude;

        // 3. 数据驱动动画：将速度值直接传递给 Animator 的混合树
        // 增加阈值保护：如果输入极其微小，强制设为 0，确保完美停在 Idle
        if (currentSpeed < moveThreshold)
        {
            currentSpeed = 0f;
        }
        animator.SetFloat(SpeedHash, currentSpeed);

        // 4. 据玩家的移动方向，自动翻转角色的朝向（脸朝左还是脸朝右）。
        if (inputDir.x > moveThreshold)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (inputDir.x < -moveThreshold)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        // 5. 物理移动：必须放在 FixedUpdate 中处理，配合 Rigidbody2D 保证丝滑不抖动
        if (inputDir.sqrMagnitude > moveThreshold * moveThreshold)
        {
            rb.MovePosition(rb.position + inputDir * (speed * Time.fixedDeltaTime));
        }
    }
}
