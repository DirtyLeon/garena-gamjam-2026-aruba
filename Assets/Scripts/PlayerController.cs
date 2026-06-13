using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private const string Idle = "root|Idle";
    private const string DodgeLeft = "root|dodge_Left";
    private const string DodgeRight = "root|dodge_Right";
    private const string DodgeDown = "root|DownDodge";

    private string _currentState;

    private void Start()
    {
        _currentState = Idle;
    }

    private void Update()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame) PlayState(DodgeLeft);
        else if (Keyboard.current.dKey.wasPressedThisFrame) PlayState(DodgeRight);
        else if (Keyboard.current.sKey.wasPressedThisFrame) PlayState(DodgeDown);
        else CheckAutoIdle();
    }

    private void CheckAutoIdle()
    {
        if (_currentState == Idle) return;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 1f)
            PlayState(Idle);
    }

    private void PlayState(string stateName)
    {
        _currentState = stateName;
        animator.Play(stateName, 0, 0f);
    }

    public void ForceIdle() => PlayState(Idle);
}
