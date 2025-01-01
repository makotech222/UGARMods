using System.Text.Json;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using World;
using World.Configuration;
using World.SceneObject;

namespace UltimateGeneralAmericanRevolutionMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, "1.3.0")]
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
            UGARPatch.Wood = Config.Bind("1. Cheats", "Add to Wood each day", -1, "-1 to disable").Value;
            UGARPatch.Horse = Config.Bind("1. Cheats", "Add to Horse each day", -1, "-1 to disable").Value;
            UGARPatch.GeneralCommandRadius = Config.Bind("1. Cheats", "Set all general command radius", -1f, "-1 to disable. 5000 will cover about the whole map.").Value;
            UGARPatch.GeneralSpottingRadius = Config.Bind("1. Cheats", "Set all general spotting radius", -1f, "-1 to disable. 5000 will cover about the whole map.").Value;
            UGARPatch.MaxGeneralStat = Config.Bind("1. Cheats", "Set all generals to max stats", false, "").Value;
            UGARPatch.MaxOfficerStat = Config.Bind("1. Cheats", "Set all officers to max stats", false, "").Value;
            UGARPatch.MaxWeaponInventory = Config.Bind("1. Cheats", "Max Weapons", false, "If weapon/naval cannon inventory is > 0, set to max amount. Will not add weapons that you don't have yet.").Value;

            UGARPatch.Difficulty_NegativeReputationModifier = Config.Bind("1. Cheats", "Negative Reputation Modifier (for all difficulties)", -1f, "Modifies the negative reputation modifier that is applied by selected difficulty. 0.75 is very easy").Value;
            UGARPatch.Difficulty_MiningBonus = Config.Bind("1. Cheats", "Mining Bonus (for all difficulties)", -1f, "Modifies the mining bonus that is applied by selected difficulty.").Value;
            UGARPatch.Difficulty_NavalMaintenance = Config.Bind("1. Cheats", "Ship Maintenance Modifier (for all difficulties)", -1f, "Modifies the ship maintenance modifier that is applied by selected difficulty.").Value;
            UGARPatch.Difficulty_RecruitsModifier = Config.Bind("1. Cheats", "Recruits Modifier (for all difficulties)", -1f, "Modifies the recruits modifier that is applied by selected difficulty.").Value;
            UGARPatch.Difficulty_SpecialistBonus = Config.Bind("1. Cheats", "Specialist Bonus Modifier (for all difficulties)", -1f, "Modifies the specialist bonus modifier that is applied by selected difficulty.").Value;

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
        public static int Wood { get; set; }
        public static int Horse { get; set; }
        public static float Difficulty_NegativeReputationModifier { get; set; }
        public static float Difficulty_MiningBonus { get; set; }
        public static float Difficulty_NavalMaintenance { get; set; }
        public static float Difficulty_RecruitsModifier { get; set; }
        public static float Difficulty_SpecialistBonus { get; set; }
        public static bool MaxGeneralStat { get; set; }
        public static float GeneralCommandRadius { get; set; }
        public static float GeneralSpottingRadius { get; set; }
        public static bool MaxOfficerStat { get; set; }
        public static bool MaxWeaponInventory { get; set; }
        public static bool ExportWeapons { get; set; }
        public static bool EnableWeaponsModification { get; set; }

        [HarmonyPatch(typeof(Country), "DailyUpdate")]
        [HarmonyPostfix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
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
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.ConstructionMaterial, ConstructionMaterial), World.EItemSource.Cheat);
            if (Provision > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Provision, Provision), World.EItemSource.Cheat);
            if (Ammunition > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Ammunition, Ammunition), World.EItemSource.Cheat);
            if (Wood > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Wood, Wood), World.EItemSource.Cheat);
            if (Horse > 0)
                __instance.inventory.itemStorage.Append(new World.CountItem(World.EInventoryType.Horse, Horse), World.EItemSource.Cheat);

            for (int i = 0; i < __instance.armyManager.generals.Count; i++)
            {
                var general = __instance.armyManager.generals[i];
                Log.LogMessage($"General Spot: {general.spottingRange}");
                general.commandRadius = GeneralCommandRadius == -1f ? general.commandRadius : GeneralCommandRadius;
                general.spottingRange = GeneralSpottingRadius == -1f ? general.spottingRange : GeneralSpottingRadius;
                general.basicSpottingRange = GeneralSpottingRadius == -1f ? general.basicSpottingRange : GeneralSpottingRadius;
                general.minSpottingRange = GeneralSpottingRadius == -1f ? general.minSpottingRange : GeneralSpottingRadius;
                general.UpdateGeneralLOSRanges();
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static void Postfix_WeaponConfig2(GameConfig __result)
        {
            var diffSettings = __result.difficultyConfig.difficultySettings.ToArray();
            foreach (var diff in diffSettings)
            {
                diff.reputationModifier = Difficulty_NegativeReputationModifier == -1f ? diff.reputationModifier : Difficulty_NegativeReputationModifier;
                diff.resourcesBonusProduction = Difficulty_MiningBonus == -1f ? diff.resourcesBonusProduction : Difficulty_MiningBonus;
                diff.shipMaintenanceMultiplier = Difficulty_NavalMaintenance == -1f ? diff.shipMaintenanceMultiplier : Difficulty_NavalMaintenance;
                diff.recruitsModifier = Difficulty_RecruitsModifier == -1f ? diff.recruitsModifier : Difficulty_RecruitsModifier;
                diff.specialistBonusProduction = Difficulty_SpecialistBonus == -1f ? diff.specialistBonusProduction : Difficulty_SpecialistBonus;
            }
            __result.difficultyConfig.difficultySettings = diffSettings;


            foreach (var cannon in __result.weapons.shipGuns) // these are ships models
            {
            }
            foreach (var cannon in __result.weapons.cannonGuns) // ship guns
            {
                string folder = "./bepinex/config/ship_cannons/";
                string file = $"{cannon.name}.json";
                if (ExportWeapons)
                {
                    var ballistics = new CannonBallistic()
                    {
                        baseY = cannon.ballistics.baseY,
                        explosionMoraleImpactRadius = cannon.ballistics.explosionMoraleImpactRadius,
                        distance = cannon.ballistics.distance,
                        explosionRadius = cannon.ballistics.explosionRadius,
                        farAngle = cannon.ballistics.farAngle,
                        gravity = cannon.ballistics.gravity,
                        horizontalSpread = cannon.ballistics.horizontalSpread,
                        mass = cannon.ballistics.mass,
                        radius = cannon.ballistics.radius,
                        scaledDistance = cannon.ballistics.scaledDistance,
                        scaledGravity = cannon.ballistics.scaledGravity,
                        time = cannon.ballistics.time,
                        velocity = cannon.ballistics.velocity,
                        verticalSpread = cannon.ballistics.verticalSpread,
                    };
                    var cannonTemplate = new CannonModuleEx()
                    {
                        name = cannon.name,
                        allowCollect = cannon.allowCollect,
                        ballistics = ballistics,
                        crew = cannon.crew,
                        groundBatterySize = cannon.groundBatterySize,
                        horizontalTurnMax = cannon.horizontalTurnMax,
                        navalBatterySize = cannon.navalBatterySize,
                        realReloadTime = cannon.realReloadTime,
                        reloadTime = cannon.reloadTime,
                        threat = cannon.threat,
                        verticalTurnMax = cannon.verticalTurnMax,
                        verticalTurnMin = cannon.verticalTurnMin,
                        weight = cannon.weight,
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(cannonTemplate, new JsonSerializerOptions() { WriteIndented = true });
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    File.WriteAllText($"{folder}{file}", json);
                }
                if (EnableWeaponsModification)
                {
                    if (!File.Exists($"{folder}{file}"))
                        continue;
                    Log.LogInfo($"Modifying weapon data for {cannon.name}");
                    var filetext = File.ReadAllText($"{folder}{file}");
                    var template = JsonSerializer.Deserialize<CannonModuleEx>(filetext);

                    cannon.ballistics.baseY = template.ballistics.baseY;
                    cannon.ballistics.distance = template.ballistics.distance;
                    cannon.ballistics.explosionMoraleImpactRadius = template.ballistics.explosionMoraleImpactRadius;
                    cannon.ballistics.explosionRadius = template.ballistics.explosionRadius;
                    cannon.ballistics.farAngle = template.ballistics.farAngle;
                    cannon.ballistics.gravity = template.ballistics.gravity;
                    cannon.ballistics.horizontalSpread = template.ballistics.horizontalSpread;
                    cannon.ballistics.mass = template.ballistics.mass;
                    cannon.ballistics.radius = template.ballistics.radius;
                    cannon.ballistics.scaledDistance = template.ballistics.scaledDistance;
                    cannon.ballistics.scaledGravity = template.ballistics.scaledGravity;
                    cannon.ballistics.time = template.ballistics.time;
                    cannon.ballistics.velocity = template.ballistics.velocity;
                    cannon.ballistics.verticalSpread = template.ballistics.verticalSpread;
                    cannon.allowCollect = template.allowCollect;
                    cannon.crew = template.crew;
                    cannon.groundBatterySize = template.groundBatterySize;
                    cannon.horizontalTurnMax = template.horizontalTurnMax;
                    cannon.navalBatterySize = template.navalBatterySize;
                    cannon.realReloadTime = template.realReloadTime;
                    cannon.reloadTime = template.reloadTime;
                    cannon.threat = template.threat;
                    cannon.verticalTurnMax = template.verticalTurnMax;
                    cannon.verticalTurnMin = template.verticalTurnMin;
                    cannon.weight = template.weight;
                }
            }
            foreach (var weapon in __result.weapons.landGuns)
            {
                string folder = "./bepinex/config/land_cannons/";
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
                    if (!File.Exists($"{folder}{file}"))
                        continue;
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
                    if (!File.Exists($"{folder}{file}"))
                        continue;
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

    public class CannonModuleEx
    {
        public string name { get; set; }
        public bool allowCollect { get; set; }
        public CannonBallistic ballistics { get; set; }
        public float crew { get; set; }
        public float groundBatterySize { get; set; }
        public float horizontalTurnMax { get; set; }
        public float navalBatterySize { get; set; }
        public float realReloadTime { get; set; }
        public float reloadTime { get; set; }
        public float threat { get; set; }
        public float verticalTurnMax { get; set; }
        public float verticalTurnMin { get; set; }
        public float weight { get; set; }
    }
    public class CannonBallistic
    {
        public float baseY { get; set; }
        public float distance { get; set; }
        public float explosionMoraleImpactRadius { get; set; }
        public float explosionRadius { get; set; }
        public float farAngle { get; set; }
        public float gravity { get; set; }
        public float horizontalSpread { get; set; }
        public float mass { get; set; }
        public float radius { get; set; }
        public float scaledDistance { get; set; }
        public float scaledGravity { get; set; }
        public float time { get; set; }
        public float velocity { get; set; }
        public float verticalSpread { get; set; }
    }
}