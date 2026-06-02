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


        Debug.Log("=== TEST 2: YoungAdultAvatar ===");
        TestYoungPatient();
        yield return new WaitForSeconds(delayBetweenTests);

        Debug.Log("=== TEST 3: OlderAdultAvatar ===");
        TestOlderPatient();
        yield return new WaitForSeconds(delayBetweenTests);

        Debug.Log("=== TEST 4: NeutralSupportAvatar ===");
        TestLowTechPatient();
    }


    private void TestYoungPatient()
    {
        string json = @"{
            ""patientId"": ""22222222-2222-2222-2222-222222222222"",
            ""patientName"": ""Paciente joven"",
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
            ""patientName"": ""Paciente adulto mayor"",
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
            ""patientName"": ""Paciente con baja familiaridad tecnológica"",
            ""age"": 45,
            ""sex"": ""O"",
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