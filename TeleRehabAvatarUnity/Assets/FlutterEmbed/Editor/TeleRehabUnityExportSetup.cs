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
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        AssetDatabase.SaveAssets();

        ProjectExporterBatchmode.ExportProjectAndroid();
    }
}
