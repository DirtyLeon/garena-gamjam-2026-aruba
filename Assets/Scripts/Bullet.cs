using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody rb;
    public float flyDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        var hittable = other.GetComponent<IHittable>();
        if(hittable == null)
            return;

        Debug.Log(gameObject.name + " hits " + other.gameObject.name);
        OnHit();
        hittable.OnHit();
    }

    public void ShootAt(Transform target)
    {
        // Optional: Position prediction fix.
        ShootAt(target.position);
    }

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

        StartCoroutine(SelfDestructCoroutine());
    }

    public void OnHit()
    {
        Destroy(gameObject);
    }

    private IEnumerator SelfDestructCoroutine()
    {
        yield return new WaitForSeconds(10f);
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }
}