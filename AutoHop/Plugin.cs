using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AutoHop;

public static class Pluginfo
{
    public const string guid = "win.deathis.autohop";
    public const string name = "Auto Hop";
    public const string version = "1.0.2";
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

    private static readonly FieldInfo HasJumped = AccessTools.Field(typeof(ENT_Player), "hasJumped");
    private bool _wasGroundedOrHanging;
    private bool _isGrounded;
    private void Update()
    {
        var player = ENT_Player.GetPlayer();
        if (!player)
        {
            _isGrounded = false;
            return;
        }

        var isGroundedOrHanging = player.IsGrounded() || player.IsHanging();
        var justLanded = _isGrounded && isGroundedOrHanging && !_wasGroundedOrHanging;
        _wasGroundedOrHanging = isGroundedOrHanging;
        _isGrounded = true;
        if (!InputManager.GetButton("Jump").Pressed || !justLanded
            || player.IsFlying() || player.IsSwimming() || player.noclip)
            return;

        HasJumped.SetValue(player, false);
    }
}
