using System.Reflection;
using Mono.Cecil;
using Newtonsoft.Json;
using Sirenix.Utilities;
using Steamworks;

namespace ContentLoaderDoorstop;

public static partial class DoorstepBepInExHandler {
    public static void StartBepInEx() {
        Assembly.LoadFrom(BepInExPreload).GetType("Doorstop.Entrypoint").GetMethod("Start")!.Invoke(null, null);
    }

    public static void CheckForContentLoaderUpdate() {
        if (!File.Exists(InactiveDoorstopAssemblyLocation)) { return; }
        if (File.Exists(DoorstopAssemblyLocation)) {
            byte[] contentLoaderDoorstopAssemblyBytes = File.ReadAllBytes(InactiveDoorstopAssemblyLocation);
            byte[] currentContentLoaderDoorstopAssemblyBytes = File.ReadAllBytes(DoorstopAssemblyLocation);
            if (!contentLoaderDoorstopAssemblyBytes.SequenceEqual(currentContentLoaderDoorstopAssemblyBytes)) { File.Delete(DoorstopAssemblyLocation); }
        }
    }
    
    public static void InstallBepInExPlugins() {
        SteamAPI.Init();
        PublishedFileId_t[] subscribedItems = GetSubscribedItems();
        if (!CheckForContentLoader(subscribedItems)) { UninstallBepInEx(); throw new Exception("ContentLoader not found"); }
        
        List<string> subscribedItemPaths = [];
        
        foreach (PublishedFileId_t publishedFileIdT in subscribedItems) {
            ulong publishedFileId = publishedFileIdT.m_PublishedFileId;
            
            if (!SteamUGC.GetItemInstallInfo(publishedFileIdT, out _, out string directory, 2048U, out _)) { continue; }
            if (!IsBepInExPlugin(directory)) { continue; }
            
            string rootDirectory = Path.Combine(directory, SpecialFolderNameRoot);
            if (Directory.Exists(rootDirectory)) { subscribedItemPaths.AddRange(InstallRootMod(rootDirectory)); }
            else {
                string pluginFolder = Path.Combine(BepInExPluginFolder, publishedFileId.ToString());
                Copy(directory, pluginFolder);
                subscribedItemPaths.AddRange(Directory.GetFiles(pluginFolder, "*", SearchOption.AllDirectories));
            }
        }

        string[] previousSubscribedItemPaths = [];
        if (File.Exists(SubscribedItemsPath)) {
            string subscribedItemsJson = File.ReadAllText(SubscribedItemsPath);
            previousSubscribedItemPaths = JsonConvert.DeserializeObject<string[]>(subscribedItemsJson) ?? [];
        }
        foreach (string previousSubscribedItemPath in previousSubscribedItemPaths) {
            if (subscribedItemPaths.Contains(previousSubscribedItemPath)) { continue; }
            
            try {
                if (File.Exists(previousSubscribedItemPath)) {
                    File.Delete(previousSubscribedItemPath);
                }
                string directory = Path.GetDirectoryName(previousSubscribedItemPath)!;
                if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any()) {
                    Directory.Delete(directory);
                }
            } catch (Exception) { /* ignored */ }
        }
        
        File.WriteAllText(SubscribedItemsPath, JsonConvert.SerializeObject(subscribedItemPaths));
    }

    private static string[] InstallRootMod(string directory) {
        string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        string[] targetFiles = new string[files.Length];
        for (int j = 0; j < files.Length; j++) {
            string file = files[j];
            string relativeFile = file.Substring(directory.Length + 1);
            string targetFile = Path.Combine(RootGameFolder, relativeFile);
            targetFiles[j] = targetFile;
            try {
                if (File.Exists(targetFile)) { File.Delete(targetFile); }
                string targetDirectory = Path.GetDirectoryName(targetFile)!;
                if (!Directory.Exists(targetDirectory)) { Directory.CreateDirectory(targetDirectory); }
                File.Copy(file, targetFile);
            } catch (Exception) { /* ignored */ }
        }
        return targetFiles;
    }
    
    private static bool CheckForContentLoader(PublishedFileId_t[] subscribedItems) {
        if (Directory.Exists(LocalInstallPath)) { return true; }
        foreach (PublishedFileId_t t in subscribedItems) { if (t.m_PublishedFileId == 3387698650UL) { return true; } }
        return false;
    }
    
    private static void UninstallBepInEx() { 
        if (Directory.Exists(BepInExFolder)) { Directory.Delete(BepInExFolder, true); }
        if (File.Exists(DoorStopConfig)) { File.Delete(DoorStopConfig); }
        if (File.Exists(DoorVersion)) { File.Delete(DoorVersion); }
    }
}