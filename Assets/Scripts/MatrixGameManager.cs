using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixGameManager : MonoBehaviour
{
    public static MatrixGameManager Instance;
    public static System.Action onTeoInit;
    public const float TOTAL_TIME = 30f;
    public const float RETRY_TIMER = 5f;
    public const int TOTAL_RETRY = 3;
    public const int TOTAL_HIT = 3;

    public FireSquadManager firesquads;

    public List<WaveInfo> waveLevels = new List<WaveInfo>();
    public List<Transform> finalArray = new List<Transform>();

    [ReadOnly] public int beatCounter = 0;

    private RhythmManager ryManager;
    private RhythmConductor ryConductor;
    [SerializeField] private int currentWave = 0;
    private int remainRetry = 0;

    private bool isActive = false;
    private bool levelPassed = false;
    private bool isConductorPlaying = false;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;
    }

    private void Start()
    {
        ryManager = RhythmManager.Instance;
        ryConductor = RhythmConductor.Instance;
        ryManager.OnNormalBeatTriggered.AddListener(OnTick);
        remainRetry = TOTAL_RETRY;
    }

    void OnEnable()
    {
        PlayerTeo.onPlayerFailed += OnChallengeFailed;  
    }

    void OnDisable()
    {
        PlayerTeo.onPlayerFailed -= OnChallengeFailed;
    }

    public void OnGameBegin()
    {
        currentWave = 0;
        remainRetry-=1;
        onTeoInit?.Invoke();
        isActive = true;

        if (!isConductorPlaying)
        {
            isConductorPlaying = true;
            ryConductor.StartSong();
        }
    }

    public void OnGameOver()
    {
        isActive = false;
        //ryConductor.StopSong();

        if (levelPassed)
        {
            // success?
            Debug.Log("PASSSS");
        }
        else
        {
            // failed?
            Debug.Log("FAILLED");
            
        }
    }

    void OnTick(int _call)
    {
        beatCounter++;
        if(!isActive)
            return;
        // Process squads to fire.

        if((beatCounter % 8) == 0)  //8 could be variable
        {
            if (currentWave < waveLevels.Count)
            {
                firesquads.flyDuration = waveLevels[currentWave].flyDuration;
                firesquads.fireGap = waveLevels[currentWave].fireGap;
                firesquads.AllFire();
                currentWave += 1;
            }
            else if(currentWave == waveLevels.Count)
            {
                // Final Special Case
                firesquads.FinalArrayFire(2f, 0.1f, finalArray);
                currentWave += 1;
            }
            else
            {
                isActive = false;
                levelPassed = true;
                OnGameOver();
            }
        }
    }

    public void OnChallengeFailed()
    {
        isActive = false;
        
        if(remainRetry == 0)
        {
            levelPassed = false;
            OnGameOver();
        }
        else
            StartCoroutine(RetryCoroutine());
    }

    public void DebugFinalArray() => firesquads.FinalArrayFire(2f, 0.1f, finalArray);

    private IEnumerator RetryCoroutine()
    {
        yield return new WaitForSeconds(RETRY_TIMER);
        OnGameBegin();
    }
    
    [System.Serializable]
    public class WaveInfo
    {
        public float flyDuration = 2;
        public float fireGap = 2;
    }
}
