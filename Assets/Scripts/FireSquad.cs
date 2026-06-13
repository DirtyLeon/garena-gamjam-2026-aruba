using UnityEngine;

public class FireSquad : MonoBehaviour
{
    public Animator anim;
    public Transform firePoint;
    public Bullet bulletPrefab;
    public HitIndicator indicator;

    public void Fire(Vector3 _targetPosition, float _flyDuration)
    {
        indicator.transform.position = _targetPosition;
        anim.SetTrigger("Shoot");
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.flyDuration = _flyDuration;
        bullet.ShootAt(_targetPosition);
        indicator.HitIncomingInvoke(_flyDuration);
    }
}
