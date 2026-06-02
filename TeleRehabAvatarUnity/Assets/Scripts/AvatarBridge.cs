using UnityEngine;
using TMPro;

public class AvatarBridge : MonoBehaviour
{
    [Header("Avatar Root")]
    [SerializeField] private Transform avatarRoot;

    [Header("Avatar Profiles")]
    [SerializeField] private GameObject youngAdultAvatar;
    [SerializeField] private GameObject adultAvatar;
    [SerializeField] private GameObject olderAdultAvatar;
    [SerializeField] private GameObject neutralSupportAvatar;

    [Header("Runtime Avatar")]
    [SerializeField] private GameObject activeAvatar;
    [SerializeField] private Animator animator;

    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;

    [Header("Speech")]
    [SerializeField] private SpeechController speechController;

    private string currentAvatarSex = "F";

    private void Awake()
    {
        DisableAllAvatars();

        if (messageText != null)
        {
            messageText.text = "Cargando perfil del paciente...";
        }
    }

    public void ApplyPatientContext(string json)
    {
        Debug.Log("Applying patient context:");
        Debug.Log(json);

        PatientContextMessage context = JsonUtility.FromJson<PatientContextMessage>(json);

        string avatarProfile = ResolveAvatarProfile(context);

        Debug.Log($"Loading avatar before backend call: {avatarProfile}");

        ApplyAvatarProfile(avatarProfile);
        ApplyContextScale(context);
    }

    public void ShowLoadingOnCurrentAvatar(string message, string animation)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        PlayAnimation(animation);
    }

    public void ShowMessageOnCurrentAvatar(string message, string emotion, string animation)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        ApplyEmotion(emotion);
        PlayAnimation(animation);

        if (speechController != null && !string.IsNullOrWhiteSpace(message))
        {
            speechController.Speak(message, currentAvatarSex);
        }
    }

    public void ReceiveMessage(string json)
    {
        Debug.Log("Avatar message received:");
        Debug.Log(json);

        AvatarMessage data = JsonUtility.FromJson<AvatarMessage>(json);

        if (messageText != null)
        {
            messageText.text = data.message;
        }

        if (!string.IsNullOrWhiteSpace(data.avatarProfile))
        {
            ApplyAvatarProfile(data.avatarProfile);
        }

        ApplyEmotion(data.emotion);
        PlayAnimation(data.animation);

        if (speechController != null && !string.IsNullOrWhiteSpace(data.message))
        {
            speechController.Speak(data.message, currentAvatarSex);
        }
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

    private void ApplyAvatarProfile(string avatarProfile)
    {
        Debug.Log($"Applying avatar profile: {avatarProfile}");

        DisableAllAvatars();

        switch (avatarProfile)
        {
            case "young_adult_support":
                activeAvatar = youngAdultAvatar != null ? youngAdultAvatar : neutralSupportAvatar;
                currentAvatarSex = "M"; 
                break;

            case "older_adult_support":
                activeAvatar = olderAdultAvatar != null ? olderAdultAvatar : neutralSupportAvatar;
                currentAvatarSex = "M"; // Tu OlderAdultAvatar es el médico masculino.
                break;

            case "adult_support":
                activeAvatar = adultAvatar != null ? adultAvatar : neutralSupportAvatar;
                currentAvatarSex = "F"; // AdultAvatar es mujer.
                break;

            case "neutral_support":
            default:
                activeAvatar = neutralSupportAvatar;
                currentAvatarSex = "F"; // NeutralSupportAvatar es mujer.
                break;
        }

        if (activeAvatar == null)
        {
            Debug.LogWarning("No avatar profile assigned. Please assign avatars in AvatarBridge inspector.");
            return;
        }

        activeAvatar.SetActive(true);

        Debug.Log($"Active avatar GameObject: {activeAvatar.name}");
        Debug.Log($"Current avatar sex for TTS: {currentAvatarSex}");

        animator = activeAvatar.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"The active avatar '{activeAvatar.name}' does not have an Animator component.");
        }
    }

    private void DisableAllAvatars()
    {
        if (avatarRoot != null)
        {
            foreach (Transform child in avatarRoot)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (youngAdultAvatar != null) youngAdultAvatar.SetActive(false);
        if (adultAvatar != null) adultAvatar.SetActive(false);
        if (olderAdultAvatar != null) olderAdultAvatar.SetActive(false);
        if (neutralSupportAvatar != null) neutralSupportAvatar.SetActive(false);

        activeAvatar = null;
        animator = null;
    }

    private void ApplyContextScale(PatientContextMessage context)
    {
        if (activeAvatar == null)
            return;

        if (context.age >= 60)
        {
            activeAvatar.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        }
        else
        {
            activeAvatar.transform.localScale = Vector3.one;
        }
    }

    private void ApplyEmotion(string emotion)
    {
        Debug.Log($"Applying emotion: {emotion}");
    }

    private void PlayAnimation(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName))
        {
            animationName = "talk";
        }

        Debug.Log($"Playing animation: {animationName}");

        if (animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        animator.ResetTrigger("idle");
        animator.ResetTrigger("talk");
        animator.ResetTrigger("celebrate");
        animator.ResetTrigger("empathetic");

        animator.SetTrigger(animationName);
    }
}

[System.Serializable]
public class AvatarMessage
{
    public string message;
    public string avatarProfile;
    public string emotion;
    public string animation;
    public string voiceStyle;
}