using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace gwomp;

public static class Pluginfo
{
    public const string guid = "gwomp";
    public const string name = "gwomp";
    public const string version = "1.0.0";
}

[BepInPlugin(Pluginfo.guid, Pluginfo.name, Pluginfo.version)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    #region Config
    // PocketHammer
    private static ConfigEntry<PocketSide> DesiredPocket;
    
    // Developer Mode
    private static ConfigEntry<SettingsManager.BuildType> GameBuildType; 
    #endregion

    private void Awake()
    {
        Logger = base.Logger;
        
        #region Config
        // PocketHammer
        DesiredPocket = Config.Bind("gwomp", "DesiredPocket", PocketSide.Left,
            "Which pocket the hammer spawns in at the start of your run");
        
        // Developer Mode
        GameBuildType = Config.Bind("gwomp", "GameBuildType", SettingsManager.BuildType.dev,
            "Whether to put the game in standard or dev mode");
        #endregion

        Harmony harmony = new(Pluginfo.guid);
        harmony.PatchAll();
        foreach (var method in harmony.GetPatchedMethods())
            Logger.LogDebug($"{Pluginfo.guid}: patched ${method.Name}");

        Logger.LogInfo($"Plugin {Pluginfo.guid} is loaded!");
    }

    // private void Update()
    // {
    //     #region Developer Mode
    //     var buildType = Traverse.Create<SettingsManager>().Field(nameof(SettingsManager.buildType));
    //     buildType.SetValue(GameBuildType.Value);
    //     #endregion
    // }


    #region AntiAntiCheat
    [HarmonyPatch(typeof(CommandConsole))]
    public static class CommandConsolePatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPatch("EnableCheatsCommand")]
        [HarmonyPatch("ExecuteCommand", [typeof(string), typeof(bool), typeof(bool)])]
        [HarmonyPostfix]
        public static void AllowCheatsPostfix(object __instance)
        {
            CommandConsole.cheatsEnabled = true;
            CommandConsole.hasCheated = false;
        }
    }

    [HarmonyPatch(typeof(CL_SaveManager), "LoadSession")]
    public static class SaveManagerPatches
    {
        public static void Postfix()
        {
            CommandConsole.cheatsEnabled = true;
            CommandConsole.hasCheated = false;
        }
    }

    [HarmonyPatch(typeof(SteamManager), "UploadGamemodeResults")]
    public static class SteamManagerPatches
    {
        public static bool Prefix()
        {
            return true;
        }
    }
    #endregion

    #region Unlimited Trinkets
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
    #endregion
    
    #region PocketHammer
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
    #endregion
    
    #region Developer Mode
    [HarmonyPatch(typeof(UT_BuildTypeToggler), "Awake")]
    public static class Patch_Awake
    {
        public static bool Prefix()
        {
            return !(GameBuildType.Value == SettingsManager.BuildType.dev);
        }
    }
    #endregion
    
    #region Consistent Sensitivity
    [HarmonyPatch(typeof(ENT_Player), nameof(ENT_Player.GrabPropUpdate))]
    public static class Patch_GrabPropUpdate
    {
        public static void Postfix(ref float ___camSpeed)
        {
            ___camSpeed = 1.0f;
        }
    }
    #endregion
}
