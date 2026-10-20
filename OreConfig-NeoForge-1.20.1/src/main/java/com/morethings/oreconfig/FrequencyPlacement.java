package com.morethings.oreconfig;

import com.mojang.serialization.Codec;
import net.minecraft.core.BlockPos;
import net.minecraft.util.RandomSource;
import net.minecraft.world.level.levelgen.placement.PlacementContext;
import net.minecraft.world.level.levelgen.placement.PlacementModifier;
import net.minecraft.world.level.levelgen.placement.PlacementModifierType;

import java.util.stream.Stream;

/**
 * Repeats the incoming position {@code multiplier} times on average (e.g. 2.5 -> 2 or 3 copies).
 * Placed in front of a feature's other modifiers, this scales its veins per chunk, whether the
 * feature uses a count or a rarity filter.
 */
public class FrequencyPlacement extends PlacementModifier {
    public static final Codec<FrequencyPlacement> CODEC = Codec.DOUBLE.fieldOf("multiplier")
            .xmap(FrequencyPlacement::new, p -> p.multiplier).codec();

    private final double multiplier;

    public FrequencyPlacement(double multiplier) {
        this.multiplier = multiplier;
    }

    @Override
    public Stream<BlockPos> getPositions(PlacementContext context, RandomSource random, BlockPos pos) {
        int copies = (int) Math.floor(multiplier);
        if (random.nextDouble() < multiplier - copies) {
            copies++;
        }
        return Stream.generate(() -> pos).limit(copies);
    }

    @Override
    public PlacementModifierType<?> type() {
        return OreConfigMod.FREQUENCY.get();
    }
}
