using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ContentLoader;

[ContentWarningPlugin("Computery.ContentLoader", "3.2", true)]
public class ContentWarningEntryPoint {
    private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
    private static readonly string RootGameFolder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    private static readonly string ContentLoaderPluginFolder = Path.GetFullPath(Path.Combine(AssemblyLocation, ".."));
    private static readonly string DoorstopAssemblyLocation = Path.Combine(RootGameFolder, "BepInEx", "Core", "ContentLoaderDoorstop.dll");
    private static readonly string InactiveDoorstopAssemblyLocation = Path.Combine(ContentLoaderPluginFolder, "BepInEx", "BepInEx", "Core", "ContentLoaderDoorstop.dll");
    private static readonly string InactiveBepInEx = Path.Combine(ContentLoaderPluginFolder, "BepInEx");
    
    static ContentWarningEntryPoint() { CheckBepInEx(); }

    static void CheckBepInEx() {
        Logger.Log("Content Warning Entrypoint Hit");
        
        if (!File.Exists(InactiveDoorstopAssemblyLocation)) {
            Logger.LogError("ContentLoaderDoorstop.dll Not Found, Something is So Very Wrong");
            return;
        }
        
        if (File.Exists(DoorstopAssemblyLocation)) {
            byte[] inactiveDoorstopAssemblyBytes = File.ReadAllBytes(InactiveDoorstopAssemblyLocation);
            byte[] doorstopAssemblyBytes = File.ReadAllBytes(DoorstopAssemblyLocation);
            if (!inactiveDoorstopAssemblyBytes.SequenceEqual(doorstopAssemblyBytes)) {
                Logger.LogWarning("ContentLoaderDoorstop.dll is not correct, reinstalling bepInEx...");
                InstallBepInEx();
            }
        }
        else {
            Logger.LogWarning("ContentLoaderDoorstop.dll is missing, installing bepInEx...");
            InstallBepInEx();
        }
        
        Logger.Log("BepInEx is already installed");
    }
    
    static void InstallBepInEx() {
        Copy(InactiveBepInEx, RootGameFolder);
        Logger.Log("BepInEx Installed");
        Modal.Show("BepInEx Has Been Installed/Updated", "A restart is required to load the plugins.", [new ModalOption("Restart", Restart)]);
    }
    
    static void Restart() {
        string[] args = Environment.GetCommandLineArgs();
        Process.Start(args[0], string.Join(" ", args[1..]));
        Application.Quit();
    }

    static void Copy(string sourceDir, string targetDir) {
        Directory.CreateDirectory(targetDir);
        foreach (string file in Directory.GetFiles(sourceDir)) {
            try {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                if (File.Exists(targetFile)) { File.Delete(targetFile); }
                File.Copy(file, targetFile);
            }
            catch (Exception) { /* ignored */ }
        }
        foreach (string directory in Directory.GetDirectories(sourceDir)) {
            Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
    }
}