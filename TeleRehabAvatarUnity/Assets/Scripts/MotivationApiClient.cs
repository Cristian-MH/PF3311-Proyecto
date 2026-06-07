using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MotivationApiClient : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField]
    private string baseUrl = "https://pf3311-azf3h8a2a3gqcbeh.eastus2-01.azurewebsites.net/api";

    [Header("Endpoints")]
    [SerializeField]
    private string motivationByIdEndpoint = "/Motivation/message";

    [SerializeField]
    private string contextMessageEndpoint = "/Motivation/context-message";

    [Header("Avatar Bridge")]
    [SerializeField]
    private AvatarBridge avatarBridge;

    private int currentRequestId = 0;

    private void Awake()
    {
        NormalizeEndpoints();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        NormalizeEndpoints();
    }
#endif

    private void NormalizeEndpoints()
    {
        if (string.IsNullOrWhiteSpace(motivationByIdEndpoint) ||
            motivationByIdEndpoint == "/Motivation")
        {
            motivationByIdEndpoint = "/Motivation/message";
        }
    }

    public void RequestMotivationMessage(string json)
    {
        NormalizeEndpoints();

        currentRequestId++;

        int requestId = currentRequestId;

        Debug.Log("Patient context received:");
        Debug.Log(json);

        PatientContextMessage context = JsonUtility.FromJson<PatientContextMessage>(json);

        if (avatarBridge != null)
        {
            // 1. Primero carga el avatar correcto.
            avatarBridge.ApplyPatientContext(json);

            // 2. Muestra loading, pero sin TTS.
            avatarBridge.ShowLoadingOnCurrentAvatar(
                "Generando mensaje motivacional personalizado...",
                "talk"
            );
        }

        // 3. Luego intenta backend por ID y si falla usa contexto.
        StartCoroutine(GetMotivationMessageFlow(context, requestId));
    }

    private IEnumerator GetMotivationMessageFlow(
        PatientContextMessage context,
        int requestId)
    {
        string message = null;

        // Primero intenta por ID solo si hay patientId y therapyId.
        if (!string.IsNullOrWhiteSpace(context.patientId) &&
            !string.IsNullOrWhiteSpace(context.therapyId))
        {
            yield return StartCoroutine(RequestMessageById(
                context,
                requestId,
                result => message = result
            ));
        }
        else
        {
            Debug.LogWarning("patientId or therapyId is empty. Skipping ID-based motivation endpoint.");
        }

        if (requestId != currentRequestId)
        {
            Debug.Log("Ignoring old motivation request.");
            yield break;
        }

        // Si por ID falló, usa el endpoint de contexto.
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("ID-based motivation failed or returned empty. Trying context-message endpoint.");

            yield return StartCoroutine(RequestMessageByContext(
                context,
                requestId,
                result => message = result
            ));
        }

        if (requestId != currentRequestId)
        {
            Debug.Log("Ignoring old context motivation request.");
            yield break;
        }

        // Si aun así falla, usamos fallback local personalizado.
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("Context-message endpoint failed. Using local personalized fallback.");
            message = BuildPersonalizedFallbackMessage(context);
        }

        AvatarMessage avatarMessage = BuildAvatarMessage(context, message);

        if (avatarBridge != null)
        {
            avatarBridge.ShowMessageOnCurrentAvatar(
                avatarMessage.message,
                avatarMessage.emotion,
                avatarMessage.animation
            );
        }
    }

    private IEnumerator RequestMessageById(
        PatientContextMessage context,
        int requestId,
        System.Action<string> onCompleted)
    {
        string url = $"{baseUrl}{motivationByIdEndpoint}";

        MotivationByIdRequest request = new MotivationByIdRequest
        {
            patientId = context.patientId,
            therapyId = context.therapyId
        };

        string body = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        using UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");

        Debug.Log($"Requesting motivation by ID from: {url}");
        Debug.Log($"Motivation by ID request body: {body}");

        yield return webRequest.SendWebRequest();

        if (requestId != currentRequestId)
        {
            onCompleted?.Invoke(null);
            yield break;
        }

        Debug.Log($"Motivation by ID response code: {webRequest.responseCode}");

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Motivation by ID failed: {webRequest.error}");
            Debug.LogWarning($"Motivation by ID response: {webRequest.downloadHandler.text}");
            onCompleted?.Invoke(null);
            yield break;
        }

        string json = webRequest.downloadHandler.text;

        Debug.Log($"Motivation by ID raw response: {json}");

        string message = ParseMessage(json);

        onCompleted?.Invoke(message);
    }

    private IEnumerator RequestMessageByContext(
        PatientContextMessage context,
        int requestId,
        System.Action<string> onCompleted)
    {
        string url = $"{baseUrl}{contextMessageEndpoint}";

        ContextMotivationRequest request = new ContextMotivationRequest
        {
            patientId = context.patientId,
            therapyId = context.therapyId,
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
        webRequest.SetRequestHeader("Accept", "application/json");

        Debug.Log($"Requesting context motivation from: {url}");
        Debug.Log($"Context motivation request body: {body}");

        yield return webRequest.SendWebRequest();

        if (requestId != currentRequestId)
        {
            onCompleted?.Invoke(null);
            yield break;
        }

        Debug.Log($"Context motivation response code: {webRequest.responseCode}");

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Context motivation API error: {webRequest.error}");
            Debug.LogError($"Context motivation response: {webRequest.downloadHandler.text}");
            onCompleted?.Invoke(null);
            yield break;
        }

        string json = webRequest.downloadHandler.text;

        Debug.Log($"Context motivation raw response: {json}");

        string message = ParseMessage(json);

        onCompleted?.Invoke(message);
    }

    private string ParseMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        MotivationResponse response = JsonUtility.FromJson<MotivationResponse>(json);

        if (response == null)
            return null;

        if (!string.IsNullOrWhiteSpace(response.message))
            return response.message;

        if (!string.IsNullOrWhiteSpace(response.Message))
            return response.Message;

        return null;
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

        return new AvatarMessage
        {
            message = message,
            avatarProfile = ResolveAvatarProfile(context),
            emotion = emotion,
            animation = animation,
            voiceStyle = ResolveVoiceStyle(context)
        };
    }

    private string ResolveAvatarProfile(PatientContextMessage context)
    {
        if (context.age < 30)
        {
            return "young_adult_support";
        }

        if (context.age >= 60)
        {
            return "older_adult_support";
        }

        if (!string.IsNullOrWhiteSpace(context.technologyLevel) &&
            context.technologyLevel.ToLowerInvariant() == "low")
        {
            return "neutral_support";
        }

        return "adult_support";
    }

    private string ResolveVoiceStyle(PatientContextMessage context)
    {
        return context.sex == "M" ? "male" : "female";
    }

    private string BuildPersonalizedFallbackMessage(PatientContextMessage context)
    {
        string patientName = string.IsNullOrWhiteSpace(context.patientName)
            ? "Paciente"
            : context.patientName;

        string therapyName = string.IsNullOrWhiteSpace(context.therapyName)
            ? "tu terapia"
            : context.therapyName;

        string condition = string.IsNullOrWhiteSpace(context.condition)
            ? "tu proceso de rehabilitación"
            : context.condition;

        string mood = string.IsNullOrWhiteSpace(context.mood)
            ? "normal"
            : context.mood.ToLowerInvariant();

        if (context.completedLastTherapy)
        {
            return $"{patientName}, excelente trabajo completando {therapyName}. Cada sesión que realizas suma a tu proceso de {condition}. Sigue avanzando con constancia, cuidando siempre cómo se siente tu cuerpo.";
        }

        if (mood.Contains("cansado") || mood.Contains("cansada"))
        {
            return $"{patientName}, gracias por continuar con {therapyName} incluso cuando te sientes con cansancio. En tu proceso de {condition}, avanzar poco a poco también cuenta. Tómate tu tiempo, escucha tu cuerpo y mantén la constancia sin exigirte de más.";
        }

        return $"{patientName}, hoy tienes una nueva oportunidad para avanzar con {therapyName}. Tu proceso de {condition} requiere paciencia, constancia y cuidado. Lo importante no es hacerlo perfecto, sino seguir dando pequeños pasos.";
    }
}

[System.Serializable]
public class MotivationByIdRequest
{
    public string patientId;
    public string therapyId;
}

[System.Serializable]
public class ContextMotivationRequest
{
    public string patientId;
    public string therapyId;
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
public class MotivationResponse
{
    public string message;
    public string Message;
}

[System.Serializable]
public class PatientContextMessage
{
    public string patientId;
    public string therapyId;
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
