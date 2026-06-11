using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    public InputActionAsset inputActionMap;

    [SerializeField] private InputActionReference inputMove;
    [SerializeField] private InputActionReference inputLook;
    [SerializeField] private InputActionReference inputJump;
    [SerializeField] private InputActionReference inputSprint;

    [ReadOnly] public Vector2 Move;
    [ReadOnly] public Vector2 Look;

    private void OnEnable()
    {
        inputActionMap.Enable();
    }

    private void OnDisable()
    {
        inputActionMap.Disable();
    }

    private void Update()
    {
        Move = inputMove.action.ReadValue<Vector2>();
        Look = inputLook.action.ReadValue<Vector2>();
    }
}
