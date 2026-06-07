using UnityEngine;

public class MixamoFakeLipSync : MonoBehaviour
{
    [Header("Speech Controller")]
    [SerializeField] private SpeechController speechController;

    [Header("Avatar Head Bone")]
    [SerializeField] private Transform headBone;

    [Header("Fake Lip Sync Movement")]
    [SerializeField] private float headRotationAmount = 2.5f;
    [SerializeField] private float sideRotationAmount = 1.2f;
    [SerializeField] private float movementSpeed = 12f;
    [SerializeField] private float smoothSpeed = 14f;

    private bool isSpeaking;
    private Quaternion originalHeadRotation;

    private void Start()
    {
        if (headBone != null)
        {
            originalHeadRotation = headBone.localRotation;
        }
        else
        {
            Debug.LogWarning("MixamoFakeLipSync: Head Bone is not assigned.");
        }

        if (speechController != null)
        {
            speechController.OnSpeechStarted += StartFakeLipSync;
            speechController.OnSpeechEnded += StopFakeLipSync;
        }
        else
        {
            Debug.LogWarning("MixamoFakeLipSync: SpeechController is not assigned.");
        }
    }

    private void OnDestroy()
    {
        if (speechController != null)
        {
            speechController.OnSpeechStarted -= StartFakeLipSync;
            speechController.OnSpeechEnded -= StopFakeLipSync;
        }
    }

    private void LateUpdate()
    {
        if (headBone == null)
            return;

        float verticalWave = isSpeaking
            ? Mathf.Abs(Mathf.Sin(Time.time * movementSpeed))
            : 0f;

        float sideWave = isSpeaking
            ? Mathf.Sin(Time.time * movementSpeed * 0.5f)
            : 0f;

        Quaternion targetRotation = originalHeadRotation * Quaternion.Euler(
            headRotationAmount * verticalWave,
            sideRotationAmount * sideWave,
            0f
        );

        headBone.localRotation = Quaternion.Lerp(
            headBone.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    private void StartFakeLipSync()
    {
        Debug.Log("Fake lip sync started.");
        isSpeaking = true;
    }

    private void StopFakeLipSync()
    {
        Debug.Log("Fake lip sync stopped.");
        isSpeaking = false;
    }
}