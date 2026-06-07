using UnityEngine;

public class HeadFakeLipSync : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Head Bone")]
    [SerializeField] private Transform headBone;

    [Header("Movement")]
    [SerializeField] private float rotationAmount = 4f;
    [SerializeField] private float sensitivity = 180f;
    [SerializeField] private float smoothSpeed = 16f;

    private Quaternion originalRotation;
    private float[] samples = new float[512];

    private void Start()
    {
        if (headBone != null)
            originalRotation = headBone.localRotation;

        if (audioSource == null)
            Debug.LogWarning("HeadFakeLipSync: Audio Source is not assigned.");

        if (headBone == null)
            Debug.LogWarning("HeadFakeLipSync: Head Bone is not assigned.");
    }

    private void LateUpdate()
    {
        if (audioSource == null || headBone == null)
            return;

        float targetRotation = 0f;

        if (audioSource.isPlaying)
        {
            float volume = GetVolume();
            targetRotation = Mathf.Clamp(volume * sensitivity, 0f, rotationAmount);

            Debug.Log($"Head lip sync volume: {volume}, rotation: {targetRotation}");
        }

        Quaternion target = originalRotation * Quaternion.Euler(targetRotation, 0f, 0f);

        headBone.localRotation = Quaternion.Lerp(
            headBone.localRotation,
            target,
            Time.deltaTime * smoothSpeed
        );
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