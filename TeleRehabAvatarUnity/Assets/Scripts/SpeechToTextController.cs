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
    [SerializeField] private int recordingSeconds = 10;
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private float delayBeforeRecording = 0.8f;

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

        Debug.Log("Available microphones:");

        foreach (string device in Microphone.devices)
        {
            Debug.Log($"Available microphone: {device}");
        }

        string microphoneName = Microphone.devices[0];

        Debug.Log($"Selected microphone: {microphoneName}");

        if (messageText != null)
            messageText.text = "Te escucho... responde ahora.";

        yield return new WaitForSeconds(delayBeforeRecording);

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

        float startTimeout = 2f;
        float elapsedStart = 0f;

        while (Microphone.GetPosition(microphoneName) <= 0 && elapsedStart < startTimeout)
        {
            elapsedStart += Time.deltaTime;
            yield return null;
        }

        if (Microphone.GetPosition(microphoneName) <= 0)
        {
            Debug.LogWarning("Microphone did not start capturing samples.");

            if (messageText != null)
                messageText.text = "El micrófono no inició la captura.";

            Microphone.End(microphoneName);
            OnResponseCaptured?.Invoke("El micrófono no inició la captura.");
            yield break;
        }

        Debug.Log("Microphone recording started. SPEAK NOW.");

        yield return new WaitForSeconds(recordingSeconds);

        int recordedPosition = Microphone.GetPosition(microphoneName);

        Microphone.End(microphoneName);

        Debug.Log("Microphone recording finished.");
        Debug.Log($"Recorded position: {recordedPosition}");
        Debug.Log($"Clip samples: {clip.samples}");
        Debug.Log($"Clip frequency: {clip.frequency}");
        Debug.Log($"Clip channels: {clip.channels}");
        Debug.Log($"Expected duration: {(float)clip.samples / clip.frequency} seconds");

        float rms = CalculateRms(clip);
        Debug.Log($"Recorded audio RMS: {rms}");

        if (rms < 0.001f)
        {
            Debug.LogWarning("Audio seems silent or too low. Microphone may not be capturing voice.");
        }

        if (messageText != null)
            messageText.text = "Procesando tu respuesta...";

        byte[] wavData = WavUtility.FromAudioClipTo16KhzMono(clip);

        Debug.Log($"Clip frequency before WAV conversion: {clip.frequency}");
        Debug.Log($"Clip channels before WAV conversion: {clip.channels}");
        Debug.Log($"Clip samples before WAV conversion: {clip.samples}");
        Debug.Log($"WAV bytes sent: {wavData.Length}");

        yield return StartCoroutine(SendAudioToBackend(wavData));
    }

    private IEnumerator SendAudioToBackend(byte[] wavData)
    {
        string url = $"{baseUrl}/Speech/transcribe";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "patient-response.wav", "audio/wav");

        using UnityWebRequest request = UnityWebRequest.Post(url, form);

        Debug.Log($"Sending audio to STT endpoint: {url}");
        Debug.Log($"Audio bytes sent: {wavData.Length}");

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

        Debug.Log($"Recognition status: {response?.recognitionStatus}");
        Debug.Log($"Texto interpretado: {interpretedText}");

        if (messageText != null)
        {
            messageText.text = $"Respuesta registrada: {interpretedText}";
        }

        OnResponseCaptured?.Invoke(interpretedText);
    }

    private float CalculateRms(AudioClip clip)
    {
        if (clip == null)
            return 0f;

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        if (samples.Length == 0)
            return 0f;

        double sum = 0;

        foreach (float sample in samples)
        {
            sum += sample * sample;
        }

        return Mathf.Sqrt((float)(sum / samples.Length));
    }
}

[System.Serializable]
public class SpeechToTextResponse
{
    public string text;
    public string recognitionStatus;
    public string rawResponse;
    public string error;
}