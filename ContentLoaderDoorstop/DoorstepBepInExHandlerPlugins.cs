using Mono.Cecil;

namespace ContentLoaderDoorstop;

public static partial class DoorstepBepInExHandler {

    static bool IsBepInExPlugin(string directory) {
        string[] files = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);
        foreach (string file in files) {
            try {
                if (!FileIsBepInExPlugin(file)) { continue; }
                return true;
            } catch (Exception) { /* ignored */ }
        }
        return false;
    }
    
    static bool FileIsBepInExPlugin(string file) {
        using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(file);
        
        bool hasTargetDLLs = false;
        bool hasPatch = false;
        foreach (ModuleDefinition module in assemblyDefinition.Modules) {
            foreach (TypeDefinition type in module.Types) {
                foreach (CustomAttribute attribute in type.CustomAttributes) {
                    if (attribute.AttributeType.Name == "BepInPlugin") { return true; }
                }
                foreach (PropertyDefinition property in type.Properties) {
                    if (property.Name != "TargetDLLs") { continue; }
                    if (property.PropertyType.FullName != "System.Collections.Generic.IEnumerable`1<System.String>") { continue; }
                    MethodDefinition? method = property.GetMethod;
                    if (method == null) { continue; }
                    if (!method.IsStatic) { continue; }
                    hasTargetDLLs = true;
                    break;
                }
                foreach (MethodDefinition method in type.Methods) {
                    if (method.Name != "Patch") { continue; }
                    if (!method.IsStatic) { continue; }
                    if (method.ReturnType.FullName != "System.Void") { continue; }
                    if (method.Parameters.Count != 1) { continue; }
                    if (method.Parameters[0].ParameterType.FullName != "Mono.Cecil.AssemblyDefinition") { continue; }
                    hasPatch = true;
                    break;
                }
            }
        }
        return (hasTargetDLLs && hasPatch);
    }
}