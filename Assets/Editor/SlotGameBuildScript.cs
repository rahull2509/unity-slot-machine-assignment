using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SlotMachineGame.Editor
{
    /// <summary>
    /// Command-line and menu automation for generating scenes and executing WebGL & Desktop builds.
    /// </summary>
    public static class SlotGameBuildScript
    {
        private const string MainScenePath = "Assets/Scenes/SlotMachineScene.unity";
        private const string WebGLOutputDir = "Build/WebGL";
        private const string WindowsOutputDir = "Build/Windows";

        [MenuItem("SlotMachine/Build WebGL", false, 10)]
        public static void BuildWebGL()
        {
            EnsureSceneReady();

            if (!Directory.Exists(WebGLOutputDir))
            {
                Directory.CreateDirectory(WebGLOutputDir);
            }

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = WebGLOutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log("[SlotGameBuildScript] Starting WebGL Build...");
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[SlotGameBuildScript] WebGL Build SUCCEEDED: {summary.totalSize} bytes in {summary.totalTime.TotalSeconds:F1}s at '{WebGLOutputDir}'");
            }
            else
            {
                Debug.LogError($"[SlotGameBuildScript] WebGL Build FAILED with {summary.totalErrors} errors.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("SlotMachine/Build Windows Standalone", false, 11)]
        public static void BuildWindows()
        {
            EnsureSceneReady();

            if (!Directory.Exists(WindowsOutputDir))
            {
                Directory.CreateDirectory(WindowsOutputDir);
            }

            string exePath = Path.Combine(WindowsOutputDir, "SlotMachineGame.exe");

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[SlotGameBuildScript] Starting Windows Build...");
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[SlotGameBuildScript] Windows Build SUCCEEDED: {summary.totalSize} bytes at '{exePath}'");
            }
            else
            {
                Debug.LogError($"[SlotGameBuildScript] Windows Build FAILED with {summary.totalErrors} errors.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        public static void EnsureSceneReady()
        {
            if (!File.Exists(MainScenePath))
            {
                Debug.Log("[SlotGameBuildScript] Scene not found. Building playable scene first...");
                SlotGameSceneBuilder.BuildPlayableScene();
            }
        }
    }
}
