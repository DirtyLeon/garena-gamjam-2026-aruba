using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家移動控制器（需搭配 CharacterController 使用）
/// 功能：WASD 移動、滑鼠視角、跳躍、衝刺
/// 
/// 設置步驟：
/// 1. 玩家物件加上 CharacterController 元件
/// 2. 掛上此腳本
/// 3. 建立一個子物件作為 Camera Holder，將 Camera 放在裡面
/// 4. 把 Camera Holder 拖到 cameraTransform 欄位
/// 5. 設定 Input Action References（從 InputSystem_Actions 拖入）
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("移動參數")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("視角參數")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("地面偵測")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask = ~0;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch;
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        // 鎖定滑鼠
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveAction.action.Enable();
        // lookAction 保持 disable
        jumpAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        moveAction.action.Disable();
        // lookAction 未啟用，不需 disable
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }

    private void Update()
    {
        GroundCheck();
        // HandleLook() 已停用，滑鼠不影響視角
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void GroundCheck()
    {
        // 使用 CharacterController 內建的 isGrounded + 額外 raycast
        isGrounded = controller.isGrounded;
        if (!isGrounded)
        {
            isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance + controller.skinWidth,
                groundMask
            );
        }
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        // 水平旋轉（轉身體）
        float yaw = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up, yaw);

        // 垂直旋轉（轉攝影機）
        if (cameraTransform != null)
        {
            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        bool isSprinting = sprintAction.action.IsPressed();

        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // 根據玩家面向方向移動
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (isGrounded && jumpAction.action.WasPressedThisFrame())
        {
            // v = sqrt(2 * g * h)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // 小的負值保持貼地
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
