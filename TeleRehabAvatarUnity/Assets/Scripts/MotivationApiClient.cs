using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MotivationApiClient : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField]
    private string baseUrl = "https://pf3311-azf3h8a2a3gqcbeh.eastus2-01.azurewebsites.net/api";

    [Header("Avatar Bridge")]
    [SerializeField]
    private AvatarBridge avatarBridge;

    private int currentRequestId = 0;

    public void RequestMotivationMessage(string json)
    {
        currentRequestId++;

        int requestId = currentRequestId;

        Debug.Log("Patient context received:");
        Debug.Log(json);

        PatientContextMessage context = JsonUtility.FromJson<PatientContextMessage>(json);

        string avatarProfile = ResolveAvatarProfile(context);

        Debug.Log($"Request {requestId} - Avatar profile resolved BEFORE backend: {avatarProfile}");

        if (avatarBridge != null)
        {
            // 1. Primero carga el avatar correcto.
            avatarBridge.ApplyPatientContext(json);

            // 2. Luego muestra estado de espera sin cambiar avatar.
            avatarBridge.ShowMessageOnCurrentAvatar(
                "Generando mensaje motivacional personalizado...",
                "neutral",
                "talk"
            );
        }

        // 3. Después llama al backend.
        StartCoroutine(PostMotivationRequest(context, requestId));
    }

    private IEnumerator PostMotivationRequest(PatientContextMessage context, int requestId)
    {
        string url = $"{baseUrl}/Motivation/context-message";

        MotivationRequest request = new MotivationRequest
        {
            patientId = context.patientId,
            patientName = context.patientName,
            age = context.age,
            sex = context.sex,
            nationality = context.nationality,
            technologyLevel = context.technologyLevel,
            condition = context.condition,
            therapyName = context.therapyName,
            mood = context.mood,
            completedLastTherapy = context.completedLastTherapy
        };

        string body = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        using UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (requestId != currentRequestId)
        {
            Debug.Log($"Ignoring old backend response. Request {requestId} is no longer active.");
            yield break;
        }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Motivation API error: {webRequest.error}");
            Debug.LogError(webRequest.downloadHandler.text);

            avatarBridge.ShowMessageOnCurrentAvatar(
    $"{context.patientName}, has avanzado en tu proceso. Sigue paso a paso con tu recuperación.",
    "empathetic",
    "empathetic"
);
            yield break;
        }

        string responseText = webRequest.downloadHandler.text;
        Debug.Log($"Motivation API response: {responseText}");

        MotivationApiResponse apiResponse =
            JsonUtility.FromJson<MotivationApiResponse>(responseText);


        AvatarMessage avatarMessage = BuildAvatarMessage(context, apiResponse.message);

        avatarBridge.ShowMessageOnCurrentAvatar(
            avatarMessage.message,
            avatarMessage.emotion,
            avatarMessage.animation
        );
    }

    private AvatarMessage BuildAvatarMessage(PatientContextMessage context, string message)
    {
        string emotion = "neutral";
        string animation = "talk";

        if (context.completedLastTherapy)
        {
            emotion = "happy";
            animation = "celebrate";
        }
        else if (!string.IsNullOrWhiteSpace(context.mood) &&
                 context.mood.ToLowerInvariant().Contains("cansado"))
        {
            emotion = "empathetic";
            animation = "empathetic";
        }

        string avatarProfile = ResolveAvatarProfile(context);

        Debug.Log($"Resolved profile: {avatarProfile}");
        Debug.Log($"Resolved animation: {animation}");

        return new AvatarMessage
        {
            message = message,
            avatarProfile = avatarProfile,
            emotion = emotion,
            animation = animation,
            voiceStyle = ResolveVoiceStyle(context)
        };
    }

    private string ResolveAvatarProfile(PatientContextMessage context)
    {
        if (!string.IsNullOrWhiteSpace(context.technologyLevel) &&
            context.technologyLevel.ToLowerInvariant() == "low")
        {
            return "neutral_support";
        }

        if (context.age < 30)
        {
            return "young_adult_support";
        }

        if (context.age >= 60)
        {
            return "older_adult_support";
        }

        return "adult_support";
    }

    private string ResolveVoiceStyle(PatientContextMessage context)
    {
        if (context.age >= 60 || context.technologyLevel == "low")
            return "calm_supportive";

        return "warm";
    }

    private string CreateFallbackMessage(PatientContextMessage context)
    {
        AvatarMessage fallback = new AvatarMessage
        {
            message = $"{context.patientName}, has avanzado en tu proceso. Sigue paso a paso con tu recuperación.",
            avatarProfile = ResolveAvatarProfile(context),
            emotion = "empathetic",
            animation = "empathetic",
            voiceStyle = ResolveVoiceStyle(context)
        };

        return JsonUtility.ToJson(fallback);
    }
}

[System.Serializable]
public class PatientContextMessage
{
    public string patientId;
    public string patientName;
    public int age;
    public string sex;
    public string nationality;
    public string technologyLevel;
    public string condition;
    public string therapyName;
    public string mood;
    public bool completedLastTherapy;
}

[System.Serializable]
public class MotivationRequest
{
    public string patientId;
    public string patientName;
    public int age;
    public string sex;
    public string nationality;
    public string technologyLevel;
    public string condition;
    public string therapyName;
    public string mood;
    public bool completedLastTherapy;
}

[System.Serializable]
public class MotivationApiResponse
{
    public string message;
}
