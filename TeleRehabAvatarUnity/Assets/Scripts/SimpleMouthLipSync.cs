using UnityEngine;

public class SimpleMouthLipSync : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Mouth Target")]
    [SerializeField] private Transform mouthTransform;

    [Header("Movement Settings")]
    [SerializeField] private float minScaleY = 1f;
    [SerializeField] private float maxScaleY = 2.2f;
    [SerializeField] private float sensitivity = 220f;
    [SerializeField] private float smoothSpeed = 18f;

    [Header("Optional Position Movement")]
    [SerializeField] private bool moveMouthDown = true;
    [SerializeField] private float maxMoveDown = 0.015f;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float[] samples = new float[512];

    private void Start()
    {
        if (mouthTransform != null)
        {
            originalScale = mouthTransform.localScale;
            originalPosition = mouthTransform.localPosition;
        }
        else
        {
            Debug.LogWarning("SimpleMouthLipSync: Mouth Transform is not assigned.");
        }

        if (audioSource == null)
            Debug.LogWarning("SimpleMouthLipSync: Audio Source is not assigned.");
    }

    private void LateUpdate()
    {
        if (audioSource == null || mouthTransform == null)
            return;

        float targetMultiplier = minScaleY;
        float moveAmount = 0f;

        if (audioSource.isPlaying)
        {
            float volume = GetVolume();

            targetMultiplier = Mathf.Clamp(
                minScaleY + volume * sensitivity,
                minScaleY,
                maxScaleY
            );

            moveAmount = Mathf.Clamp(volume * sensitivity * maxMoveDown, 0f, maxMoveDown);

            Debug.Log($"Mouth lip sync volume: {volume}, scaleY: {targetMultiplier}");
        }

        Vector3 targetScale = new Vector3(
            originalScale.x,
            originalScale.y * targetMultiplier,
            originalScale.z
        );

        mouthTransform.localScale = Vector3.Lerp(
            mouthTransform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );

        if (moveMouthDown)
        {
            Vector3 targetPosition = originalPosition + new Vector3(0f, -moveAmount, 0f);

            mouthTransform.localPosition = Vector3.Lerp(
                mouthTransform.localPosition,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    private float GetVolume()
    {
        audioSource.GetOutputData(samples, 0);

        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / samples.Length);
    }
}