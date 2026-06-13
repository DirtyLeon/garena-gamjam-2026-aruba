using System.Collections;
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

    public void FinalArrayFire(float _duration, float _gap, List<Transform> _list)
    {
        flyDuration = _duration;
        fireGap = _gap;
        StartCoroutine(FinalArrayFireCoroutine(_list));
    }

    private IEnumerator FinalArrayFireCoroutine(List<Transform> targetList)
    {
        for(int i = 0; i < targetList.Count; i++)
        {
            var firesquad = fireSquadList[i % fireSquadList.Count];
            firesquad.Fire(targetList[i].position, flyDuration);
            yield return new WaitForSeconds(fireGap);
        }
    }
}
