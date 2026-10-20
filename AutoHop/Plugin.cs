using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace AutoHop;

public static class Pluginfo
{
    public const string guid = "win.deathis.autohop";
    public const string name = "Auto Hop";
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

    private void Update()
    {
        if (InputManager.GetButton("Jump").Pressed)
        {
            var player = ENT_Player.GetPlayer();
            var canJumpBoost = player.IsCrouching() && player.curBuffs.GetBuff("addJumpBoost") > 0.0 && player.IsGrounded(true) && player.GetLowestGripStrength() > 2.0; // Taken from decompiled code
            if ((player.IsGrounded(true) || player.IsHanging())
                && !player.IsFlying() && !player.IsSwimming() && !player.noclip)
                player.Jump(canJumpBoost);
        }
    }
}
