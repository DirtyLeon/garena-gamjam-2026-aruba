using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HitIndicator : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image fill;

    private CancellationTokenSource indicateCancel;

    private void Awake()
    {
        canvasGroup.alpha = 0;
    }

    public void HitIncomingInvoke(float duration) => HitIncoming(duration).Forget();
    
    private async UniTask HitIncoming(float duration)
    {
        
        if(indicateCancel.Token != null)
        {
            indicateCancel.Cancel();
            indicateCancel.Dispose();
        }

        indicateCancel = new CancellationTokenSource();

        fill.rectTransform.localScale = Vector3.zero;
        canvasGroup.DOFade(1, .3f).ToUniTask().Forget();
        await fill.rectTransform.DOScale(1f, duration).ToUniTask(cancellationToken: indicateCancel.Token);
        canvasGroup.alpha = 0f;
    }
}
