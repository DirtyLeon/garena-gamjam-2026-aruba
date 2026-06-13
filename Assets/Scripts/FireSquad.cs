using UnityEngine;

public class FireSquad : MonoBehaviour
{
    public Animator anim;
    public Transform firePoint;
    public Bullet bulletPrefab;
    public HitIndicator indicator;

    public void Fire(Vector3 _targetPosition, float _flyDuration)
    {
        var instancedIndicator = Instantiate(indicator);
        instancedIndicator.transform.position = _targetPosition;
        anim.SetTrigger("Shoot");
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.flyDuration = _flyDuration;
        bullet.SetIndicator(instancedIndicator);
        bullet.ShootAt(_targetPosition);
        instancedIndicator.HitIncomingInvoke(_flyDuration);
    }
}
