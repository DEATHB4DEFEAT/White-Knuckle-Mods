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
    private new static ManualLogSource _logger;

    private void Awake()
    {
        _logger = Logger;

        Harmony harmony = new(Pluginfo.guid);
        harmony.PatchAll();
        #if DEBUG
        foreach (var method in harmony.GetPatchedMethods())
            _logger.LogDebug($"{Pluginfo.guid}: patched ${method.Name}");
        #endif

        _logger.LogInfo($"Plugin {Pluginfo.guid} is loaded!");
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
