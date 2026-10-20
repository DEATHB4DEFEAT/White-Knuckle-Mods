using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DevMode;

public static class Pluginfo
{
    public const string guid = "win.deathis.DevMode";
    public const string name = "Developer Mode";
    public const string version = "1.0.0";
}

[BepInPlugin(Pluginfo.guid, Pluginfo.name, Pluginfo.version)]
public class Plugin : BaseUnityPlugin
{
    private new static ManualLogSource _logger;

    private static ConfigEntry<SettingsManager.BuildType> GameBuildType; 

    private void Awake()
    {
        _logger = Logger;

        GameBuildType = Config.Bind("gwomp", "GameBuildType", SettingsManager.BuildType.dev,
            "Whether to put the game in standard or dev mode");

        Harmony harmony = new(Pluginfo.guid);
        harmony.PatchAll();
        #if DEBUG
        foreach (var method in harmony.GetPatchedMethods())
            _logger.LogDebug($"{Pluginfo.guid}: patched ${method.Name}");
        #endif

        _logger.LogInfo($"Plugin {Pluginfo.guid} is loaded!");
    }

    // private void Update()
    // {
    //     var buildType = Traverse.Create<SettingsManager>().Field(nameof(SettingsManager.buildType));
    //     buildType.SetValue(GameBuildType.Value);
    // }


    [HarmonyPatch(typeof(UT_BuildTypeToggler), "Awake")]
    public static class Patch_Awake
    {
        public static bool Prefix()
        {
            return !(GameBuildType.Value == SettingsManager.BuildType.dev);
        }
    }
}
