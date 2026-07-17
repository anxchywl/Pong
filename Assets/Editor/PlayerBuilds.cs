using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Pong.Editor
{
    /// Builds a player. One entry per target so a build is a command rather than a memory of which
    /// boxes were ticked, and the same command runs from the menu or from a machine with no menus.
    public static class PlayerBuilds
    {
        private const string DefaultRoot = "Builds";

        [MenuItem("Pong/Build/macOS")]
        public static void MacOS()
        {
            Build(BuildTarget.StandaloneOSX, Path.Combine(Root(), "macOS", "Pong.app"));
        }

        [MenuItem("Pong/Build/Windows")]
        public static void Windows()
        {
            Build(BuildTarget.StandaloneWindows64, Path.Combine(Root(), "Windows", "Pong.exe"));
        }

        [MenuItem("Pong/Build/Linux")]
        public static void Linux()
        {
            Build(BuildTarget.StandaloneLinux64, Path.Combine(Root(), "Linux", "Pong"));
        }

        [MenuItem("Pong/Build/Android")]
        public static void Android()
        {
            Build(BuildTarget.Android, Path.Combine(Root(), "Android", "Pong.apk"));
        }

        /// Produces the Xcode project. Signing and a device take it from there; Unity's part of an
        /// iOS build ends here.
        [MenuItem("Pong/Build/iOS")]
        public static void IOS()
        {
            Build(BuildTarget.iOS, Path.Combine(Root(), "iOS"));
        }

        /// Builds the scenes the project actually ships, in the order it ships them, so a build can
        /// never disagree with the editor about what the game is.
        private static void Build(BuildTarget target, string location)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes: the player would have nothing to open");
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = location,
                target = target,
                options = BuildOptions.None
            });

            BuildSummary summary = report.summary;
            Debug.Log(
                $"{target} build {summary.result}: {summary.totalErrors} errors, " +
                $"{summary.totalWarnings} warnings, {summary.totalSize / 1048576} MB, " +
                $"{summary.totalTime.TotalSeconds:0} s -> {location}"
            );

            // batchmode exits zero on a failed build unless something says otherwise, and a build
            // that reports success while having produced nothing is worse than one that fails
            if (summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        /// Honours -buildRoot so a machine can put the player somewhere other than the working copy.
        private static string Root()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(arguments, "-buildRoot");
            return flag >= 0 && flag + 1 < arguments.Length ? arguments[flag + 1] : DefaultRoot;
        }
    }
}
