using UnityEngine;

public class TestBulletSpawner : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float spawnDistance = 5f;

    private float _timer;

    private void Start()
    {
        // Position spawner in front of target (facing them)
        transform.position = target.position + target.forward * spawnDistance;
        transform.position = new Vector3(transform.position.x, target.position.y + 1.2f, transform.position.z);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnBullet();
        }
    }

    private void SpawnBullet()
    {
        var bullet = Instantiate(bulletPrefab);
        bullet.transform.rotation = Quaternion.Euler(0, -90f, 0);

        // Random height offset
        float randomY = transform.position.y + Random.Range(-0.8f, 0.8f);
        bullet.transform.position = new Vector3(transform.position.x, randomY, transform.position.z);

        // Add sphere collider
        var col = bullet.AddComponent<SphereCollider>();
        col.radius = 0.3f;
        col.isTrigger = true;

        // Add rigidbody for movement
        var rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;

        // Shoot horizontally toward target
        var dir = (target.position - transform.position);
        dir.y = 0;
        rb.linearVelocity = dir.normalized * bulletSpeed;

        // Add hit detection + auto destroy
        bullet.AddComponent<TestBullet>();
    }
}

public class TestBullet : MonoBehaviour
{
    private bool _hit;

    private void OnTriggerEnter(Collider other)
    {
        if (_hit) return;
        if (!other.name.Contains("Hitbox")) return;
        _hit = true;
        Debug.Log($"<color=red>命中！</color> 碰到: {other.name}");
        Destroy(gameObject);
    }

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void OnDestroy()
    {
        if (!_hit)
            Debug.Log("<color=green>未命中！子彈飛過去了</color>");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var col = GetComponent<SphereCollider>();
        if (col != null)
            Gizmos.DrawWireSphere(transform.TransformPoint(col.center), col.radius * transform.lossyScale.x);
    }
}
