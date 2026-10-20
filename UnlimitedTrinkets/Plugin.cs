using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace UnlimitedTrinkets;

public static class Pluginfo
{
    public const string guid = "win.deathis.UnlimitedTrinkets";
    public const string name = "Unlimited Trinkets";
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

    [HarmonyPatch(typeof(UI_TrinketPicker), "SelectTrinket")]
    public static class Patch_SelectTrinket
    {
        public static void Postfix(UI_TrinketPicker __instance)
        {
            __instance.playButton.interactable = true;
        }
    }

    [HarmonyPatch(typeof(UI_TrinketPicker), "UpdateTrinketActivation")]
    public static class Patch_UpdateTrinketActivation
    {
        public static void Postfix(UI_TrinketPicker __instance)
        {
            __instance.playButton.interactable = true;
            __instance.costText.text = "";
        }
    }
}
