using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class SpeechToTextController : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField]
    private string baseUrl = "https://pf3311-azf3h8a2a3gqcbeh.eastus2-01.azurewebsites.net/api";

    [Header("Recording")]
    [SerializeField] private int recordingSeconds = 5;
    [SerializeField] private int sampleRate = 16000;

    [Header("UI")]
    [SerializeField] private TMP_Text messageText;

    public event Action<string> OnResponseCaptured;

    public void StartListening()
    {
        StartCoroutine(ListenRoutine());
    }

    private IEnumerator ListenRoutine()
    {
        Debug.Log("STT started. Abriendo micrófono para escuchar al paciente.");

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone detected.");

            if (messageText != null)
                messageText.text = "No se detectó micrófono.";

            OnResponseCaptured?.Invoke("No se detectó micrófono.");
            yield break;
        }

        string microphoneName = Microphone.devices[0];

        Debug.Log($"Microphone detected: {microphoneName}");

        if (messageText != null)
            messageText.text = "Te escucho... ¿cómo te sentiste hoy?";

        AudioClip clip = Microphone.Start(
            microphoneName,
            false,
            recordingSeconds,
            sampleRate
        );

        if (clip == null)
        {
            Debug.LogWarning("Microphone recording did not start.");

            if (messageText != null)
                messageText.text = "No pude iniciar el micrófono.";

            OnResponseCaptured?.Invoke("No pude iniciar el micrófono.");
            yield break;
        }

        Debug.Log("Microphone recording started.");

        yield return new WaitForSeconds(recordingSeconds);

        Microphone.End(microphoneName);

        Debug.Log("Microphone recording finished.");

        if (messageText != null)
            messageText.text = "Procesando tu respuesta...";

        byte[] wavData = WavUtility.FromAudioClip(clip);

        yield return StartCoroutine(SendAudioToBackend(wavData));
    }

    private IEnumerator SendAudioToBackend(byte[] wavData)
    {
        string url = $"{baseUrl}/Speech/transcribe";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "patient-response.wav", "audio/wav");

        using UnityWebRequest request = UnityWebRequest.Post(url, form);

        Debug.Log($"Sending audio to STT endpoint: {url}");

        yield return request.SendWebRequest();

        Debug.Log($"STT response code: {request.responseCode}");

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"STT error: {request.error}");
            Debug.LogError(request.downloadHandler.text);

            if (messageText != null)
                messageText.text = "No pude interpretar tu respuesta.";

            OnResponseCaptured?.Invoke("No pude interpretar tu respuesta.");
            yield break;
        }

        string json = request.downloadHandler.text;

        Debug.Log($"STT raw response: {json}");

        SpeechToTextResponse response = JsonUtility.FromJson<SpeechToTextResponse>(json);

        string interpretedText = response != null && !string.IsNullOrWhiteSpace(response.text)
            ? response.text
            : "No se detectó texto.";

        Debug.Log($"Texto interpretado: {interpretedText}");

        if (messageText != null)
        {
            messageText.text = $"Respuesta registrada: {interpretedText}";
        }

        OnResponseCaptured?.Invoke(interpretedText);
    }
}

[System.Serializable]
public class SpeechToTextResponse
{
    public string text;
}