using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    public InputActionAsset inputActionMap;

    [SerializeField] private InputActionReference inputMove;
    [SerializeField] private InputActionReference inputLook;
    [SerializeField] private InputActionReference inputJump;
    [SerializeField] private InputActionReference inputSprint;

    private void OnEnable()
    {
        inputActionMap.Enable();
    }

    private void OnDisable()
    {
        inputActionMap.Disable();
    }

    
}
