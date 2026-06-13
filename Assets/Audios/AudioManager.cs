using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // 單例實體
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer audioMixer;
    // 對應 Mixer 中 Exposed Parameters 的名稱
    private const string BGM_VOL_PARAM = "BGM_Vol";
    private const string SFX_VOL_PARAM = "SFX_Vol";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- BGM 控制 ---
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 避免同首音樂重複觸發

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayBGM() // 強制重播
    {
        if (bgmSource.clip == null) return;
        bgmSource.Play();
    }

    // --- 基本 SFX 控制 (PlayOneShot) ---
    /// <summary>
    /// 播放基本音效
    /// </summary>
    /// <param name="clip">音效檔案</param>
    /// <param name="volumeScale">音量縮放比例 (預設為 1)</param>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            // 直接使用 PlayOneShot 疊加播放，不會切斷前一個音效
            sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning("PlaySFX 接收到的 AudioClip 為 null！");
        }
    }
    public void PlaySFX()
    {
        if (sfxSource.clip == null)
        {
            Debug.LogWarning("PlaySFX 沒有指定 AudioClip！");
            return;
        }
        sfxSource.PlayOneShot(sfxSource.clip);
    }
    // --- 混音器音量控制 (對接 UI Slider，數值範圍請設定 0 ~ 1) ---
    public void SetBGMVolume(float sliderValue)
    {
        float clampedValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedValue) * 20f;
        audioMixer.SetFloat(BGM_VOL_PARAM, dB);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float clampedValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedValue) * 20f;
        audioMixer.SetFloat(SFX_VOL_PARAM, dB);
    }
}