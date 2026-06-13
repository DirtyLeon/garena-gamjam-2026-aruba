using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HitIndicator : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image fill;

    private void Awake()
    {
        canvasGroup.alpha = 0;
    }

    public void HitIncomingInvoke(float duration) => HitIncoming(duration).Forget();
    
    private async UniTask HitIncoming(float duration)
    {

        fill.rectTransform.localScale = Vector3.zero;
        canvasGroup.DOFade(1, .3f).ToUniTask().Forget();
        await fill.rectTransform.DOScale(1f, duration);
        canvasGroup.alpha = 0f;
    }
}
