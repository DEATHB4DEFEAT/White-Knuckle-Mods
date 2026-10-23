using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AntiAntiCheat;

public static class Pluginfo
{
    public const string guid = "win.deathis.antianticheat";
    public const string name = "Anti Anti-Cheat";
    public const string version = "1.0.0";
}

[BepInPlugin(Pluginfo.guid, Pluginfo.name, Pluginfo.version)]
public class Plugin : BaseUnityPlugin
{
    private static ManualLogSource _logger;
    private static Harmony _harmony;

    private void Awake()
    {
        _logger = Logger;

        _harmony = new(Pluginfo.guid);
        _harmony.PatchAll();
        #if DEBUG
        foreach (var method in _harmony.GetPatchedMethods())
            _logger.LogInfo($"{Pluginfo.guid}: patched {method.Name}");
        #endif

        _logger.LogInfo($"Plugin {Pluginfo.guid} has loaded");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _logger.LogInfo($"Plugin {Pluginfo.guid} has unloaded");
    }


    [HarmonyPatch(typeof(CommandConsole))]
    public static class CommandConsolePatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPatch("EnableCheatsCommand")]
        [HarmonyPatch("ExecuteCommand", [typeof(string), typeof(bool), typeof(bool)])]
        [HarmonyPostfix]
        public static void AllowCheatsPostfix(object __instance)
        {
            CommandConsole.cheatsEnabled = false;
            CommandConsole.hasCheated = false;
        }
    }

    [HarmonyPatch(typeof(CL_SaveManager), "LoadSession")]
    public static class SaveManagerPatches
    {
        public static void Postfix()
        {
            CommandConsole.cheatsEnabled = false;
            CommandConsole.hasCheated = false;
        }
    }

    [HarmonyPatch(typeof(CommandConsole), "MayUseCommand")]
    public class CommandConsolePatch
    {
        public static bool Prefix(ref object __result)
        {
            __result = Enum.Parse(typeof(CommandConsole)
                .GetNestedType("CommandUsageKind", BindingFlags.NonPublic), "MayUse");

            return false;
        }
    }
}
