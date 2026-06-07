using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SpeechController : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField]
    private string baseUrl = "https://pf3311-azf3h8a2a3gqcbeh.eastus2-01.azurewebsites.net/api";

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [Header("Speech To Text")]
    [SerializeField]    
    private SpeechToTextController speechToTextController;

    [Header("Interaction")]
    [SerializeField]
    private string followUpQuestion = "Antes de terminar, ¿cómo te sentiste realizando la terapia de hoy?";

    [SerializeField]
    private float extraWaitAfterAudio = 0.8f;

    public event System.Action OnFullInteractionCompleted;

    private Coroutine currentSpeechFlow;
    private int speechSessionId = 0;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.mute = false;
    }

    public void Speak(string message, string sex)
    {
        Speak(message, sex, false);
    }

    public void Speak(string message, string sex, bool askFollowUpAfterSpeech)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("TTS message is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sex))
        {
            sex = "F";
        }

        speechSessionId++;

        if (currentSpeechFlow != null)
        {
            StopCoroutine(currentSpeechFlow);
            currentSpeechFlow = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        int sessionId = speechSessionId;

        currentSpeechFlow = StartCoroutine(
            SpeechFlow(message, sex, askFollowUpAfterSpeech, sessionId)
        );
    }

    private IEnumerator SpeechFlow(
        string message,
        string sex,
        bool askFollowUpAfterSpeech,
        int sessionId)
    {
        Debug.Log("Starting motivational message TTS.");

        bool messagePlayed = false;

        yield return StartCoroutine(RequestAndPlayAudio(
            message,
            sex,
            sessionId,
            success => messagePlayed = success
        ));

        if (sessionId != speechSessionId)
            yield break;

        if (!messagePlayed)
        {
            Debug.LogWarning("Motivational message was not played.");
            OnFullInteractionCompleted?.Invoke();
            yield break;
        }

        if (!askFollowUpAfterSpeech)
        {
            OnFullInteractionCompleted?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Starting follow-up question TTS.");

        bool questionPlayed = false;

        yield return StartCoroutine(RequestAndPlayAudio(
            followUpQuestion,
            sex,
            sessionId,
            success => questionPlayed = success
        ));

        if (sessionId != speechSessionId)
            yield break;

        if (!questionPlayed)
        {
            Debug.LogWarning("Follow-up question was not played.");
            OnFullInteractionCompleted?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Follow-up question finished. Starting STT.");

        if (speechToTextController != null)
        {
            speechToTextController.OnResponseCaptured -= HandlePatientResponseCaptured;
            speechToTextController.OnResponseCaptured += HandlePatientResponseCaptured;
            speechToTextController.StartListening();
        }
        else
        {
            Debug.LogWarning("SpeechToTextController is not assigned in SpeechController.");
            OnFullInteractionCompleted?.Invoke();
        }
    }

    private IEnumerator RequestAndPlayAudio(
        string message,
        string sex,
        int sessionId,
        System.Action<bool> onCompleted)
    {
        string url = $"{baseUrl}/Speech/synthesize";

        SpeechRequest request = new SpeechRequest
        {
            text = message,
            sex = sex
        };

        string body = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        using UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

        // Your backend currently returns MP3.
        // If later you change the backend to WAV, change AudioType.MPEG to AudioType.WAV.
        webRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "*/*");

        Debug.Log($"Requesting TTS audio from {url}. Sex: {sex}");
        Debug.Log(body);

        yield return webRequest.SendWebRequest();

        if (sessionId != speechSessionId)
        {
            Debug.Log("Ignoring old TTS response.");
            onCompleted?.Invoke(false);
            yield break;
        }

        Debug.Log($"TTS response code: {webRequest.responseCode}");

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"TTS API error: {webRequest.error}");
            Debug.LogError(webRequest.downloadHandler.text);
            onCompleted?.Invoke(false);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);

        if (clip == null)
        {
            Debug.LogError("TTS audio clip is null.");
            onCompleted?.Invoke(false);
            yield break;
        }

        Debug.Log($"TTS clip loaded. Length: {clip.length}, Frequency: {clip.frequency}");

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned.");
            onCompleted?.Invoke(false);
            yield break;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"TTS audio playing: {message}");

        float maxWait = Mathf.Max(clip.length + 3f, 6f);
        float elapsed = 0f;

        while (audioSource.isPlaying && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(extraWaitAfterAudio);

        Debug.Log($"TTS audio finished. Elapsed: {elapsed}");

        onCompleted?.Invoke(true);
    }

    private void HandlePatientResponseCaptured(string response)
    {
        Debug.Log($"Patient response captured: {response}");

        if (speechToTextController != null)
        {
            speechToTextController.OnResponseCaptured -= HandlePatientResponseCaptured;
        }

        OnFullInteractionCompleted?.Invoke();
    }
}

[System.Serializable]
public class SpeechRequest
{
    public string text;
    public string sex;
}