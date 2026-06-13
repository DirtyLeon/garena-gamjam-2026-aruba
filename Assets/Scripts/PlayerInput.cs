using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputHandler : MonoBehaviour
{
    public InputActionAsset inputActionMap;

    [SerializeField] public InputActionReference inputMove;
    [SerializeField] public InputActionReference inputLook;
    [SerializeField] public InputActionReference inputJump;
    [SerializeField] public InputActionReference inputSprint;

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
