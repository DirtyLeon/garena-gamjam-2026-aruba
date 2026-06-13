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
        fill.color = Color.red;
        canvasGroup.DOFade(1, .3f);
        await fill.rectTransform.DOScale(1f, duration).AsyncWaitForCompletion();
        
        //canvasGroup.alpha = 0f;
        canvasGroup.DOFade(0, .5f);
        await fill.rectTransform.DOPunchScale(1 * Vector3.one, .5f).AsyncWaitForCompletion();
    }

    public void HighlightGreen()
    {
        fill.color = Color.green;
    }
}
