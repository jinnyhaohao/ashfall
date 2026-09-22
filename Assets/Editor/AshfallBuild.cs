using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AshfallBuild
{
    const string ScenePath = "Assets/Scenes/Ashfall.unity";

    [MenuItem("Ashfall/Build Windows")]
    public static void BuildWindows()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.defaultScreenWidth=1280;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
        PlayerSettings.defaultScreenHeight=720;
        PlayerSettings.fullScreenMode=UnityEngine.FullScreenMode.Windowed;
        Directory.CreateDirectory("Builds/DepthAndMotion");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/DepthAndMotion/Ashfall.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Ashfall build failed: " + report.summary.result);
    }
}
