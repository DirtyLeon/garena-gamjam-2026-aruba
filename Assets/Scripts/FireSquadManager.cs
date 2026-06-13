using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireSquadManager : MonoBehaviour
{
    [Header("References")]
    public Bullet bulletPrefab;
    public List<FireSquad> fireSquadList = new List<FireSquad>();
    public List<Transform> targetList = new List<Transform>();

    [Header("Parameters")]
    public float flyDuration = 1f;
    public float fireGap = 1f;

    private Queue<int> aimmedList = new Queue<int>();

    public void AllFire() => AllFireCoroutine().Forget();

    public async UniTask AllFireCoroutine()
    {
        aimmedList.Clear();
        foreach(var firesquad in fireSquadList)
        {
            var targetIndex = Random.Range(0, targetList.Count);
            while (aimmedList.Contains(targetIndex))
            {
                targetIndex = (targetIndex + 1) % targetList.Count;
            }

            aimmedList.Enqueue(targetIndex);
            if(aimmedList.Count > 2)
                aimmedList.Dequeue();

            firesquad.Fire(targetList[targetIndex].position, flyDuration);

            await UniTask.WaitForSeconds(fireGap);
        }
    }
}
