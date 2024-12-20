using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using Steamworks;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace BepInExFix;

[ContentWarningPlugin("Computery.ContentLoader.BepInEx", "1.0", true)]
[BepInPlugin("Computery.ContentLoader.BepInEx", "Content Loader BepInEx", "1.0.0")]
public class ContentLoaderBepInEx : BaseUnityPlugin {
    public void Awake() { Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly()); }
}

[HarmonyPatch(typeof(Plugin))]
public class PluginPatchAgain {
    public void PatchNonBepinExShit(Assembly assembly)
    {
        if (DirectoryHasBepInExPlugin(Path.GetDirectoryName(assembly.Location))) { return; }
        Harmony.CreateAndPatchAll(assembly);
    }
    
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Plugin.LoadAssemblyFromFile))]
    static IEnumerable<CodeInstruction> LoadAssemblyFromFilePatch(IEnumerable<CodeInstruction> instructions) {
        CodeMatcher? codeMatcher = new CodeMatcher(instructions);
        codeMatcher.SearchForward(i =>
            i.opcode == OpCodes.Callvirt &&
            i.OperandIs(AccessTools.Method(typeof(Harmony), nameof(Harmony.PatchAll), new[] { typeof(Assembly) })));
        codeMatcher.RemoveInstruction();
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
            AccessTools.Method(typeof(PluginPatchAgain), nameof(PatchNonBepinExShit))));
        
        codeMatcher.Advance(-1);
        codeMatcher.RemoveInstruction();
        codeMatcher.Advance(-2);
        codeMatcher.RemoveInstruction();
        
        codeMatcher.ThrowIfInvalid("LoadAssemblyFromFilePatch did not work!");
        return codeMatcher.InstructionEnumeration();
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
}

[HarmonyPatch(typeof(PluginHandler))]
public class PluginHandlerPatch {
    static bool hasShownError = false;
    [HarmonyPrefix]
    [HarmonyPatch("OnItemInstalled")]
    public static bool OnItemInstalled(ItemInstalled_t param) {
        if (!hasShownError) {
            Modal.ShowError("Content Loader Disables Hot Loading",
                "Content Loader disables the hot loading of mods.\n" +
                "Please restart the game to load the new mod.\n" +
                "This is done to prevent issues with both mod loaders.\n" +
                "Sorry!\n\n" +
                "This error will only show once per game session.");
            hasShownError = true;
        }
        return false; 
    }
}

[HarmonyPatch(typeof(SelectedPluginUI))]
public class PluginPatch {
    [HarmonyPatch(nameof(SelectedPluginUI.DrawPlugin))]
    [HarmonyPostfix]
    public static void DrawPluginPatchFunction(SelectedPluginUI __instance, ModManagerPlugin plugin, MainMenuModManagerPage page) {
        AllowBepInExModUploading allowBepInExModUploading = GameHandler.Instance.SettingsHandler.GetSetting<AllowBepInExModUploading>();
        if (!allowBepInExModUploading.Value) { return; }
        
        Debug.Log("DrawPluginPatchFunction: " + __instance + " " + plugin + " " + page);
        
        if (plugin.PluginExternal != null) {
            string folder = Path.Combine(plugin.PluginExternal.assembly.Location, "..");
            string SteamWorkshopFile = Path.Combine(folder, "SteamWorkshop");
            if (File.Exists(SteamWorkshopFile)) {
                string subscribedId = File.ReadAllText(SteamWorkshopFile);
                
                // Check if we're the uploader
                PublishedFileId_t subscribedId_t = new PublishedFileId_t(ulong.Parse(subscribedId));
                bool weArePublisher = PluginHandler.PublishedPlugins.Any(p => p.PublishedFileId == subscribedId_t);
                if (weArePublisher) {
                    PluginPublished pluginPublished = PluginHandler.PublishedPlugins.First(p => p.PublishedFileId == subscribedId_t);
                    plugin.PluginPublished = pluginPublished;
                    
                    PluginLocal pluginLocal = new PluginLocal {
                        directory = folder,
                        assembly = plugin.PluginExternal.assembly,
                        info = plugin.PluginExternal.InfoFromAssembly.Value
                    };
                    plugin.PluginLocal = pluginLocal;
                    __instance.DrawPublishScreen(plugin, page);
                }
            }
            else {
                PluginLocal pluginLocal = new PluginLocal {
                    directory = folder,
                    assembly = plugin.PluginExternal.assembly,
                    info = plugin.PluginExternal.InfoFromAssembly.Value
                };
                plugin.PluginLocal = pluginLocal;
                __instance.DrawPublishScreen(plugin, page);
            }
        }
    }
}

[ContentWarningSetting]
public class AllowBepInExModUploading : BoolSetting, IExposedSetting {
    public override void ApplyValue() { }
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => "Allow BepInEx Mod Uploading";
    protected override bool GetDefaultValue() {
        return false;
    }
}