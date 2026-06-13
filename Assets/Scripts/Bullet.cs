using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody rb;
    public float flyDuration = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        var hittable = collision.gameObject.GetComponent<IHittable>();
        if(hittable == null)
            return;

        Debug.Log(gameObject.name + " hits " + collision.gameObject.name);
        OnHit();
        hittable.OnHit();

    }

    public void ShootAt(Transform target) => ShootAt(target.position);

    public void ShootAt(Vector3 targetPosition)
    {
        if (rb == null)
        {
            Debug.LogError("Bullet Rigidbody is not assigned.");
            return;
        }

        Vector3 displacement = targetPosition - rb.position;

        if (flyDuration <= 0f)
        {
            Debug.LogWarning("flyDuration must be greater than 0.");
            return;
        }
        transform.LookAt(targetPosition);

        Vector3 requiredVelocity = displacement / flyDuration;
        rb.linearVelocity = requiredVelocity;
    }

    public void OnHit()
    {
        Destroy(gameObject);
    }
}