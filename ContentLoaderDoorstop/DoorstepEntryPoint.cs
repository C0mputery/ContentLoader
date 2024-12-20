using System.Reflection;
using ContentLoaderDoorstop;
using UnityEngine;

namespace Doorstop;

public class Entrypoint {
    public static void Start() {
        try {
            DoorstepBepInExHandler.UpdateBepInEx();
            DoorstepBepInExHandler.MoveBepInExPlugins();
            DoorstepBepInExHandler.StartBepInEx();
        }
        catch (Exception e) {
            File.WriteAllText("DoorstopError.txt", e.ToString());
        }
    }
}