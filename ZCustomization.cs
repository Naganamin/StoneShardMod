using System.Text;
using ModShardLauncher;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;

namespace ZCustomization;

public static class FunctionalExtensions
{
    public static TResult Let<T, TResult>(this T value, Func<T, TResult> func)
        where T : class
    {
        return func(value);
    }

    public static void Let<T>(this T? value, Action<T> action)
        where T : class
    {
        if (value != null)
            action(value);
    }
}

public record CaravanShop(string Shop, List<int> Items, int Sell);

public class ZCustomization : Mod
{
    private record ScriptSet(
        string Name,
        string File,
        EventType EventType = EventType.Create,
        uint SubType = 0
    );

    public override string Author => "Fubuchi";
    public override string Name => "ZCustomization";
    public override string Description => "Personal customization mod: Alda honey and more.";
    public override string Version => "1.0.0.0";
    public override string TargetVersion => "0.9.2.12";

    public override void PatchMod()
    {
        CaravanShop();
        OsbrookInnkeeperShop();
        BrynnElfMerchantShop();
        BoneCharmLimit();
        ShowDen();
        RepairKit();
        EnhanceWeapons();
        BuffArmors();
        AddApAndSpScroll();
    }

    private static void CaravanShop()
    {
        new List<CaravanShop>()
        {
            new("gml_Object_o_npc_alda_caravan_Other_19",
                new List<string>()
                {
                    "o_inv_honey",
                    "o_inv_thyme",
                    "o_inv_whitefish_raw",
                    "o_inv_crab",
                    "o_inv_mussel",
                    "o_inv_truffle",
                    "o_inv_azurecap"
                }.Let(MapToGameItemList), 3),
            new ("gml_Object_o_npc_leif_caravan_Other_19",
                new List<string>
                {
                    "o_inv_citrus",
                    "o_inv_fig",
                    "o_inv_salt",
                    "o_inv_spice_sentia",
                    "o_inv_milk",
                    "o_inv_cheese"
                }.Let(MapToGameItemList), 3)
        }
        .ForEach(shop =>
        {
            var (script, items, sell) = shop;
            items
            .Select(item => $"ds_list_add(selling_loot_object, {item}, {sell})")
            .Aggregate(
                new StringBuilder(""),
                (acc, item) => acc.AppendLine(item),
                acc =>
                {
                    if (acc.Length >= Environment.NewLine.Length)
                    {
                        acc.Length -= Environment.NewLine.Length;
                    }
                    return acc.ToString();
                }
            )
            .Let(sells =>
            {
                Msl.LoadGML(script)
                    .MatchAll()
                    .InsertBelow(sells)
                    .Save();
            });
        });
    }

    private static void OsbrookInnkeeperShop()
    {
        var sells = new List<(string, int)>()
        {
            ("o_inv_brandy", 1),
            ("o_inv_salt", 3),
            ("o_inv_recipe_fish_broth", 1),
            ("o_inv_recipe_omelette_vegs", 1),
            ("o_inv_recipe_pickled_cabbage", 1),
            ("o_inv_recipe_truffle_salad", 1),
            ("o_inv_recipe_truffle_steak", 1),
            ("o_inv_recipe_honey_azurecap", 1),
        }
        .Select(item => $"ds_list_add(selling_loot_object, {ItemIndex(item.Item1)}, {item.Item2})")
        .Aggregate(
            new StringBuilder(""),
            (acc, item) => acc.AppendLine(item),
            acc =>
            {
                if (acc.Length >= Environment.NewLine.Length)
                {
                    acc.Length -= Environment.NewLine.Length;
                }
                return acc.ToString();
            }
        );

        Msl.LoadGML("gml_Object_o_npc_innkeeper_osbrook_Other_19")
            .MatchAll()
            .InsertBelow(sells)
            .Save();
    }

    private static void BrynnElfMerchantShop()
    {
        var censer = ItemIndex("o_inv_censer");
        var sellT3Mace = "ds_list_add(selling_loot_object, \"Chekan\", 1)";
        var sellCenser = $"ds_list_add(selling_loot_object, {censer}, 1)";
        var sellHelmet = "ds_list_add(selling_loot_object, \"Fjall Helmet\", 1)";

        var sells = string.Join("\n", new List<string> {
            sellT3Mace,
            sellCenser,
            sellHelmet
        });

        var sellT4Mace = """
        if (scr_globaltile_reputation_get() >= 4000) {
            ds_list_add(selling_loot_object, "Nistrian Flail",  1)
        }
        """;

        Msl.LoadGML("gml_Object_o_npc_merchant_elf_brynn_Other_19")
            .MatchAll()
            .InsertBelow(sells)
            .MatchAll()
            .InsertBelow(sellT4Mace)
            .Save();
    }

    private static void BoneCharmLimit()
    {
        var gulon = "GulonKills";
        Msl.LoadGML("gml_GlobalScript_scr_consum_hilda_enchant_assign")
            .MatchFrom(gulon)
            .ReplaceBy($"_attribute_value = min((2 + 0.5 * (scr_atr(\"{gulon}\", 0))), 15)")
            .Save();

        var crawler = "CrawlerKills";
        Msl.LoadGML("gml_GlobalScript_scr_consum_hilda_enchant_assign")
            .MatchFrom(crawler)
            .ReplaceBy($"_attribute_value = min((2 + 0.5 * (scr_atr(\"{crawler}\", 0))), 15)")
            .Save();

        var youngTroll = "YoungTrollKills";
        Msl.LoadGML("gml_GlobalScript_scr_consum_hilda_enchant_assign")
            .MatchFrom(youngTroll)
            .ReplaceBy($"_attribute_value = (-(min((5 + 0.5 * (scr_atr(\"{youngTroll}\", 0))), 15)))")
            .Save();

        var boar = "BoarKills";
        Msl.LoadGML("gml_GlobalScript_scr_consum_hilda_enchant_assign")
            .MatchFrom(boar)
            .ReplaceBy($"_attribute_value = (-(min((5 + 0.25 * (scr_atr(\"{boar}\", 0))), 20)))")
            .Save();

        var harpy = "HarpyKills";
        Msl.LoadGML("gml_GlobalScript_scr_consum_hilda_enchant_assign")
            .MatchFrom(harpy)
            .ReplaceBy($"_attribute_value = min((5 + 0.25 * (scr_atr(\"{harpy}\", 0))), 20)")
            .Save();
    }

    private static void ShowDen()
    {
        // Press F3 to show current time and some hunting locations
        Msl.LoadGML("gml_Object_o_player_KeyPress_114")
            .MatchAll()
            .InsertBelow(
            """
            var hours = ds_map_find_value(global.timeDataMap, "hours")
            var minutes = ds_map_find_value(global.timeDataMap, "minutes")
            scr_actionsLogUpdate(string_ext("Current time: {0}:{1}", [hours, minutes]))

            var locations = ["bear", "moose", "troll", "osbrook", "ruin"]
            for (var i = 0; i < array_length(locations); i++) {
                var search = locations[i]
                scr_actionsLogUpdate("Search: " + search)
                var key = ds_map_find_first(global.locationMap)
                while (!is_undefined(key)) {
                    var location = ds_map_find_value(global.locationMap, key)
                    if (string_pos(search, string_lower(location.name)) > 0) {
                        scr_actionsLogUpdate(string_ext("x: {0}, y: {1}, name: {2}", [location.x, location.y, location.name]))
                    }
                    key = ds_map_find_next(global.locationMap, key)
                }
            }
            """)
            .Save();
    }

    private static void RepairKit()
    {
        Msl.LoadGML("gml_Object_o_skill_repair_item_Other_11")
            .MatchFromUntil(
                "else if (parent.object_index == o_inv_repkit)",
                "_repair_value *= 1.33"
            ).ReplaceBy("""
            else if (parent.object_index == o_inv_repkit)
            {
            with (interact_id)
            {
                _repair_value = 15
                if scr_passive_skill_is_open(o_pass_skill_self_repair, o_player)
                    _repair_value *= 1.33
            """)
            .Save();
    }

    private static void EnhanceWeapons()
    {
        ModifyWeapon(new HashSet<string>() {
            "Vault Raider Hatchet",
            "Sergeant Hatchet",
            "Ancestral Hatchet",
            "Haakon Axe",
            "Chekan",
            "Nistrian Flail",
            "Decorated Warhammer",
            "Grandmaster Flail"
        });
    }

    private static void ModifyWeapon(HashSet<string> names)
    {
        var NAME = 0;
        var TIER = 1;
        var RARITY = 4;
        var SHOCK_DAMAGE = 20;
        var FROST_DAMAGE = 23;
        var ARCANE_DAMAGE = 24;
        var SACRED_DAMAGE = 26;
        var TAGS = 76;

        void FixUnique(string[] weapon)
        {
            var tags = weapon[TAGS];
            var rarity = weapon[RARITY];
            if (tags == "unique" && rarity != "Unique")
            {
                weapon[RARITY] = "Unique";
            }
        }

        string BuffTier3(string[] weapon)
        {
            weapon[ARCANE_DAMAGE] = "1";
            weapon[SACRED_DAMAGE] = "1";
            return string.Join(";", weapon);
        }

        string BuffTier4(string[] weapon)
        {
            weapon[ARCANE_DAMAGE] = "2";
            weapon[SACRED_DAMAGE] = "2";
            weapon[SHOCK_DAMAGE] = "1";
            weapon[FROST_DAMAGE] = "1";
            return string.Join(";", weapon);
        }

        string BuffTier5(string[] weapon)
        {
            weapon[ARCANE_DAMAGE] = "2";
            weapon[SACRED_DAMAGE] = "2";
            weapon[SHOCK_DAMAGE] = "2";
            weapon[FROST_DAMAGE] = "2";
            return string.Join(";", weapon);
        }

        WEAPONS
            .Select(weapon => weapon.Split(";"))
            .Where(weapon => names.Contains(weapon[NAME]))
            .Select(weapon =>
            {
                var original = string.Join(";", weapon);
                FixUnique(weapon);
                var buffed = weapon[TIER] switch
                {
                    "3" => BuffTier3(weapon),
                    "4" => BuffTier4(weapon),
                    "5" => BuffTier5(weapon),
                    _ => throw new InvalidOperationException("Unreachable")
                };
                return (original, buffed);
            })
            .ToList()
            .ForEach(data =>
            {
                var (original, buffed) = data;
                ReplaceInTable("gml_GlobalScript_table_weapons", original, buffed);
            });
    }

    private static void BuffArmors()
    {
        ModifyArmorToUnique(new HashSet<string>()
        {
            "Fjall Helmet"
        });
    }

    private static void ModifyArmorToUnique(HashSet<string> names)
    {
        var NAME = 0;
        var RARITY = 5;

        ARMORS
            .Select(armor => armor.Split(";"))
            .Where(armor => names.Contains(armor[NAME]))
            .Select(armor =>
            {
                var original = string.Join(";", armor);
                armor[RARITY] = "Unique";
                var changed = string.Join(";", armor);
                return (original, changed);
            })
            .ToList()
            .ForEach(data =>
            {
                var (find, replace) = data;
                ReplaceInTable("gml_GlobalScript_table_armor", find, replace);
            });
    }

    private void AddApAndSpScroll()
    {
        Msl.AddFunction(ModFiles.GetCode("gml_GlobalScript_scr_zcustomization_generator.gml"), "scr_zcustomization_generator");
        Msl.AddFunction(ModFiles.GetCode("gml_GlobalScript_scr_zcustomization_textloader.gml"), "scr_zcustomization_textloader");

        Msl
        .LoadAssemblyAsString("gml_Object_o_textLoader_Other_25").MatchFrom("pushglb.v global.attribute_order_all_without_damage\r\ncall.i ds_list_add(argc=16)\r\npopz.v")
        .InsertBelow("call.i gml_Script_scr_zcustomization_textloader(argc=0)\r\npopz.v")
        .Save();

        CreateGameObject("o_loot_zspscroll", "s_loot_zspscroll", "o_consument_loot");
        CreateGameObject("o_inv_zspscroll", "s_inv_zspscroll", "o_inv_consum", true);

        CreateGameObject("o_loot_zapscroll", "s_loot_zapscroll", "o_consument_loot");
        CreateGameObject("o_inv_zapscroll", "s_inv_zapscroll", "o_inv_consum", true);

        new List<ScriptSet>
        {
            new("o_loot_zspscroll", "gml_Object_o_loot_zspscroll_Create_0.gml"),

            new("o_inv_zspscroll", "gml_Object_o_inv_zspscroll_Create_0.gml"),
            new("o_inv_zspscroll", "gml_Object_o_inv_zspscroll_Other_10.gml", EventType.Other, 10),
            new("o_inv_zspscroll", "gml_Object_o_inv_zspscroll_Other_24.gml", EventType.Other, 24),

            new("o_loot_zapscroll", "gml_Object_o_loot_zapscroll_Create_0.gml"),

            new("o_inv_zapscroll", "gml_Object_o_inv_zapscroll_Create_0.gml"),
            new("o_inv_zapscroll", "gml_Object_o_inv_zapscroll_Other_10.gml", EventType.Other, 10),
            new("o_inv_zapscroll", "gml_Object_o_inv_zapscroll_Other_24.gml", EventType.Other, 24),
        }.ForEach(script =>
        {
            Msl.AddNewEvent(
                script.Name,
                ModFiles.GetCode(script.File),
                script.EventType,
                script.SubType
            );
        });

        var scrolls = new List<string>
        {
            "o_inv_zspscroll",
            "o_inv_zapscroll"
        }
        .Select(ItemIndex)
        .Select(index => $"ds_list_add(selling_loot_object, {index}, 2)");

        Msl.LoadGML("gml_Object_o_npc_merchant_elf_brynn_Other_19")
        .MatchAll()
        .InsertBelow(string.Join("\n", scrolls))
        .Save();
    }

    private static UndertaleGameObject CreateGameObject(string objName, string sprName = "", string perName = "", bool persistent = false, bool awake = true)
    {
        var tempObject = Msl.AddObject(objName);
        if (sprName != "")
            tempObject.Sprite = Msl.GetSprite(sprName);
        tempObject.Visible = true;
        if (perName != "")
            tempObject.ParentId = Msl.GetObject(perName);
        tempObject.Persistent = persistent;
        tempObject.Awake = awake;
        return tempObject;
    }

    private static void ReplaceInTable(string tableName, string find, string replace)
    {
        var table = ModLoader.GetTable(tableName) ?? new List<string>();

        var index = table.IndexOf(find);

        if (index >= 0)
        {
            table[index] = replace;
            ModLoader.SetTable(table, tableName);
        }
    }

    private static List<int> MapToGameItemList(List<string> objects)
    {
        return objects
            .Select(ItemIndex)
            .ToList();
    }

    private static int ItemIndex(string item)
    {
        return DataLoader.data.GameObjects.IndexOf(
            DataLoader.data.GameObjects.First(
                x => x.Name.Content == item
            )
        );
    }

    private static readonly List<string> WEAPONS = new List<string>()
    {
        "name;Tier;id;Slot;rarity;Mat;Price;Markup;MaxDuration;Rng;;Armor_Piercing;Armor_Damage;Bodypart_Damage;;Slashing_Damage;Piercing_Damage;Blunt_Damage;Rending_Damage;Fire_Damage;Shock_Damage;Poison_Damage;Caustic_Damage;Frost_Damage;Arcane_Damage;Unholy_Damage;Sacred_Damage;Psionic_Damage;;FMB;Hit_Chance;CRT;CRTD;CTA;PRR;Block_Power;Block_Recovery;;Bleeding_Chance;Daze_Chance;Stun_Chance;Knockback_Chance;Immob_Chance;Stagger_Chance;;MP;MP_Restoration;Cooldown_Reduction;Abilities_Energy_Cost;Skills_Energy_Cost;Spells_Energy_Cost;Magic_Power;Miscast_Chance;Miracle_Chance;Miracle_Power;Bonus_Range;;max_hp;Health_Restoration;Healing_Received;Crit_Avoid;Fatigue_Gain;Lifesteal;Manasteal;Damage_Received;;Pyromantic_Power;Geomantic_Power;Venomantic_Power;Electromantic_Power;Cryomantic_Power;Arcanistic_Power;Astromantic_Power;Psimantic_Power;;Balance;tags;upgrade;fireproof;NoDrop;",
        "[ SWORDS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Arna Sword;2;sword01;sword;Unique;metal;475;1;70;1;;5;;;;17;;;;;;;;;;;;;;-3;;;;;6;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special exc;;;;",
        "Drifter Sword;2;sword02;sword;Common;metal;475;1;70;1;;5;;;;18;;;;;;;;;;;;;;;;;;5;5;4;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Footman Sword;3;sword03;sword;Common;metal;1250;1;90;1;;10;;;;21;;;;;;;;;;;;;;;;;;6;7;6;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Veteran Sword;4;sword04;sword;Common;metal;2775;1;110;1;;10;;;;24;;;;;;;;;;;;;;;;;;7;9;8;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Skonfert Sword;4;sword05;sword;Common;metal;2925;1;120;1;;10;;;;24;;;;;;;;;;;;;;;3;;;4;9;10;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Heirloom Sword;4;sword06;sword;Unique;metal;2775;1;95;1;;10;;;;24;;;;;;;;;;;;;;-3;;;;6;7;6;;;;;;;;;;;;;;5;-5;5;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "Knightly Sword;5;sword07;sword;Common;metal;5875;1;125;1;;15;;;;28;;;;;;;;;;;;;;;;;;8;12;10;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Radiant Sword;5;sword08;sword;Unique;metal;5875;1;125;1;;15;;;;25;;;;;;;;;;;2;;;;;;;8;12;12;5;;;;;;;;;;5;;;5;;;;;;;;;;;;5;;;;;;;;;;;;;;2;unique;;;;",
        "// OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Homemade Blade;1;sword09;sword;Common;metal;100;1;50;1;;5;;;;16;;;;;;;;;;;;;;;3;;;;2;;;;5;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Recruit Blade;2;sword10;sword;Common;metal;500;1;65;1;;5;;;;18;;;;;;;;;;;;;;;4;;;;4;;;;8;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Guard Blade;2;sword11;sword;Common;metal;500;1;65;1;;5;;;;18;;;;;;;;;;;;;;;4;;;2;4;;;;4;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Brigand Blade;2;sword12;sword;Common;metal;500;1;65;1;;5;;4;;18;;;;;;;;;;;;;;;3;;;;4;;;;8;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;;;;",
        "Guild Blade;3;sword13;sword;Common;metal;1300;1;80;1;;10;;;;21;;;;;;;;;;;;;;;5;;;;6;;;;11;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Norse Blade;3;sword14;sword;Common;metal;1300;1;80;1;;10;;;;21;;;;;;;;;;;;;;;3;1;5;;3;;;;9;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;fjall;;;;",
        "Katzbalger;4;sword15;sword;Common;metal;2900;1;95;1;;10;;;;24;;;;;;;;;;;;;;;6;;;;8;;;;14;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Jarl Blade;4;sword16;sword;Unique;metal;2900;1;90;1;;10;;7;;24;;;;;;;;;;;;;;;4;3;10;;8;;;;14;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "Templar Blade;5;sword17;sword;Common;metal;6125;1;110;1;;15;;;;28;;;;;;;;;;;;;;-1;7;;;;10;;;;18;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Royal Blade;5;sword18;sword;Unique;metal;6125;1;130;1;;15;;;;28;;;;;;;;;;;;;;;7;;;;10;;;;18;;;;;;;;;;;-5;;;;;;;;;;;;;;;-5;;;;;;;;;;;2;unique;;;;",
        "Theurgist Blade;5;sword19;sword;Unique;metal;6125;1;100;1;;15;;;;24;;;;;;;;;;;4;;;;7;;;;10;;;;18;;;;;;;;;-5;;5;;;;3;10;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "// CLEAVERS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Battle Cleaver;1;sword20;sword;Common;metal;75;1;50;1;;3;;;;17;;;;;;;;;;;;;;4;;2;;;;;;;8;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Militia Falchion;2;sword21;sword;Common;metal;450;1;70;1;;3;;;;19;;;;;;;;;;;;;;5;;3;;;;;;;11;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Jagged Falchion;3;sword22;sword;Common;metal;1200;1;85;1;;7;;;;22;;;;;;;;;;;;;;6;;4;;;;;;;14;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Mercenary Falchion;4;sword23;sword;Common;metal;2650;1;100;1;;7;;;;26;;;;;;;;;;;;;;7;;5;;;;;;;17;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Captain Messer;5;sword24;sword;Common;metal;5600;1;120;1;;10;;;;29;;;;;;;;;;;;;;8;;6;;;;;;;20;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Guardsman Broadsword;5;sword25;sword;Unique;metal;5600;1;115;1;;10;;15;;29;;;;;;;;;;;;;;8;;6;15;;5;3;;;20;;;;;;;;;;;15;-5;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "// SABERS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Nomad Saber;3;sword26;sword;Common;metal;1400;1;75;1;;5;;16;;22;;;;;;;;;;;;;;;;;;;;;;;18;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;elven;;;;",
        "Elven Saber;4;sword27;sword;Common;metal;3150;1;90;1;;5;;20;;25;;;;;;;;;;;;;;;;;;;;;;;22;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;elven;;;;",
        "Jibean Scimitar;5;sword29;sword;Common;metal;6625;1;105;1;;5;;24;;29;;;;;;;;;;;;;;;;;;;;;;;26;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;elven;;;;",
        "Ancient Scimitar;5;sword28;sword;Unique;metal;6625;1;85;1;;5;;30;;26;;;;;;;;;;;;;;;;5;10;;;;;;34;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "Decorated Saber;5;sword30;sword;Unique;metal;6625;1;105;1;;10;;24;;29;;;;;;;;;;;;;;-3;3;;;;;;;;26;;;;;;;;;;;5;-5;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "// FLAVOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Wooden Sword;1;sword01f;sword;Common;wood;5;1;25;1;;-50;-50;;;;;10;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;1;special;;;;",
        "Rusty Sword;1;sword02f;sword;Common;metal;5;1;15;1;;;-30;;;;;10;;;;2;;;;;;;;33;-10;-33;;;;-10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;1;special;;;;",
        "[ AXES ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// HATCHETS - OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Hatchet;1;axe01;axe;Common;metal;75;1;60;1;;5;8;8;;17;;;;;;;;;;;;;;;3;;;;;;;;5;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Bearded Hatchet;2;axe02;axe;Common;metal;400;1;80;1;;10;11;11;;19;;;;;;;;;;;;;;;4;;;;;;;;8;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Skadian Hatchet;3;axe03;axe;Common;metal;1075;1;100;1;;10;16;14;;22;;;;;;;;;;;;;;;4;;5;;;;;;12;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;2;skadia;;;;",
        "Vault Raider Hatchet;3;axe04;axe;Common;metal;1075;1;100;1;;10;14;14;;22;;;;;;;;;;;;;;;5;;;;;;;;10;;;;;5;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;2;fjall;;;;",
        "Sergeant Hatchet;4;axe05;axe;Common;metal;2425;1;120;1;;15;17;17;;26;;;;;;;;;;;;;;;6;;;;;;;;16;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Knightly Hatchet;5;axe06;axe;Common;metal;5100;1;140;1;;15;20;20;;29;;;;;;;;;;;;;;;7;;;;;;;;20;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Honorary Hatchet;5;axe07;axe;Unique;metal;5100;1;120;1;;15;20;25;;28;;;;;;;;;;;;;;;4;;;;;;;;26;;;;;;;;;-5;;15;-5;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "Haakon Axe;5;axe08;axe;Unique;metal;5100;1;155;1;;15;20;20;;29;;;;;;;;;;;;;;-3;7;;;;;;;;20;;;;;;;;5;;;20;;;;;;;;;;;;;;5;;;;;;;;;;;;3;unique;;;;",
        "// HATCHETS - DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Hunter Hatchet;2;axe09;axe;Common;metal;400;1;90;1;;10;;7;;19;;;;;;;;;;;;;;;;;;3;5;;;;7;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Naval Hatchet;3;axe10;axe;Common;metal;1025;1;110;1;;10;;10;;22;;;;;;;;;;;;;;;;;;4;6;;;;10;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Eastern Sagaris;3;axe11;axe;Common;metal;1125;1;120;1;;10;;12;;22;;;;;;;;;;;;;;;;;;3;6;;;;10;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;4;elven;;;;",
        "Boarding Hatchet;4;axe12;axe;Common;metal;2300;1;130;1;;15;;13;;26;;;;;;;;;;;;;;;;;;5;7;;;;13;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Siege Engineer Hatchet;4;axe13;axe;Common;metal;2300;1;160;1;;15;;13;;26;;;;;;;;;;;;;;;;;;4;11;3;;;7;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Brynn Hatchet;5;axe14;axe;Common;metal;4850;1;140;1;;15;;15;;27;;;;;;;;;;;;;;;;;;8;8;;;;20;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Noble Hatchet;5;axe15;axe;Common;metal;4850;1;170;1;;15;;15;;29;;;;;;;;;;;;;;;;;;6;8;;;;15;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Ancestral Hatchet;5;axe16;axe;Common;metal;4850;1;200;1;;20;;15;;29;;;;;;;;;;;;;;;;3;;6;8;;5;;15;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;4;unique;;;;",
        "// HANDAXES - OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Makeshift Axe;1;axe17;axe;Common;metal;50;1;45;1;;10;;12;;16;;;;;;;;;;;;;;2;;;10;;;;;;;;;;;7;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Felling Axe;1;axe18;axe;Common;metal;75;1;65;1;;10;;12;;18;;;;;;;;;;;;;;;;;10;;;;;;;;;;;7;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Norse Axe;2;axe19;axe;Common;metal;400;1;90;1;;15;;15;;20;;;;;;;;;;;;;;;;;12;;;;;;;;;;;10;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Footman Broadaxe;3;axe20;axe;Common;metal;1075;1;110;1;;15;;18;;23;;;;;;;;;;;;;;;;;15;;;;;;;;;;;13;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Dwarven Axe;3;axe21;axe;Common;metal;975;1;95;1;;15;;20;;24;;;;;;;;;;;;;;4;;;24;;;;;;;;;;;13;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;1;fjall;;;;",
        "Veteran Broadaxe;4;axe22;axe;Common;metal;2425;1;130;1;;20;;22;;27;;;;;;;;;;;;;;;;;18;;;;;;;;;;;16;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Noble Axe;5;axe23;axe;Common;metal;5100;1;155;1;;20;;26;;31;;;;;;;;;;;;;;;;;20;;;;;;;;;;;20;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Exquisite Tabar;5;axe24;axe;Unique;metal;5100;1;140;1;;20;15;26;;31;;;;;;;;;;;;;;;;;20;;;;;;15;;;;;20;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "Gilded Axe;5;axe25;axe;Unique;metal;5100;1;145;1;;20;;26;;31;;;;;;;;;;;;;;;;2;20;;;;;;;;;;;25;;5;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "// HANDAXES - ANTI-ARMOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Footman Axe;3;axe26;axe;Common;metal;1075;1;115;1;;25;20;16;;23;;;;;;;;;;;;;;;4;;10;;;;;;;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Raider Axe;3;axe27;axe;Common;metal;1075;1;100;1;;25;20;21;;22;;;;;;;;;;;;;;;;1;5;;;;;;;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Mercenary Axe;4;axe28;axe;Common;metal;2425;1;140;1;;25;24;20;;27;;;;;;;;;;;;;;;5;;12;;;;;;;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Knightly Axe;5;axe29;axe;Common;metal;5100;1;160;1;;30;28;24;;31;;;;;;;;;;;;;;;6;;15;;;;;;;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Aldwynn Axe;5;axe30;axe;Common;metal;5100;1;160;1;;30;28;24;;31;;;;;;;;;;;;;;2;6;3;15;;;;;;;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Baron Axe;5;axe31;axe;Unique;metal;5100;1;175;1;;30;28;24;;31;;;;;;;;;;;;;;;6;;15;;;;;;;;;;;;;;;-5;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "[ MACES ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Spiked Club;1;mace01;mace;Common;wood;75;1;60;1;;15;;7;;;2;15;;;;;;;;;;;;;;;;;;;;;4;5;;;;10;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Militia Morning Star;2;mace02;mace;Common;wood;375;1;80;1;;15;;10;;;3;16;;;;;;;;;;;;;;;;;;;;;7;7;;;;15;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Morning Star;3;mace03;mace;Common;metal;975;1;100;1;;20;;20;;;2;20;;;;;;;;;;;;;;;;;;;;;10;10;;;;20;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Skadian Bludgeon;3;mace04;mace;Common;metal;1125;1;120;1;;20;;;;;;22;;;;;;;;;;;;-5;;;18;;;;;;;13;;;;13;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;2;skadia;;;;",
        "Veteran Mace;4;mace05;mace;Common;metal;2175;1;120;1;;20;;16;;;3;23;;;;;;;;;;;;;;;;;;;;;13;12;;;;25;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Druzhyna Bludgeon;4;mace06;mace;Common;metal;2625;1;145;1;;20;;;;;;26;;;;;;;;;;;;-6;;;22;;;;;;;16;;;;16;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;skadia;;;;",
        "Knightly Morning Star;5;mace07;mace;Common;metal;4600;1;140;1;;25;;20;;;3;26;;;;;;;;;;;;;;;;;;;;;15;15;;;;30;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Chervenian Bulava;5;mace08;mace;Common;metal;4600;1;160;1;;25;;;;;;29;;;;;;;;;;;;-7;;;26;;;;;;;20;;;;20;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;nistra;;;;",
        "Baron Morning Star;5;mace09;mace;Unique;metal;4600;1;135;1;;25;;25;;;4;26;;;;;;;;;;;;;;3;10;;;;;;20;15;;;;30;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "Voivod Mace;5;mace10;mace;Unique;metal;4600;1;175;1;;33;;;;;;29;;;;;;;;;;;;-7;;;26;;;;;;;20;5;;;20;;;5;-5;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "// CONTROL;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Cudgel;1;mace11;mace;Common;wood;25;1;45;1;;10;;;;;;16;;;;;;;;;;;;;;;;2;;;;;;8;4;5;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Reinforced Club;1;mace12;mace;Common;wood;75;1;70;1;;20;;;;;;17;;;;;;;;;;;;;;;;2;;;;;;10;4;5;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Militia Club;2;mace13;mace;Common;metal;375;1;95;1;;20;;;;;;18;;;;;;;;;;;;-2;;;;3;;;;;;12;7;11;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Soldier Club;2;mace14;mace;Common;metal;375;1;100;1;;20;;;;;;18;;;;;;;;;;;;;;;;3;;;;;;14;8;8;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Flanged Mace;3;mace15;mace;Common;metal;975;1;120;1;;25;;;;;;21;;;;;;;;;;;;;;;;4;;;;;;16;10;12;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Shishpar;3;mace16;mace;Common;metal;1125;1;140;1;;25;10;;;;;21;;;;;;;;;;;;;;;;2;;;;;;18;10;14;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;2;elven;;;;",
        "Reinforced Flanged Mace;4;mace17;mace;Common;metal;2175;1;150;1;;25;;;;;;24;;;;;;;;;;;;;;;;5;;;;;;20;13;16;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Bozdogan;4;mace18;mace;Common;metal;2500;1;145;1;;25;15;;;;;24;;;;;;;;;;;;;;;;4;;;;;;24;13;16;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;elven;;;;",
        "Knightly Flanged Mace;5;mace19;mace;Common;metal;4600;1;170;1;;30;;;;;;28;;;;;;;;;;;;;;;;6;;;;;;25;15;20;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Noble Flanged Mace;5;mace20;mace;Common;metal;4825;1;195;1;;30;;10;;;;28;;;;;;;;;;;;-1;;;;4;;;;;;25;15;20;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Ghazi Flanged Mace;5;mace21;mace;Unique;metal;4600;1;175;1;;30;5;;;;;27;;;;;;;;;1;;;;;;;6;;;;;;33;19;20;;;;;;;;15;;;;;;;;;;;;5;;;;;;;;;;;;;;1;unique;;;;",
        "Decorated Flanged Mace;5;mace22;mace;Unique;metal;4600;1;160;1;;35;25;;;;;28;;;;;;;;;;;;;;;;6;;;;;;25;15;25;;25;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "// HAMMERS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Footman Warhammer;3;mace23;mace;Common;metal;1075;1;110;1;;30;35;;;;;20;;;;;;;;;;;;-2;4;;;;;;;;;10;;;;16;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Brynn Warhammer;3;mace24;mace;Common;metal;1075;1;120;1;;30;35;;;;;20;;;;;;;;;;;;;4;3;;;;;;;5;5;;;;16;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Chekan;3;mace25;mace;Common;metal;1200;1;125;1;;33;40;10;;;;20;;;;;;;;;;;;;1;;;;;;;;;8;;;;10;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;skadia;;;;",
        "Mercenary Warhammer;4;mace26;mace;Common;metal;2425;1;130;1;;30;40;;;;;23;;;;;;;;;;;;-3;5;;;;;;;;;12;;;;20;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Fisted Warhammer;4;mace27;mace;Common;metal;2425;1;120;1;;35;40;;;;;23;;;;;;;;;;;;;5;;;;;;;;;16;;;;24;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Captain Warhammer;5;mace28;mace;Common;metal;5100;1;145;1;;40;45;;;;;27;;;;;;;;;;;;;6;;;;;;;;;15;;;;30;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Knightly Warhammer;5;mace29;mace;Common;metal;5100;1;155;1;;35;45;;;;;27;;;;;;;;;;;;;6;3;;;;;;;;20;;;;20;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Grotesque Warhammer;5;mace30;mace;Common;metal;5100;1;140;1;;35;45;;;;;27;;;;;;;;;;;;-4;6;;;;;;;;;15;5;;;25;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Decorated Warhammer;5;mace31;mace;Unique;metal;5100;1;145;1;;35;50;15;;;;27;;;;;;;;;;;;;6;;;;;;;;;15;;;;25;;;;-5;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "// FLAILS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Flail;1;mace32;mace;Common;wood;75;1;50;1;;5;;5;;;;18;;;;;;;;;;;;4;-2;2;;;;;;;;10;;;;12;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Rebel Flail;1;mace33;mace;Common;wood;75;1;50;1;;5;;5;;;;18;;;;;;;;;;;;6;-1;3;;;;;;;;10;;;;12;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Militia Flail;2;mace34;mace;Common;metal;425;1;70;1;;5;;7;;;;20;;;;;;;;;;;;6;-4;3;;;;;;;;14;;;;18;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Recruit Flail;2;mace35;mace;Common;metal;425;1;75;1;;5;;12;;;;20;;;;;;;;;;;;8;-5;3;;;;;;;4;11;;;;18;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Footman Flail;3;mace36;mace;Common;metal;1125;1;95;1;;10;;15;;;2;21;;;;;;;;;;;;8;-8;4;;;;;;;7;13;;;;28;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Vehement Flail;3;mace37;mace;Common;metal;1250;1;80;1;;10;;10;;;;22;;;;;;;;;1;;;10;-6;5;5;;;;;;;18;;;;24;;;;;;15;-5;;;;;;;;;;;;;;;;;;;;;;;;;0;magic;;;;",
        "Nistrian Flail;4;mace38;mace;Common;metal;2925;1;135;1;;15;;12;;;;27;;;;;;;;;;;;10;-8;5;;;;;;;;28;;;;30;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;nistra;;;;",
        "Veteran Flail;4;mace39;mace;Common;metal;2550;1;100;1;;10;;18;;;;27;;;;;;;;;;;;12;-6;6;8;;;;;;9;15;;;;35;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Knightly Flail;5;mace40;mace;Common;metal;5350;1;120;1;;15;;15;;;;31;;;;;;;;;;;;12;-10;6;;;;;;;;26;;;;36;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Tripleheaded Flail;5;mace41;mace;Common;metal;5350;1;105;1;;15;;26;;;3;28;;;;;;;;;;;;14;-8;7;12;;;;;;10;18;;;;41;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Elven Flail;5;mace42;mace;Common;metal;5900;1;135;1;;20;;15;;;;29;;;;;;;;;;;;12;-10;3;;;;;;;;30;10;;;32;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;elven;;;;",
        "Amir Double Flail;5;mace43;mace;Common;metal;5350;1;120;1;;15;;30;;;;31;;;;;;;;;;;;12;-8;6;20;;;;;;12;20;;;;45;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "Grandmaster Flail;5;mace44;mace;Common;metal;5350;1;130;1;;15;;15;;;;26;;;;;;;;;5;;;14;-10;6;;;;;;;;30;;;;30;;;;;;25;-10;;;;;;;;;;;;;5;;;;;;;;;;;;0;unique;;;;",
        "// FLAVOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;\\;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Metal Rod;1;mace01f;mace;Common;metal;5;1;50;1;;;;;;;;14;;;;;;;;;;;;3;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;;;;",
        "Carpenter Hammer;1;mace02f;mace;Common;metal;25;1;40;1;;3;;;;;;15;;;;;;;;;;;;2;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;Wheels2;;;",
        "Forging Hammer;1;mace03f;mace;Common;metal;50;1;60;1;;5;;;;;;17;;;;;;;;;;;;1;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;special;;;;",
        "[ DAGGERS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Commoner Dagger;1;dagger01;dagger;Common;metal;50;1;35;1;;5;;;;;13;;;;;;;;;;;;;;3;2;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Shiv;1;dagger02;dagger;Common;metal;25;1;25;1;;5;;;;;13;;;;;;;;;;;;;;2;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Bollock;2;dagger03;dagger;Common;metal;275;1;50;1;;5;;;;;15;;;;;;;;;;;;;;4;3;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Dirk;2;dagger04;dagger;Common;metal;275;1;50;1;;5;;;;;15;;;;;;;;;;;;;1;4;3;;;;;;;14;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Rondel;3;dagger05;dagger;Common;metal;750;1;60;1;;10;;;;;17;;;;;;;;;;;;;;5;4;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Baselard;3;dagger06;dagger;Common;metal;750;1;60;1;;10;;;;;17;;;;;;;;;;;;;;5;1;;3;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Misericorde;4;dagger07;dagger;Common;metal;1700;1;70;1;;10;;;;;19;;;;;;;;;;;;;;6;5;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Jambiya;4;dagger08;dagger;Common;metal;1950;1;70;1;;10;;10;;;19;;;;;;;;;;;;;;2;5;;;;;;;26;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;elven;;;;",
        "Decorated Dagger;4;dagger09;dagger;Unique;metal;1700;1;65;1;;10;;;;;19;;;;;;;;;;;;;;9;5;;;;;;;20;;;;;;;;;;;-4;-4;;;;;;;;;;;;;;;;;;;;;;;;;4;unique;;;;",
        "Stiletto;5;dagger10;dagger;Common;metal;3575;1;85;1;;15;;;;;22;;;;;;;;;;;;;;7;6;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;elven;;;;",
        "Assassin Dagger;5;dagger11;dagger;Common;metal;3575;1;85;1;;15;;;;;22;;;;;;;;;;;;;-2;7;7;5;;;;;;18;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Ceremonial Dagger;5;dagger12;dagger;Unique;metal;3575;1;70;1;;15;;5;;;18;;;;;;;;;;3;;;;7;6;;;;;;;32;;;;;;;;;;;;;;;1;;;;;;;;;;;;;;;;;;;;;;4;unique;;;;",
        "// DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Recruit Dagger;2;dagger13;dagger;Common;metal;250;1;60;1;;5;;;;;14;;;;;;;;;;;;;;;;;4;4;3;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Soldier Dagger;3;dagger14;dagger;Common;metal;650;1;75;1;;10;;;;;16;;;;;;;;;;;;;;;;;6;6;4;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Merchant Dagger;3;dagger15;dagger;Common;metal;650;1;75;1;;10;;;;;16;;;;;;;;;;;;;-1;;;;6;6;1;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Duelist Dagger;4;dagger16;dagger;Common;metal;1450;1;90;1;;10;;;;;18;;;;;;;;;;;;;;;;;9;11;7;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Veteran Dagger;4;dagger17;dagger;Common;metal;1450;1;90;1;;10;;;;;18;;;;;;;;;;;;;;;;;8;8;6;;;16;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;4;aldor;;;;",
        "Gilded Dagger;4;dagger18;dagger;Unique;metal;1450;1;80;1;;10;;;;;18;;;;;;;;;;;;;;5;;;8;8;6;;;16;;;;;;;;;;;;;;-2;;;;;;;;;;;5;;;;;;;;;;;;4;unique;;;;",
        "Parrying Dagger;5;dagger19;dagger;Common;metal;3050;1;110;1;;15;;;;;21;;;;;;;;;;;;;;;;;12;13;11;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Nobleman Quillon;5;dagger20;dagger;Common;metal;3050;1;105;1;;15;;;;;21;;;;;;;;;;;;;;;;;10;10;8;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Ducal Dagger;5;dagger21;dagger;Unique;metal;3050;1;120;1;;15;;;;;21;;;;;;;;;;;;;-2;;2;;10;11;9;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;unique;;;;",
        "// FLAVOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Kitchen Knife;1;dagger01f;dagger;Common;metal;10;1;25;1;;-20;;;;;9;;;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;special;;;;",
        "[ 2H SWORDS];;;;;;10;;;;;ї;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// DUEL;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "War Scythe;1;gsword01;2hsword;Common;metal;125;1;70;1;;10;;;;19;;;;;;;;;;;;;;;2;;;;4;;;;;;;;;10;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Crude Longsword;2;gsword02;2hsword;Common;metal;675;1;95;1;;10;;;;22;;;;;;;;;;;;;;;4;;;;6;;;;;;;;;15;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Footman Longsword;3;gsword03;2hsword;Common;metal;1775;1;120;1;;15;;;;25;;;;;;;;;;;;;;;6;;;;8;;;;;;;;;20;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Mercenary Longsword;4;gsword04;2hsword;Common;metal;4000;1;145;1;;20;;;;29;;;;;;;;;;;;;;;10;;;;8;;;;;;;;;25;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Sergeant Longsword;4;gsword05;2hsword;Common;metal;4000;1;150;1;;20;;;;29;;;;;;;;;;;;;;;8;;;;10;;;;15;;;;;15;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Noble Longsword;5;gsword06;2hsword;Common;metal;8425;1;170;1;;25;;;;34;;;;;;;;;;;;;;;10;;;;12;;;;;;;;;30;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Ornate Longsword;5;gsword07;2hsword;Unique;metal;8425;1;145;1;;25;;;;34;;;;;;;;;;;;;;;10;;;5;10;;;;;;;;;30;;;4;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// AOE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Warbrand;1;gsword08;2hsword;Common;metal;125;1;65;1;;10;;;;19;;;;;;;;;;;;;;2;;2;;;;;;;5;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Recruit Bastard Sword;2;gsword09;2hsword;Common;metal;700;1;90;1;;10;;;;22;;;;;;;;;;;;;;4;;3;;;;;;;10;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Footman Bastard Sword;3;gsword10;2hsword;Common;metal;1825;1;110;1;;15;;;;25;;;;;;;;;;;;;;6;;4;;;;;;;15;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Nistrian Rhomphaia;3;gsword11;2hsword;Common;metal;2025;1;110;1;;10;;;;26;;;;;;;;;;;;;;7;-3;4;;;;;;;19;;;;;8;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;nistra;;;;",
        "Claymore;4;gsword12;2hsword;Common;metal;4125;1;130;1;;20;;;;29;;;;;;;;;;;;;;8;;5;;;;;;;20;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Flamberg;5;gsword13;2hsword;Common;metal;8675;1;140;1;;25;;15;;35;;;;;;;;;;;;;;14;;6;;;;;;;30;;;;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Grossemesser;5;gsword14;2hsword;Common;metal;8675;1;155;1;;25;;;;34;;;;;;;;;;;;;;10;-2;7;10;;;;;;25;;;;;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Captain Greatsword;5;gsword15;2hsword;Common;metal;8675;1;160;1;;25;;;;34;;;;;;;;;;;;;;10;;6;;;;;;;25;;;;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Blademaster Greatsword;5;gsword16;2hsword;Unique;metal;8675;1;170;1;;33;;;;34;;;;;;;;;;;;;;13;;6;;3;3;;;;31;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// COUNTER;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Vanguard Twohander;3;gsword17;2hsword;Common;metal;1725;1;130;1;;15;;;;25;;;;;;;;;;;;;;;;;;6;9;9;;;;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Duelling Twohander;4;gsword18;2hsword;Common;metal;3875;1;140;1;;20;;;;29;;;;;;;;;;;;;;;;;;8;12;11;;;;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Sergeant Twohander;4;gsword19;2hsword;Common;metal;3875;1;155;1;;20;;;;29;;;;;;;;;;;;;;;;;;6;10;11;;;7;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Knightly Twohander;5;gsword20;2hsword;Common;metal;8150;1;180;1;;25;;;;34;;;;;;;;;;;;;;-2;;-2;;12;15;13;;;;;;;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Exquisite Twohander;5;gsword21;2hsword;Unique;metal;8150;1;200;1;;25;;;;35;;;;;;;;;;;;;;;;;;10;17;16;;;;;;;;15;;;;;;30;;;;;;;;;;;;-5;;;;;;;;;;;;;;0;unique;;;;",
        "Espadon;5;gsword22;2hsword;Unique;metal;8150;1;175;1;;30;;;;34;;;;;;;;;;;;;;;3;3;;10;15;13;;;;;;;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "[ SPEARS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Spear;1;spear01;spear;Common;metal;100;1;80;1;;10;;;;;17;;;;;;;;;;;;;-2;4;;;;;;;;10;;;;8;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Hilda Spear;2;spear02;spear;Unique;metal;525;1;95;1;;15;;5;;;17;;;;;;;;;;;;;-4;6;;;;;;;;12;;;;18;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;3;special exc;;;;",
        "Hunting Spear;2;spear03;spear;Common;metal;525;1;105;1;;15;;;;;19;;;;;;;;;;;;;-4;6;;;;;;;;15;;;;12;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Godentag;2;spear04;spear;Common;metal;525;1;115;1;;15;;;;;17;;;;;;;;;;;;;2;2;;;;;;;;20;10;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Ox Tongue Spear;3;spear05;spear;Common;metal;1400;1;130;1;;20;;;;;22;;;;;;;;;;;;;-6;8;;;;;;;;20;;;;16;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Ahlspiess;4;spear06;spear;Common;metal;3400;1;155;1;;25;;;;;26;;;;;;;;;;;;;-8;10;;;;;;;;25;;;;20;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Sponton;5;spear07;spear;Common;metal;7150;1;180;1;;30;;;;;29;;;;;;;;;;;;;-10;12;;;;;;;;30;;;;24;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Faceless Spear;5;spear08;spear;Unique;metal;7150;1;200;1;;30;;15;;;29;;;;;;;;;;;;;-10;12;3;5;;;;;;30;;;36;12;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "Radiant Spear;5;spear09;spear;Unique;metal;7150;1;165;1;;30;;;;;26;;;;;;;;;;3;;;-10;12;;;;;;;;30;;;;24;;;;3;;;20;;;;;;;;;;;;9;;3;;;;;;;;;;;;2;unique;;;;",
        "// DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Militia Spear;2;spear10;spear;Common;metal;500;1;130;1;;15;;;;;19;;;;;;;;;;;;;;4;;;6;7;5;;;;;;20;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Footman Spear;3;spear11;spear;Common;metal;1300;1;160;1;;20;;;;;22;;;;;;;;;;;;;;6;;;8;10;7;;;;;;25;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Norse Spear;3;spear12;spear;Common;metal;1300;1;160;1;;20;;5;;;22;;;;;;;;;;;;;;6;;20;8;7;5;;;;;;25;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;3;fjall;;;;",
        "Veteran Spear;4;spear13;spear;Common;metal;3025;1;190;1;;25;;;;;26;;;;;;;;;;;;;;8;;;10;13;9;;;;;;30;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Protasan;5;spear14;spear;Common;metal;6375;1;225;1;;30;;;;;29;;;;;;;;;;;;;;10;;;12;16;12;;;;;;35;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Castellier Spear;5;spear15;spear;Unique;metal;7150;1;260;1;;30;;;;;29;;;;;;;;;;;;;;10;;;12;21;18;15;;;;;35;;;;;;;;30;;;;;;;;;;;10;;;;-3;;;;;;;;;;;2;unique;;;;",
        "// FORKS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Pitchfork;1;spear16;spear;Common;metal;100;1;80;1;;5;;;;;16;;;;;;;;;;;;;;2;;;;;;;;10;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "War Pitchfork;2;spear17;spear;Common;metal;525;1;105;1;;10;;;;;18;;;;;;;;;;;;;;4;2;;;4;;;;20;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Mancatcher;2;spear18;spear;Common;metal;525;1;105;1;;10;;;;;17;;;;;;;;;;;;;;2;;;4;4;;;;20;;;;20;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Brandistock;3;spear19;spear;Common;metal;1350;1;130;1;;15;;;;;21;;;;;;;;;;;;;;6;3;;;6;;;;25;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;3;aldor;;;;",
        "Fauchard Fork;4;spear20;spear;Common;metal;3150;1;155;1;;20;;;;;24;;;;;;;;;;;;;;8;4;;;8;;;;30;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Spetum;5;spear21;spear;Common;metal;6625;1;180;1;;25;;;;;28;;;;;;;;;;;;;;10;5;;;10;;;;45;;;;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Ornate Fork;5;spear22;spear;Unique;metal;6625;1;165;1;;30;;;;;28;;;;;;;;;;;;;;10;5;;7;10;;;;45;;;;20;;;;;-5;;30;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "// HALBERDS - AXE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Guard Halberd;2;spear23;spear;Common;metal;625;1;105;1;;20;15;14;;20;;;;;;;;;;;;;;5;;3;;;;;;;;;;;;13;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Soldier Halberd;3;spear24;spear;Common;metal;1625;1;130;1;;25;20;18;;23;;;;;;;;;;;;;;7;;4;;;;;;;;;;;;16;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Mercenary Halberd;4;spear25;spear;Common;metal;3625;1;155;1;;30;25;22;;27;;;;;;;;;;;;;;9;;5;;;;;;;;;;;;19;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Sabre Halberd;5;spear26;spear;Common;metal;7650;1;180;1;;35;30;26;;31;;;;;;;;;;;;;;11;;6;;;;;;;;;;;;22;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Ceremonial Halberd;5;spear27;spear;Unique;metal;7650;1;165;1;;35;30;35;;31;;;;;;;;;;;;;;13;;8;15;;;;;;12;;;;;22;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// HALBERDS - SPEAR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Recruit Billhook;2;spear28;spear;Common;metal;625;1;110;1;;20;8;8;;;20;;;;;;;;;;;;;4;;;10;;;;;;;;;;11;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Guisarme;3;spear29;spear;Common;metal;1625;1;140;1;;25;12;12;;;23;;;;;;;;;;;;;6;;;15;;;;;;;;;;14;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Bill;4;spear30;spear;Common;metal;3625;1;170;1;;30;16;16;;;27;;;;;;;;;;;;;8;;;20;;;;;;;;;;17;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Scorpion Halberd;5;spear31;spear;Common;metal;7650;1;195;1;;35;20;20;;;31;;;;;;;;;;;;;10;;;25;;;;;;;;;;20;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "[2H AXES ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// POLEAXE - DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Militia Poleaxe;2;gaxe01;2haxe;Common;metal;575;1;110;1;;15;;;;23;;;;;;;;;;;;;;;;;;;5;6;;;15;;;20;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Soldier Poleaxe;3;gaxe02;2haxe;Common;metal;1500;1;135;1;;20;;;;26;;;;;;;;;;;;;;;;;;;7;8;;;20;;;25;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Veteran Poleaxe;4;gaxe03;2haxe;Common;metal;3400;1;180;1;;25;;;;30;;;;;;;;;;;;;;;;;;;9;10;;;25;;;30;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Sergeant Poleaxe;4;gaxe04;2haxe;Common;metal;3400;1;160;1;;25;;;;30;;;;;;;;;;;;;;;;;;3;5;10;;;25;;;39;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Knightly Poleaxe;5;gaxe05;2haxe;Common;metal;7150;1;210;1;;30;;;;35;;;;;;;;;;;;;;;;;;;11;12;;;30;;;35;;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Captain Poleaxe;5;gaxe06;2haxe;Common;metal;7150;1;190;1;;30;;;;35;;;;;;;;;;;;;;;;;;4;9;12;;;30;;;40;;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Maze Warden Poleaxe;5;gaxe07;2haxe;Unique;metal;7150;1;200;1;;33;;;;28;;;;;;;;;3;;3;;;;;;;;10;16;;;28;;;28;18;;;;3;;;33;-9;9;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// POLEAXE - AOE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Recruit Heavy Broadaxe;2;gaxe08;2haxe;Common;metal;625;1;105;1;;15;25;;;23;;;;;;;;;;;;;;6;;4;7;;;;;;;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Soldier Heavy Broadaxe;3;gaxe09;2haxe;Common;metal;1625;1;130;1;;20;30;;;26;;;;;;;;;;;;;;8;;5;10;;;;;;;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Nistrian Bardiche;3;gaxe10;2haxe;Common;metal;1775;1;135;1;;20;20;;;26;;;;;;;;;;;;;;8;;5;10;;;;;;8;;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;nistra;;;;",
        "Eederax;4;gaxe11;2haxe;Common;metal;3625;1;155;1;;25;35;;;30;;;;;;;;;;;;;;10;;6;12;;;;;;;;;10;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Garrison Eederax;5;gaxe12;2haxe;Common;metal;7650;1;180;1;;30;40;;;35;;;;;;;;;;;;;;12;;7;15;;;;;;;;;12;;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Skadian Bardiche;5;gaxe13;2haxe;Common;metal;8425;1;210;1;;30;30;;;35;;;;;;;;;;;;;;12;;7;20;;;;;;12;;;;;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;skadia;;;;",
        "// GLAIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Doloire;1;gaxe14;2haxe;Common;metal;125;1;80;1;;10;;20;;18;;;;;;;;;;;;;;1;2;;;2;;;;;15;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Voulge;2;gaxe15;2haxe;Common;metal;625;1;105;1;;10;;25;;21;;;;;;;;;;;;;;;4;;;3;;;;;20;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Billhook;2;gaxe16;2haxe;Common;metal;625;1;105;1;;10;5;25;;21;;;;;;;;;;;;;;;4;;;3;;;;;15;;;5;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Jagged Voulge;3;gaxe17;2haxe;Common;metal;1625;1;130;1;;15;;30;;24;;;;;;;;;;;;;;;6;;5;4;;;;;30;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Skadian Sovnya;3;gaxe18;2haxe;Common;metal;1775;1;145;1;;10;;30;;24;;;;;;;;;;;;;;;6;2;;5;;;;;25;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;skadia;;;;",
        "Glaive;4;gaxe19;2haxe;Common;metal;3625;1;155;1;;15;;35;;28;;;;;;;;;;;;;;;8;;;5;;;;;30;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Forholt Staff;5;gaxe20;2haxe;Common;metal;7650;1;180;1;;20;;40;;32;;;;;;;;;;;;;;;10;;;6;;;;;35;;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Decorated Voulge;5;gaxe21;2haxe;Unique;metal;7650;1;165;1;;20;;45;;32;;;;;;;;;;;;;;-1;10;3;;6;;;;;35;;;15;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "// LONGAXES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Lumberjack Longaxe;1;gaxe22;2haxe;Common;metal;125;1;75;1;;15;;10;;19;;;;;;;;;;;;;;2;-7;;;;;;;;;;;;;15;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Militia Longaxe;2;gaxe23;2haxe;Common;metal;625;1;100;1;;20;;15;;24;;;;;;;;;;;;;;;-8;;;;;;;;;;;;;20;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Footman Longaxe;3;gaxe24;2haxe;Common;metal;1625;1;125;1;;25;;20;;28;;;;;;;;;;;;;;;-10;;;;;;;;;;;;;25;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;1;fjall;;;;",
        "Veteran Longaxe;4;gaxe25;2haxe;Common;metal;3625;1;150;1;;30;;25;;32;;;;;;;;;;;;;;;-12;;;;;;;;;;;;;30;;;;;;45;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Eastern Longaxe;4;gaxe26;2haxe;Common;metal;4000;1;165;1;;33;10;25;;32;;;;;;;;;;;;;;;-10;;;;;;;;;;;;;30;;;;;;45;;;;;;;;;;;;;;;;;;;;;;;;;;2;elven;;;;",
        "Double Headed Longaxe;5;gaxe27;2haxe;Common;metal;7650;1;160;1;;35;;30;;36;;;;;;;;;;;;;;8;-14;4;8;;;;;;;;;;;35;;;;;;50;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Elven Longaxe;5;gaxe28;2haxe;Common;metal;7650;1;175;1;;35;15;25;;36;;;;;;;;;;;;;;;-10;;;;;;;;;;;;;35;;;;;;50;;;;;;;;;;;;;;;;;;;;;;;;;;2;elven;;;;",
        "Ornate Longaxe;5;gaxe29;2haxe;Unique;metal;7650;1;160;1;;35;;30;;36;;;;;;;;;;;;;;-3;-9;;;;4;;;;5;;;;;40;;;;;;50;;;;;;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "// FLAVOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Scythe;1;gaxe01f;2haxe;Common;metal;25;0;50;1;;-15;;;;10;;;;;;;;;;;;;;10;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;special;;;;",
        "[ 2H MACES];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// POLEMACES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Recruit Polemace;2;gmace01;2hmace;Common;metal;625;1;110;1;;25;30;;;;;22;;;;;;;;;;;;6;;4;;;;;;;;15;;;;17;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Soldier Polemace;3;gmace02;2hmace;Common;metal;1625;1;140;1;;30;35;;;;;25;;;;;;;;;;;;8;;5;;;;;;;;20;;;;20;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Veteran Polemace;4;gmace03;2hmace;Common;metal;3625;1;170;1;;35;40;;;;;29;;;;;;;;;;;;10;;6;;;;;;;;25;;;;23;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Nistrian Polemace;4;gmace04;2hmace;Common;metal;4000;1;185;1;;35;30;5;;;;29;;;;;;;;;;;;10;;6;5;;;;;;10;25;;;;20;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;nistra;;;;",
        "Knightly Polemace;5;gmace05;2hmace;Common;metal;7650;1;195;1;;35;45;;;;;34;;;;;;;;;;;;12;;7;;;;;;;;30;;;;25;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Exquisite Grandmace;5;gmace06;2hmace;Unique;metal;7650;1;155;1;;40;45;;;;;34;;;;;;;;;;;;9;4;7;;;;;;;;30;;15;;25;;;;;;35;;5;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// POLEHAMMERS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Militia Polehammer;2;gmace07;2hmace;Common;metal;550;1;130;1;;35;45;;;;;21;;;;;;;;;;;;;4;;;;6;4;;;;15;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Kriegshammer;3;gmace08;2hmace;Common;metal;1450;1;160;1;;40;40;8;;;;24;;;;;;;;;;;;;5;;;;8;6;;;;20;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Mercenary Polehammer;4;gmace09;2hmace;Common;metal;3600;1;190;1;;45;55;;;;;28;;;;;;;;;;;;;6;;;;11;10;;;;25;;;;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Veteran Polehammer;4;gmace10;2hmace;Common;metal;3275;1;210;1;;45;55;;;;;28;;;;;;;;;;;;;7;;;;10;8;;;;30;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Hawk Beak;4;gmace11;2hmace;Common;metal;3275;1;175;1;;40;40;;;;;29;;;;;;;;;;;;;5;;10;;10;8;;;10;25;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Knightly Polehammer;5;gmace12;2hmace;Common;metal;6875;1;245;1;;50;60;;;;;32;;;;;;;;;;;;;7;;;;12;10;;;;33;;;;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Brynn Polehammer;5;gmace13;2hmace;Common;metal;6875;1;225;1;;50;50;;;;;32;;;;;;;;;;;;;9;1;;1;12;10;;;;27;;;;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Ornate Polehammer;5;gmace14;2hmace;Unique;metal;6875;1;215;1;;55;60;;;;;32;;;;;;;;;;;;-4;7;;;;12;10;;;10;30;;;;10;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "Gilded Polehammer;5;gmace15;2hmace;Unique;metal;6875;1;190;1;;50;75;;;;;32;;;;;;;;;;;;;7;;;;12;10;;;;30;10;;;;;5;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// CLUBS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Big Club;1;gmace16;2hmace;Common;wood;50;1;40;1;;20;;5;;;;18;;;;;;;;;;;;2;-5;;;;;;;;;;5;15;;15;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Heavy Warclub;1;gmace17;2hmace;Common;metal;150;2;85;1;;25;;5;;;;20;;;;;;;;;;;;;-5;;;;;;;;;;6;15;;15;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Reinforced Warclub;2;gmace18;2hmace;Common;metal;525;1;110;1;;30;;8;;;;23;;;;;;;;;;;;;-7;;;;;;;;;;10;20;;20;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Spiked Warclub;3;gmace19;2hmace;Common;metal;1400;1;140;1;;25;;10;;;;26;;;;;;;;;;;;3;-9;;5;;;;;;15;;10;25;;25;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Veteran War Maul;4;gmace20;2hmace;Common;metal;3400;1;170;1;;35;;12;;;;30;;;;;;;;;;;;;-11;;;;;;;;;;20;30;;30;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Knightly War Maul;5;gmace21;2hmace;Common;metal;7150;1;195;1;;40;;15;;;;35;;;;;;;;;;;;;-13;;;;;;;;;;25;35;;35;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Elven War Maul;5;gmace22;2hmace;Common;metal;7850;1;215;1;;45;20;15;;;;35;;;;;;;;;;;;;-5;;;;;;;;;;25;35;;35;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;elven;;;;",
        "// HEAVY FLAILS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Heavy Flail;1;gmace23;2hmace;Common;metal;100;1;80;1;;10;;15;;;;21;;;;;;;;;;;;6;-1;;;;;;;;;20;15;;;30;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Long Flail;2;gmace24;2hmace;Common;metal;450;1;105;1;;10;;20;;;;24;;;;;;;;;;;;8;-3;;;;;;;;;25;20;;;40;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Woodsman Heavy Flail;2;gmace25;2hmace;Common;metal;600;1;115;1;;14;;20;;;;24;;;;;;;;;;;;11;-2;;;;;;;;;28;22;;;32;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Maen Flail;3;gmace26;2hmace;Common;metal;1550;1;130;1;;15;;25;;;;28;;;;;;;;;;;;10;-5;2;5;;;;;;;33;25;;;40;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Soldier Heavy Flail;3;gmace27;2hmace;Common;metal;1325;1;150;1;;15;;20;;;;28;;;;;;;;;;;;8;-5;;;;;;;;;30;28;;;46;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Veteran Heavy Flail;4;gmace28;2hmace;Common;metal;3400;1;155;1;;20;;30;;;;32;;;;;;;;;;;;12;-7;;;;;;;;;35;30;;;45;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Knightly Heavy Flail;5;gmace29;2hmace;Common;metal;7650;1;180;1;;25;;35;;;;36;;;;;;;;;;;;14;-9;;;;;;;;;45;35;;;50;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Noble Heavy Flail;5;gmace30;2hmace;Common;metal;7650;1;190;1;;25;;40;;;2;35;;;;;;;;;;;;14;-9;;15;;;;;;10;36;32;;;50;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Ordermarshal Flail;5;gmace31;2hmace;Unique;metal;7650;1;165;1;;25;;35;;;;33;;;;;;;;;4;;;14;-9;;;;;;;;;40;40;;;50;;;;-9;;33;-9;;;;;;;;;;;;;;;;;;;;;;;;;1;unique;;;;",
        "[ BOWS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// SKIRMISH;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Bow;1;bow01;bow;Common;wood;75;1;35;8;;5;;;;;15;;;;;;;;;;;;;-3;-65;;;;;;;;;;;5;;10;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Guard Bow;2;bow02;bow;Common;wood;400;1;50;8;;5;;;;;17;;;;;;;;;;;;;-5;-60;;;;;;;;;;;8;;15;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Soldier Bow;3;bow03;bow;Common;wood;1075;1;60;8;;10;;;;;20;;;;;;;;;;;;;-7;-55;;;;;;;;;;;12;;20;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Norse Bow;3;bow04;bow;Common;wood;1075;1;60;8;;10;;10;;;20;;;;;;;;;;;;;-7;-60;;10;;;;;;;;;12;;20;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;fjall;;;;",
        "Veteran Bow;4;bow05;bow;Common;wood;2425;1;70;9;;10;;;;;23;;;;;;;;;;;;;-7;-50;;;;;;;;;;;16;;25;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Firn Bow;4;bow06;bow;Common;wood;2425;1;70;9;;10;;5;;;23;;;;;;;;;;;;;-9;-50;;;;;;;;;;;20;;20;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Aldor Bow;5;bow07;bow;Common;wood;5100;1;85;9;;15;;;;;27;;;;;;;;;;;;;-12;-50;;;;;;;;;;;20;;30;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Relict Longbow;5;bow08;bow;Unique;wood;6125;1;110;8;;15;;;;;25;;;;;;;;4;;;;;-15;-61;;;;;;;;;;;20;;30;;;;-5;;35;;9;;;;;;;;;;;;7;;;;;;;;;;;;0;unique;;;;",
        "// SHORTBOW;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Training Shortbow;1;bow09;bow;Common;wood;75;1;30;7;;5;;;;;15;;;;;;;;;;;;;-1;-60;;;;;;;;10;;;;8;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Dirwin Shortbow;2;bow10;bow;Unique;wood;400;1;50;8;;5;;;;;17;;;;;;;;;;;;;-2;-55;;;;;;;;13;;;;11;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;special exc;;;;",
        "Hunting Shortbow;2;bow11;bow;Common;wood;400;1;40;7;;5;;;;;17;;;;;;;;;;;;;-2;-55;;;;;;;;13;;;;11;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Yew Shortbow;3;bow12;bow;Common;wood;1075;1;50;7;;10;;;;;20;;;;;;;;;;;;;-4;-50;;;;;;;;15;;;;14;;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Nomad Shortbow;3;bow13;bow;Common;wood;1200;1;60;7;;10;;;;;20;;;;;;;;;;;;;-5;-50;;;;;;;;17;;;;10;7;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;elven;;;;",
        "Skirmisher Shortbow;4;bow14;bow;Common;wood;2425;1;60;8;;10;;;;;23;;;;;;;;;;;;;-6;-45;;;;;;;;18;;;;17;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Nistrian Bow;4;bow15;bow;Common;wood;2650;1;60;8;;10;;;;;23;;;;;;;;;;;;;-6;-45;;5;;;;;;23;;;;15;;;;;;;24;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Curved Shortbow;5;bow16;bow;Common;wood;5100;1;70;8;;15;;;;;27;;;;;;;;;;;;;-8;-40;;;;;;;;20;;;;20;;;;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Ornate Bow;5;bow17;bow;Unique;wood;5100;1;70;9;;15;;12;;;27;;;;;;;;;;;;;-8;-36;;;;;;;;25;;;;20;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// LONGBOW;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Soldier Longbow;3;bow18;bow;Common;wood;1300;1;70;10;;15;;;;;23;;;;;;;;;;;;;6;-65;;;;;;;;;;;20;17;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Eeders Longbow;4;bow19;bow;Common;wood;2900;1;85;10;;15;;;;;27;;;;;;;;;;;;;8;-60;;;;;;;;;;;25;21;;;;;;;45;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Sharpshooter Bow;5;bow20;bow;Common;wood;6125;1;100;11;;20;;;;;31;;;;;;;;;;;;;10;-55;;;;;;;;;;;30;25;;;;;;;50;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Sand Guard Bow;5;bow21;bow;Unique;wood;6125;1;100;12;;20;;15;;;31;;;;;;;;;;;;;5;-50;;;;;;;;;;;30;25;;;;;-5;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "Decorated Longbow;5;bow22;bow;Unique;wood;6125;1;90;10;;20;;;;;31;;;;;;;;;;;;;10;-55;3;;;;;;;15;;;33;33;;;;;;;50;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "[ XBOWS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// LIGHT;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Training Crossbow;1;crossbow01;crossbow;Common;wood;100;1;50;6;;10;;;;;18;;;;;;;;;;;;;-5;-36;;;;;;;;;;;;8;10;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Hunting Crossbow;2;crossbow02;crossbow;Common;wood;525;1;65;6;;10;;;;;20;;;;;;;;;;;;;-7;-32;;;;;;;;;;;;11;13;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Soldier Crossbow;3;crossbow03;crossbow;Common;wood;1400;1;80;6;;15;;;;;23;;;;;;;;;;;;;-10;-28;;;;;;;;;;;;14;17;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Maen Crossbow;3;crossbow04;crossbow;Common;wood;1400;1;70;6;;15;;5;;;22;;;;;;;;;;;;;-10;-28;3;;;;;;;;;;;14;17;;;;-5;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Aldwynn Crossbow;4;crossbow05;crossbow;Common;wood;3150;1;105;7;;25;10;;;;27;;;;;;;;;;;;;-12;-24;;;;;;;;;;;;15;24;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Eastern Crossbow;4;crossbow06;crossbow;Common;wood;3775;1;95;8;;20;;;;;25;;;;;;;;;;;;;-14;-24;;;;;;;;8;;;;17;21;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;1;elven;;;;",
        "Brynn Crossbow;5;crossbow07;crossbow;Common;wood;6625;1;110;7;;30;;;;;31;;;;;;;;;;;;;-15;-20;;;;;;;;;;;;25;20;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Ornate Crossbow;5;crossbow08;crossbow;Unique;wood;6625;1;100;8;;20;;;;;29;;;;;;;;;;;;;-15;-17;;;;;;;;10;;;;20;25;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "// MEDIUM;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Belt Crossbow;2;crossbow09;crossbow;Common;wood;600;1;55;7;;15;;;;;22;;;;;;;;;;;;;;-42;;;;;;;;17;;;15;;20;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Cranequin Crossbow;3;crossbow10;crossbow;Common;wood;1575;1;70;8;;20;;;;;25;;;;;;;;;;;;;;-38;;;;;;;;20;;;20;;25;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Mercenary Crossbow;4;crossbow11;crossbow;Common;wood;3500;1;85;8;;25;;;;;29;;;;;;;;;;;;;;-34;;;;;;;;23;;;25;;30;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Garrison Crossbow;5;crossbow12;crossbow;Common;wood;7400;1;100;8;;25;;;;;34;;;;;;;;;;;;;;-30;;;;;;;;25;;;30;;35;;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Royal Huntmaster Crossbow;5;crossbow13;crossbow;Unique;wood;7400;1;100;8;;25;;15;;;34;;;;;;;;;;;;;;-30;;15;;;;;;33;;;30;;35;;;;;;25;;;;;;;;;;;;10;;;;;;;;;;;;;;0;unique;;;;",
        "// HEAVY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Guard Crossbow;2;crossbow14;crossbow;Common;wood;650;1;50;8;;20;;10;;;24;;;;;;;;;;;;;;-55;;;;;;;;;;;20;20;;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Lever Crossbow;3;crossbow15;crossbow;Common;wood;1725;1;60;9;;25;;15;;;28;;;;;;;;;;;;;;-50;;;;;;;;;;;25;25;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;1;aldor;;;;",
        "Windlass Crossbow;4;crossbow16;crossbow;Common;wood;3875;1;70;9;;30;;20;;;32;;;;;;;;;;;;;;-45;;;;;;;;;;;30;30;;;;;;;35;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Siege Crossbow;5;crossbow17;crossbow;Common;wood;8150;1;85;9;;30;;25;;;36;;;;;;;;;;;;;;-40;;;;;;;;;;;35;35;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;aldor;;;;",
        "Guardsman Crossbow;5;crossbow18;crossbow;Unique;wood;8150;1;95;8;;30;15;25;;;36;;;;;;;;;;;;;-5;-36;;;;;;;;;;;50;40;;;;;;;40;;;;;;;;;;;;;;;;;;;;;;;;;;0;unique;;;;",
        "[ SLINGS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Makeshift Sling;1;sling01;sling;Common;leather;;1;;9;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Training Sling;2;sling02;sling;Common;leather;;1;;9;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Peasant Sling;2;sling03;sling;Common;leather;;1;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Hunting Sling;2;sling04;sling;Common;leather;;1;;11;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Scout Sling;3;sling05;sling;Common;leather;;1;;9;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Militia Sling;3;sling06;sling;Common;metal;;1;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Skirmisher Sling;4;sling07;sling;Common;leather;;1;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Battle Sling;4;sling08;sling;Common;metal;;1;;11;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "Ornate Sling;5;sling09;sling;Unique;leather;;1;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;0;WIP;;;;",
        "[ STAFFS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// UTILITY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Healer Staff;2;staff01;2hStaff;Common;wood;400;1;60;1;;;;;;;;13;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;-3;;10;-3;;-3;;;;;;1;7;;8;;;;;;;;;;;;;;2;aldor;;;;",
        "Monk Staff;3;staff02;2hStaff;Common;wood;1075;1;75;1;;;;;;;;14;;;;;;;;;1;;;;;;;;;;;;;;;;;;;;3;-5;;15;-3;;-5;;;;1;;;;7;12;;;;;;;;;;;;;;2;magic;;;;",
        "Dwarven Staff;3;staff03;2hStaff;Common;wood;1075;1;75;1;;;;;;;;13;;3;;;;;;;;;;;;4;;;;;;;;;;;;;;;3;-5;;15;-3;;;2;;;;;;;;12;;;;;;;;;;;;;;2;fjall;;;;",
        "Hierophant Staff;4;staff04;2hStaff;Common;metal;2425;1;90;1;;;;;;;;12;;;;;;;;;5;;;-3;;;;;;;;;;;;;;;;;6;-7;;15;-4;;-7;;;;;;;;;16;;;-3;;;;;;;;;;;2;magic;;;;",
        "Battlemage Staff;4;staff15;2hStaff;Common;metal;2425;1;100;1;;;;;;;;19;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;4;-7;;8;-4;;-7;;;;;;;;8;16;;;;;;;;;;;;;;2;magic;;;;",
        "Skadian Staff;4;staff05;2hStaff;Common;metal;2425;1;90;1;;;;;;;;14;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;4;-7;;15;-4;;-7;;;;;4;2;;;16;;;;;;;;;;;;;;2;skadia;;;;",
        "Spellweaver Staff;5;staff06;2hStaff;Common;metal;5100;1;105;1;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;-11;;15;-5;5;-9;;;;;;;;;20;;;;;;;;;;;;;;2;magic;;;;",
        "Farseer Staff;5;staff07;2hStaff;Unique;metal;5100;1;105;1;;;;;;;;14;;;;;;;3;;3;;;-5;;;;;;;;;;;;;;;;;5;-9;;15;-5;;-12;;;1;;;;;10;20;;;;;;;;;;;;;;2;unique;;;;",
        "// WARLOCK;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Travelling Staff;1;staff08;2hStaff;Common;wood;75;1;65;1;;5;;;;;;14;;;;;;;;;;;;;;;;;5;;;;;;;10;;10;;;;;;-3;-3;;;;;;;;;;7;;;;;;;;;;;;;;;2;aldor;;;;",
        "Pilgrim Staff;2;staff09;2hStaff;Common;wood;400;1;90;1;;5;;;;;;16;;;;;;;;;;;;;;;;;7;;;;;;;8;;13;;4;;;;-3;-3;;;;;;;;;;10;5;;;;;;;;;;;;;;2;aldor;;;;",
        "Quarterstaff;2;staff10;2hStaff;Common;metal;400;1;100;1;;5;;;;;;16;;;;;;;;;;;;;;;;;8;3;;;;;;12;;10;;;;;;-3;-3;;;;;;;;;;10;;;;;;;;;;;;;;;2;aldor;;;;",
        "Reinforced Staff;3;staff11;2hStaff;Common;metal;1075;1;125;1;;5;;;;;;18;;;;;;;;;;;;;;;;;10;;;;;10;;14;;17;;;;;;-4;-4;;;;;;;;;;13;;;;;;;;;;;;;;;2;aldor;;;;",
        "Vehement Warstaff;3;staff12;2hStaff;Common;metal;1075;1;110;1;;5;;;;;;17;;;;;;;;;1;;;;;2;;;10;;;;;;;12;;17;;;;;;-4;-4;;;;;;;;;;9;;;;;;;;;;;;;;;2;magic;;;;",
        "Maen Warstaff;4;staff13;2hStaff;Common;metal;2425;1;130;1;;10;;;;;;21;;;;;;;;;;;;;4;;;;12;;;;;;;16;;21;;;;;;-4;-4;;;2;5;;;;;;16;10;;;;;;;;;;;;;;2;aldor;;;;",
        "Battlemage Warstaff;4;staff14;2hStaff;Common;metal;2425;1;145;1;;10;;;;;;20;;;;;;;1;;;;;;;;;5;12;;5;;;;;16;;21;;;;;;-6;-4;5;;;;;;;;;16;;;;;;;;;;;;;;;2;magic;;;;",
        "Geomancer Staff;4;staff16;2hStaff;Common;wood;2425;1;160;1;;10;;;;;;20;;;;;;;3;;;;;;;;;;12;8;;;;;;16;;21;;;;;;-4;-4;;;;;;;;;;16;;;;-4;;;5;;;;;;;;2;magic;;;;",
        "Cryomancer Staff;4;staff17;2hStaff;Common;metal;2425;1;120;1;;10;;;;;;16;;;;;;5;;;;;;;;;;;12;;;;;;;16;;33;;;;;;-4;-4;;;;;;;;;;16;;;;;;;;;;5;;;;;2;magic;;;;",
        "Arcanist Staff;4;staff18;2hStaff;Common;metal;2425;1;120;1;;10;;;;;;16;;;;;;;5;;;;;;;;;;12;;;;;;;16;;21;;6;;;;-4;-4;3;;;;;;;;;16;;;5;;;;;;;;5;;;;2;magic;;;;",
        "Knightly Warstaff;5;staff19;2hStaff;Common;metal;5100;1;185;1;;15;10;;;;;24;;;;;;;;;;;;;;;;;15;5;5;;;;;25;;25;;;;;;-5;-5;;;;;;;;;;25;;;;;;;;;;;;;;;2;aldor;;;;",
        "Gilded Warstaff;5;staff20;2hStaff;Common;metal;5600;1;125;1;;10;;;;;;24;;;;;;;;;;;;;;;;;15;;;;;;;20;;20;;9;;-4;;-5;-5;7;;;;;;;;;15;;;;;;;;;;;;;;;2;magic;;;;",
        "Templar Warstaff;5;staff21;2hStaff;Common;metal;5100;1;155;1;;10;;;;;;22;;;;;;;;;2;;;;;;;;10;;;;;;;15;;20;;5;;;;-5;-5;;;3;8;;;;;;20;;;5;;;;;;;;;;;;2;magic;;;;",
        "Axonian Warstaff;5;staff22;2hStaff;Unique;metal;5100;1;145;1;;10;;;;;;20;;;;;;;3;;3;;;;;4;;;15;;;;;;;20;;25;;;;;;-5;-5;;;4;;;;;;;20;;;;;;;;;;;;;;;1;unique;;;;",
        "Mage General Warstaff;5;staff23;2hStaff;Unique;metal;5100;1;155;1;;10;;;;;;20;;;;;;;4;;;;;;;;;;15;;;;;;;20;;25;;;;;;-5;-5;8;-4;;;;;;;;20;;4;4;;;;;;;;;;;;2;unique;;;;",
        "Stargazer Warstaff;5;staff24;2hStaff;Unique;metal;5100;1;140;1;;10;;;;;;24;;;;;;;;;;;;-3;;;;3;15;;;;;;;20;;25;;;;;;-5;-5;;-3;;;;;;;;33;;;;-3;;;;;;;;;;;2;unique;;;;",
        "Hermit Staff;5;staff25;2hStaff;Unique;wood;5100;1;175;1;;10;;;;;;20;;;;3;;;;;3;;;;;;;;15;;;;;;;20;;25;;8;4;;;-5;-5;;;;;;;8;4;5;20;;;;;;;;;;;;;;;2;unique;;;;",
        "Vampiric Staff;5;staff26;2hStaff;Unique;metal;5100;1;130;1;;13;;;;;;18;;;;;;;;9;;;;;;;9;;13;;;;13;;;13;;26;;;;;;-6;-6;;;;;;;;-9;;26;;13;;;;;;;;;;;;;2;special;;;;",
        "// MAGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Cammock;1;staff27;2hStaff;Common;wood;125;1;30;1;;;;;;;;11;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;10;-4;2;-1;;3;;;;;;;;;;;;;;;;;;;;;2;aldor;;;;",
        "Hexer Staff;2;staff28;2hStaff;Common;wood;650;1;40;1;;;;;;;;10;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;10;-5;4;;;5;;;;;;;;;;;;;;;;;;;;;2;magic;;;;",
        "Witch Staff;2;staff29;2hStaff;Unique;wood;650;1;30;1;;;;;;;;6;;;;;;;;3;;3;;;;;;;;;;;;;;;;;;;;;;10;-5;4;;3;5;;;;;;;;;;;;;;;;;;;;;2;special;;;;",
        "Conjurer Staff;3;staff30;2hStaff;Common;metal;1725;1;50;1;;;;;;;;12;;1;;;;1;1;;;;;;;;;;;;;;;;;;;;;;;;;15;-6;6;;;7;;;;;;;;;;;;;;;;;;;;;2;magic;;;;",
        "Necromancer Staff;3;staff31;2hStaff;Unique;wood;1725;1;33;1;;;;;;;;9;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;13;-13;13;-6;;13;;;;;;;-9;;9;-6;;;;;;;;;;;2;special;;;;",
        "Pyromancer Staff;4;staff32;2hStaff;Common;metal;3875;1;60;1;;;;;;;;12;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;15;-7;6;;;18;;;;;;;;;;;;5;;;;;;;;;2;magic;;;;",
        "Electromancer Staff;4;staff33;2hStaff;Common;wood;3875;1;60;1;;;;;;;;12;;;5;;;;;;;;;;;;;;;;;;;;;;;;;6;;;;15;-7;4;;;9;;;;;;;;;;;;;;;5;;;;;;2;magic;;;;",
        "Astromancer Staff;4;staff34;2hStaff;Common;metal;3875;1;60;1;;;;;;;;11;;2;;;;2;2;;;;;;;;;;;;;;;;;;;;;;;-5;;15;-7;4;;;9;;;;;;;;;;;;;;;;;;5;;;2;magic;;;;",
        "Psimancer Staff;4;staff35;2hStaff;Common;wood;3875;1;60;1;;;;;;;;12;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;15;-7;4;-3;;9;;;;;;;;;;;;;;;;;;;5;;2;magic;;;;",
        "Venomancer Staff;4;staff36;2hStaff;Common;metal;3875;1;60;1;;;;;;;;12;;;;3;2;;;;;;;;;;;;;;;;;;;;;;;;;;;15;-7;4;;;9;;;;;;;;;5;;;;;5;;;;;;;2;magic;;;;",
        "Court Wizard Staff;5;staff37;2hStaff;Common;wood;8150;1;55;1;;;;;;;;17;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;15;-8;10;;1;15;;;;;;;;;;;;;;;;;;;;;2;magic;;;;",
        "Orient Staff;5;staff38;2hStaff;Unique;wood;8150;1;75;1;;;;;;;;14;;3;;;;;;;3;;;;;;;;;;;;;;;;;;;5;5;-5;;15;-10;10;;;11;;;;;;;;;;;;;;;;;;;;;2;unique;;;;",
        "[ TOOLS];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Shackles;1;shackles;tool;Common;metal;50;1;100;1;;;;;;;;;;;;;;;;;;;;100;;-100;;-100;;;;;;;;;;;;;;100;;;;;100;;;;;;;;;;;;;;;;;;;;;;;1;special;;;;",
        "Chain;1;chain;chain;Common;metal;50;1;100;1;;;;;;;;10;;;;;;;;;;;;20;;;;;;;;;;10;10;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;;;;",
        "Lute;1;lute;lute;Common;wood;100;1;10;1;;;;;;;;8;;;;;;;;;;;;50;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;;;;",
        "Pickaxe;1;pick;pick;Common;metal;60;1;60;1;;10;10;10;;;15;;;;;;;;;;;;;15;;1;;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;;;;",
        "Sickle;1;sickle;tool;Common;metal;30;1;20;1;;;;;;10;;;;;;;;;;;;;;25;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;Foraging;;;",
        "Broom;1;broom;tool;Common;wood;10;1;10;1;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;special;;;;"
    };

    private static readonly List<string> ARMORS = new List<string>()
    {
        "name;Tier;id;Slot;class;rarity;Mat;Price;Markup;MaxDuration;DEF;;PRR;Block_Power;Block_Recovery;EVS;Crit_Avoid;FMB;Hit_Chance;Weapon_Damage;Armor_Piercing;Armor_Damage;CRT;CRTD;CTA;Damage_Received;Fortitude;;MP;MP_Restoration;Abilities_Energy_Cost;Skills_Energy_Cost;Spells_Energy_Cost;Magic_Power;Miscast_Chance;Miracle_Chance;Miracle_Power;Cooldown_Reduction;;VSN;max_hp;Health_Restoration;Healing_Received;Lifesteal;Manasteal;Bonus_Range;Received_XP;Damage_Returned;;Bleeding_Resistance;Knockback_Resistance;Stun_Resistance;Pain_Resistance;Fatigue_Gain;;Physical_Resistance;Nature_Resistance;Magic_Resistance;;Slashing_Resistance;Piercing_Resistance;Blunt_Resistance;Rending_Resistance;Fire_Resistance;Shock_Resistance;Poison_Resistance;Caustic_Resistance;Frost_Resistance;Arcane_Resistance;Unholy_Resistance;Sacred_Resistance;Psionic_Resistance;;Pyromantic_Power;Geomantic_Power;Venomantic_Power;Electromantic_Power;Cryomantic_Power;Arcanistic_Power;Astromantic_Power;Psimantic_Power;;tags;fireproof;IsOpen;NoDrop;fragment_cloth01;fragment_cloth02;fragment_cloth03;fragment_cloth04;fragment_leather01;fragment_leather02;fragment_leather03;fragment_leather04;fragment_metal01;fragment_metal02;fragment_metal03;fragment_metal04;fragment_gold;",
        "[ SHIELDS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// SKIRMISHER SHIELDS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Board Shield;1;shield01;shield;Light;Common;wood;25;0.5;60;;;10;7;10;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Oaken Shield;2;shield02;shield;Light;Common;wood;250;1.;80;;;12;9;10;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Round Shield;3;shield03;shield;Light;Common;wood;625;1.;100;;;14;11;10;;;;;;;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Dwarven Shield A;3;shield04;shield;Light;Common;wood;625;1.;100;;;12;10;10;;;;;;;;;;3;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Dwarven Shield B;3;shield04b;shield;Light;Common;wood;625;1.;100;;;12;10;10;;;;;;;;;;3;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Dwarven Shield C;3;shield04c;shield;Light;Common;wood;625;1.;100;;;12;10;10;;;;;;;;;;3;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Kalkan;4;shield05;shield;Light;Common;wood;1400;1.;120;;;16;13;10;;;;;;;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "Reinforced Kalkan;5;shield06;shield;Light;Common;wood;2975;1.;140;;;18;13;10;;;;;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "Orient Shield;5;shield07;shield;Light;Common;metal;3275;1.1;140;;;20;15;7;;;;;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "// BUCKLERS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Fist Shield;1;shield08;shield;Light;Common;wood;50;2.;45;;;5;2;15;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Leather Targe;2;shield09;shield;Light;Common;leather;125;1.;60;;;7;4;15;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Recruit Buckler;2;shield10;shield;Light;Common;wood;125;1.;60;;;6;6;12;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;4;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Buckler;3;shield11;shield;Light;Common;metal;325;1.;75;;;9;6;15;;;;;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Rondache;4;shield12;shield;Light;Common;metal;750;1.;90;;;11;8;15;;;;;;;;;;9;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Aristocrat Buckler;5;shield13;shield;Light;Common;metal;1575;1.;90;;;13;10;15;;;;;;;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Maaf Shield;5;shield14;shield;Light;Common;metal;1975;1.25;105;;;14;12;15;;;;;;;;;;9;;;;-2;;;;;;;;;;;;;;;;;;;;;;7;7;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "// MEDIUM SHIELDS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Heater Shield A;2;shield15;shield;Medium;Common;wood;325;1.;140;;;22;18;;-2;;;;;;;;;;;;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Heater Shield B;2;shield15b;shield;Medium;Common;wood;325;1.;140;;;22;18;;-2;;;;;;;;;;;;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Heater Shield C;2;shield15c;shield;Medium;Common;wood;325;1.;140;;;22;18;;-2;;;;;;;;;;;;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Heater Shield D;2;shield15d;shield;Medium;Common;wood;325;1.;140;;;22;18;;-2;;;;;;;;;;;;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Heater Shield E;2;shield15e;shield;Medium;Common;wood;325;1.;140;;;22;18;;-2;;;;;;;;;;;;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Heater Shield F;2;shield15f;shield;Medium;Common;wood;325;1.;140;;;22;18;;-2;;;;;;;;;;;;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Skadian Shield;3;shield16;shield;Medium;Common;wood;850;1.;175;;;26;22;;-4;;;;;;;;;;;;;-6;-2;;;;;;;;;;;;;;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;skadia;;;;;;;;;;;;;;;;;",
        "Kite Shield;4;shield17;shield;Medium;Common;wood;1875;1.;210;;;30;26;;-6;;;;;;;;;;;;;-8;-2;;;;;;;;;;;;;;;;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Huskarl Shield;4;shield18;shield;Medium;Common;wood;1875;1.;210;;;27;23;;-5;;;;;;;;;4;;;;-8;-2;;;;;;;;;;;;;;;;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Crow Heater Shield;4;shield19;shield;Medium;Unique;wood;2250;1.2;210;;;26;27;;-2;;;;;;;;;;;10;;-4;-2;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "Long Shield;5;shield20;shield;Medium;Common;wood;3950;1.;245;;;34;30;;-8;;;;;;;;;;;;;-10;-2;;;;;;;;;;;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Uroboros Shield;5;shield21;shield;Medium;Unique;wood;3950;1.;245;;;31;27;;-8;;;;3;;;;;;;;;-8;-2;;;;3;;;;;;;6;;10;;;;;10;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "Aldian Shield;5;shield22;shield;Medium;Unique;metal;3950;1.;185;;;30;30;;-10;;;;;;;;;;;;;-12;-3;;;;;;;;;;;;;;;;;;;;10;10;;;15;;5;10;10;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "// RIDER SHIELDS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Nomad Adarga;3;shield23;shield;Medium;Common;leather;975;1.15;165;;;16;18;5;-1;4;;;;;;;;;;;;-4;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "Rider Shield;3;shield24;shield;Medium;Common;wood;850;1.;150;;;20;16;5;-1;;;;;;;;;;;;;-4;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Squire Shield;4;shield25;shield;Medium;Common;wood;1875;1.;180;;;24;20;5;-2;;;;;;;;;;;;;-6;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Skirmisher Shield;4;shield26;shield;Medium;Common;wood;1875;1.;180;;;22;18;8;;;;;;;;;;;;;;-5;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Knightly Shield;5;shield27;shield;Medium;Common;wood;3950;1.;210;;;28;24;5;-3;;;;;;;;;;;;;-8;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Beast Slayer Shield;5;shield28;shield;Medium;Unique;wood;3950;1.;210;;;25;23;5;-3;;;;;;;;;;;;;-8;-1;;;;;;;;;;;;;;;;;;;;8;8;8;8;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "Joust Shield;5;shield29;shield;Medium;Unique;wood;3950;1.;210;;;31;31;5;-3;;;;;;;;;;-5;;;-8;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "// STORM SHIELDS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Footman Shield;3;shield30;shield;Heavy;Common;wood;1000;1.;250;;;32;32;-20;-12;4;;;;;;;;;;;;-12;-3;;;;;;;;;;;;;;;;;;;;;13;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Assault Shield;4;shield31;shield;Heavy;Common;wood;2250;1.;300;;;36;36;-20;-14;6;;;;;;;;;;;;-14;-3;;;;;;;;;;;;;;;;;;;;;16;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Sun Shield;5;shield32;shield;Heavy;Unique;wood;4750;1.;350;;;40;40;-20;-16;8;;;;;;;;;;;;-16;-3;-5;;-3;;;7;;-5;;;;;;;;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "Guardian Shield;5;shield33;shield;Heavy;Unique;wood;4750;1.;350;;;44;44;-13;-16;8;;;;;;;;5;;10;;-16;-3;;;;;;;;;;;;;;;;;;;;;20;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "// TOWER SHIELDS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Nistrian Shield;3;shield34;shield;Heavy;Common;wood;1175;1.;300;;;37;42;-25;-16;7;;;;;;;;;;;;-16;-4;;;;;;;;;;;;;;;;;;;;;20;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;nistra;;;;;;;;;;;;;;;;;",
        "Tower Shield A;4;shield35;shield;Heavy;Common;wood;2625;1.;360;;;38;40;-20;-20;9;;;;;;;;;;;;-18;-4;;;;;;;;;;;;;;;;;;;;;20;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Tower Shield B;4;shield36;shield;Heavy;Common;wood;2625;1.;360;;;41;46;-25;-18;12;;;;;;;;;;;;-18;-4;;;;;;;;;;;;;;;;;;;;;20;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Brynn Pavesa;5;shield37;shield;Heavy;Common;wood;5550;1.;420;;;45;50;-25;-20;13;;;;;;;;;;;;-20;-4;;;;;;;;;;;;;;;;;;;;;26;15;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Orient Tower Shield;5;shield38;shield;Heavy;Unique;wood;5550;1.;420;;;50;55;-31;-24;16;;;;;;;;;;;;-20;-5;;;;;;;;;;;;;;;;;;;;;31;18;;-10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "[ HELMETS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// LIGHT OFFENSIVE ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Broad Brim Hat;1;head01;Head;Light;Common;cloth;25;1.;30;1;;;;;2;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;;2;;5;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;;;;;;;;;;",
        "Peasant Hat;1;head02;Head;Light;Common;cloth;25;1.;30;1;;;;;3;;;;;;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;2;;2;;;;3;;;;;;3;3;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;;;;;;;;;;",
        "Hood;2;head03;Head;Light;Common;cloth;200;1.;40;2;;;;;4;;-1;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;4;;4;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;;;;;;;;;;",
        "Hired Blade Cowl;2;head04;Head;Light;Unique;cloth;225;1.15;50;2;;;;;3;;-2;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;4;;4;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;2;;;;;;;;;;;;;",
        "Forester Hat;3;head05;Head;Light;Common;cloth;500;1.;50;3;;;;;6;;;4;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;6;;6;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;;;;;;;;;;",
        "Mercenary Hat;4;head06;Head;Light;Common;cloth;1150;1.;60;4;;;;;8;;-1;;;;;4;7;;;;;;;;;;;;;;;;;;;;;;;;;;8;;8;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;1;;;;;;;;;",
        "// LIGHT DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Cap;1;head08;Head;Light;Common;cloth;25;1.;60;2;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;8;;5;;;;6;;;;3;;;3;;;;;;;;;;;;;;;;;;;;aldor;;1;;2;;;;;;;;;;;;;",
        "Arming Cap;2;head09;Head;Light;Common;cloth;200;1.;80;3;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;10;;7;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;2;;;;;;;;;;;;",
        "Plague Doctor Mask;2;head10;Head;Light;Unique;leather;200;1.;80;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;-2;;5;15;;;;;;;10;;7;;;;5;;;;;;;;;;65;;;;;;;;;;;;;;;;;special;;1;;;;;;2;;;;;;;;;",
        "Padded Coif;3;head11;Head;Light;Common;cloth;500;1.;100;5;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;12;;10;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;2;;;;;;;;;;;;",
        "Fjall Leather Cap;3;head11;Head;Light;Common;cloth;500;1.;100;4;;;;;5;10;-1;;;;;;;;;8;;;;-3;;;;;;;;;;;;;;;;;;;12;;10;;;;8;8;8;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;1;;;1;;;2;;;;;;;;;",
        "Closed Coif;4;head12;Head;Light;Common;cloth;1150;1.;120;7;;;;;5;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;14;;12;;;;12;;;;;;8;;;;;;;;;;;;;;;;;;;;;aldor;;;;;3;;;;;;;;;;;;",
        "Norrheim Helmet;4;head13;Head;Light;Common;leather;1325;1.15;100;6;;;;;3;7;;;;;;1;;2;;;;;;;;;;;;;;;;;;;;;;;;;18;;14;;;;12;6;;;;;;15;;;;;;;;;;;;;;;;;;;;fjall;;1;;;;;;3;;;;;;;;;",
        "Berserk Bear Cowl;5;head07;Head;Light;Common;leather;2675;1.1;90;6;;;;;4;;;;3;;;3;;;;;;;;;;;;;;;-3;;;;;;;;;;;;22;;10;10;;;10;10;;;;;;20;;;;;;;;;;;;;;;;;;;;fjall;;1;;2;;;;2;;;;;;;;;",
        "Knightly Coif;5;head14;Head;Light;Common;metal;2300;0.95;150;9;;;;;6;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;16;;15;;;;14;;;;6;;;6;;;;;;;;;;;;;;;;;;;;aldor;;;;;2;;;;;;;;1;;;;",
        "// COIFS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Mail Coif;3;head15;Head;Light;Common;metal;1025;2.;160;7;;;;;;;;;;;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;12;;10;;;;15;;;;12;;;12;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;;2;;;;",
        "Squire Mail Coif;4;head16;Head;Light;Common;metal;2300;2.;190;9;;;;;;;;;;;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;14;;12;;;;18;;;;12;;;12;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;;3;;;;",
        ";5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// MEDIUM LIGHT;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Cervellier;2;head17;Head;Medium;Common;metal;375;1.;100;7;;;;;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;12;;12;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;2;;;;;",
        "Cone Helmet;2;head18;Head;Medium;Common;metal;275;0.75;100;5;;;;;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;10;;10;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;2;;;;;",
        "Skadian Helmet;2;head19;Head;Medium;Unique;metal;375;1.;100;7;;;;;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;12;;10;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;1;;;;;;;;;;2;;;;;",
        "Flat Kettle Helmet;2;head20;Head;Medium;Common;metal;425;1.15;100;8;;;;;;3;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;12;;10;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;2;;;;;",
        "Mail Cervellier;3;head21;Head;Medium;Common;metal;1175;1.15;150;11;;;;;;;;;;;;;;;;;;-3;-1;;;;;;;;;;;;;;;;;;;;15;;15;;;;14;;;;15;;;15;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;2;1;;;;",
        "War Hat;3;head22;Head;Medium;Common;metal;1025;1.;125;10;;;;;;5;;;;;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;16;;15;;;;14;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;3;;;;;",
        "Open Sentinet;3;head23;Head;Medium;Common;metal;925;0.9;115;10;;;;;;;;;;;;;;;;3;;-2;;;;;;;;;;;;;;;;;;;;;14;;17;;;;14;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;3;;;;;",
        "Eared Cervellier;3;head24;Head;Medium;Unique;metal;1225;1.2;115;9;;;;;;7;;;;;;;;;;10;;-2;;;;;;;;;;;;;;;;;;;;;15;;24;;;;14;;;;5;;;5;;;;;;;;;;;;;;;;;;;;special;;1;;;;;;;;;;2;;1;;;",
        "Sallet;4;head25;Head;Medium;Common;metal;2300;1.;150;13;;;;;;;;;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;18;;20;;;;17;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;4;;;;;",
        "Barbute;5;head26;Head;Medium;Common;metal;4850;1.;175;16;;;;;;;;;;;;;;;;;;-4;;;;;;;;;;;;;;;;;;;;;21;;25;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;5;;;;;",
        "// MEDIUM HEAVY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Pot Helm;2;head27;Head;Medium;Common;metal;400;0.85;100;8;;;;;;;;;;;;;;;;;;-6;-1;;;;;;;;;;;;;;;;;;;;15;;15;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;2;;;;;",
        "Half Bascinet;2;head28;Head;Medium;Common;metal;525;1.15;120;9;;;;;;;;;;;;;;;;;;-3;-1;;;;;;;;;;;;;;;;;;;;17;;15;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;1;;;;2;;;;;",
        "Reinforced Sentinet;3;head29;Head;Medium;Common;metal;1100;0.9;135;11;;;;;;;;;;;;;;;;5;5;-3;-1;;;;;;;;;;;;;;;;;;;;20;;23;;;;18;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;3;;;;;",
        "Open Bascinet;3;head30;Head;Medium;Common;metal;1225;1.;150;12;;;;;;;;;;;;;;;;3;;-5;-1;;;;;;;;;;;;;;;;;;;;20;;20;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;1;;;;2;;;;;",
        "Fjall Helmet;3;;Head;Medium;Common;metal;1100;0.9;115;10;;2;;;;;;;;;;1;;;;3;;-5;-1;;;;;;;;;;;;;;;;;;;;16;;16;10;;;16;8;8;;;;;5;;;;;;;;;;;;;;;;;;;;fjall;;1;;;;;;2;;;;1;1;;;;",
        "Bascinet With Grate;4;head31;Head;Medium;Common;metal;2750;1.;180;15;;;;;;;;;;;;;;;;5;;-7;-1;;;;;;;;;;;;;;;;;;;;23;;25;;;;23;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;4;;;;;",
        "Captain Barbute;5;head32;Head;Medium;Common;metal;5825;1.;210;18;;;;;;;;;;;;;;;;7;;-9;-1;;;;;;;;;;-1;;;;;;;;;;26;;30;;;;24;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;;;;;;;;;5;;;;;",
        "Decorated Barbute;5;head33;Head;Medium;Unique;metal;5825;1.;210;18;;;;;;;;4;;;;;;;;9;;-9;-1;;;;5;;;;;;-1;;;;;;;;;;26;;30;;;;24;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;1;;;;;;;;;;3;;;;2;",
        "// HEAVY LIGHT;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Visoreal Cervellier;3;head34;Head;Heavy;Common;metal;1425;1.;200;14;;;;;;6;;-2;;;;;;;;8;;-12;-3;;;;;;;;;;-1;;;;;;;;;;26;;25;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;3;;;;;",
        "Below Face Sallet;4;head35;Head;Heavy;Common;metal;3225;1.;205;18;;;;;;6;;-2;;;;;;;;6;;-11;-3;;;;;;;;;;-1;;;;;;;;;;26;;26;;;;21;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;4;;;;;",
        "Sallet With Visor;4;head36;Head;Heavy;Common;metal;3225;1.;240;18;;;;;;9;;-3;;;;;;;;10;;-14;-3;;;;;;;;;;-1;;;;;;;;;;30;;30;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;4;;;;;",
        "Visoreal Barbute;4;head37;Head;Heavy;Common;metal;3225;1.;265;19;;;;;;11;;-4;;;;;;;;8;;-18;-4;;;;;;;;;;-1;;;;;;;;;;35;;34;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;4;;;;;",
        "Decorated Sallet;4;head38;Head;Heavy;Unique;metal;3225;1.;240;18;;;;;;18;;-3;;;;;;;;20;;-14;-3;;;;;;;;;;-1;;;;;;;;;;30;;30;;;;25;;5;;;;;;;;;;;;10;;;;;;;;;;;;;unique;;;;;;;;;;;;4;;;;;",
        "Visoreal Sentinet;5;head39;Head;Heavy;Common;metal;6800;1.;265;22;;;;;;10;;-6;;;;;;;;14;;-16;-5;;;;;;;;;;-2;;;;;;;;;;42;;40;;;;32;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;5;;;;;",
        "Gryphon Sallet;5;head40;Head;Heavy;Common;metal;6800;1.;280;22;;;;;;12;;-6;;;;;;;;12;;-16;-5;;;;;;;;;;-2;;;;;;;;;;40;;47;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;5;;;;;",
        "Luxurious Sallet;5;head41;Head;Heavy;Unique;metal;6800;1.;310;22;;;;;5;12;;-6;;;;;;;;12;;-16;-5;;;;5;;;;;;-1;;;;;;;5;;;34;;35;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;3;;;;2;",
        "Ceremonial Sentinet;5;head42;Head;Heavy;Unique;metal;6800;1.;280;22;;;;;;10;;-4;;;;;;;;18;;-13;-4;;;;;;;;-5;;-2;;;;;;;;;;40;;40;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;3;;;;3;",
        "Vagabond Knight Sentinet;5;head43;Head;Heavy;Unique;metal;6800;1.;225;20;;;;;;18;;-6;3;;;;;;;12;;-16;-5;;;-6;3;;3;;;;-1;;;;;6;;;;;40;;40;;;;30;10;10;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;5;;;;;",
        "// HEAVY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Joust Bascinet;3;head44;Head;Heavy;Unique;metal;1625;1.;270;17;;4;;;;9;;-4;;;;;;;-5;12;;-16;-5;;;;;;;;;;-2;;;;;;;;;;32;;30;;;;26;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;3;;;;;",
        "Klappvisor Bascinet;3;head45;Head;Heavy;Common;metal;1625;1.;300;17;;;;;;9;;-4;;;;;;;;12;;-16;-5;;;;;;;;;;-2;;;;;;;;;;32;;30;;;;26;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;3;;;;;",
        "Hounskull Bascinet;4;head46;Head;Heavy;Common;metal;3675;1.;360;21;;;;;;12;;-5;;;;;;;;15;;-18;-5;;;;;;;;;;-2;;;;;;;;;;36;;35;;;;31;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;5;;;;;",
        "Pigfaced Bascinet;5;head47;Head;Heavy;Common;metal;7775;1.;420;25;;;;;;15;;-6;;;;;;;;18;;-20;-5;;;;;;;;;;-2;;;;;;;;;;40;;40;;;;36;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;5;;;;;",
        "Grand Bascinet;5;head48;Head;Heavy;Common;metal;7775;1.;460;28;;;;;;17;;-8;;;;;;;;18;;-25;-6;;;;;;;;;;-3;;;;;;;;;;42;;42;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;5;;;;;",
        "// BUDGET;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Visoreal Pot Helm;3;head49;Head;Heavy;Common;metal;1325;1.;250;15;;;;;;5;;-3;;;;;;;;5;;-14;-4;;;;;;;;;;-2;;;;;;;;;;28;;27;;;;23;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;4;;;;;",
        "Topfhelm;4;head50;Head;Heavy;Common;metal;3000;1.;300;19;;;;;;7;;-4;;;;;;;;7;;-16;-4;;;;;;;;;;-2;;;;;;;;;;32;;30;;;;27;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;4;;;;;",
        "Joust Topfhelm;4;head51;Head;Heavy;Unique;metal;3000;1.;270;19;;4;;;;7;;-4;;;;;;;-5;7;;-16;-4;;;;;;;;;;-2;;;;;;;;;;32;;30;;;;27;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;4;;;;1;",
        "Radiant Topfhelm;5;head52;Head;Heavy;Unique;metal;6325;1.;350;23;;;;;;14;;-3;;;;;;;;8;;-14;-4;;;;;;3;;;;-2;;;;;;;;;;36;;38;;;;30;;;;;;;;;;;;;;33;;;;;;;;;;;;;unique;;;;;;;;;;;;4;;;;1;",
        "// COWLS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Apprentice Cowl;1;head53;Head;Light;Common;cloth;75;1.;20;1;;;;;;;;;;;;;;;;;;;1;;;;;-1;;;;;;;;;;;;;;;3;;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;;;;;;;;;;",
        "Apprentice Mage Cowl;2;head54;Head;Light;Common;cloth;475;1.;40;2;;;;;;;;;;;;;;;;;;3;2;;;;4;-2;;;;;;;;;;;;;;;4;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;2;;;;;;;;;;;;;",
        "Witch Hat;2;head55;Head;Light;Unique;cloth;575;1.2;20;2;;;;;;;;;;;;;;;;;;;;;;;6;;3;6;;;;;;;;;;;;;4;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;1;;2;;;;;;;;;;;;;",
        "Wanderer Cowl;3;head56;Head;Light;Common;cloth;1275;1.;50;3;;;;;;;;;;;;;;;;;;5;2;;;;6;-3;;;;;1;;;;;;;;;;5;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;2;;;;;;;;;;;;;",
        "Battlemage Cowl;4;head57;Head;Light;Common;cloth;2450;0.85;70;5;;;;;;;;;;;;;;;;;;7;3;;;;9;-4;;;;;1;;;;;;;;;;9;;;;;;6;9;9;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;3;;;;;;;;;;;;",
        "Arcanist Cowl;4;head58;Head;Light;Common;cloth;2875;1.;60;4;;;;;4;;;;;;;;;;;;;6;2;;;;;-4;;;;;1;;;;;9;;;;;6;;;;;;4;;;;;;;;;;;;;20;;;;;;;;;;6;;;;magic;;1;;;3;;;;;;;;;;;;",
        "Pyromancer Cowl;4;head59;Head;Light;Common;cloth;2875;1.;60;4;;;;;;;;;;;;;;;;;;6;2;;;;;-4;2;5;;;1;;;;;;;;;;6;;;;;;4;;;;;;;;20;;;;;;;;;;6;;;;;;;;;magic;;1;;;3;;;;;;;;;;;;",
        "Geomancer Cowl;4;head60;Head;Light;Common;cloth;2875;1.;80;5;;;;;;;;;;;;;;;-5;8;;6;2;;;;;-4;;;;;1;;;;;;;;;;6;;;;;;5;;;;;;;;;;;;;10;;;;;;6;;;;;;;;magic;;1;;;3;;;;;;;;;;;;",
        "Electromancer Cowl;4;head61;Head;Light;Common;cloth;2875;1.;60;4;;;;;;;;;;;;;;;;;;8;3;;;-5;;-4;;;;;1;;;;;;;;;;6;;;;;;4;;;;;;;;;20;;;;;;;;;;;;6;;;;;;magic;;1;;;3;;;;;;;;;;;;",
        "Cryomancer Cowl;4;head62;Head;Light;Common;cloth;2875;1.;60;4;;;;;;10;;;;;;;;;;;;6;2;;;;;-5;;;;;1;5;;;;;;;;;6;;;;;;4;;;;;;;;;;;;25;;;;;;;;;;6;;;;;magic;;1;;;3;;;;;;;;;;;;",
        "Spellweaver Cowl;5;head63;Head;Light;Common;cloth;6075;1.;70;5;;;;;;;;;;;;;;;;;;9;3;;;-6;10;-5;;;-5;;1;;;;;;;;;;7;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;3;;;;;;;;;;;;",
        "// CIRCLETS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Crimson Headband;1;head64;Head;Light;Common;cloth;20;;15;;;;;;2;;;;;;;2;;;;;;;;;;;;;2;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;1;;1;;;;;;;;;;;;;",
        "Academy Circlet;2;head65;Head;Light;Common;gem;575;1.;20;;;;;;;;;;;;;;;;;;;;;;;;8;;1;3;-5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;;;;1;;;;;;;;1;",
        "Topaz Circlet;3;head66;Head;Light;Common;gem;1375;0.9;35;;;;;;;;;;;;;;;;;;;;;;;;9;;2;5;-6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;;;;1;;;;;;;;;",
        "Golden Circlet;3;head67;Head;Light;Common;gold;1825;1.2;20;;;;;;;;;;;;;;;;;;;;;;;;11;-1;2;5;-6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;;;;1;;;;;;;;2;",
        "Dwarven Circlet;4;head68;Head;Light;Common;wood;4150;1.2;60;;;;;;;5;;;;;;;;;;;;;;;;;11;;5;9;-7;;1;;;;;;;;;;;;;;;;7;7;7;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;1;;;;;;1;;;;;;;;;",
        "Battle Mage Circlet;4;head69;Head;Light;Common;gem;3450;1.;30;;;;;;;;;;;;;;;;;;;6;3;;;;12;;3;7;-7;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;;;;1;;;;;;;;;",
        "Ruby Circlet;5;head70;Head;Light;Common;gem;7300;1.;45;;;;;;;;;;;;;;;;;;;;;;;-5;13;;3;9;-12;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;;;;1;;;;;;;;;",
        "Court Wizard Circlet;5;head71;Head;Light;Common;gold;8375;1.15;20;;;;;;;;;;;;;;;;;;;;;;;;15;;4;11;-8;;1;;;;;;;;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;magic;;1;;;;;;;;;;;;;;3;",
        "Hermit Circlet;5;head72;Head;Light;Unique;wood;7300;1.;35;;;;;;;;;;;;;;;;;;;4;2;;;;11;;4;7;-8;;;4;2;;;;1;;;;;;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;1;;;;;;1;;;;;;;;;",
        "// CHESTPIECES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// LIGHT - UTILITY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Tattered Rags;1;chest01;Chest;Light;Common;cloth;25;1.;15;1;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;1;;;;;;;;;;;;;",
        "Old Shirt;1;chest02;Chest;Light;Common;cloth;10;;10;1;;;;;2;;;;;;;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;0;;;;3;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Linen Shirt;1;chest03;Chest;Light;Common;cloth;25;1.;20;1;;;;;3;;;;;;;;;;;;;;;;-5;;;;;;;;;;;;;;;;;;0;;;;5;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Fine Shirt;2;chest04;Chest;Light;Common;cloth;125;1.;20;1;;;;;6;;;;;;;;;;;;;;;;-6;;;;;;;;;;;;;;;;;;2;;;;7;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Dirwin Cloak;2;chest05;Chest;Light;Unique;cloth;100;0.8;20;1;;;;;3;;;3;;;;;;;;;;;;;-6;;;;;;;;;;;;;;;;;;2;;;;9;;2;5;;;;;;5;;;;;;;;;;;;;;;;;;;;special exc;;;;;1;;;1;;;;;;;;;",
        "Velmir Caftan;2;chest06;Chest;Light;Unique;cloth;100;0.8;20;1;;;;;5;;;;;;;;;2;;5;;;;;-4;;;;;;;;;;;;;;;;;;5;;;;4;4;5;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;1;1;;;;;;;;;;;;",
        "Merchant Garment;3;chest07;Chest;Light;Common;cloth;325;1.;25;2;;;;;9;;;;;;;;;;;;;;;;-7;;;;;;;;;;;;;;;;;;4;;;;9;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;2;;;;;;;;;;;;;",
        "Elven Garment;4;chest08;Chest;Light;Common;cloth;750;1.;30;2;;;;;12;;;;;;;;;6;;;;;;;-8;;;;;;;;;;;;;;;;;;6;;;;12;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;1;1;;;;;;;;;;;;",
        ";5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// LIGHT - DOUBLETS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Leather Vest;1;chest09;Chest;Light;Common;leather;50;1.;30;1;;;;;2;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;2;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;;;",
        "Fur Vest;1;chest10;Chest;Light;Common;leather;50;1.;20;1;;;;;4;;;;;;;1;5;2;;;;;;;;;;;;;;;;;;;;;;;;;2;;;;;;8;;;;;;;;;;;;20;;;;;;;;;;;;;;;fjall;;;;1;;;;1;;;;;;;;;",
        "Leo Doublet;2;chest11;Chest;Light;Unique;cloth;375;1.5;40;2;;;;;2;;;;3;;;;;2;;;;;;;;;3;;;;;;;;;;;;;;;;2;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;2;;;1;;;;;;;;;",
        "Padded Leather Vest;2;chest12;Chest;Light;Common;leather;250;1.;40;2;;;;;4;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;4;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;2;;;;;;;;;",
        "Padded Doublet;3;chest13;Chest;Light;Common;cloth;650;1.;50;3;;;;;6;;-2;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;6;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;1;;;;;;;;;;;;",
        "Duelist Doublet;4;chest14;Chest;Light;Common;cloth;1475;1.;60;4;;;;;8;;;;;;;2;;8;;;;;;;;;;;;;;;;;;;;;;;;;8;;;5;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;1;;;;;;;;;;;",
        "Pourpoint;4;chest15;Chest;Light;Common;cloth;1475;1.;75;6;;;;;6;5;;;;;;;;5;-5;;;;;;;;;;;;;;;;;;;;;;;;16;;;;;;9;;;;;;5;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;1;;;;;;;;;;;",
        "Skaar Garment;4;chest16;Chest;Light;Common;cloth;1625;1.1;40;3;;;;;3;;-3;;;;;4;20;4;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;4;;;;;;;;;;;;15;;;;;;;;;;;;;;;fjall;;;;;;1;;1;;;;;;;;;",
        "Blademaster Doublet;5;chest17;Chest;Light;Common;cloth;3125;1.;90;7;;2;;;5;;;2;;;;2;;6;;;;;;;;;;;;;;;;;;;;;;;;;13;;;;;;11;;;;5;5;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;2;;;;;;;;;;;",
        "Fencer Doublet;5;chest18;Chest;Light;Common;cloth;3125;1.;70;5;;;;;10;;;;6;;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;8;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;1;1;;;;;;;;;;;",
        "Aristocrat Doublet;5;chest19;Chest;Light;Common;cloth;3125;1.;80;6;;;;;8;8;;;;;;;;10;;5;;;;;-5;;;;;;-5;;;;;;;;;;;;14;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;2;;1;;;;;;;;;;;",
        "// LIGHT - DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Gambeson;2;chest20;Chest;Light;Common;cloth;250;1.;55;3;;4;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;10;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;2;;;;;;;;;;;;",
        "Gambeson Magistrate;2;chest21;Chest;Light;Common;cloth;250;1.;55;3;;4;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;10;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;2;;;;;;;;;;;;",
        "Long Gambeson;3;chest22;Chest;Light;Common;cloth;650;1.;80;5;;6;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;14;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;3;;;;;;;;;;;",
        "Vehement Gambeson;3;chest23;Chest;Light;Common;cloth;750;1.15;55;4;;2;;;4;;;;;;;;;;;;;;;-4;;;3;;;;-3;;;;;;;;;;;;8;;;;;;10;10;10;;;;;;;;;;;;10;;;;;;;;;;;;;magic;;;;;2;1;;;;;;;;;;;",
        "Sentian Gambeson;3;chest24;Chest;Light;Common;cloth;650;1.;70;5;;3;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;11;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;3;;;;;;;;;;;",
        "Lamellar Quilted Coat;4;chest25;Chest;Light;Common;leather;1475;1.;85;7;;6;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;14;;;;;;12;;;;5;10;10;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;1;3;;;;;;;",
        "Hejmdar Garment;4;chest26;Chest;Light;Common;leather;1700;1.15;55;6;;4;;;7;;;;;;;;;;-5;;;;;;;;;;;;;;;;;;;;;;;;12;;;;;;7;10;5;;;;;;;;;;20;;;;;;;;;;;;;;;fjall;;;;;;;;2;2;;;;;;;;",
        "Battlemage Gambeson;4;chest27;Chest;Light;Common;cloth;1700;1.15;65;6;;2;;;5;;;;;;;;;;;;;;;-5;;;6;-2;;;;;;;;;;;;;;;14;;;;;;10;15;15;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;;2;2;;;;;;;;;;",
        "Mirror Quilted Coat;5;chest28;Chest;Light;Common;cloth;3575;1.15;115;10;;7;;;3;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;20;;;;;;18;;;;;;5;10;;;;;;;;;;;;;;;;;;;;aldor;;;;;;2;;;2;;;1;;;;;",
        "Magehunter Gambeson;5;chest29;Chest;Light;Common;cloth;3575;1.15;85;8;;6;;;6;;;;;;;;;;;;;;;-6;;;4;;;;;;;;;;;;;;;;14;;;;;;12;20;20;;;;;;;;;;;;15;;;;;;;;;;;;;aldor;;;;;1;;2;;2;;;;;;;;",
        "Captain Gambeson;5;chest30;Chest;Light;Common;cloth;3125;1.;100;9;;8;;;5;3;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;16;;;;;;14;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;2;2;1;;;;;;;;;",
        "Hermit Garment;5;chest31;Chest;Light;Unique;cloth;3900;1.25;100;7;;4;;;6;;;;;;;;;;;;;6;3;;;;12;;;;;;;6;3;;;;;;;;16;;;;;;14;25;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;1;2;;1;;;;;;;;;",
        "Royal Ranger Gambeson;5;chest32;Chest;Light;Unique;cloth;3900;1.25;145;9;;;;;8;;-5;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;24;;;;;;20;20;;;;;;20;;;;;;;;;;;;;;;;;;;;unique;;;;1;1;;3;;;;;;;;;;",
        "// MEDIUM - CUIRASSES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Arna Cuirass;2;chest33;Chest;Medium;Unique;metal;575;1.;76;7;;;;;-3;4;;;;;;;;;;;;-3;-1;;;;;;;;;;;;;;;;;4;;;7;;;;5;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;1;;;;;;;;2;;;1;;",
        ";2;;;;;;0;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Soldier Cuirass;3;chest34;Chest;Medium;Common;metal;1500;1.;120;12;;;;;-4;6;;;;;;;;;;;;-5;-1;;;;;;;;;;;;;;;;;;;;15;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;1;;;;;1;;;1;;",
        "Mercenary Cuirass;4;chest35;Chest;Medium;Common;metal;3400;1.;145;15;;;;;-5;8;;;;;;;;;;;;-7;-1;;;;;;;;;;;;;;;;;;;;18;;;;;;18;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;1;;;;;1;;;2;;",
        "Captain Cuirass;5;chest36;Chest;Medium;Common;metal;7175;1.;170;18;;;;;-6;10;;;;;;;;;;;;-9;-1;;;;;;;;;;;;;;;;;;;;21;;;;;;22;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;1;;;;;;;1;1;;2;;",
        "// MEDIUM - MAILS & BRIGANDINES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Recruit Mail;2;chest38;Chest;Medium;Common;metal;475;0.95;105;6;;;;;;;;;;;;;;;;3;;-2;;;;;;;;;;;;;;;;;;;;;17;;;;;;10;;;;12;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;1;;;;2;;;;",
        "Road Guard Brigandine;2;chest39;Chest;Medium;Common;metal;575;1.15;125;7;;;;;-1;;;;;;;;;;;3;;-2;;;;;;;;;;;;;;;;;;;;;20;;;;5;;12;;;;;6;3;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;1;;;;2;;;;",
        "Vehement Mail;3;chest37;Chest;Medium;Common;metal;1175;0.9;160;8;;;;;-1;;;;;;;;;;;8;;-4;;;;;;;;;;;;;;;;;;;;;25;;;;;;10;5;15;;8;;;;;;;;;;10;;;;;;;;;;;;;aldor;;;;;;;;;1;;;;2;;;;",
        "Soldier Hauberk;3;chest40;Chest;Medium;Common;metal;1300;1.;180;10;;;;;-2;;;;;;;;;;;5;;-4;;;;;;;;;;;;;;;;;;;;;20;;;;;;10;;;;15;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;3;;;;",
        "Hauberk Magistrate;3;chest41;Chest;Medium;Common;metal;1300;1.;180;10;;;;;-2;;;;;;;;;;;5;;-4;;;;;;;;;;;;;;;;;;;;;20;;;;;;10;;;;15;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;3;;;;",
        "Footman Brigandine;3;chest42;Chest;Medium;Common;metal;1300;1.;140;10;;;;;-2;;;;;;;;;;;5;;-4;;;;;;;;;;;;;;;;;;;;;20;;;;;;13;;;;;7;4;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;1;;;;;;2;;;;",
        "Mail Mantle;4;chest45;Chest;Medium;Common;metal;3100;1.05;125;13;;;;;-3;;;;;;;;;;;7;;-3;;;;-5;5;;;;;;;;;;;;;;;;16;;;;-5;;9;15;15;;11;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;;;;;2;;;;",
        "Skadian Mail;4;chest44;Chest;Medium;Common;metal;3400;1.15;200;13;;;;;-3;;;;;;;;;;;7;;-6;;;;;;;;;;;;;;;;;;;;;25;;;;;;14;;;;21;6;;;;;;;;;;;;;;;;;;;;;;skadia;;;;;;;;;;;;;3;1;;;",
        "Veteran Brigandine;4;chest43;Chest;Medium;Common;metal;2950;1.;185;13;;;;;-3;;;;;;;;;;;7;;-7;;;;;;;;;;;;;;;;;;;;;23;;;;;;17;;;;;8;5;;;;;;;;;;;;;;;;;;;;;aldor;;;;;1;;;;;;;;2;;;;",
        "Duelist Brigandine;4;chest46;Chest;Medium;Common;metal;2950;1.;125;11;;2;;;-3;;;;;;;;;2;;7;;-6;;;;;;;;;;;;;;;;;;;;;32;;;;;;10;;;;;20;10;;;;;;;;;;;;;;;;;;;;;aldor;;;;;1;;;;1;;;;;1;;;",
        "Captain Brigandine;5;chest47;Chest;Medium;Common;metal;6225;1.;195;16;;;;;-4;;;;;;;;;;;10;;-8;;;;;;;;;;;;;;;;;;;;;26;;;;;;20;;;;;10;5;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;1;;1;;;;1;1;;;",
        "// HEAVY - BRIGANDINES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Vehement Brigandine;3;chest48;Chest;Heavy;Common;metal;2100;1.15;160;13;;;;;-8;10;;;;;;;;;;8;;-10;-3;-5;;;;;;;;;;;;;;;;;;;26;;;;;;21;10;20;;;4;4;;;;;;;;10;;;;;;;;;;;;;magic;;;;;;;;;;;2;;1;;;;",
        "Sergeant Brigandine;4;chest49;Chest;Heavy;Common;metal;4325;1.05;265;18;;;;;-10;15;;;;;;;;;;12;;-12;-3;;;;;;;;;;;;;;;;;;;;30;;;;;;23;;;;;7;12;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;1;;2;1;;;",
        "Mercenary Brigandine;4;chest50;Chest;Heavy;Common;metal;3725;0.9;240;17;;;;;-9;13;;;;;;;;;;12;;-10;-2;;;;;;;;;;;;;;;;;;;;33;;;;;;20;;;;;5;10;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;2;;;2;1;;",
        "Knightly Brigandine;5;chest51;Chest;Heavy;Common;metal;8725;1.;280;22;;;;;-12;15;;;;;;;;;;16;;-14;-3;;;;;;;;;;;;;;;;;;;;34;;;;;;26;;;;;7;12;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;3;;2;;",
        "Elven Brigandine;5;chest52;Chest;Heavy;Common;metal;9600;1.1;310;23;;;;;-13;15;;;;;;;;;;12;;-14;-4;;;;;;;;;;;;;;;;;;;;36;5;;;;;28;;;;;10;10;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;4;;1;;",
        "// HEAVY - PLATE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Old Fashioned Armor;3;chest54;Chest;Heavy;Common;metal;2100;1.;300;17;;;;;-14;15;;;;;;;;;;;;-16;-5;;;;;;;;;;;;;;;;;;;;32;;;;;;30;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;1;;2;;",
        "Flagbearer Armor;4;chest55;Chest;Heavy;Common;metal;4725;1.;360;21;;;;;-17;18;;;;;;;;;;;;-18;-5;;;;;;;;;;;;;;;;;;;;36;;;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;1;;3;;",
        "Scale Armor;4;chest56;Chest;Heavy;Common;metal;3775;0.8;400;19;;;;;-14;14;;;;;;;;;;;;-14;-5;;;;;;;;;;;;;;;;;;;;32;;;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;2;;;;;;;;;;4;;",
        "Dwarven Armor;4;chest53;Chest;Heavy;Common;metal;3775;0.8;250;15;;;;;-12;10;-2;;;;;;;;-3;;;-13;-4;;;;;;;;;;;;;;;;;;;;25;;;7;;;25;10;;;;5;;15;;;;;;;;;;;;;;;;;;;;fjall;;;;1;;;;;;;;;3;;;;",
        "Joust Armor;4;chest57;Chest;Heavy;Unique;metal;4725;1.;360;21;;5;5;;-17;18;;;;;;;;;-5;;;-18;-5;;;;;;;;;;;;;;;;;;;;36;;;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;1;3;1;",
        "Noble Armor;5;chest58;Chest;Heavy;Common;metal;10475;1.05;440;25;;;;;-20;20;;;;;;;;;;;;-14;-5;;;;;;;;;;;;;;;;;;;;36;;;;;;36;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;2;3;;",
        "Knightly Armor;5;chest59;Chest;Heavy;Common;metal;9975;1.;420;25;;3;;5;-20;20;;;;;;;;;;;;-20;-5;;;;;;;;;;;;;;;;;;;;40;;;;;;36;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;1;1;3;;",
        "Ceremonial Armor;5;chest60;Chest;Heavy;Unique;metal;12450;1.25;420;26;;;;;-20;26;;;;;;;;;;;;-20;-5;;;;;;;;;;;7;5;25;;;;;;;40;;;;;;36;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;1;2;2;;",
        "Vagabond Knight Armor;5;chest61;Chest;Heavy;Unique;metal;12450;1.25;300;24;;;;;-16;20;;;;;;;;;;;;-16;-4;;;;;-6;6;;;;;;;;;6;;;;;33;;;;;;33;9;9;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;2;;;;;;;;1;;4;;",
        "// MAGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Apprentice Robe;1;chest55;Chest;Light;Common;cloth;100;1.;30;1;;;;;;;;;;;;;;;;;;2;1;;;;3;;;;;;;;;;;;;;;;1;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;1;;;;;;;;;;;;",
        "Mahir Robe;2;chest56;Chest;Light;Unique;cloth;675;1.;40;2;;;;;;;;;;;;;;;;;;2;1;-3;;;;;;;;;;;;;;;;5;;;2;;;;5;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;1;;;1;;;;;;;;;",
        "Jonna Mantle;2;chest57;Chest;Light;Unique;cloth;675;1.;50;2;;;;;;;;;;;;;;;;;;;;;;;3;-3;;;;;;;;;;;;;;;2;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;2;;;;;;;;;;;;",
        "Apprentice Mage Robe;2;chest58;Chest;Light;Common;cloth;675;1.;40;2;;;;;;;;;;;;;;;;;;4;2;;;;5;;;;;;;;;;;;;;;;2;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;2;1;;;;;;;;;;;;",
        "Wanderer Mantle;3;chest59;Chest;Light;Common;cloth;1825;1.;50;3;;;;;;;;;;;;;;;;;;8;5;;;;8;;;;;;;;;;;;;;;;3;;;;8;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;2;1;;;;;;;;;;;;",
        "Hired Mage Mantle;3;chest60;Chest;Light;Common;cloth;1825;1.;70;4;;;;;;;;;;;;;;;;;;4;2;;;;5;;;;;;;;;;;;;;;;6;;;;;;8;7;7;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;3;;;;;;;;;;;;",
        "Soldier Mantle;3;chest61;Chest;Light;Common;cloth;1825;1.;55;3;;;;;;;;;;;;;;;;;;6;3;;;;7;-3;1;;;;;;;;;;;;;;3;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;1;2;;;;;;;;;;;;",
        "Battlemage Mantle;4;chest62;Chest;Light;Common;cloth;4125;1.;90;5;;;;;;;;;;;;;;;;;;8;4;-4;;;8;;;;;;;;;;;;;;;;12;;;;7;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;;;1;;;;;;",
        "Maen Mantle;4;chest63;Chest;Light;Common;cloth;4125;1.;80;4;;;;;;;;;;;;;;;;;;8;5;;;;9;;;;-3;;;;;;;;;;;;4;;;;;;6;8;8;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;1;;;;;;;;",
        "Arcanist Mantle;4;chest64;Chest;Light;Common;cloth;4125;1.;60;4;;;;;;;;;;;;;;;;;;5;2;;;;;;;;;;;;;;;9;;;;;4;;;;;;6;;;;;;;;;;;;;20;;;;;;;;;;9;;;;magic;;;;;4;;;;;;;;;;;;",
        "Pyromancer Mantle;4;chest65;Chest;Light;Common;cloth;4125;1.;60;4;;;;;;;;;;;;;;;;;;5;2;;;;;;2;4;;;;;;;;;;;;;4;;;;;;6;;;;;;;;20;;;;;;;;;;9;;;;;;;;;magic;;;;;4;;;;;;;;;;;;",
        "Geomancer Mantle;4;chest66;Chest;Light;Common;cloth;4125;1.;80;4;;;;;;;;;;;;;;;-4;9;;5;2;;;;;;;;;;;;;;;;;;;;4;;;;;;6;;;;;;;;;;;;;10;;;;;;9;;;;;;;;magic;;;;;4;;;;;;;;;;;;",
        "Electromancer Mantle;4;chest67;Chest;Light;Common;cloth;4125;1.;60;4;;;;;;;;;;;;;;;;;;10;4;;;-3;;;;;;;;;;;;;;;;;4;;;;;;6;;;;;;;;;20;;;;;;;;;;;;9;;;;;;magic;;;;;4;;;;;;;;;;;;",
        "Cryomancer Mantle;4;chest68;Chest;Light;Common;cloth;4125;1.;60;4;;;;;;12;;;;;;;;;;;;5;2;;;;;-5;;;;;;7;;;;;;;;;4;;;;;;6;;;;;;;;;;;;25;;;;;;;;;;9;;;;;magic;;;;;4;;;;;;;;;;;;",
        "Spellweaver Mantle;5;chest69;Chest;Light;Common;cloth;8725;1.;50;5;;;;;;;;;;;;;;;;;;10;5;;;-5;12;;;;-7;;;;;;;;;;;;5;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;5;;;;;;;;;;;;",
        "Red Silk Mantle;5;chest70;Chest;Light;Common;cloth;8725;1.;70;6;;;;;8;;;;;;;;;;;;;15;8;;;;12;-8;;;;;;;;;;;;;;;7;;;;;;10;7;7;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;5;;;;;;;;;;;;",
        "Court Wizard Mantle;5;chest71;Chest;Light;Common;cloth;8725;1.;60;4;;;;;;;;;;;;;;;;;;10;5;;;;14;;3;6;;;;;;;;;;;;;3;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;4;;;;;;;;;;;2;",
        "[ GLOVES ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// LIGHT;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Work Gloves;1;arms01;Arms;Light;Common;leather;25;1.;45;1;;;;;;;-1;;;;;;4;;;;;;;;-3;;;;;;;;;;;;;;;;;;8;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Blacksmith Mittens;1;arms02;Arms;Light;Common;leather;50;1.5;60;2;;1;;;;;1;;;;;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;20;;;;;;9;;;;;;;;25;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;2;;;;;;;;;",
        "Leather Gloves;2;arms03;Arms;Light;Common;leather;175;1.;60;3;;;;;;;-2;;;;;;8;;;;;;;;-4;;;;;;;;;;;;;;;;;;10;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;2;;;;;;;;;",
        "Leather Bracers;2;arms04;Arms;Light;Common;leather;175;1.;50;2;;;;;;;-2;2;;;;;8;;;;;;;;-6;;;;;;;;;;;;;;;;;;6;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;2;;;;;;;;;",
        "Leo Bracers;2;arms05;Arms;Light;Unique;leather;250;1.3;55;2;;;;;;;-1;1;;;;;;;;;;;;-4;;;;-1;;;;;;;;;;;;;;;8;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;2;;;;;;;;;",
        "Falconeer Gloves;3;arms06;Arms;Light;Common;leather;500;1.;75;4;;;;;;;-3;;;;;;12;;;;;;;;-5;;;;;;;;;;;;;;;;;;12;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;;;",
        "Tied Gloves;4;arms07;Arms;Light;Common;leather;1350;1.;90;6;;;;;;;-4;;;;;1;16;;;;;;;;-6;;;;;;;;;;;;;;;;;;14;;;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;;;",
        "Duelist Gloves;4;arms08;Arms;Light;Common;leather;1350;1.;90;6;;;;;;;-4;;;;;;16;3;;;;;;;-6;;;;;;;;;;;;;;;;;;14;;;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;;;",
        "Blademaster Gloves;5;arms09;Arms;Light;Common;leather;3300;1.;105;8;;2;;;;;-5;2;;;;1;20;;;;;;;;-7;;;;;;;;;;;;;;;;;;16;;;;;;14;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;;;",
        "// MEDIUM;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Splint Vambraces;2;arms10;Arms;Medium;Common;metal;375;1.;90;7;;;;;;;-1;1;;;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;12;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;1;;;;;",
        "Steel Vambraces;3;arms11;Arms;Medium;Common;metal;1000;1.;115;9;;;;;;;-2;1;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;15;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;1;;;1;;",
        "Mail Gloves;3;arms12;Arms;Medium;Common;metal;1000;1.;165;12;;;;;;;;;;;;;;;;;;-2;-1;;;;;;;;;;;;;;;;;;;;20;;;;;;10;;;;10;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;2;;;;",
        "Soldier Gloves;3;arms13;Arms;Medium;Common;metal;1000;1.;150;13;;;;;;;;;;;;;;;;;;-5;-1;;;;;;;;;;;;;;;;;;;;20;;;;;;14;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;1;;1;;;;",
        "Riveted Gloves;4;arms14;Arms;Medium;Common;metal;2225;1.;170;15;;;;;;;-1;;;;;;;;;;;-7;-1;;;;;;;;;;;;;;;;;;;;20;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;1;;1;;;;",
        "Veteran Gloves;4;arms15;Arms;Medium;Common;metal;2225;1.;190;15;;;;;;;;1;;;;;;;;;;-7;-1;;;;;;;;;;;;;;;;;;;;25;;;;;;15;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;1;;1;;;;",
        "Captain Gloves;5;arms16;Arms;Medium;Common;metal;4700;1.;210;18;;;;;;;;;;;;;;;;;;-9;-1;;;;;;;;;;;;;;;;;;;;26;;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;1;2;1;;;;",
        "Guardian Gloves;5;arms17;Arms;Medium;Unique;metal;5425;1.15;230;16;;3;;;;;;;;;;;;;-3;;;-5;-1;;;;;;;;;;;;;;;;;;;;30;;;;;;15;;;;15;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;1;1;2;;;;",
        "// HEAVY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Plate Mittens;3;arms18;Arms;Heavy;Common;metal;1475;1.;250;16;;2;;;;5;3;;;;;;;;;;;-14;-4;;;;;;;;;;;;;;;;;;;;29;;;;;;23;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;4;;;;;",
        "Plate Gloves;4;arms19;Arms;Heavy;Common;metal;3350;1.;300;20;;3;;;;7;4;;;;;;;;;;;-16;-4;;;;;;;;;;;;;;;;;;;;33;;;;;;28;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;3;1;;;;",
        "Knight Gauntlets;5;arms20;Arms;Heavy;Common;metal;7075;1.;350;24;;4;;;;10;5;;;;;;;;;;;-18;-4;;;;;;;;;;;;;;;;;;;;37;;;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;3;2;;;;",
        "Gilded Plate Gloves;5;arms21;Arms;Heavy;Unique;metal;8825;1.25;300;23;;5;;;;12;5;;3;;;;;;;;;-18;-3;-3;;;;;;;;;;;;;;;;;;;37;;;;7;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;2;2;;;1;",
        "// MAGE BRACERS;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Novice Hand Wraps;1;arms22;Arms;Light;Common;cloth;50;0.5;30;1;;;;;;;;;;;;;;;;;;2;;;;;2;-1;;;;;;;;;;;;;;;2;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Jonna Gloves;2;arms23;Arms;Light;Unique;leather;600;1.1;45;1;;;;;;;;;;;;;;;;;;4;2;;;;4;-2;;;;;;;;;;;;;;;3;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;2;;;;;;;;;",
        "Apprentice Mage Gloves;2;arms24;Arms;Light;Common;leather;550;1.;40;1;;;;;;;;;;;;;;;;;;4;2;;;;4;-1;2;;;;;;;;;;;;;;4;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;1;;;;1;;;;;;;;;",
        "Sorcerer Bracers;3;arms25;Arms;Light;Common;leather;1475;1.;50;3;;;;;;;;;;;;;;;;;;6;3;;;;6;;3;;;;;;;;;;;;;;6;;;;;;5;5;5;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;1;;;;1;;;;;;;;;",
        "Orient Bracers;4;arms26;Arms;Light;Common;leather;3350;1.;80;2;;;;;;;;;;;;;;;;;;8;4;;;;8;;4;;;;;;;;;;;;;;8;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;2;;;;;;;;2;",
        "Ruby Mage Bracers;5;arms27;Arms;Light;Common;leather;7075;1.;70;2;;;;;;;;;;;;;;;;;;10;3;;;;12;;5;;-5;;;;;;;;;;;;10;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;;;;2;;;;;;;;2;",
        "Sapphire Mage Bracers;5;arms28;Arms;Light;Common;leather;7075;1.;70;2;;;;;;;;;;;;;;;;;;15;6;;;-4;10;;5;;;;;;;;;6;;;;;10;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;;;;2;;;;;;;;2;",
        "Antique Wristbands;5;arms29;Arms;Light;Unique;metal;7775;1.1;55;2;;;;;;;;;;;;;;;;;;10;5;;;;14;;5;5;;;;;;;;;1;;;;4;;;;;;2;3;3;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;4;",
        "// MAGE GLOVES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Mystic Gloves;4;arms30;Arms;Light;Common;leather;2800;1.;80;5;;;;;;;;;;;;;;;;;;8;2;;;;;-4;;5;-6;;;;;;;;;;;;16;;;;;;9;7;7;;;;;;;;;;;;25;;;;;;;;;;;;;magic;;;;2;;;;1;;;;;;;;;",
        "Battlemage Gloves;4;arms31;Arms;Light;Common;leather;2800;1.;95;7;;;;;;;;;;;;;;;;;;6;2;;;;;-4;;;-6;;;;;;;;;;;;24;;;;;;15;15;15;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;;;;2;1;;;;;;;;",
        "Court Wizard Gloves;5;arms32;Arms;Light;Common;leather;5900;1.;105;6;;;;;;;;;;;;;;;;;;9;3;;;;;-6;2;;-8;;;;;;;;;;;;10;;;;;;14;14;14;4;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;1;;;1;;;;;;;;2;",
        "[ BOOTS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;];;;;;;;;;;;;;;;;;",
        "// LIGHT DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Rusted Shackles;1;boots01;Legs;Heavy;Common;metal;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;-100;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Peasant Shoes;1;boots02;Legs;Light;Common;leather;50;1.;60;2;;;;;2;;;;;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;8;4;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;;;;;;",
        "Hide Boots;2;boots03;Legs;Light;Common;leather;275;1.;80;3;;;;;4;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;10;7;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;2;;;;;;;;",
        "Tanned Boots;3;boots04;Legs;Light;Common;leather;750;1.;100;5;;;;;6;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;12;10;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;3;;;;;;;;",
        "Norse Boots;3;boots05;Legs;Light;Common;leather;825;1.1;90;5;;;;;5;;;;;;;;;4;;;;;1;;;;;;;;;;;;;;;;;;;;17;11;;;;;10;;;;;;;15;;;;;;;;;;;;;;;;;;;;fjall;;;;1;;;;;2;;;;;;;;",
        "Elven Boots;4;boots06;Legs;Light;Common;leather;1850;1.1;130;7;;;;;6;;-3;;;;;;;4;;;;;;;-5;;;;;;;;;;;;;;;;;;12;10;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;3;;;;;;;;",
        "Duelist Boots;4;boots07;Legs;Light;Common;leather;1675;1.;120;7;;;;;8;;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;14;12;;;;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;4;;;;;;;;",
        "Magehunter Boots;5;boots08;Legs;Light;Common;leather;3525;1.;140;9;;;;;10;;-2;;;;;;;5;;;;;;;;;;-2;;;;;;;;;;;;;;;16;15;;;;;14;20;20;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;5;;;;;;;;",
        ";5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// LIGHT UTILITY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Sharp Shoes;1;boots09;Legs;Light;Common;leather;25;1.;45;1;;;;;;;;;;;;;;;;;;;1;;;;;;;;;;;;;;;;;;;;2;2;;;7;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;;;;;;",
        "Travelling Shoes;2;boots10;Legs;Light;Common;leather;175;1.;60;2;;;;;;;;;;;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;4;4;;;10;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;;;;;;",
        "Mahir Boots;2;boots11;Legs;Light;Common;cloth;175;1.;60;1;;;;;2;;;;;;;;;;;5;;;2;;;;;;;;;;;;;;;;;;;;4;4;;;15;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;2;;;;;;;;;",
        "Town Shoes;3;boots12;Legs;Light;Common;leather;500;1.;75;3;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;6;6;;;14;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;2;;;;;;;;;",
        "Riding Boots;3;boots13;Legs;Light;Common;leather;600;1.2;85;4;;;;;;;;;;;;;;;;;;5;2;;;;;;;;;;;;;;;;;;;;6;6;;;9;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;;;",
        "Fancy Boots;4;boots14;Legs;Light;Common;leather;1125;1.;90;4;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;8;8;;;17;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;4;;;;;;;;",
        "Court Wizard Boots;5;boots15;Legs;Light;Common;leather;2825;1.2;105;5;;;;;;;;;;;;;;;;;;;5;;;-3;;-1;;3;;;;;;;;;;;;;10;10;;;20;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;2;;;;;3;;;;;;;;",
        "// MEDIUM;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Recruit Boots;2;boots16;Legs;Medium;Common;metal;375;1.;100;9;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;15;14;;;;;15;;;;;;5;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;2;;;;;;;1;;",
        "Splinted Boots;2;boots17;Legs;Medium;Common;metal;375;1.;120;9;;;;;-3;;;;;;;;;;;;;-3;-1;;;;;;;;;;;;;;;;;;;;20;10;;;;;15;;;;5;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;2;;;;1;;;;",
        "Mail Boots;3;boots18;Legs;Medium;Common;metal;1000;1.;165;12;;;;;-4;;;;;;;;;;;;;-5;-1;;;;;;;;;;;;;;;;;;;;20;16;;;;;11;;;;15;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;2;;;;",
        "Veteran Greaves;4;boots19;Legs;Medium;Common;metal;2225;1.;180;15;;;;;-5;;;;;;;;;;;;;-7;-1;;;;;;;;;;;;;;;;;;;;23;20;;;;;21;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;3;;;;;;;1;;",
        "Captain Boots;5;boots20;Legs;Medium;Common;metal;4700;1.;210;18;;;;;-6;;;;;;;;;;;;;-9;-1;;;;;;;;;;;;;;;;;;;;26;25;;;;;24;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;2;;;3;;;;;",
        "Sardar Boots;5;boots21;Legs;Medium;Unique;metal;4700;1.;180;15;;;;;3;;-3;;;;;;;;;;;-9;-1;;;;;;;;;;;;;;;;;;;;33;23;;;;;20;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;1;;;;2;;1;;;1;;",
        "Engraved Boots;5;boots22;Legs;Medium;Unique;metal;4700;1.;170;18;;;;;-6;;;;;;;;;;;;;-7;-1;-4;;;5;;;;;;;;;;;;;;;;26;33;;;15;;24;8;8;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;1;;;2;;;1;1;",
        "// HEAVY;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        ";3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Scale Boots;4;boots23;Legs;Heavy;Common;metal;2675;0.8;330;18;;;;;-8;6;;;;;;;;;;;;-12;-3;;;;;;;;;;;;;;;;;;;;26;27;;;;;25;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;1;;;;3;;",
        "Sergeant Sabatons;4;boots24;Legs;Heavy;Common;metal;3350;1.;300;20;;;;;-10;7;;;;;;;;;;;;-16;-4;;;;;;;;;;;;;;;;;;;;33;30;;;;;28;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;1;;1;3;;",
        "Noble Sabatons;5;boots25;Legs;Heavy;Common;metal;7075;1.;350;24;;;;;-12;10;;;;;;;;;;;;-18;-4;;;;;;;;;;;;;;;;;;;;43;39;;;;;33;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;1;;1;4;;",
        "Aldwynn Sabatons;5;boots26;Legs;Heavy;Common;metal;8125;1.15;385;25;;;;;-13;11;;;;;;;;;;;;-21;-5;;;;;;;;;;;;;;;;;;;;37;35;;;;;36;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;1;1;4;;",
        "[ BELTS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// DODGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Rope Belt;1;belt01;Waist;Light;Common;cloth;25;1.;20;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Embroidered Belt;2;belt02;Waist;Light;Common;cloth;100;1.;25;;;;;;3;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;1;",
        "Nomad Sash;3;belt03;Waist;Light;Common;cloth;275;1.1;35;;;;;;4;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;1;;;;;;;;;;;;;",
        "Ryn Gilded Belt;3;belt05;Waist;Light;Unique;leather;300;1.2;25;;;;;;3;;;;;;;1;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;;;;1;;;;;;;;1;",
        "Duelist Belt;4;belt04;Waist;Light;Common;leather;575;1.;35;;;;;;5;4;;;;;;;;3;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;1;;;;;",
        "Aristocrat Belt;5;belt06;Waist;Light;Common;leather;1225;1.;40;;;;;5;6;;;;;;;;;4;;;;;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;;;;;1;",
        "// DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Leather Belt;1;belt07;Waist;Light;Common;leather;50;2.;35;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;;;;;;",
        "Chain Belt;2;belt08;Waist;Light;Common;metal;100;1.;50;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;1;;;;;",
        "Footman Girdle;3;belt09;Waist;Light;Common;leather;250;1.;60;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;10;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;;;;;;",
        "Veteran Belt;4;belt09;Waist;Light;Common;leather;575;1.;70;;;5;;;;7;;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;12;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;1;;;;;",
        "Mercenary Girdle;4;belt10;Waist;Light;Common;leather;575;1.;70;;;6;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;12;3;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;1;;;;;",
        "Knightly Belt;5;belt11;Waist;Light;Common;metal;1225;1.;85;;;7;3;;;10;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;15;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;2;;;;;",
        "Royal Champion Belt;5;belt12;Waist;Light;Unique;metal;1225;1.;85;;;8;;5;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;10;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;2;;;;1;;;;1;",
        "// OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Peasant Sash;1;belt13;Waist;Light;Common;cloth;25;1.;35;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Norse Belt;2;belt14;Waist;Light;Common;leather;100;1.1;35;;;;;;;;-2;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;1;;;;;;;;;",
        "Jorgrim Belt;2;belt15;Waist;Light;Unique;leather;125;1.15;40;;;;;;;;-2;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;2;;;;;;;;;",
        "Fur Belt;3;belt16;Waist;Light;Common;leather;250;1.;45;;;;;;;;-3;;;;;;9;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;10;;;;;;;;;;;;;;;aldor;;;;1;;;;1;;;;;;;;;",
        "Skirmisher Belt;4;belt17;Waist;Light;Common;leather;575;1.;55;;;;;;;;-4;2;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;2;;;;;;;;;",
        "Wolfbrother Belt;4;belt18;Waist;Light;Common;leather;575;1.;55;;;;;;;;-4;;;;5;;12;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;;;;1;;;;;",
        "Captain Waistband;5;belt19;Waist;Light;Common;leather;1225;1.;65;;;;;;;;-5;1;;;;;15;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;1;1;;;;;;;;",
        "Voivod Belt;5;belt20;Waist;Light;Unique;leather;1400;1.15;75;;;;;;;;-5;;;;;;10;;;5;;3;;;;;;;;;;;;3;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;3;;;;;;;;;",
        "// MAGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Waistband;1;belt21;Waist;Light;Common;cloth;25;1.;10;;;;;;;;;;;;;;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;;",
        "Ornate Girdle;2;belt22;Waist;Light;Common;leather;125;1.;15;;;;;;;;;;;;;;;;;;;2;1;;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;1;;;;;;;;;;;;;",
        "Luxorious Belt;3;belt23;Waist;Light;Common;leather;400;1.2;20;;;;;;;;;;;;;;;;;;;3;;;;;;-3;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;;;;1;;;;;;;;1;",
        "Elven Girdle;3;belt24;Waist;Light;Common;cloth;375;1.1;20;;;;;;1;;;;;;;;;;;;;;3;;;;;-3;;;;;;;;;;;;;;;;;;;3;;;3;3;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;1;;;;;;;;1;;;;;",
        "Mystic Belt;4;belt25;Waist;Light;Common;leather;750;1.;25;;;;;;;;;;;;;;;;;;;5;;;;-3;;-4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;magic;;;;;;;;1;;;;;;;;2;",
        "Ruby Sash;4;belt26;Waist;Light;Common;cloth;900;1.2;25;;;;;;;;;;;;;;;;;;;;;;;;;-4;;;-4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;1;;;;;;;;;;;;;",
        "Court Wizard Belt;5;belt27;Waist;Light;Common;leather;1900;1.2;30;;;;;;;;;;;;;;;;;;;;;;;;;-5;1;5;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;;;;1;;;;;;;;2;",
        "Spellweaver Girdle;5;belt28;Waist;Light;Common;cloth;1900;1.2;30;;;;;;;;;;;;;;;;;;;;;;;-4;;-6;;;-5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;1;;;;;;;;;;;;1;",
        "Serpent Order Belt;5;belt29;Waist;Light;Unique;leather;2050;1.3;30;;;;;;3;;;;;;;;;;;;;;;-5;;;;-5;;;;;;5;;5;;;;;;;;;;;;;;5;;;;;;;;;15;;;;;;;;;;;;;;;;;unique;;;;;;;;2;;;;1;;;;;",
        "[ RINGS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// UNIVERSAL;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Verren Ring;1;ring01;Ring;Light;Unique;metal;30;1.;30;;;;;;;;-1;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;;;;;;;;;;",
        "Velmir Ring;1;ring02;Ring;Light;Unique;metal;30;1.;40;;;;;;;;;1;;;;1;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;;;;;;;;;;",
        "Copper Ring;1;ring03;Ring;Light;Common;metal;15;1.;40;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;1;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Antique Copper Ring;2;ring04;Ring;Light;Common;metal;20;1.;40;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Brass Ring;2;ring05;Ring;Light;Common;metal;20;1.;50;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Bronze Amethyst Ring;3;ring06;Ring;Light;Common;metal;60;1.;50;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Brass Signet;3;ring07;Ring;Light;Common;metal;60;1.;50;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Steel Signet;3;ring08;Ring;Light;Common;metal;60;1.;55;;;3;2;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Brass Agate Ring;3;ring09;Ring;Light;Common;metal;60;1.;55;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "// WARRIOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Silver Ring;2;ring10;Ring;Light;Common;silver;40;1.;30;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;2;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Exquisite Silver Ring;3;ring11;Ring;Light;Common;silver;120;1.;40;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Silver Jade Ring;3;ring12;Ring;Light;Common;silver;120;1.;40;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;5;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Berserk Ring;3;ring13;Ring;Light;Common;leather;120;1.;40;;;;;;;;;;;;5;2;6;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Silver Amethyst Ring;4;ring14;Ring;Light;Common;silver;280;1.;50;;;;;;;;-2;;;;;;;;;;;;;;;;;;;;;;;;;8;;;;5;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Silver Aquamarine Ring;4;ring15;Ring;Light;Common;silver;280;1.;50;;;;;;;;;;;;;;;;;;;5;;;;;;;;;;;;3;;;;;;;;;6;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Silver Topaz Ring;4;ring16;Ring;Light;Common;silver;280;1.;50;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;6;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Silver Emerald Ring;5;ring17;Ring;Light;Common;silver;600;1.;55;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;6;1;5;;;;;;;7;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Silver Sapphire Ring;5;ring18;Ring;Light;Common;silver;600;1.;55;;;;;;;;;;;;;;;;;;;7;2;;;;;;;;;;;;;;;;;;;;7;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Silver Ruby Ring;5;ring19;Ring;Light;Common;silver;600;1.;55;;;;;;;;;3;;;;;;;;;;;;;;;;;;;-4;;;;;;;;;;;;7;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "// MAGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Gold Ring;2;ring20;Ring;Light;Common;gold;100;1.;15;;;;;;;;;;;;;;;;;;;2;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Carved Gold Ring;3;ring21;Ring;Light;Common;gold;260;1.;20;;;;;;;;;;;;;;;;;;;4;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Exquisite Gold Ring;4;ring22;Ring;Light;Common;gold;580;1.;25;;;;;;;;;;;;;;;;;;;8;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Gold Aquamarine Ring;4;ring23;Ring;Light;Common;gold;580;1.;25;;;;;;;;;;;;;;;;;;;4;;;;;4;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Gold Ruby Ring;4;ring24;Ring;Light;Common;gold;580;1.;25;;;;;;;;;;;;;;;;;;;4;;;;;4;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Gold Diamond Ring;5;ring25;Ring;Light;Common;gold;1220;1.;30;;;;;;;;;;;;;;;;;;;5;5;;;-3;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Gold Sapphire Ring;5;ring26;Ring;Light;Common;gold;1220;1.;30;;;;;;;;;;;;;;;;;;;5;;;;;7;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Exquisite Ruby Ring;5;ring27;Ring;Light;Common;gold;1220;1.;30;;;;;;;;;;;;;;;;;;;5;;;;;6;;;;-5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "// HYBRID;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Silver Insert Gold Ring;2;ring28;Ring;Light;Common;silver;80;1.;25;;;;;;;;;;;;;;;;;;;;;;;;2;;;;;;;2;;;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Morion Ring;4;ring29;Ring;Light;Common;silver;440;1.;35;;;;;;;;;;3;;;;;;;;;;;;;;3;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Hermit Ring;5;ring30;Ring;Light;Unique;wood;920;1.;40;;;;;;;;;;;;;;;;;;;5;5;;;;;;;;-5;;;5;5;;;;;;;;;;;;;;5;10;;;;;;;;;;;;;;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "[ NECKLACES ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "// DEFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Copper Chain;1;neck01;Amulet;Light;Common;metal;20;1.;35;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;1;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Crude Amulet;1;neck02;Amulet;Light;Common;wood;20;1.;35;;;;;;1;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Amber Amulet;2;neck03;Amulet;Light;Common;gem;120;1.;50;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;3;;;;;;;;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Hilda Amulet;2;neck04;Amulet;Light;Unique;gem;120;1.;50;;;;;;;;;1;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;;;;;;;;;;",
        "Silver Aquamarine Amulet;3;neck05;Amulet;Light;Common;silver;320;1.;60;;;;;;;;;;;;;;;;;4;;;;;;;;;;;;;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Runic Amulet;3;neck06;Amulet;Light;Common;metal;220;0.7;60;;;1;;;1;1;-1;;;;;1;;;;;;;;;;;;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Silver Amethyst Amulet;4;neck07;Amulet;Light;Common;silver;700;1.;70;;;;;;;3;;;;;;;;;;6;;;;;;;;;;;;;;5;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Warding Hand Amulet;4;neck08;Amulet;Light;Unique;gold;700;1.;99;;;3;;;3;3;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;9;;;;;;;;;;;;;unique;;;;;;;;;;;;;;;;;",
        "Silver Emerald Amulet;5;neck09;Amulet;Light;Common;silver;1460;1.;85;;;;;;;5;;;;;;;;;;8;;;;;;;;;;;;;;10;;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "Crescent Amulet;5;neck10;Amulet;Light;Common;silver;1460;1.;85;;;;;;;5;;;;;;;;;;10;;;;;;;;;;;;;1;8;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "// MAGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Lucky Talisman;2;neck11;Amulet;Light;Common;gold;200;1.;25;;;;;;;;;;;;;;;;;;;2;;;;;;-1;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Hexer Talisman;2;neck12;Amulet;Light;Common;metal;200;1.;25;;;;;;;;;;;;;;;;;;;;3;;;;6;3;;;;;;-6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;9;;;;;;;;;3;;;;special;;;;;;;;;;;;;;;;;",
        "Gold Talisman;3;neck13;Amulet;Light;Common;gold;520;1.;30;;;;;;;;;;;;;;;;;;;5;;;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Jade Talisman;3;neck14;Amulet;Light;Common;gold;520;1.;30;;;;;;;;;;;;;;;;;;;3;;;;;3;;2;;;;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "Phylactery Talisman;3;neck15;Amulet;Light;Unique;gem;666;1.;33;;;;;;;;;;;;;;;;13;;;;3;;;;9;;;;;;-1;;3;;9;;;;;;;;;;;;;;;;;;;;;;;;;;33;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Jibean Talisman;4;neck16;Amulet;Light;Common;gold;1160;1.;35;;;;;;;;;;;;;;;;;;;8;2;;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "Sapphire Talisman;4;neck17;Amulet;Light;Common;gem;1160;1.;35;;;;;;;;;;;;;;;;;;;3;;;;;6;;3;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Venemist Talisman;4;neck18;Amulet;Light;Unique;silver;1160;1.;35;;;;;;;;;;;;;;;;;;;;;;;;6;;;;;;1;;;;;;1;9;;;;;;;;;;;;;;;;;;;;;;;13;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Gold Diamond Talisman;5;neck19;Amulet;Light;Common;gold;2440;1.;40;;;;;;;;;;;;;;;;;;;4;2;;;;7;;4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Gold Sapphire Talisman;5;neck20;Amulet;Light;Common;gold;2440;1.;40;;;;;;;;;;;;;;;;;;;6;;;;;9;;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Oracle Star;5;neck21;Amulet;Light;Common;gold;2440;1.;40;;;;;;;;;;;;;;;;;;;10;;-4;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "// OFFENSIVE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Fang Pendant;1;neck22;Amulet;Light;Common;metal;40;1.;50;;;;;;;;;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Bow Pendant;2;neck23;Amulet;Light;Common;metal;160;1.;65;;;;;;;;-1;2;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Trophies Necklace;2;neck24;Amulet;Light;Common;cloth;160;1.;65;;;;;;;;;1;;;;1;10;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;fjall;;;;;;;;;;;;;;;;;",
        "Jorgrim Pendant;2;neck25;Amulet;Light;Unique;metal;160;1.;65;;;;;;;;;1;;;;;;;;;;;;;;;;;;;;;;;3;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;;;;;;;;;;;;;",
        "Pearl Necklace;3;neck26;Amulet;Light;Common;gem;420;1.;80;;;;;;;;;1;;;;;;;;;;;;;;;;;;;-5;;;;;;;;;;;;;;;;;;;10;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Lazurite Pendant;3;neck27;Amulet;Light;Common;gem;420;1.;80;;;;;;;;;;;;;;;;;;;4;;;-5;;;;;;;;;;;;;;;;;;;;;;;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Carved Topaz Pendant;4;neck28;Amulet;Light;Common;gem;920;1.;95;;;;;;;;;3;;;;;;;;;;;3;;-4;;;;;;-4;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Pagan Pendant;4;neck29;Amulet;Light;Unique;silver;1313;1.;66;;;;;;;9;;;;;;;;;;;;;;;;;;;;;;;;;;;6;6;;;;;;;;;;;-6;-6;-6;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;;;;;;;;;;;;;",
        "Ruby Pendant;5;neck30;Amulet;Light;Common;silver;1960;1.;110;;;;;;;;;4;;;;;;;;;;;4;;-5;;;;;;-5;;;;;;;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;;;;;;;;;;;;;",
        "Everflaming Pendant;5;neck31;Amulet;Light;Common;gem;1960;1.;110;;;;;;;;;;;;;2;;;;5;;;;;;;;;;;-6;;;;;;;;;;;;;;;;;;;7;;;;;;;33;;;;;;;;;;;;;;;;;;;elven;;;;;;;;;;;;;;;;;",
        "[ CLOAKS ];;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;];;;;;;;;;;;;;;;;;",
        "// GENERIC;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Worn Cloak;1;cloak01;Back;Light;Common;cloth;25;1.;20;;;;;;1;;;;;;;;;;;;;;;;;;;;;;;;;;1;;;;;;;;;;;;5;;;4;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;2;;;;;;;;;;;;;",
        "Travelling Cloak;2;cloak02;Back;Light;Common;cloth;75;0.75;25;;;;;;2;;;;;;;;;;;;;;2;;;;;;;;;;;;3;;;;;;;;;;;;10;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;1;;;;;;;;;;;;",
        "Azure Cape;2;cloak03;Back;Light;Unique;cloth;100;1.;25;;;;;;2;;;;;;;;;;;;;;1;;;;;;;;;;;;1;5;;;;;;;;;;;8;;;6;;;;;;;;;;;;;;;;;;;;;;;;;;special exc;;;;;2;;;;;;;;;;;;",
        "Wanderer Cloak;3;cloak04;Back;Light;Common;cloth;275;1.;30;;;;;;3;;;;;;;;;;;;;;3;;-3;;;;;;;;;;3;;;;;;;;;;;;12;;;8;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;2;;;;;;;;;;;;",
        "// WARRIOR;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Joust Cape;3;cloak05;Back;Light;Unique;cloth;350;1.25;40;;;;;;;;;;;;;;;;-5;;;;;;;;;;;;;;;;;;;;;;;;8;;;5;;;5;5;5;;;;;;;;;;;;;;;;;;;;;;;;;special;;;;;4;;;;;;;;;;;;",
        "Vigilante Cloak;3;cloak06;Back;Light;Common;cloth;275;1.;30;;;;;;;;;1;;;;1;5;;;;;;1;;;;;;;;;;;;;;;;;;;;5;;;;;;5;;;;;;;;;;;;;;;;;;;;;;;;;;;aldor;;;;;3;;;;;;;;;;;;",
        "Jibean Cape;4;cloak07;Back;Light;Common;cloth;725;1.15;35;;;;;;;;;;;;;2;;3;;;;;;;-5;;;;;;;;;;;;;;;;;;7;;;;;;7;7;;;;;;;;;;;;;;;;;;;;;;;;;;elven;;;;;3;;;;;;;;;;;;",
        "Nistrian Noble Cloak;5;cloak08;Back;Light;Common;cloth;1500;1.15;40;;;;;;;9;;;;;;;;;-3;;;;;;;;;;;;;;;;;;;;;;;;9;;;;;;9;9;;;;;;;;;;;;;;;;;;;;;;;;;;nistra;;;;;3;;;;;;;;;;;;",
        "// MAGE;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
        "Academy Cloak;2;cloak09;Back;Light;Common;cloth;150;1.3;25;;;;;;;;;;;;;;;;;;;3;1;;;;;-1;;;;;;;;;;;;;;;;;;;;;;5;5;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;;;;;;;;;",
        "Battlemage Cloak;3;cloak10;Back;Light;Common;cloth;375;1.3;30;;;;;;;;-2;;;;;;;;;;;;;-3;;;;-2;;;;;;;;;;;;;;;;;;;;;4;12;12;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;;;;;;;;;",
        "Red Velvet Cape;4;cloak11;Back;Light;Common;cloth;800;1.3;35;;;;;;;;;;;;;;;;;;;5;3;;;;;-4;;;;;;;;;;;;;;;;;;;;;;10;10;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;;;;;;;;;",
        "Court Wizard Cape;5;cloak12;Back;Light;Common;cloth;1700;1.3;40;;;;;;;;;;;;;;;;;;;;4;;;-4;;;;6;;;;;;;;;;;;;;;;;;;;12;12;;;;;;;;;;;;;;;;;;;;;;;;;magic;;;;;2;;;;;;;;;;;;",
        "Occult Cloak;5;cloak13;Back;Light;Unique;cloth;2025;1.55;40;;;;;;;;;;;;;;;;;;;;;;;;6;;;;;;;;;;3;6;;;;;;;;;;;;9;9;;;;;;;;;;;;6;;;;;;;;;;;;;special;;;;;4;;;;;;;;;;;;"
    };
}
