using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var sphere = GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            return;
        }

        var capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + capsule.center, capsule.radius);
        }
    }
}
