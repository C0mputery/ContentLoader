using System.Reflection;
using ContentLoaderDoorstop;
using Steamworks;
using UnityEngine;

namespace Doorstop;

public class Entrypoint {
    public static void Start() {
        try {
            DoorstepBepInExHandler.LoadNecessaryAssemblies();
            DoorstepBepInExHandler.CheckForContentLoaderUpdate();
            DoorstepBepInExHandler.InstallBepInExPlugins();
            DoorstepBepInExHandler.StartBepInEx();
        }
        catch (Exception e) {
            File.WriteAllText("DoorstopError.txt", e.ToString());
        }
    }
}