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

        }
    }

    public class UGARPatch
    {
        public static ManualLogSource Log { get; set; }

        [HarmonyPatch(typeof(Country), "DailyUpdate")]
        [HarmonyPostfix]
        private static void Postfix(Country __instance)
        {
            Log.LogMessage("Setting Daily Update");
            Log.LogMessage($"Reputation: {SceneManager.instance.Reputation}");
            
            __instance.inventory.money = 9999999;
            __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.ConstructionMaterial, 99999f), World.EItemSource.Cheat);
            __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Provision, 99999f), World.EItemSource.Cheat);
            __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Ammunition, 99999f), World.EItemSource.Cheat);
            for (int i = 0; i < __instance.armyManager.generals.Count; i++)
            {
                Log.LogMessage("Updating General " + i.ToString());
                var general = __instance.armyManager.generals[i];
                general.commandRadius = 500f;
                general.PromoteAttribute(EAttribute.Intelligence, 100.0f);
                general.PromoteAttribute(EAttribute.Endurance, 100.0f);
                general.PromoteAttribute(EAttribute.Perception, 100.0f);
                general.PromoteAttribute(EAttribute.Charisma, 100.0f);
                general.PromoteAttribute(EAttribute.Willpower, 100.0f);
                general.UpdateAttributes();

            }
            for (int i = 0; i < __instance.armyManager.units.Count; i++)
            {
                Log.LogMessage("Updating unit " + i.ToString());
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
                Log.LogMessage("Updating garrison " + i.ToString());
                var unit = __instance.armyManager.garrisons[i];
                unit.PromoteAttribute(EAttribute.Intelligence, 300.0f);
                unit.PromoteAttribute(EAttribute.Charisma, 300.0f);
                unit.PromoteAttribute(EAttribute.Perception, 300.0f);
                unit.PromoteAttribute(EAttribute.Willpower, 300.0f);
                unit.PromoteAttribute(EAttribute.Endurance, 300.0f);
                unit.UpdateAttributes();

            }
            for (int i = 0; i < __instance.inventory.itemStorage.items.Count; i++)
            {
                var item = __instance.inventory.itemStorage.items[i];
                if ((item.asset.ToString().Contains("WeaponTemplate")) ||
                    (item.asset.ToString().Contains("CannonModule")))
                {
                    if (item.count > 0)
                    {
                        Log.LogMessage($"Setting item {item.asset.ToString()} count.");
                        item.count = 99999;
                    }
                }
            }
            //Log.LogMessage("Ships: " + __instance.inventory.shipStorage.ships.Count.ToString());
            //for (int i = 0; i < __instance.inventory.shipStorage.ships.Count; i++)
            //{
            //    Log.LogMessage("Updating ship " + i.ToString());
            //    var ship = __instance.inventory.shipStorage.ships[i];
            //    ship.Model.PromoteAttribute(EAttribute.Intelligence, 300.0f);
            //    ship.Model.PromoteAttribute(EAttribute.Charisma, 300.0f);
            //    ship.Model.PromoteAttribute(EAttribute.Perception, 300.0f);
            //    ship.Model.PromoteAttribute(EAttribute.Willpower, 300.0f);
            //    ship.Model.PromoteAttribute(EAttribute.Endurance, 300.0f);

                //}
        }
    }
}
