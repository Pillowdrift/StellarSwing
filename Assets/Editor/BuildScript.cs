using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Release builds for Steam. From the menu (Stellar Swing > Build ...) or the command line:
//   Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildAll
// Output goes to Builds/<Platform>/, one folder per Steam depot.
public static class BuildScript
{
  const string Root = "Builds";
  const string Product = "Stellar Swing";

  [MenuItem("Stellar Swing/Build All")]
  public static void BuildAll()
  {
    bool ok = BuildMac() & BuildWindows() & BuildLinux();
    if (Application.isBatchMode)
      EditorApplication.Exit(ok ? 0 : 1);
  }

  [MenuItem("Stellar Swing/Build Mac")]
  public static bool BuildMac()
  {
    // One app for both Intel and Apple Silicon Macs
    UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.Build.OSArchitecture.x64ARM64;
    // Steam launches StellarSwing.app/Contents/MacOS/Stellar Swing (set in Steamworks)
    return Build(BuildTarget.StandaloneOSX, "Mac", "StellarSwing.app");
  }

  // Executable names must match the launch options set in Steamworks for app 1319990
  [MenuItem("Stellar Swing/Build Windows")]
  public static bool BuildWindows()
  {
    return Build(BuildTarget.StandaloneWindows64, "Windows", Product + ".exe");
  }

  [MenuItem("Stellar Swing/Build Linux")]
  public static bool BuildLinux()
  {
    return Build(BuildTarget.StandaloneLinux64, "Linux", "StellarSwing.x86_64");
  }

  static bool Build(BuildTarget target, string folder, string executable)
  {
    string dir = Path.Combine(Root, folder);
    // Start clean so files from older builds never get uploaded
    if (Directory.Exists(dir))
      Directory.Delete(dir, true);
    Directory.CreateDirectory(dir);

    var options = new BuildPlayerOptions
    {
      scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
      locationPathName = Path.Combine(dir, executable),
      target = target,
      options = BuildOptions.None,
    };

    var report = BuildPipeline.BuildPlayer(options);
    var summary = report.summary;
    Debug.Log($"[BuildScript] {folder}: {summary.result}, {summary.totalErrors} errors, {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0}s");

    // Unity leaves debug symbols next to the player; they don't belong in the Steam depot
    foreach (var junk in Directory.GetDirectories(dir, "*_BurstDebugInformation_DoNotShip")
                         .Concat(Directory.GetDirectories(dir, "*_BackUpThisFolder_ButDontShipItWithYourGame")))
      Directory.Delete(junk, true);

    return summary.result == BuildResult.Succeeded;
  }
}
