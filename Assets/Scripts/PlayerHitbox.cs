using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var bullet = other.GetComponentInParent<TestBullet>();
        if (bullet == null) bullet = other.GetComponent<TestBullet>();
        if (bullet == null) return;

        bullet.Hit(gameObject.name);
    }

    private void OnDrawGizmos()
    {
        var sphere = GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(sphere.center), sphere.radius * transform.lossyScale.x);
            return;
        }

        var capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            Gizmos.color = Color.yellow;
            var center = transform.TransformPoint(capsule.center);
            Gizmos.DrawWireSphere(center, capsule.radius);
        }
    }
}
