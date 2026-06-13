using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
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
            var up = transform.up * (capsule.height * 0.5f - capsule.radius);
            Gizmos.DrawWireSphere(center + up, capsule.radius);
            Gizmos.DrawWireSphere(center - up, capsule.radius);
            Gizmos.DrawLine(center + up + transform.right * capsule.radius, center - up + transform.right * capsule.radius);
            Gizmos.DrawLine(center + up - transform.right * capsule.radius, center - up - transform.right * capsule.radius);
            Gizmos.DrawLine(center + up + transform.forward * capsule.radius, center - up + transform.forward * capsule.radius);
            Gizmos.DrawLine(center + up - transform.forward * capsule.radius, center - up - transform.forward * capsule.radius);
        }
    }
}
