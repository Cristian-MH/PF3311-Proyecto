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

        //yield return RunSingleTest("=== TEST 1: AdultAvatar ===", TestAdultPatient);
       // yield return RunSingleTest("=== TEST 2: YoungAdultAvatar ===", TestYoungPatient);
        yield return RunSingleTest("=== TEST 3: OlderAdultAvatar ===", TestOlderPatient);
        //yield return RunSingleTest("=== TEST 4: NeutralSupportAvatar ===", TestLowTechPatient);

        Debug.Log("=== ALL TEST CASES COMPLETED ===");
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
        Debug.Log("Interaction completed. Moving to next avatar.");
        interactionFinished = true;
    }

    private void TestAdultPatient()
    {
        string json = @"{
            ""patientId"": ""11111111-1111-1111-1111-111111111111"",
            ""patientName"": ""Valeria"",
            ""age"": 35,
            ""sex"": ""F"",
            ""nationality"": ""Costa Rica"",
            ""technologyLevel"": ""medium"",
            ""condition"": ""rehabilitación de pierna"",
            ""therapyName"": ""Movilidad suave de pierna"",
            ""mood"": ""normal"",
            ""completedLastTherapy"": true
        }";

        motivationApiClient.RequestMotivationMessage(json);
    }

    private void TestYoungPatient()
    {
        string json = @"{
            ""patientId"": ""22222222-2222-2222-2222-222222222222"",
            ""patientName"": ""Cristian"",
            ""age"": 22,
            ""sex"": ""M"",
            ""nationality"": ""Costa Rica"",
            ""technologyLevel"": ""medium"",
            ""condition"": ""rehabilitación de hombro"",
            ""therapyName"": ""Movilidad suave de hombro"",
            ""mood"": ""normal"",
            ""completedLastTherapy"": false
        }";

        motivationApiClient.RequestMotivationMessage(json);
    }

    private void TestOlderPatient()
    {
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

    private void TestLowTechPatient()
    {
        string json = @"{
            ""patientId"": ""44444444-4444-4444-4444-444444444444"",
            ""patientName"": ""María"",
            ""age"": 45,
            ""sex"": ""F"",
            ""nationality"": ""Costa Rica"",
            ""technologyLevel"": ""low"",
            ""condition"": ""movilidad general"",
            ""therapyName"": ""Rutina inicial de movilidad"",
            ""mood"": ""normal"",
            ""completedLastTherapy"": false
        }";

        motivationApiClient.RequestMotivationMessage(json);
    }
}