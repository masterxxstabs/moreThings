package com.morethings.oreconfig;

import net.minecraft.core.registries.Registries;
import net.minecraft.resources.ResourceLocation;
import net.minecraft.tags.TagKey;
import net.minecraft.world.level.block.Block;
import net.minecraft.world.level.block.state.BlockState;
import net.minecraftforge.registries.ForgeRegistries;
import org.jetbrains.annotations.Nullable;

/** One "<block or #tag>;<frequency>;<density>" entry from the config. */
public record OreRule(@Nullable ResourceLocation block, @Nullable TagKey<Block> tag, double frequency, double density) {

    public boolean matches(BlockState state) {
        if (tag != null) {
            return state.is(tag);
        }
        return block.equals(ForgeRegistries.BLOCKS.getKey(state.getBlock()));
    }

    @Nullable
    public static OreRule parse(String entry) {
        String[] parts = entry.trim().split(";");
        if (parts.length != 3) {
            return null;
        }
        double frequency;
        double density;
        try {
            frequency = Double.parseDouble(parts[1].trim());
            density = Double.parseDouble(parts[2].trim());
        } catch (NumberFormatException e) {
            return null;
        }
        if (frequency < 0 || density < 0 || Double.isNaN(frequency) || Double.isNaN(density)) {
            return null;
        }

        String target = parts[0].trim();
        boolean isTag = target.startsWith("#");
        ResourceLocation id = ResourceLocation.tryParse(isTag ? target.substring(1) : target);
        if (id == null) {
            return null;
        }
        return isTag
                ? new OreRule(null, TagKey.create(Registries.BLOCK, id), frequency, density)
                : new OreRule(id, null, frequency, density);
    }
}
