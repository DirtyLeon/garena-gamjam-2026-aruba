using System.Collections.Generic;
using UnityEngine;

public class PlayerTeo : MonoBehaviour
{
    public static System.Action onPlayerFailed;

    public PlayerInputHandler input;

    public Animator anim;

    public List<PlayerHitbox> hitboxes = new List<PlayerHitbox>();

    private float horizontal = 0f;
    private float vertical = 0f;
    private const int MAX_HEALTH = 2;
    [ReadOnly] private int currentHealth = 2;

    private void OnEnable()
    {
        MatrixGameManager.onTeoInit += Init;
        foreach(var hitbox in hitboxes)
            hitbox.onHitAction += OnHit;
    }

    void OnDisable()
    {
        MatrixGameManager.onTeoInit -= Init;
        foreach(var hitbox in hitboxes)
            hitbox.onHitAction -= OnHit;
    }

    private void Update()
    {
        horizontal = input.Move.x;
        vertical = input.Move.y;

        Movement();
    }

    private void Movement()
    {
        if (horizontal < -.1f)
            anim.SetBool("LeftDodge", true);
        else
            anim.SetBool("LeftDodge", false);

        if(horizontal > .1f)
            anim.SetBool("RightDodge", true);
        else
            anim.SetBool("RightDodge", false);

        if(vertical < -.1f)
            anim.SetBool("DownDodge", true);
        else
            anim.SetBool("DownDodge", false);
    }

    public void Init()
    {
        currentHealth = MAX_HEALTH;
    }

    public void OnHit()
    {
        currentHealth -= 1;
        if(currentHealth <= 0)
            onPlayerFailed?.Invoke();
    }
}
