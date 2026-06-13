using UnityEngine;

public class FireSquad : MonoBehaviour
{
    public Animator anim;
    public Transform firePoint;
    public Bullet bulletPrefab;

    public void Fire(Vector3 _targetPosition, float _flyDuration)
    {
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.flyDuration = _flyDuration;
        bullet.ShootAt(_targetPosition);
    }
}
