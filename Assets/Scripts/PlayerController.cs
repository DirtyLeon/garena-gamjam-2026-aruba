using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation State Names")]
    [SerializeField] private string idleStateName   = "Idle";
    [SerializeField] private string dodgeAStateName = "Dodge_A";
    [SerializeField] private string dodgeSStateName = "Dodge_S";
    [SerializeField] private string dodgeDStateName = "Dodge_D";

    private string _currentState = "";

    private void Start()
    {
        PlayClip(idleStateName);
    }

    private void Update()
    {
        HandleInput();
        CheckAutoIdle();
    }

    private void HandleInput()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame) PlayClip(dodgeAStateName);
        if (Keyboard.current.sKey.wasPressedThisFrame) PlayClip(dodgeSStateName);
        if (Keyboard.current.dKey.wasPressedThisFrame) PlayClip(dodgeDStateName);
    }

    private void CheckAutoIdle()
    {
        if (_currentState == idleStateName) return;

        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(_currentState) && info.normalizedTime >= 1f)
            PlayClip(idleStateName);
    }

    private void PlayClip(string stateName)
    {
        _currentState = stateName;
        animator.Play(stateName, 0, 0f);
    }

    public void ForceIdle() => PlayClip(idleStateName);
    public string CurrentState => _currentState;
}