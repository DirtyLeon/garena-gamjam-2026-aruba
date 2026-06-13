using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public enum GameScene
{
    Scene_1,
    Scene_2,
    Scene_3,
    Scene_4
}

/// <summary>
/// 場景載入 Singleton。
/// 負責轉場動畫（Fade + Timeline）、轉場 SFX 的播放與停止。
/// </summary>
public class LoadSceneSystem : MonoBehaviour
{
    private static LoadSceneSystem instance;
    public static LoadSceneSystem Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("[LoadSceneSystem] Instance 尚未初始化！請確認 Resources/LoadSceneSystem prefab 存在。");
            }
            return instance;
        }
    }

  

    // --- Events ---
    public event Action onLoadSceneStart;
    public event Action onLoadSceneCompleted;

    [Header("Transition SFX")]
    [SerializeField] private AudioClip transitionSFXClip; // 3 sec, loop
    [SerializeField] private AudioSource transitionAudioSource;

    [Header("Fade Material")]
    [SerializeField] private Material fadeMaterial; // SG_FadInOut.mat

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private static readonly int OpacityID = Shader.PropertyToID("_Opacity");

    private bool isLoading;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 外部呼叫：載入指定場景（by enum）
    /// </summary>
    public void LoadScene(GameScene scene)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneRoutine(scene.ToString()));
    }

    /// <summary>
    /// 外部呼叫：載入指定場景（by name）
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    /// <summary>
    /// 外部呼叫：載入指定場景（by build index）
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return StartCoroutine(LoadSceneInternal(() => SceneManager.LoadSceneAsync(sceneName)));
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        yield return StartCoroutine(LoadSceneInternal(() => SceneManager.LoadSceneAsync(sceneIndex)));
    }

    private IEnumerator LoadSceneInternal(Func<AsyncOperation> loadAction)
    {
        isLoading = true;

        // ========== 階段：準備 Load Scene ==========
        // Fade Out（遮住畫面）: Opacity 0 → 1
        yield return StartCoroutine(FadeMaterialOpacity(0f, 1f, fadeDuration));

        onLoadSceneStart?.Invoke();

        // ========== 階段：開始 Load Scene ==========
        // 開始非同步載入
        AsyncOperation asyncLoad = loadAction.Invoke();
        asyncLoad.allowSceneActivation = false;

        // 播放 transition SFX（loop）
        PlayTransitionSFX();

        // ========== 階段：等待直到讀取完畢 && SFX 最後一拍 ==========
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 等待 SFX 播完當前這一輪（對齊最後一拍）
        yield return StartCoroutine(WaitForSFXLastBeat());

        // ========== 階段：Load Scene Completed ==========
        // 停止 SFX
        StopTransitionSFX();

        // 啟動場景
        asyncLoad.allowSceneActivation = true;

        // 等待場景真正載入完成
        while (!asyncLoad.isDone)
            yield return null;

        // Fade In（顯示新場景）: Opacity 1 → 0
        yield return StartCoroutine(FadeMaterialOpacity(1f, 0f, fadeDuration));
        
        // 新場景已載入，播放 FadeIn_Matrix 底下的 Timeline
        yield return StartCoroutine(PlayFadeInTimeline());


        onLoadSceneCompleted?.Invoke();

        isLoading = false;
    }

    // --- Fade Material 控制 ---

    /// <summary>
    /// Lerp SG_FadInOut.mat 的 surfaceInput.Opacity
    /// </summary>
    private IEnumerator FadeMaterialOpacity(float from, float to, float duration)
    {
        if (fadeMaterial == null)
        {
            Debug.LogWarning("[LoadSceneSystem] fadeMaterial 未指定，跳過 fade。");
            yield break;
        }

        Debug.Log($"[LoadSceneSystem] FadeMaterialOpacity 開始: {from} → {to}, 時長 {duration}s");

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeMaterial.SetFloat(OpacityID, Mathf.Lerp(from, to, t));
            
            yield return null;
        }

        fadeMaterial.SetFloat(OpacityID, to);
        Debug.Log($"[LoadSceneSystem] FadeMaterialOpacity 完成: Opacity = {to}");
    }

    // --- FadeIn Timeline 控制 ---

    /// <summary>
    /// 找到場景中的 FadeIn_Matrix 物件，播放它的 PlayableDirector，等待播完。
    /// </summary>
    private IEnumerator PlayFadeInTimeline()
    {
        var fadeInObj = GameObject.Find("FadeIn_Matrix");
        if (fadeInObj == null)
        {
            Debug.LogWarning("[LoadSceneSystem] 場景中找不到 FadeIn_Matrix 物件，跳過 Timeline。");
            yield break;
        }

        var timelineObj = fadeInObj.transform.Find("Timeline_FadeIn");
        if (timelineObj == null)
        {
            Debug.LogWarning("[LoadSceneSystem] FadeIn_Matrix 底下找不到 Timeline_FadeIn，跳過 Timeline。");
            yield break;
        }

        var director = timelineObj.GetComponent<PlayableDirector>();
        if (director == null)
        {
            Debug.LogWarning("[LoadSceneSystem] Timeline_FadeIn 上沒有 PlayableDirector，跳過 Timeline。");
            yield break;
        }

        director.Play();
        Debug.Log($"[LoadSceneSystem] PlayFadeInTimeline 開始播放, 時長: {director.duration}s");

        // 等待 Timeline 播放完畢
        while (director.state == PlayState.Playing)
        {
            yield return null;
        }

        Debug.Log("[LoadSceneSystem] PlayFadeInTimeline 播放完畢");
    }

    // --- Transition SFX 控制 ---

    private void PlayTransitionSFX()
    {
        if (transitionAudioSource == null || transitionSFXClip == null) return;

        transitionAudioSource.clip = transitionSFXClip;
        transitionAudioSource.loop = true;
        transitionAudioSource.Play();
    }

    private void StopTransitionSFX()
    {
        if (transitionAudioSource == null) return;
        transitionAudioSource.Stop();
    }

    /// <summary>
    /// 等待目前 SFX clip 播完這一輪（對齊最後一拍後再停止）。
    /// </summary>
    private IEnumerator WaitForSFXLastBeat()
    {
        if (transitionAudioSource == null || transitionSFXClip == null)
            yield break;

        float clipLength = transitionSFXClip.length;
        float currentTime = transitionAudioSource.time;
        float remaining = clipLength - currentTime;

        // 如果剩餘極短（< 0.05s），等下一輪完整播完
        if (remaining < 0.05f)
            remaining += clipLength;

        yield return new WaitForSecondsRealtime(remaining);
    }
}
