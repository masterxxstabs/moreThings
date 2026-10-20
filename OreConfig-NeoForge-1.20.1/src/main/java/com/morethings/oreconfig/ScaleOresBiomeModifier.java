package com.morethings.oreconfig;

import com.mojang.serialization.Codec;
import net.minecraft.core.Holder;
import net.minecraft.world.level.biome.Biome;
import net.minecraft.world.level.levelgen.GenerationStep;
import net.minecraft.world.level.levelgen.placement.PlacedFeature;
import net.minecraftforge.common.world.BiomeModifier;
import net.minecraftforge.common.world.ModifiableBiomeInfo;

import java.util.List;
import java.util.ListIterator;

/** Swaps every ore feature in every biome for a copy scaled by the config. */
public class ScaleOresBiomeModifier implements BiomeModifier {

    @Override
    public void modify(Holder<Biome> biome, Phase phase, ModifiableBiomeInfo.BiomeInfo.Builder builder) {
        if (phase != Phase.MODIFY || !OreConfig.ENABLED.get()) {
            return;
        }
        for (GenerationStep.Decoration step : GenerationStep.Decoration.values()) {
            List<Holder<PlacedFeature>> features = builder.getGenerationSettings().getFeatures(step);
            ListIterator<Holder<PlacedFeature>> it = features.listIterator();
            while (it.hasNext()) {
                Holder<PlacedFeature> original = it.next();
                Holder<PlacedFeature> scaled = OreScaler.scale(original);
                if (scaled == null) {
                    it.remove();
                } else if (scaled != original) {
                    it.set(scaled);
                }
            }
        }
    }

    @Override
    public Codec<? extends BiomeModifier> codec() {
        return OreConfigMod.SCALE_ORES.get();
    }
}
