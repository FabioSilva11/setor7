using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class Setor07AndroidBuild
{
    public static void Build()
    {
        EditorUserSettings.desiredImportWorkerCount = 1;
        Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = 1;
        AndroidExternalToolsSettings.maxJvmHeapSize = 1024;
        AndroidExternalToolsSettings.stopGradleDaemonsOnExit = true;
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
            AndroidExternalToolsSettings.jdkRootPath = javaHome;
        var android = NamedBuildTarget.Android;
        PlayerSettings.productName = "Setor 07";
        PlayerSettings.SetApplicationIdentifier(android, "com.kirito.setor07");
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCodeGeneration(android, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetIl2CppCompilerConfiguration(android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
        AssetDatabase.SaveAssets();

        const string lobbyIcons = "Assets/Blocks/MultiplayerSession/Scenes/QuickJoin/Icons";
        foreach (var iconPath in Directory.EnumerateFiles(lobbyIcons, "*.png"))
        {
            var assetPath = iconPath.Replace(Path.DirectorySeparatorChar, '/');
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath) == null)
                throw new Exception("Invalid UI texture import: " + assetPath);
        }

        var output = Path.GetFullPath("Builds/Android/Setor07.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Shooter/TestScenes/[BB] Shooter.unity" },
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.CompressWithLz4
        };
        Debug.Log("SETOR07_BUILD_START " + output);
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("SETOR07_BUILD_RESULT " + report.summary.result +
            " errors=" + report.summary.totalErrors +
            " size=" + report.summary.totalSize +
            " elapsed=" + report.summary.totalTime);
        if (report.summary.result != BuildResult.Succeeded || !File.Exists(output))
            throw new Exception("Android build failed. Inspect the build log for the first error.");
    }
}

public sealed class Setor07LowMemoryGradle : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 1000;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var root = Directory.GetParent(path).FullName;
        var properties = Path.Combine(root, "gradle.properties");
        var keys = new[]
        {
            "org.gradle.jvmargs", "org.gradle.workers.max",
            "org.gradle.parallel", "org.gradle.daemon"
        };
        var existing = File.Exists(properties) ? File.ReadAllLines(properties) : Array.Empty<string>();
        var retained = existing.Where(line =>
            !keys.Any(key => line.TrimStart().StartsWith(key + "=", StringComparison.Ordinal)));
        File.WriteAllLines(properties, retained.Concat(new[]
        {
            "org.gradle.jvmargs=-Xmx1024m -XX:MaxMetaspaceSize=384m -XX:+UseSerialGC -Dfile.encoding=UTF-8",
            "org.gradle.workers.max=1",
            "org.gradle.parallel=false",
            "org.gradle.daemon=false"
        }));
    }
}
