using UnityEngine;

public class HittableHitbox : MonoBehaviour, IHittable
{
    public UnityEngine.Events.UnityEvent onHitEvent;

    public void OnHit()
    {
        onHitEvent?.Invoke();
    }
}
