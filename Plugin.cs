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
using Fight;
using World.Configuration;
using I2.Loc;
using UnityEngine;
using TerrainComposer2;

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

            UGARPatch.ExportWeapons = Config.Bind("2. Weapon Modification", "Export Weapon JSON", true, "If true, will export and overwrite all the weapon json files.").Value;
            UGARPatch.EnableWeaponsModification = Config.Bind("2. Weapon Modification", "Enable Weapon Modification", false, "If true, will read weapon json files and overwrite values in game.").Value;
            Config.GetSetting<bool>("2. Weapon Modification", "Export Weapon JSON").Value = false;
            Config.Save();
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
        public static bool ExportWeapons { get; set; }
        public static bool EnableWeaponsModification { get; set; }

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
            if (MaxOfficerStat)
            {
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

        [HarmonyPatch(typeof(GameConfig))]
        [HarmonyPatch("Load")]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        private static void Postfix_WeaponConfig2(GameConfig __result)
        {
            foreach (var weapon in __result.weapons.firearms)
            {
                string folder = "./bepinex/config/weapons/";
                string file = $"{weapon.name}.json";
                if (ExportWeapons)
                {
                    Log.LogInfo("Exporting weapon json data");
                    var weaponTemplateEx = new WeaponTemplateEx()
                    {
                        name = weapon.name,
                        effectiveRange = weapon.effectiveRange,
                        ammoCost = weapon.ammoCost,
                        baseReload = weapon.baseReload,
                        damage = weapon.damage,
                        damageDegradation = weapon.damageDegradation.keys.Select(y => y.m_Value).ToArray(),
                        meleeDamage = weapon.meleeDamage,
                        numberOfShots = weapon.numberOfShots,
                        price = weapon.price,
                        productionCost = weapon.productionCost,
                        randLow = weapon.randLow,
                        randHi = weapon.randHi,
                        speedModifier = weapon.speedModifier,
                        collateralRadius = weapon.collateralRadius,
                        altitude = weapon.altitude,
                        allowCollect = weapon.allowCollect,
                        cannonRequiredStaff = weapon.cannonRequiredStaff,
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(weaponTemplateEx, new JsonSerializerOptions() { WriteIndented = true });
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    File.WriteAllText($"{folder}{file}", json);
                }
                if (EnableWeaponsModification)
                {
                    Log.LogInfo($"Modifying weapon data for {weapon.name}");
                    var filetext = File.ReadAllText($"{folder}{file}");
                    var template = JsonSerializer.Deserialize<WeaponTemplateEx>(filetext);
                    weapon.effectiveRange = template.effectiveRange;
                    weapon.ammoCost = template.ammoCost;
                    weapon.baseReload = template.baseReload;
                    weapon.damage = template.damage;
                    weapon.collateralRadius = template.collateralRadius;
                    weapon.meleeDamage = template.meleeDamage;
                    weapon.numberOfShots = template.numberOfShots;
                    weapon.price = template.price;
                    weapon.productionCost = template.productionCost;
                    weapon.randLow = template.randLow;
                    weapon.randHi = template.randHi;
                    weapon.speedModifier = template.speedModifier;
                    weapon.altitude = template.altitude;
                    weapon.allowCollect = template.allowCollect;
                    weapon.cannonRequiredStaff = template.cannonRequiredStaff;
                    var keys = new Keyframe[weapon.damageDegradation.keys.Count];
                    for (int i = 0; i < weapon.damageDegradation.keys.Count; i++)
                    {
                        var keyframe = weapon.damageDegradation.keys[i];
                        keyframe.m_Value = template.damageDegradation[i];
                        keys[i] = keyframe;
                    }
                    weapon.damageDegradation.SetKeys(keys);
                }
            }
        }
    }

    public class WeaponTemplateEx
    {
        public string name { get; set; }
        public float effectiveRange { get; set; }
        public float ammoCost { get; set; }
        public float baseReload { get; set; }
        public float damage { get; set; }
        public float collateralRadius { get; set; }
        public float[] damageDegradation { get; set; }
        public float meleeDamage { get; set; }
        public int numberOfShots { get; set; }
        public int price { get; set; }
        public float productionCost { get; set; }
        public float randLow { get; set; }
        public float randHi { get; set; }
        public float speedModifier { get; set; }
        public float altitude { get; set; }
        public bool allowCollect { get; set; }
        public int cannonRequiredStaff { get; set; }
    }
}