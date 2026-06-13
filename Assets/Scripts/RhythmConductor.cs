using System;
using UnityEngine;

public class RhythmConductor : MonoBehaviour
{
    public static RhythmConductor Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Song Configuration")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float firstBeatOffset = 0f; // 音訊開頭的靜音秒數 (或延遲時間)

    [Header("Runtime Info (Read Only)")]
    [ReadOnly] [SerializeField] private float secondsPerBeat;

    public float BPM
    {
        get => bpm;
        set
        {
            bpm = value;
            UpdateSecondsPerBeat();
        }
    }

    public float SecondsPerBeat => secondsPerBeat;

    private double dspSongTimeStart;
    private bool isPlaying = false;

    public bool IsPlaying => isPlaying;

    // 當前歌曲播放到的拍數 (小數，例如 2.5 拍)
    public float CurrentBeat { get; private set; }
    
    // 當前整數拍 (例如 1, 2, 3...)
    public int CurrentWholeBeat => Mathf.FloorToInt(CurrentBeat);

    private int lastReportedWholeBeat = -1;

    // 拍點事件：傳入當前整數拍數 (可用於 UI 縮放、角色節奏點頭等)
    public event Action<int> OnBeatTriggered; 
    public event Action OnSongStart;
    public event Action OnSongEnd;
    public event Action OnSongStop;

    private void Awake()
    {
        UpdateSecondsPerBeat();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        UpdateSecondsPerBeat();
    }

    private void UpdateSecondsPerBeat()
    {
        if (bpm > 0)
        {
            secondsPerBeat = 60f / bpm;
        }
        else
        {
            secondsPerBeat = 0f;
        }
    }

    /// <summary>
    /// 開始播放音樂並啟動節拍計算器
    /// </summary>
    public void StartSong()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("RhythmConductor: AudioSource 或 AudioClip 未指派！");
            return;
        }

        lastReportedWholeBeat = -1;
        
        // 紀錄音訊系統的精準時間 (dspTime)
        dspSongTimeStart = AudioSettings.dspTime;
        
        // 立即在播放前計算初始拍數，避免其他腳本在同影格內讀取到上一局的殘留拍數
        double songPositionSeconds = 0 - firstBeatOffset;
        CurrentBeat = (float)(songPositionSeconds / SecondsPerBeat);

        audioSource.Play();
        isPlaying = true;
        
        OnSongStart?.Invoke();
    }

    /// <summary>
    /// 停止音樂與計算，並重設計時器
    /// </summary>
    public void StopSong()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        isPlaying = false;

        // 立即歸零拍數與狀態
        double songPositionSeconds = 0 - firstBeatOffset;
        CurrentBeat = (float)(songPositionSeconds / SecondsPerBeat);
        lastReportedWholeBeat = -1;

        OnSongStop?.Invoke();
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (!audioSource.isPlaying)
        {
            isPlaying = false;
            OnSongEnd?.Invoke();
            return;
        }

        // 計算歌曲撥放秒數與拍數 (使用精準的 dspTime)
        double songPositionSeconds = AudioSettings.dspTime - dspSongTimeStart - firstBeatOffset;
        CurrentBeat = (float)(songPositionSeconds / SecondsPerBeat);

        // 偵測是否越過下一個整數拍
        int currentWholeBeatFloor = Mathf.FloorToInt(CurrentBeat);
        if (currentWholeBeatFloor > lastReportedWholeBeat)
        {
            lastReportedWholeBeat = currentWholeBeatFloor;
            OnBeatTriggered?.Invoke(lastReportedWholeBeat);
        }
    }
}
