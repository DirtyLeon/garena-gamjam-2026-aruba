using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 8方向節奏按鍵定義
/// </summary>
public enum RhythmDirection
{
    W, // 上 (North)
    A, // 左 (West)
    D, // 右 (East)
    Q, // 左上 (North-West)
    E, // 右上 (North-East)
    Z, // 左下 (South-West)
    X, // 下 (South)
    C  // 右下 (South-East)
}

public enum HitResult
{
    Perfect,
    Barely,
    Miss
}

[Serializable]
public class RhythmCue
{
    [Tooltip("目標拍數 (例如第 4.0 拍)")]
    public float targetBeat;

    [Tooltip("玩家在此拍點需要按下的方向")]
    public RhythmDirection targetDirection = RhythmDirection.W;
    
    [HideInInspector] public bool isProcessed = false;
}

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }

    [Header("Hit Windows (Seconds)")]
    [Tooltip("完美判定的秒數範圍 (正負值，例如 +-0.05秒)")]
    [SerializeField] private float perfectWindow = 0.05f; 

    [Tooltip("普通/勉強判定的秒數範圍 (正負值，例如 +-0.15秒)")]
    [SerializeField] private float barelyWindow = 0.15f;  

    [Header("Level Configuration")]
    [Tooltip("此關卡啟用的方向按鍵。若清單為空，則代表允許全部 8 個方向")]
    [SerializeField] private List<RhythmDirection> allowedDirections = new List<RhythmDirection>();

    [Tooltip("NPC 演出提示提前的拍數 (N 拍)")]
    [SerializeField] private float npcPromptLeadBeats = 1f;

    [Tooltip("關卡中的所有互動拍點 (可在此手動新增或透過腳本動態生成)")]
    [SerializeField] private List<RhythmCue> levelCues = new List<RhythmCue>();

    [Header("Events")]
    [Tooltip("當有判定結果時觸發：傳入判定結果、時間誤差秒數、以及該拍點的目標方向")]
    public UnityEvent<HitResult, float, RhythmDirection> OnHitResult;

    [Tooltip("一般節拍觸發事件 (傳入整數拍數)")]
    public UnityEvent<int> OnNormalBeatTriggered;

    [Tooltip("玩家互動拍點觸發事件 (傳入目標方向與目標拍數)")]
    public UnityEvent<RhythmDirection, float> OnPlayerBeatTriggered;

    [Tooltip("NPC 提示拍點觸發事件 (傳入目標方向與目標拍數)")]
    public UnityEvent<RhythmDirection, float> OnNPCBeatTriggered;

    private int nextCueIndex = 0;
    private int nextPlayerTriggerIndex = 0;
    private int nextNPCTriggerIndex = 0;

    public IReadOnlyList<RhythmCue> LevelCues => levelCues;
    public int NextCueIndex => nextCueIndex;

    /// <summary>
    /// 獲取下一個即將到來的拍點
    /// </summary>
    public RhythmCue GetNextCue()
    {
        if (nextCueIndex >= 0 && nextCueIndex < levelCues.Count)
        {
            return levelCues[nextCueIndex];
        }
        return null;
    }

    private List<RhythmCue> originalLevelCues = new List<RhythmCue>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 先進行升序排序，確保備份的順序正確
        levelCues.Sort((a, b) => a.targetBeat.CompareTo(b.targetBeat));

        // 備份最初始的拍點資料，以利重新播放時還原
        originalLevelCues.Clear();
        foreach (var cue in levelCues)
        {
            originalLevelCues.Add(new RhythmCue
            {
                targetBeat = cue.targetBeat,
                targetDirection = cue.targetDirection,
                isProcessed = false
            });
        }
    }

    private void Start()
    {
        // 確保拍點按照目標拍數升序排序
        levelCues.Sort((a, b) => a.targetBeat.CompareTo(b.targetBeat));

        if (RhythmConductor.Instance != null)
        {
            RhythmConductor.Instance.OnSongStart += ResetCues;
            RhythmConductor.Instance.OnSongStop += ResetCues;
            RhythmConductor.Instance.OnBeatTriggered += HandleNormalBeatTriggered;
        }
    }

    private void OnDestroy()
    {
        if (RhythmConductor.Instance != null)
        {
            RhythmConductor.Instance.OnSongStart -= ResetCues;
            RhythmConductor.Instance.OnSongStop -= ResetCues;
            RhythmConductor.Instance.OnBeatTriggered -= HandleNormalBeatTriggered;
        }
    }

    private void HandleNormalBeatTriggered(int beatCount)
    {
        OnNormalBeatTriggered?.Invoke(beatCount);
    }

    /// <summary>
    /// 重設所有拍點的狀態 (在歌曲重新播放或啟動時調用)
    /// </summary>
    public void ResetCues()
    {
        nextCueIndex = 0;
        nextPlayerTriggerIndex = 0;
        nextNPCTriggerIndex = 0;

        // 從備份還原拍點，清除上一局動態生成的拍點並重設狀態
        levelCues.Clear();
        foreach (var cue in originalLevelCues)
        {
            levelCues.Add(new RhythmCue
            {
                targetBeat = cue.targetBeat,
                targetDirection = cue.targetDirection,
                isProcessed = false
            });
        }
    }

    private void Update()
    {
        if (RhythmConductor.Instance == null || !RhythmConductor.Instance.IsPlaying) return;

        float currentBeat = RhythmConductor.Instance.CurrentBeat;
        float secPerBeat = RhythmConductor.Instance.SecondsPerBeat;

        // 1. NPC 提示拍點觸發 (提前 npcPromptLeadBeats 拍)
        while (nextNPCTriggerIndex < levelCues.Count)
        {
            RhythmCue nextCue = levelCues[nextNPCTriggerIndex];
            if (currentBeat >= nextCue.targetBeat - npcPromptLeadBeats)
            {
                OnNPCBeatTriggered?.Invoke(nextCue.targetDirection, nextCue.targetBeat);
                nextNPCTriggerIndex++;
            }
            else
            {
                break;
            }
        }

        // 2. 玩家互動拍點觸發 (正好在 targetBeat)
        while (nextPlayerTriggerIndex < levelCues.Count)
        {
            RhythmCue nextCue = levelCues[nextPlayerTriggerIndex];
            if (currentBeat >= nextCue.targetBeat)
            {
                OnPlayerBeatTriggered?.Invoke(nextCue.targetDirection, nextCue.targetBeat);
                nextPlayerTriggerIndex++;
            }
            else
            {
                break;
            }
        }

        // 3. 自動 Miss 檢測：當前拍數已超過下一個拍點的「勉強判定範圍」，玩家仍未輸入
        while (nextCueIndex < levelCues.Count)
        {
            RhythmCue nextCue = levelCues[nextCueIndex];
            float barelyWindowBeats = barelyWindow / secPerBeat;

            if (currentBeat > nextCue.targetBeat + barelyWindowBeats)
            {
                if (!nextCue.isProcessed)
                {
                    nextCue.isProcessed = true;
                    TriggerHitResult(HitResult.Miss, barelyWindow, nextCue.targetDirection); // 自動 Miss
                }
                nextCueIndex++;
            }
            else
            {
                break; // 剩餘的拍點都還在未來或判定區間內
            }
        }
    }

    /// <summary>
    /// 當玩家按下方向按鍵時調用此方法進行判定
    /// </summary>
    /// <param name="pressedDirection">玩家按下的方向</param>
    public void RegisterPlayerInput(RhythmDirection pressedDirection)
    {
        if (RhythmConductor.Instance == null || !RhythmConductor.Instance.IsPlaying || nextCueIndex >= levelCues.Count) return;

        // 如果此關卡有設定限制的方向，且玩家按下的方向不在允許清單中，則忽略此輸入
        if (allowedDirections.Count > 0 && !allowedDirections.Contains(pressedDirection))
        {
            return;
        }

        float currentBeat = RhythmConductor.Instance.CurrentBeat;
        float secPerBeat = RhythmConductor.Instance.SecondsPerBeat;

        // 只判定當前最前方的未處理拍點
        RhythmCue activeCue = levelCues[nextCueIndex];

        // 計算拍數誤差並轉換為秒數誤差
        float deviationBeats = currentBeat - activeCue.targetBeat;
        float deviationSeconds = deviationBeats * secPerBeat;
        float absDeviation = Mathf.Abs(deviationSeconds);

        // 進行判定窗口檢查
        if (absDeviation <= barelyWindow)
        {
            activeCue.isProcessed = true;
            nextCueIndex++;

            // 檢查方向是否正確
            if (pressedDirection == activeCue.targetDirection)
            {
                HitResult result = (absDeviation <= perfectWindow) ? HitResult.Perfect : HitResult.Barely;
                TriggerHitResult(result, deviationSeconds, activeCue.targetDirection);
            }
            else
            {
                // 方向錯誤則判定為 Miss
                TriggerHitResult(HitResult.Miss, deviationSeconds, activeCue.targetDirection);
            }
        }
        else if (deviationSeconds < -barelyWindow)
        {
            // 按得太早 (超出勉強判定範圍的前界)
            // 關鍵調整：不將此拍點設為已處理，亦不推進 nextCueIndex。
            // 這會忽視玩家在窗口外的過早按鍵，避免連續輸入把未來的拍點吃掉。
            Debug.Log($"[RhythmManager] Early press ignored. Current Beat: {currentBeat:F2}, Target Beat: {activeCue.targetBeat:F2}");
        }
    }

    /// <summary>
    /// 動態向關卡添加拍點
    /// </summary>
    public void AddRhythmCue(float targetBeat, RhythmDirection direction)
    {
        RhythmCue newCue = new RhythmCue
        {
            targetBeat = targetBeat,
            targetDirection = direction,
            isProcessed = false
        };
        levelCues.Add(newCue);
        levelCues.Sort((a, b) => a.targetBeat.CompareTo(b.targetBeat));
    }

    private void TriggerHitResult(HitResult result, float deviation, RhythmDirection targetDirection)
    {
        OnHitResult?.Invoke(result, deviation, targetDirection);
    }
}
