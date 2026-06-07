using UnityEngine;

public class SimpleLipSyncController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Mouth BlendShape")]
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private string mouthOpenBlendShapeName = "mouthOpen";

    [Header("Sensitivity")]
    [SerializeField] private float sensitivity = 1200f;
    [SerializeField] private float smoothSpeed = 12f;
    [SerializeField] private float maxMouthOpen = 100f;

    private int mouthBlendShapeIndex = -1;
    private float currentMouthValue = 0f;
    private float[] audioSamples = new float[256];

    private void Start()
    {
        ResolveBlendShape();
    }

    private void Update()
    {
        if (audioSource == null || faceRenderer == null || mouthBlendShapeIndex < 0)
            return;

        float targetValue = 0f;

        if (audioSource.isPlaying)
        {
            float volume = GetAudioVolume();
            targetValue = Mathf.Clamp(volume * sensitivity, 0f, maxMouthOpen);
        }

        currentMouthValue = Mathf.Lerp(
            currentMouthValue,
            targetValue,
            Time.deltaTime * smoothSpeed
        );

        faceRenderer.SetBlendShapeWeight(mouthBlendShapeIndex, currentMouthValue);
    }

    private void ResolveBlendShape()
    {
        if (faceRenderer == null)
        {
            Debug.LogWarning("SimpleLipSyncController: Face Renderer is not assigned.");
            return;
        }

        Mesh mesh = faceRenderer.sharedMesh;

        if (mesh == null)
        {
            Debug.LogWarning("SimpleLipSyncController: Face mesh is null.");
            return;
        }

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string blendShapeName = mesh.GetBlendShapeName(i);
            Debug.Log($"BlendShape found: {blendShapeName}");

            if (blendShapeName.ToLowerInvariant()
                .Contains(mouthOpenBlendShapeName.ToLowerInvariant()))
            {
                mouthBlendShapeIndex = i;
                Debug.Log($"Mouth BlendShape selected: {blendShapeName}");
                return;
            }
        }

        Debug.LogWarning($"BlendShape '{mouthOpenBlendShapeName}' was not found.");
    }

    private float GetAudioVolume()
    {
        audioSource.GetOutputData(audioSamples, 0);

        float sum = 0f;

        for (int i = 0; i < audioSamples.Length; i++)
        {
            sum += audioSamples[i] * audioSamples[i];
        }

        return Mathf.Sqrt(sum / audioSamples.Length);
    }
}