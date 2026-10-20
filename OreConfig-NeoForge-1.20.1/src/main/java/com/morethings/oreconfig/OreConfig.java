package com.morethings.oreconfig;

import net.minecraftforge.common.ForgeConfigSpec;

import java.util.List;

public final class OreConfig {
    public static final ForgeConfigSpec SPEC;

    public static final ForgeConfigSpec.BooleanValue ENABLED;
    public static final ForgeConfigSpec.DoubleValue GLOBAL_FREQUENCY;
    public static final ForgeConfigSpec.DoubleValue GLOBAL_DENSITY;
    public static final ForgeConfigSpec.BooleanValue GLOBAL_AFFECTS_NON_ORES;
    public static final ForgeConfigSpec.ConfigValue<List<? extends String>> OVERRIDES;

    static {
        ForgeConfigSpec.Builder b = new ForgeConfigSpec.Builder();

        b.comment("Ore generation settings. Changes apply to newly generated chunks the next time a world is loaded.")
                .push("general");
        ENABLED = b.comment("Master switch. When false, ore generation is left completely vanilla.")
                .define("enabled", true);
        GLOBAL_FREQUENCY = b.comment(
                        "Multiplier for how many ore veins generate per chunk, applied to every ore.",
                        "1.0 = vanilla, 2.0 = twice as many veins, 0.5 = half, 0 = no ores at all.")
                .defineInRange("globalFrequency", 1.0, 0.0, 64.0);
        GLOBAL_DENSITY = b.comment(
                        "Multiplier for how many blocks are in each ore vein, applied to every ore.",
                        "1.0 = vanilla, 2.0 = veins twice as big. Very high values can slow down world generation.")
                .defineInRange("globalDensity", 1.0, 0.0, 16.0);
        GLOBAL_AFFECTS_NON_ORES = b.comment(
                        "If true, the global multipliers also apply to non-ore 'blobs' that use the ore feature",
                        "(dirt, gravel, granite, diorite, andesite, tuff, clay, ...).")
                .define("globalAffectsNonOres", false);
        b.pop();

        b.comment("Per-ore settings. These are multiplied on top of the global multipliers.").push("ores");
        OVERRIDES = b.comment(
                        "Format: \"<block or #tag>;<frequency>;<density>\"",
                        "  #forge:ores/diamond;2.0;1.5  -> diamonds: 2x as many veins, 1.5x bigger veins",
                        "  minecraft:emerald_ore;0;1    -> no emeralds",
                        "  minecraft:gravel;0.5;1       -> half as many gravel blobs (works for non-ores too)",
                        "Tags (starting with #) cover stone, deepslate and modded variants of an ore.",
                        "The first entry that matches a vein is used.")
                .defineListAllowEmpty(List.of("overrides"), () -> List.of(
                        "#forge:ores/coal;1.0;1.0",
                        "#forge:ores/iron;1.0;1.0",
                        "#forge:ores/copper;1.0;1.0",
                        "#forge:ores/gold;1.0;1.0",
                        "#forge:ores/redstone;1.0;1.0",
                        "#forge:ores/lapis;1.0;1.0",
                        "#forge:ores/diamond;1.0;1.0",
                        "#forge:ores/emerald;1.0;1.0",
                        "#forge:ores/quartz;1.0;1.0",
                        "#forge:ores/netherite_scrap;1.0;1.0"
                ), o -> o instanceof String s && OreRule.parse(s) != null);
        b.pop();

        SPEC = b.build();
    }

    private OreConfig() {}
}
