using System.Collections;
using UnityEngine;

public class AvatarTester : MonoBehaviour
{
    [SerializeField] private MotivationApiClient motivationApiClient;

    [Header("Test timing")]
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float delayBetweenTests = 10f;

    private void Start()
    {
        StartCoroutine(RunAvatarTests());
    }

    private IEnumerator RunAvatarTests()
    {
        yield return new WaitForSeconds(initialDelay);

        Debug.Log("=== TEST 1: AdultAvatar ===");
        TestAdultPatient();
        yield return new WaitForSeconds(delayBetweenTests);

        Debug.Log("=== TEST 2: YoungAdultAvatar ===");
        TestYoungPatient();
        yield return new WaitForSeconds(delayBetweenTests);

        Debug.Log("=== TEST 3: OlderAdultAvatar ===");
        TestOlderPatient();
        yield return new WaitForSeconds(delayBetweenTests);

        Debug.Log("=== TEST 4: NeutralSupportAvatar ===");
        TestLowTechPatient();
    }

    private void TestAdultPatient()
    {
        string json = @"{
            ""patientId"": ""11111111-1111-1111-1111-111111111111"",
            ""patientName"": ""Valeria"",
            ""age"": 35,
            ""sex"": ""M"",
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
            ""sex"": ""F"",
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