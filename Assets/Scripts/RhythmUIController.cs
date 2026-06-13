using UnityEngine;
using TMPro;

public class RhythmUIController : MonoBehaviour
{
    [Header("TextMeshPro UI References")]
    [Tooltip("TextMeshPro Text component to display current beat")]
    [SerializeField] private TextMeshProUGUI beatText;
    
    [Tooltip("TextMeshPro Text component to display next cue hint")]
    [SerializeField] private TextMeshProUGUI nextCueText;

    [Tooltip("TextMeshPro Text component to display hit accuracy results (PERFECT, BARELY, MISS)")]
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Hit Result Animation Settings")]
    [Tooltip("The initial punch scale factor when a result is triggered (e.g. 1.5 = 150% size)")]
    [SerializeField] private float punchScale = 1.5f;

    [Tooltip("Speed at which the text shrinks back to its original scale")]
    [SerializeField] private float shrinkSpeed = 12f;

    [Tooltip("How long the hit result text stays visible before disappearing (in seconds)")]
    [SerializeField] private float displayDuration = 1.2f;

    private Vector3 originalResultScale = Vector3.one;
    private float resultTimer = 0f;

    private void Start()
    {
        if (resultText != null)
        {
            originalResultScale = resultText.transform.localScale;
            // Clear result text on start
            resultText.text = "";
        }

        // Subscribe to hit result event
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnHitResult.AddListener(HandleHitResult);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnHitResult.RemoveListener(HandleHitResult);
        }
    }

    private void Update()
    {
        UpdateBeatText();
        UpdateNextCueText();
        UpdateResultAnimation();
    }

    private void UpdateBeatText()
    {
        if (RhythmConductor.Instance == null || beatText == null) return;

        float currentBeat = RhythmConductor.Instance.CurrentBeat;
        bool isPlaying = RhythmConductor.Instance.IsPlaying;

        if (isPlaying)
        {
            beatText.text = $"Beat: {currentBeat:F1}";
        }
        else
        {
            beatText.text = "Beat: -- (Stopped)";
        }
    }

    private void UpdateNextCueText()
    {
        if (RhythmManager.Instance == null || RhythmConductor.Instance == null || nextCueText == null) return;

        bool isPlaying = RhythmConductor.Instance.IsPlaying;
        int nextCueIndex = RhythmManager.Instance.NextCueIndex;
        int totalCues = RhythmManager.Instance.LevelCues.Count;

        // If BGM is stopped
        if (!isPlaying)
        {
            if (totalCues > 0 && nextCueIndex >= totalCues)
            {
                nextCueText.text = "Level Finished!";
            }
            else
            {
                nextCueText.text = "Ready...";
            }
            return;
        }

        // Get the next upcoming cue
        RhythmCue nextCue = RhythmManager.Instance.GetNextCue();

        if (nextCue != null)
        {
            float currentBeat = RhythmConductor.Instance.CurrentBeat;
            // Calculate distance to target beat
            float beatsRemaining = nextCue.targetBeat - currentBeat;

            if (beatsRemaining > 0)
            {
                nextCueText.text = $"Next: [{nextCue.targetDirection}] at Beat {nextCue.targetBeat:F1} ({beatsRemaining:F1} beats left)";
            }
            else
            {
                // Current beat has entered the timing window
                nextCueText.text = $"Next: [{nextCue.targetDirection}] at Beat {nextCue.targetBeat:F1} (NOW!)";
            }
        }
        else
        {
            // All cues processed
            nextCueText.text = "Level Finished!";
        }
    }

    private void HandleHitResult(HitResult result, float deviation, RhythmDirection direction)
    {
        if (resultText == null) return;

        // Update text content and color based on the hit accuracy
        switch (result)
        {
            case HitResult.Perfect:
                resultText.text = "PERFECT!";
                resultText.color = Color.green;
                break;
            case HitResult.Barely:
                resultText.text = "BARELY";
                resultText.color = Color.yellow;
                break;
            case HitResult.Miss:
                resultText.text = "MISS!";
                resultText.color = Color.red;
                break;
        }

        // Trigger the visual Punch scaling effect
        resultText.transform.localScale = originalResultScale * punchScale;

        // Reset display timer
        resultTimer = displayDuration;
    }

    private void UpdateResultAnimation()
    {
        if (resultText == null) return;

        // Smoothly interpolate back to the original scale
        resultText.transform.localScale = Vector3.Lerp(
            resultText.transform.localScale, 
            originalResultScale, 
            Time.deltaTime * shrinkSpeed
        );

        // Countdown display timer and clear text when finished
        if (resultTimer > 0f)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0f)
            {
                resultText.text = "";
            }
        }
    }
}
