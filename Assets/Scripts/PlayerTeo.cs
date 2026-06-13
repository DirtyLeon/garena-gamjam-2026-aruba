using UnityEngine;

public class PlayerTeo : MonoBehaviour
{
    public PlayerInputHandler input;

    public Animator anim;

    private float horizontal = 0f;
    private float vertical = 0f;

    private void Update()
    {
        horizontal = input.Move.x;
        vertical = input.Move.y;

        Movement();
    }

    private void Movement()
    {
        if (horizontal < -.1f)
        {
            anim.SetBool("LeftDodge", true);
        }
        else
        {
            anim.SetBool("LeftDodge", false);
        }

        if(horizontal > .1f)
            anim.SetBool("RightDodge", true);
        else
            anim.SetBool("RightDodge", false);

        if(vertical < .1f)
            anim.SetBool("DownDodge", true);
        else
            anim.SetBool("DownDodge", false);
    }
}
