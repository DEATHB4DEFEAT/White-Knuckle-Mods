using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace UnlimitedTrinkets;

public static class Pluginfo
{
    public const string guid = "win.deathis.UnlimitedTrinkets";
    public const string name = "Unlimited Trinkets";
    public const string version = "1.1.0";
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

    [HarmonyPatch(typeof(UI_TrinketPicker), "SelectTrinket")]
    public static class Patch_SelectTrinket
    {
        public static bool Prefix(UI_TrinketPicker __instance, Trinket t)
        {
            __instance.ShowTrinketDescription(t);
            var trinketCost = 0;
            var bindingCost = 1;
            var alreadySelected = __instance.selectedTrinkets.Any(x => x.name == t.name);
            var lclick = Input.GetMouseButtonUp(1);
            if (alreadySelected && (lclick || t.isBinding))
            {
                if (__instance.selectedTrinkets.Remove(t))
                    AudioManager.PlayUISound(t.isBinding ? __instance.bindingDeselectClip : __instance.trinketDeselectClip);
            }
            else
                AudioManager.PlayUISound(t.isBinding ? __instance.bindingSelectClip : __instance.trinketSelectClip);

            foreach (var selectedTrinket in __instance.selectedTrinkets)
            {
                if (!selectedTrinket.isBinding)
                    trinketCost += selectedTrinket.cost;
                else
                    bindingCost += selectedTrinket.cost;
            }
            if (!lclick || (t.isBinding && !alreadySelected))
            {
                if (t.isBinding)
                {
                    if (!alreadySelected)
                    {
                        bindingCost += t.cost;
                        __instance.selectedTrinkets.Add(t);
                    }
                }
                else
                {
                    trinketCost += t.cost;
                    __instance.selectedTrinkets.Add(t);
                }
            }
            __instance.playButton.interactable = true;
            __instance.costText.text = "";
            var curGamemode = AccessTools.Field(typeof(UI_TrinketPicker), "currentGamemode").GetValue(__instance)
	            is M_Gamemode gamemode ? gamemode.GetGamemodeName() : null;
            StatManager.saveData.SetGamemodeTrinkets(curGamemode, [.. __instance.selectedTrinkets.Select(x => x.name)]);
            AccessTools.Method(typeof(UI_TrinketPicker), "UpdateTrinketActivation").Invoke(__instance, null);
            return false;
        }

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

    [HarmonyPatch(typeof(UI_TrinketPicker_Icon), "SetTrinket")]
    public static class Patch_SetTrinket
    {
	    private static void Postfix(UI_TrinketPicker_Icon __instance, Trinket t, UI_TrinketPicker picker = null)
	    {
		    if (picker == null || t.isBinding) return;
		    var val = (RectTransform)__instance.transform.Find("CountText");
		    if (val != null) return;

		    val = (RectTransform)Instantiate(picker.costText.gameObject, __instance.transform).transform;
		    var component = val.GetComponent<TMP_Text>();
		    val.name = "CountText";
		    val.pivot = new Vector2(0f, 0f);
		    val.anchoredPosition = new Vector2(0f, -64f);
		    val.SetSiblingIndex(3);
		    component.margin = new Vector4(0f, 0f, 0f, 0f);
		    component.text = "";
		    component.color = Color.white;
		    component.fontSize = 16f;
		    component.raycastTarget = false;
		    component.alignment = TextAlignmentOptions.BottomLeft;
		    var trinketCountHandler = val.AddComponent<TrinketCountHandler>();
		    trinketCountHandler.text = component;
		    trinketCountHandler.parent = picker;
		    trinketCountHandler.trinketName = t.name;
	    }
    }
}

public class TrinketCountHandler : MonoBehaviour
{
	public TMP_Text text;
	public UI_TrinketPicker parent;
	public string trinketName;

	private void Update()
	{
		var gameMode = StatManager.saveData.GetGameMode(AccessTools.Field(typeof(UI_TrinketPicker), "currentGamemode")
			.GetValue(parent) is M_Gamemode gamemode ? gamemode.GetGamemodeName() : null);
		if (gameMode != null)
		{
			var num = gameMode.activeTrinkets.Count(x => x == trinketName);
			text.text = num <= 1 ? "" : $"{num}x";
		}
	}
}
