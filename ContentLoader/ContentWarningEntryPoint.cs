using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ContentLoader;

[ContentWarningPlugin("Computery.ContentLoader", "1.0", true)]
public class ContentWarningEntryPoint {
    private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
    private static readonly string RootGameFolder = Path.Combine(Application.dataPath, "..");
    private static readonly string ContentLoaderPluginFolder = Path.Combine(AssemblyLocation, "..");
    private static readonly string DoorstopAssemblyLocation = Path.Combine(RootGameFolder, "BepInEx", "Core", "ContentLoaderDoorstop.dll");
    private static readonly string InactiveDoorstopAssemblyLocation = Path.Combine(ContentLoaderPluginFolder, "BepInEx", "BepInEx", "Core", "ContentLoaderDoorstop.dll");
    private static readonly string InactiveBepInEx = Path.Combine(ContentLoaderPluginFolder, "BepInEx");
    
    static ContentWarningEntryPoint() {
        UpdateBepInEx();
    }

    static void UpdateBepInEx() {
        Logger.Log("Content Warning Entrypoint Hit");
        
        if (!File.Exists(InactiveDoorstopAssemblyLocation)) {
            Logger.LogError("ContentLoaderDoorstop.dll Not Found");
            return;
        }
        
        bool updateBepInEx = false;
        if (File.Exists(DoorstopAssemblyLocation)) {
            byte[] inactiveDoorstopAssemblyBytes = File.ReadAllBytes(InactiveDoorstopAssemblyLocation);
            byte[] doorstopAssemblyBytes = File.ReadAllBytes(DoorstopAssemblyLocation);
            if (!inactiveDoorstopAssemblyBytes.SequenceEqual(doorstopAssemblyBytes)) {
                Logger.LogWarning("ContentLoaderDoorstop.dll is not correct, reinstalling bepInEx...");
                updateBepInEx = true;
            }
        }
        else {
            Logger.LogWarning("ContentLoaderDoorstop.dll is missing, installing bepInEx...");
            updateBepInEx = true;
        }
        
        if (updateBepInEx) {
            Copy(InactiveBepInEx, RootGameFolder);
            Logger.Log("BepInEx Installed");
            Modal.Show("BepInEx Has Been Installed/Updated", "A restart is required to load the plugins.", [new ModalOption("Restart", Restart)]);
            return;
        }
        
        Logger.Log("BepInEx is already installed");
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