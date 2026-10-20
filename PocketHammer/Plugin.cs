using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PocketHammer;

public static class Pluginfo
{
    public const string guid = "win.deathis.pockethammer";
    public const string name = "Pocket Hammer";
    public const string version = "1.0.0";
}

[BepInPlugin(Pluginfo.guid, Pluginfo.name, Pluginfo.version)]
public class Plugin : BaseUnityPlugin
{
    private static ManualLogSource _logger;
    private static Harmony _harmony;

    public static ConfigEntry<PocketSide> DesiredPocket;

    private void Awake()
    {
        _logger = Logger;

        DesiredPocket = Config.Bind(Pluginfo.guid, nameof(DesiredPocket), PocketSide.Left,
            "Which pocket the hammer spawns in at the start of your run");

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

    [HarmonyPatch(typeof(CL_GameManager), "Start")]
    public static class Patch_Start {
        public static void Postfix()
        {
            ENT_Player.GetInventory().pockets[DesiredPocket.Value != 0 ? 1 : 0]
                .pouch.AddItemToPouch(ENT_Player.GetInventory().GetItemByTag("hammer"));
        }
    }

    public enum PocketSide
    {
        Left,
        Right,
    }
}
