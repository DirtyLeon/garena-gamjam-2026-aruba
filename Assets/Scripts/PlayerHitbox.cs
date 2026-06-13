using UnityEngine;

public class PlayerHitbox : MonoBehaviour, IHittable
{
    public System.Action onHitAction;
    public void OnHit()
    {
        onHitAction?.Invoke();
    }
}
