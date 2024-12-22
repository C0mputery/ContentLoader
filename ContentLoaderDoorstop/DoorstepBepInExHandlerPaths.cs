using System.Reflection;
using Steamworks;

namespace ContentLoaderDoorstop;

public static partial class DoorstepBepInExHandler {
    private static readonly string DoorstopAssemblyLocation = Assembly.GetExecutingAssembly().Location;
    private static readonly string BepInExCoreFolder = Path.GetDirectoryName(DoorstopAssemblyLocation)!;
    private static readonly string RootGameFolder = Path.GetFullPath(Path.Combine(BepInExCoreFolder, "..", ".."));
    private static readonly string BepInExPreload = Path.Combine(BepInExCoreFolder, "BepInEx.Preloader.dll");
    private static readonly string ManagedFolder = Path.Combine(RootGameFolder, "Content Warning_Data", "Managed");
    private static readonly string SteamworksAssembly = Path.Combine(ManagedFolder, "com.rlabrecque.steamworks.net.dll");
    private static readonly string NewtonsoftAssembly = Path.Combine(ManagedFolder, "Newtonsoft.Json.dll");
    private static readonly string BepInExFolder = Path.Combine(RootGameFolder, "BepInEx");
    private static readonly string BepInExPluginFolder = Path.Combine(BepInExFolder, "plugins");
    private static readonly string ContentWarningPluginFolder = Path.Combine(RootGameFolder, "Plugins");
    private static readonly string DoorStopConfig = Path.Combine(RootGameFolder, "doorstop_config.ini");
    private static readonly string DoorVersion = Path.Combine(RootGameFolder, ".doorstop_version");
    private static readonly string LocalInstallPath = Path.Combine(ContentWarningPluginFolder, "ContentLoader");
    private static readonly string SubscribedItemsPath = Path.Combine(RootGameFolder, "subscribedItems.json");
    private const string SpecialFolderNameRoot = "Root";
}