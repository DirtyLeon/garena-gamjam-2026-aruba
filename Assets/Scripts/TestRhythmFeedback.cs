using UnityEngine;

public class TestRhythmFeedback : MonoBehaviour
{
    [Header("Visual Effects")]
    [Tooltip("Transform to pulse on beat (Optional)")]
    [SerializeField] private Transform beatPulseTarget;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 10f;

    private Vector3 originalScale = Vector3.one;

    private void Start()
    {
        if (beatPulseTarget != null)
        {
            originalScale = beatPulseTarget.localScale;
        }

        // Subscribe to beat trigger
        if (RhythmConductor.Instance != null)
        {
            RhythmConductor.Instance.OnBeatTriggered += HandleBeatTriggered;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (RhythmConductor.Instance != null)
        {
            RhythmConductor.Instance.OnBeatTriggered -= HandleBeatTriggered;
        }
    }

    private void Update()
    {
        // Smooth scale recovery back to original size
        if (beatPulseTarget != null)
        {
            beatPulseTarget.localScale = Vector3.Lerp(beatPulseTarget.localScale, originalScale, Time.deltaTime * pulseSpeed);
        }
    }

    // Handles beat trigger pulse
    private void HandleBeatTriggered(int beatCount)
    {
        Debug.Log($"<color=cyan>[Beat Pulse] Beat: {beatCount}</color>");

        if (beatPulseTarget != null)
        {
            beatPulseTarget.localScale = originalScale * pulseScale;
        }
    }
}
