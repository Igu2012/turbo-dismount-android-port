using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class PortBuild
{
    static string[] Scenes => new[]
    {
        "Assets/TurboDismount/Scenes/LoadingScreen.unity",
        "Assets/TurboDismount/Scenes/Vehicles.unity",
        "Assets/TurboDismount/Scenes/UI/UIBuilder.unity",
        "Assets/TurboDismount/Scenes/HUD.unity"
    }.Concat(Directory.GetFiles("Assets/TurboDismount/Data_ios/Levels", "*.unity")
        .Select(path => path.Replace('\\', '/'))).ToArray();

    static void Prepare(string platform)
    {
        PlayerSettings.companyName = "Igu2012";
        PlayerSettings.productName = "Turbo Dismount";
            PlayerSettings.bundleVersion = "1.0.0";
        EditorBuildSettings.scenes = Scenes.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
        if (platform == "Android")
        {
            AndroidExternalToolsSettings.jdkRootPath = "/usr/lib/jvm/java-11-openjdk-amd64";
            AndroidExternalToolsSettings.sdkRootPath = "/home/ubuntu/turbo-dismount-assets/android-sdk";
            AndroidExternalToolsSettings.ndkRootPath = "/home/ubuntu/turbo-dismount-assets/android-sdk/ndk/23.1.7779620";
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Icon.png");
            if (icon != null) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { icon }, IconKind.Application);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.igu2012.turbodismountfan");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.useCustomKeystore = false;
        }
        else if (platform == "WebGL")
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.nameFilesAsHashes = false;
            PlayerSettings.WebGL.dataCaching = true;
        }
    }

    static void Run(BuildTarget target, string output, BuildOptions options = BuildOptions.None)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = Scenes,
            locationPathName = output,
            target = target,
            options = options
        });
        Debug.Log($"BUILD_RESULT target={target} result={report.summary.result} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings} size={report.summary.totalSize}");
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + report.summary.result);
    }

    public static void BuildWebGL()
    {
        Prepare("WebGL");
        Run(BuildTarget.WebGL, "Builds/WebGL");
    }

    public static void BuildAndroid()
    {
        Prepare("Android");
        Run(BuildTarget.Android, "Builds/Android/TurboDismount-FanPort-ARM64.apk");
    }
}

#if UNITY_WEBGL
public class DesktopControlsForWebGL : MonoBehaviour
{
    void Awake()
    {
        if (Application.isMobilePlatform) return;
        foreach (var canvas in FindObjectsOfType<Canvas>(true))
        {
            var n = canvas.name.ToLowerInvariant();
            if (n.Contains("mobile") || n.Contains("touch") || n.Contains("joystick")) canvas.gameObject.SetActive(false);
        }
    }
}
#endif

public static class PortBuildInfo
{
    [MenuItem("Turbo Dismount Port/Build WebGL")]
    public static void MenuWebGL() => PortBuild.BuildWebGL();
    [MenuItem("Turbo Dismount Port/Build Android ARM64")]
    public static void MenuAndroid() => PortBuild.BuildAndroid();
}

public class WebGLDesktopUi : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL
        if (Application.isMobilePlatform) return;
        foreach (var canvas in FindObjectsOfType<Canvas>(true))
        {
            var n = canvas.name.ToLowerInvariant();
            if (n.Contains("mobile") || n.Contains("touch") || n.Contains("joystick")) canvas.gameObject.SetActive(false);
        }
#endif
    }
}
