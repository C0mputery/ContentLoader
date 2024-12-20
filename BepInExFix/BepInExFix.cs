using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BepInExFix;

[ContentWarningPlugin("Computery.BepInExFix", "1.0", true)]
[BepInPlugin("Computery.BepInExFix", "BepInEx Fix", "1.0.0")]
public class BepInExFix : BaseUnityPlugin {
    public void Awake() {
       Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}

[HarmonyPatch(typeof(Plugin))]
public class PluginPatch {
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Assembly), nameof(Assembly.LoadFrom), new[] { typeof(string) })]
    public static Assembly LoadFrom(string assemblyFile) =>
        throw new NotImplementedException("Harmony didn't fill the reverse patch for Assembly.LoadFrom(string)???? Oh no!");
    
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Plugin.LoadAssemblyFromFile))]
    static IEnumerable<CodeInstruction> LoadAssemblyFromFilePatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchForward(i => i.opcode == OpCodes.Call && i.OperandIs(AccessTools.Method(typeof(Assembly), nameof(Assembly.LoadFrom), [typeof(string)])))
            .RemoveInstruction()
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PluginPatch), nameof(LoadFrom))))
            .ThrowIfInvalid("LoadAssemblyFromFile did not work!")
            .InstructionEnumeration();
    }
}