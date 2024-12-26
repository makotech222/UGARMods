using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Linq;
using BepInEx.Configuration;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using World.SceneObject;
using World;

namespace UltimateGeneralAmericanRevolutionMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            UGARPatch.Log = Log;

            LoadConfig();

            Harmony.CreateAndPatchAll(typeof(UGARPatch));
        }

        public void LoadConfig()
        {
            UGARPatch.Money = Config.Bind("1. Cheats", "Set Money each day", -1, "-1 to disable").Value;
            UGARPatch.ConstructionMaterial = Config.Bind("1. Cheats", "Add to Construction Material each day", -1, "-1 to disable").Value;
            UGARPatch.Provision = Config.Bind("1. Cheats", "Add to Provisions each day", -1, "-1 to disable").Value;
            UGARPatch.Ammunition = Config.Bind("1. Cheats", "Add to Ammunition each day", -1, "-1 to disable").Value;
            UGARPatch.GeneralCommandRadius = Config.Bind("1. Cheats", "Set all general command radius", -1, "-1 to disable. 5000 will cover about the whole map.").Value;
            UGARPatch.MaxGeneralStat = Config.Bind("1. Cheats", "Set all generals to max stats", false, "").Value;
            UGARPatch.MaxOfficerStat = Config.Bind("1. Cheats", "Set all officers to max stats", false, "").Value;
            UGARPatch.MaxWeaponInventory = Config.Bind("1. Cheats", "Max Weapons", false, "If weapon/naval cannon inventory is > 0, set to max amount. Will not add weapons that you don't have yet.").Value;
        }
    }

    public class UGARPatch
    {
        public static ManualLogSource Log { get; set; }

        public static int Money { get; set; }
        public static int ConstructionMaterial { get; set; }
        public static int Provision { get; set; }
        public static int Ammunition { get; set; }
        public static bool MaxGeneralStat { get; set; }
        public static float GeneralCommandRadius { get; set; }
        public static bool MaxOfficerStat { get; set; }
        public static bool MaxWeaponInventory { get; set; }

        [HarmonyPatch(typeof(Country), "DailyUpdate")]
        [HarmonyPostfix]
        private static void Postfix(Country __instance)
        {
            if (SceneManager.instance.PlayerCountry != __instance)
            {
                Log.LogMessage("Update for enemy country? Skip");
                return;
            }
            Log.LogMessage("Setting Daily Update");
            __instance.inventory.money = Money == -1 ? __instance.inventory.money : Money;
            Log.LogMessage($"Setting Money: {Money}");
            if (ConstructionMaterial > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.ConstructionMaterial, 99999f), World.EItemSource.Cheat);
            if (Provision > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Provision, 99999f), World.EItemSource.Cheat);
            if (Ammunition > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Ammunition, 99999f), World.EItemSource.Cheat);

            for (int i = 0; i < __instance.armyManager.generals.Count; i++)
            {
                var general = __instance.armyManager.generals[i];
                general.commandRadius = GeneralCommandRadius == -1 ? general.commandRadius : GeneralCommandRadius;
                if (MaxGeneralStat)
                {
                    general.PromoteAttribute(EAttribute.Intelligence, 100.0f);
                    general.PromoteAttribute(EAttribute.Endurance, 100.0f);
                    general.PromoteAttribute(EAttribute.Perception, 100.0f);
                    general.PromoteAttribute(EAttribute.Charisma, 100.0f);
                    general.PromoteAttribute(EAttribute.Willpower, 100.0f);
                    general.UpdateAttributes();
                }
            }
            if (MaxOfficerStat) { 
            for (int i = 0; i < __instance.armyManager.units.Count; i++)
            {
                var unit = __instance.armyManager.units[i];
                unit.PromoteAttribute(EAttribute.Intelligence, 300.0f);
                unit.PromoteAttribute(EAttribute.Charisma, 300.0f);
                unit.PromoteAttribute(EAttribute.Perception, 300.0f);
                unit.PromoteAttribute(EAttribute.Willpower, 300.0f);
                unit.PromoteAttribute(EAttribute.Endurance, 300.0f);
                unit.UpdateAttributes();

            }
            for (int i = 0; i < __instance.armyManager.garrisons.Count; i++)
            {
                var unit = __instance.armyManager.garrisons[i];
                unit.PromoteAttribute(EAttribute.Intelligence, 300.0f);
                unit.PromoteAttribute(EAttribute.Charisma, 300.0f);
                unit.PromoteAttribute(EAttribute.Perception, 300.0f);
                unit.PromoteAttribute(EAttribute.Willpower, 300.0f);
                unit.PromoteAttribute(EAttribute.Endurance, 300.0f);
                unit.UpdateAttributes();

            }
        }
            if (MaxWeaponInventory)
            {
                for (int i = 0; i < __instance.inventory.itemStorage.items.Count; i++)
                {
                    var item = __instance.inventory.itemStorage.items[i];
                    if ((item.asset.ToString().Contains("WeaponTemplate")) ||
                        (item.asset.ToString().Contains("CannonModule")))
                    {
                        if (item.count > 0)
                        {
                            item.count = 99999;
                        }
                    }
                }
            }
        }
    }
}
