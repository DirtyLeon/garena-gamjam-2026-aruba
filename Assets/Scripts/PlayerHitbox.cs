using UnityEngine;

public class PlayerHitbox : MonoBehaviour, IHittable
{
    public void OnHit()
    {
        Debug.Log("Teo got hit.");
    }
}
