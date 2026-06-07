using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class TeleRehabUnityExportSetup
{
    private const string MainScenePath = "Assets/Scenes/MainScene.unity";

    public static void ConfigureAndExportAndroid()
    {
        Debug.Log("Configuring TeleRehabAvatarUnity for Flutter Android export.");

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScenePath, true)
        };

        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.Android,
            ScriptingImplementation.IL2CPP
        );
        // Unity 6 requires minimum Android API level 25 or higher
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;

        // Let Unity use the installed/recommended target SDK automatically
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        AssetDatabase.SaveAssets();

        ProjectExporterBatchmode.ExportProjectAndroid();
    }
}
