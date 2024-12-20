using System.Reflection;
using Mono.Cecil;
using Steamworks;

namespace ContentLoaderDoorstop;

public class DoorstepBepInExHandler {
    private static readonly string DoorstopAssemblyLocation = Assembly.GetExecutingAssembly().Location;
    private static readonly string RootGameFolder = Path.Combine(DoorstopAssemblyLocation, "..", "..", "..");
    private static readonly string InactiveDoorstopAssemblyLocation = Path.Combine(RootGameFolder, "Plugins", "ContentLoader", "BepInEx", "ContentLoaderDoorstop.dll");
    private static readonly string BepInExPreload = Path.Combine(DoorstopAssemblyLocation, "..", "BepInEx.Preloader.dll");
    private static readonly string ManagedFolder = Path.Combine(RootGameFolder, "Content Warning_Data", "Managed");
    private static readonly string SteamworksAssembly = Path.Combine(ManagedFolder, "com.rlabrecque.steamworks.net.dll");
    private static readonly string BepInExPluginFolder = Path.Combine(RootGameFolder, "BepInEx", "plugins");
    private static readonly string ContentWarningPluginFolder = Path.Combine(RootGameFolder, "plugins");

    public static void UpdateBepInEx() {
        if (!File.Exists(InactiveDoorstopAssemblyLocation)) { return; }
        if (File.Exists(DoorstopAssemblyLocation)) {
            byte[] contentLoaderDoorstopAssemblyBytes = File.ReadAllBytes(InactiveDoorstopAssemblyLocation);
            byte[] currentContentLoaderDoorstopAssemblyBytes = File.ReadAllBytes(DoorstopAssemblyLocation);
            if (!contentLoaderDoorstopAssemblyBytes.SequenceEqual(currentContentLoaderDoorstopAssemblyBytes)) {
                File.Delete(DoorstopAssemblyLocation); 
            }
        }
    }

    public static void StartBepInEx() { Assembly.LoadFrom(BepInExPreload).GetType("Doorstop.Entrypoint").GetMethod("Start").Invoke(null, null); }
    
    public static void MoveBepInExPlugins() {
        Directory.CreateDirectory(BepInExPluginFolder);
        
        MoveLocalPlugins();
        SteamAPI.Init();
        PublishedFileId_t[] subscribedItems = GetSubscribedItems();
        
        RemoveUnsubscribedPlugins(subscribedItems);
        MoveSubscribedPlugins(subscribedItems);
    }
    
    private static void MoveLocalPlugins() {
        string[] directories = Directory.GetDirectories(DoorstepBepInExHandler.ContentWarningPluginFolder);
        foreach (string directory in directories) {
            if (!DirectoryHasBepInExPlugin(directory)) { continue; }
            string targetDirectory = Path.Combine(DoorstepBepInExHandler.BepInExPluginFolder, Path.GetFileName(directory));
            Copy(directory, targetDirectory);
        }
    }
    
    private static void RemoveUnsubscribedPlugins(PublishedFileId_t[] subscribedItems) {
        string[] directories = Directory.GetDirectories(DoorstepBepInExHandler.BepInExPluginFolder);
        foreach (string directory in directories) {
            if (!File.Exists(Path.Combine(directory, "SteamWorkshop"))) { continue; }
            string subscribedId = File.ReadAllText(Path.Combine(directory, "SteamWorkshop"));
            if (subscribedItems.Any(t => t.ToString() == subscribedId)) { continue; }
            Directory.Delete(directory, true);
        }
    }
    
    private static void MoveSubscribedPlugins(PublishedFileId_t[] subscribedItems) {
        Assembly.LoadFrom(DoorstepBepInExHandler.SteamworksAssembly);
        
        foreach (PublishedFileId_t t in subscribedItems) {
            if (!SteamUGC.GetItemInstallInfo(t, out _, out string directory, 2048U, out _)) { continue; }
            if (!DirectoryHasBepInExPlugin(directory)) { continue; }
            string targetDirectory = Path.Combine(DoorstepBepInExHandler.BepInExPluginFolder, t.ToString());
            Copy(directory, targetDirectory);
            File.WriteAllText(Path.Combine(targetDirectory, "SteamWorkshop"), t.ToString());
        }
    }

    static bool DirectoryHasBepInExPlugin(string directory) {
        string[] files = Directory.GetFiles(directory, "*.dll");
        foreach (string file in files) {
            if (!FileIsBepInExPlugin(file)) { continue; }
            return true;
        }
        return false;
    }
    
    static bool FileIsBepInExPlugin(string file) {
        using AssemblyDefinition? assemblyDefinition = AssemblyDefinition.ReadAssembly(file);
        foreach (ModuleDefinition? module in assemblyDefinition.Modules) {
            foreach (TypeDefinition? type in module.Types) {
                foreach (CustomAttribute? attribute in type.CustomAttributes) {
                    if (attribute.AttributeType.Name == "BepInPlugin") { return true; }
                }
            }
        }
        return false;
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
    
    private static PublishedFileId_t[] GetSubscribedItems() {
        try {
            uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
            PublishedFileId_t[] pvecPublishedFileID = new PublishedFileId_t[numSubscribedItems];
            SteamUGC.GetSubscribedItems(pvecPublishedFileID, numSubscribedItems);
            return pvecPublishedFileID;
        } catch (Exception) { return []; }
    }
}