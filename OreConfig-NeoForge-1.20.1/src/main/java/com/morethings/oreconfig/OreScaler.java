package com.morethings.oreconfig;

import net.minecraft.core.Holder;
import net.minecraft.resources.ResourceLocation;
import net.minecraft.world.level.block.Blocks;
import net.minecraft.world.level.block.state.BlockState;
import net.minecraft.world.level.levelgen.feature.ConfiguredFeature;
import net.minecraft.world.level.levelgen.feature.Feature;
import net.minecraft.world.level.levelgen.feature.configurations.OreConfiguration;
import net.minecraft.world.level.levelgen.placement.PlacedFeature;
import net.minecraft.world.level.levelgen.placement.PlacementModifier;
import net.minecraftforge.common.Tags;
import net.minecraftforge.registries.ForgeRegistries;
import org.jetbrains.annotations.Nullable;

import java.util.ArrayList;
import java.util.IdentityHashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;

/**
 * Builds scaled copies of ore features. Results are cached per original feature so every biome
 * shares the same replacement object, which keeps Minecraft's feature ordering consistent.
 */
public final class OreScaler {
    private static final Map<PlacedFeature, Optional<Holder<PlacedFeature>>> CACHE = new IdentityHashMap<>();
    @Nullable
    private static List<OreRule> rules;

    private OreScaler() {}

    public static synchronized void reset() {
        CACHE.clear();
        rules = null;
    }

    /** Returns the scaled feature, the original if unchanged, or null if the feature should be removed. */
    @Nullable
    public static synchronized Holder<PlacedFeature> scale(Holder<PlacedFeature> original) {
        return CACHE.computeIfAbsent(original.value(), f -> Optional.ofNullable(build(original))).orElse(null);
    }

    @Nullable
    private static Holder<PlacedFeature> build(Holder<PlacedFeature> original) {
        PlacedFeature placed = original.value();
        ConfiguredFeature<?, ?> configured = placed.feature().value();
        if (!(configured.config() instanceof OreConfiguration ore)) {
            return original;
        }

        double[] multipliers = resolve(ore);
        if (multipliers == null) {
            return original;
        }
        double frequency = multipliers[0];
        double density = multipliers[1];
        if (frequency == 1.0 && density == 1.0) {
            return original;
        }
        if (frequency <= 0.0 || density <= 0.0) {
            OreConfigMod.LOGGER.debug("Removing ore feature {}", describe(original));
            return null;
        }

        Holder<ConfiguredFeature<?, ?>> feature = placed.feature();
        if (density != 1.0) {
            int size = Math.max(1, (int) Math.round(ore.size * density));
            @SuppressWarnings("unchecked")
            Feature<OreConfiguration> oreFeature = (Feature<OreConfiguration>) configured.feature();
            ConfiguredFeature<OreConfiguration, Feature<OreConfiguration>> resized = new ConfiguredFeature<>(
                    oreFeature, new OreConfiguration(ore.targetStates, size, ore.discardChanceOnAirExposure));
            feature = Holder.<ConfiguredFeature<?, ?>>direct(resized);
        }

        List<PlacementModifier> placement = placed.placement();
        if (frequency != 1.0) {
            placement = new ArrayList<>(placement);
            placement.add(0, new FrequencyPlacement(frequency));
        }

        OreConfigMod.LOGGER.debug("Scaling ore feature {} (frequency x{}, density x{})", describe(original), frequency, density);
        return Holder.direct(new PlacedFeature(feature, placement));
    }

    /** Returns {frequency, density} for this vein, or null if the config doesn't touch it. */
    @Nullable
    private static double[] resolve(OreConfiguration ore) {
        boolean isOre = false;
        OreRule rule = null;
        for (OreConfiguration.TargetBlockState target : ore.targetStates) {
            isOre |= isOre(target.state);
            if (rule == null) {
                rule = findRule(target.state);
            }
        }

        boolean useGlobal = isOre || OreConfig.GLOBAL_AFFECTS_NON_ORES.get();
        if (!useGlobal && rule == null) {
            return null;
        }
        double frequency = useGlobal ? OreConfig.GLOBAL_FREQUENCY.get() : 1.0;
        double density = useGlobal ? OreConfig.GLOBAL_DENSITY.get() : 1.0;
        if (rule != null) {
            frequency *= rule.frequency();
            density *= rule.density();
        }
        return new double[]{frequency, density};
    }

    @Nullable
    private static OreRule findRule(BlockState state) {
        if (rules == null) {
            rules = new ArrayList<>();
            for (String entry : OreConfig.OVERRIDES.get()) {
                OreRule parsed = OreRule.parse(entry);
                if (parsed == null) {
                    OreConfigMod.LOGGER.warn("Ignoring invalid ore override '{}'", entry);
                } else {
                    rules.add(parsed);
                }
            }
        }
        for (OreRule rule : rules) {
            if (rule.matches(state)) {
                return rule;
            }
        }
        return null;
    }

    private static boolean isOre(BlockState state) {
        if (state.is(Tags.Blocks.ORES) || state.is(Blocks.ANCIENT_DEBRIS)) {
            return true;
        }
        ResourceLocation id = ForgeRegistries.BLOCKS.getKey(state.getBlock());
        return id != null && id.getPath().endsWith("_ore");
    }

    private static String describe(Holder<PlacedFeature> feature) {
        return feature.unwrapKey().map(k -> k.location().toString()).orElse("<inline>");
    }
}
