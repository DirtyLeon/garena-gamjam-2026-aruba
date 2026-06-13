using UnityEngine;

public class PlayerCat : MonoBehaviour
{
    public PlayerInputHandler input;
    public Animator anim;
    public float smoothedRatio = .015f;
    private float moveX = 0f, moveY = 0f;

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        moveX = Mathf.Lerp(moveX, input.Move.x, smoothedRatio);
        moveY = Mathf.Lerp(moveY, input.Move.y, smoothedRatio);
        anim.SetFloat("catX", moveX);
        anim.SetFloat("catY", moveY);
    }

    public void OnHit()
    {
        
    }
}
