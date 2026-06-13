using UnityEngine;

public class TestBulletSpawner : MonoBehaviour
{
    [SerializeField] private Transform target;
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
        var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";

        // Random height: sometimes head, sometimes body, sometimes miss (too high/low)
        float randomY = transform.position.y + Random.Range(-0.8f, 0.8f);
        bullet.transform.position = new Vector3(transform.position.x, randomY, transform.position.z);
        bullet.transform.localScale = Vector3.one * 0.2f;

        // Make it red
        bullet.GetComponent<Renderer>().material.color = Color.red;

        // Set collider as trigger
        bullet.GetComponent<SphereCollider>().isTrigger = true;

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
}
