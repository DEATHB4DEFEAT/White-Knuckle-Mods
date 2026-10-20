using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ConsistentSensitivity;

public static class Pluginfo
{
    public const string guid = "win.deathis.ConsistentSensitivity";
    public const string name = "Consistent Sensitivity";
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
            _logger.LogDebug($"{Pluginfo.guid}: patched ${method.Name}");
        #endif

        _logger.LogInfo($"Plugin {Pluginfo.guid} has loaded");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _logger.LogInfo($"Plugin {Pluginfo.guid} has unloaded");
    }

    [HarmonyPatch(typeof(ENT_Player), nameof(ENT_Player.GrabPropUpdate))]
    public static class Patch_GrabPropUpdate
    {
        public static void Postfix(ref float ___camSpeed)
        {
            ___camSpeed = 1.0f;
        }
    }
}
