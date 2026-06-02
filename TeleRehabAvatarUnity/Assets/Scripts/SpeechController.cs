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

    public void Speak(string message, string sex)
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

        StartCoroutine(RequestSpeechAudio(message, sex));
    }

    private IEnumerator RequestSpeechAudio(string message, string sex)
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
        webRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "*/*");

        Debug.Log($"Requesting TTS audio. Sex: {sex}");
        Debug.Log(body);

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"TTS API error: {webRequest.error}");
            Debug.LogError(webRequest.downloadHandler.text);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);

        if (clip == null)
        {
            Debug.LogError("TTS audio clip is null.");
            yield break;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned.");
            yield break;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log("TTS audio playing.");
    }
}

[System.Serializable]
public class SpeechRequest
{
    public string text;
    public string sex;
}