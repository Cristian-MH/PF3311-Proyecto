using UnityEngine;
using TMPro;

public class AvatarBridge : MonoBehaviour
{
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

        private void Awake()
    {
        DisableAllAvatars();

        if (messageText != null)
        {
            messageText.text = "Cargando perfil del paciente...";
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
                break;

            case "older_adult_support":
                activeAvatar = olderAdultAvatar != null ? olderAdultAvatar : neutralSupportAvatar;
                break;

            case "adult_support":
                activeAvatar = adultAvatar != null ? adultAvatar : neutralSupportAvatar;
                break;

            case "neutral_support":
            default:
                activeAvatar = neutralSupportAvatar;
                break;
        }

        if (activeAvatar == null)
        {
            Debug.LogWarning("No avatar profile assigned. Please assign avatars in AvatarBridge inspector.");
            return;
        }

        activeAvatar.SetActive(true);

        animator = activeAvatar.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"The active avatar '{activeAvatar.name}' does not have an Animator component.");
        }
    }

    private void DisableAllAvatars()
    {
        if (youngAdultAvatar != null) youngAdultAvatar.SetActive(false);
        if (adultAvatar != null) adultAvatar.SetActive(false);
        if (olderAdultAvatar != null) olderAdultAvatar.SetActive(false);
        if (neutralSupportAvatar != null) neutralSupportAvatar.SetActive(false);
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

        // For humanoid avatars, emotion should be represented mainly through:
        // - animation
        // - facial blendshapes later
        // - voice style later
        //
        // We do not change material color here because humanoid models have multiple renderers/materials.
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

    public void ShowMessageOnCurrentAvatar(string message, string emotion, string animation)
{
    if (messageText != null)
    {
        messageText.text = message;
    }

    ApplyEmotion(emotion);
    PlayAnimation(animation);
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