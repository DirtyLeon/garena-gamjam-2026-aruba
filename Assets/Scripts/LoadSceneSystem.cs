using System;
using System.Collections;
using UnityEngine;
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
/// 負責轉場動畫、Loading bar、轉場 SFX 的播放與停止。
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

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;     // Loading 畫面根物件
    [SerializeField] private UnityEngine.UI.Slider loadingBar; // Loading bar

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

        // 初始隱藏 loading 畫面
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
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
        // 顯示 Loading 畫面（Loading bar 進畫面）
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (loadingBar != null)
            loadingBar.value = 0f;

        onLoadSceneStart?.Invoke();

        // ========== 階段：開始 Load Scene ==========
        // 開始非同步載入
        AsyncOperation asyncLoad = loadAction.Invoke();
        asyncLoad.allowSceneActivation = false;

        // 播放 transition SFX（loop）
        PlayTransitionSFX();

        // ========== 階段：等待直到讀取完畢 && SFX 最後一拍 ==========
        // 等待場景載入至 0.9（Unity 慣例，0.9 = 載入完成但尚未啟動）
        while (asyncLoad.progress < 0.9f)
        {
            if (loadingBar != null)
                loadingBar.value = asyncLoad.progress / 0.9f;

            yield return null;
        }

        // 載入完成，loading bar 填滿
        if (loadingBar != null)
            loadingBar.value = 1f;

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

        // 移除讀取畫面
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        onLoadSceneCompleted?.Invoke();

        isLoading = false;
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
    /// clip 長度 3 秒，計算剩餘時間讓它自然結束這一次循環。
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
