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
    private static ManualLogSource _logger;
    private static Harmony _harmony;

    public static ConfigEntry<SettingsManager.BuildType> GameBuildType;

    private void Awake()
    {
        _logger = Logger;

        GameBuildType = Config.Bind(Pluginfo.guid, nameof(GameBuildType), SettingsManager.BuildType.dev,
            "Whether to put the game in standard or dev mode");

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
        var buildType = Traverse.Create<SettingsManager>().Field(nameof(SettingsManager.buildType));
        buildType.SetValue(SettingsManager.BuildType.standard);
        _logger.LogInfo($"Plugin {Pluginfo.guid} has unloaded");
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
            return GameBuildType.Value != SettingsManager.BuildType.dev;
        }
    }
}
