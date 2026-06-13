using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 簡易玩家控制器，只需要一個 Move Action。
/// 
/// 設置：
/// 1. 物件加上 Rigidbody
/// 2. 物件加上 Collider
/// 3. 掛上此腳本
/// 4. Move Action 拖入 InputSystem_Actions → Player → Move
/// 
/// 操控：
/// - W/S = 前進/後退
/// - A/D = 左右轉向
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CIFI_PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("移動")]
    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float deceleration = 15f;

    [Header("轉向")]
    [SerializeField] private float steerSpeed = 90f;
    [SerializeField] private float minSpeedToSteer = 1f;

    [Header("物理")]
    [SerializeField] private float lateralGrip = 8f;
    [SerializeField] private float centerOfMassY = -0.3f;

    private Rigidbody rb;
    private float throttle;
    private float steer;
    private float forwardSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        var com = rb.centerOfMass;
        com.y = centerOfMassY;
        rb.centerOfMass = com;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
    }

    private void Update()
    {
        Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        throttle = input.y;
        steer = input.x;
        UnityEngine.Debug.Log(input);
    }

    private void FixedUpdate()
    {
        forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        ApplyMovement();
        ApplySteering();
        ApplyLateralGrip();
        ClampSpeed();
    }

    private void ApplyMovement()
    {
        if (Mathf.Abs(throttle) > 0.01f)
        {
            rb.AddForce(transform.forward * throttle * moveSpeed, ForceMode.Acceleration);
        }
        else if (Mathf.Abs(forwardSpeed) > 0.5f)
        {
            // 自然減速
            rb.AddForce(-transform.forward * Mathf.Sign(forwardSpeed) * deceleration, ForceMode.Acceleration);
        }
    }

    private void ApplySteering()
    {
        if (Mathf.Abs(steer) < 0.01f) return;
        if (Mathf.Abs(forwardSpeed) < minSpeedToSteer) return;

        float dir = forwardSpeed >= 0f ? 1f : -1f;
        float angle = steer * steerSpeed * dir * Time.fixedDeltaTime;
        Quaternion rot = Quaternion.Euler(0f, angle, 0f);
        rb.MoveRotation(rb.rotation * rot);
    }

    private void ApplyLateralGrip()
    {
        Vector3 lateralVel = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        rb.AddForce(-lateralVel * lateralGrip, ForceMode.Acceleration);
    }

    private void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    /// <summary>目前速度 km/h</summary>
    public float SpeedKmh => forwardSpeed * 3.6f;
}
