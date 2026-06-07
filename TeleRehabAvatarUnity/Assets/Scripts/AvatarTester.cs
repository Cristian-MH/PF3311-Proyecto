using System.Collections;
using UnityEngine;

public class AvatarTester : MonoBehaviour
{
    [SerializeField] private MotivationApiClient motivationApiClient;
    [SerializeField] private SpeechController speechController;

    [Header("Test timing")]
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float safetyTimeout = 90f;
    [SerializeField] private float delayAfterInteraction = 1.5f;

    private bool interactionFinished;

    private void Start()
    {
        StartCoroutine(RunAvatarTests());
    }

    private IEnumerator RunAvatarTests()
    {
        yield return new WaitForSeconds(initialDelay);

        yield return RunSingleTest("=== TEST: OlderAdultAvatar ===", TestOlderPatient);

        Debug.Log("=== TEST COMPLETED ===");
    }

    private IEnumerator RunSingleTest(string testName, System.Action testAction)
    {
        Debug.Log(testName);

        interactionFinished = false;

        if (speechController != null)
        {
            speechController.OnFullInteractionCompleted -= HandleInteractionCompleted;
            speechController.OnFullInteractionCompleted += HandleInteractionCompleted;
        }
        else
        {
            Debug.LogWarning("SpeechController is not assigned in AvatarTester.");
        }

        testAction.Invoke();

        float elapsed = 0f;

        while (!interactionFinished && elapsed < safetyTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!interactionFinished)
        {
            Debug.LogWarning($"Test finished by safety timeout: {testName}");
        }

        if (speechController != null)
        {
            speechController.OnFullInteractionCompleted -= HandleInteractionCompleted;
        }

        yield return new WaitForSeconds(delayAfterInteraction);
    }

    private void HandleInteractionCompleted()
    {
        Debug.Log("Interaction completed.");
        interactionFinished = true;
    }

    private void TestOlderPatient()
    {
        string patientName = "Don Manuel";
        string condition = "rehabilitación lumbar";
        string therapyName = "Movilidad suave de espalda";

        if (speechController != null)
        {
            speechController.SetPatientContext(
                patientName,
                condition,
                therapyName
            );
        }

        string json = @"{
            ""patientId"": ""33333333-3333-3333-3333-333333333333"",
            ""patientName"": ""Don Manuel"",
            ""age"": 70,
            ""sex"": ""M"",
            ""nationality"": ""Costa Rica"",
            ""technologyLevel"": ""medium"",
            ""condition"": ""rehabilitación lumbar"",
            ""therapyName"": ""Movilidad suave de espalda"",
            ""mood"": ""cansado"",
            ""completedLastTherapy"": false
        }";

        motivationApiClient.RequestMotivationMessage(json);
    }
}