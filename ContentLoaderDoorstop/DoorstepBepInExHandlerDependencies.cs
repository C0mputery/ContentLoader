using System.Reflection;
using Steamworks;

namespace ContentLoaderDoorstop;

public static partial class DoorstepBepInExHandler {
    public static void LoadNecessaryAssemblies() {
        Assembly.LoadFrom(SteamworksAssembly);
        Assembly.LoadFrom(NewtonsoftAssembly);
    }
    private static PublishedFileId_t[] GetSubscribedItems() {
        try {
            uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
            PublishedFileId_t[] pvecPublishedFileID = new PublishedFileId_t[numSubscribedItems];
            SteamUGC.GetSubscribedItems(pvecPublishedFileID, numSubscribedItems);
            return pvecPublishedFileID;
        } catch (Exception) { return []; }
    }
    
    // Stolen from stackoverflow
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