using UnityEngine;

public class PlayerNeo : MonoBehaviour, IHittable
{
    public void OnHit()
    {
        Debug.Log("Neo is hit.");
    }
}
