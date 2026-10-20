package com.morethings.oreconfig;

import com.mojang.serialization.Codec;
import net.minecraft.core.registries.Registries;
import net.minecraft.world.level.levelgen.placement.PlacementModifierType;
import net.minecraftforge.common.MinecraftForge;
import net.minecraftforge.common.world.BiomeModifier;
import net.minecraftforge.event.server.ServerStoppedEvent;
import net.minecraftforge.eventbus.api.IEventBus;
import net.minecraftforge.fml.ModLoadingContext;
import net.minecraftforge.fml.common.Mod;
import net.minecraftforge.fml.config.ModConfig;
import net.minecraftforge.fml.javafmlmod.FMLJavaModLoadingContext;
import net.minecraftforge.registries.DeferredRegister;
import net.minecraftforge.registries.ForgeRegistries;
import net.minecraftforge.registries.RegistryObject;
import org.slf4j.Logger;
import com.mojang.logging.LogUtils;

@Mod(OreConfigMod.MOD_ID)
public class OreConfigMod {
    public static final String MOD_ID = "oreconfig";
    public static final Logger LOGGER = LogUtils.getLogger();

    public static final DeferredRegister<Codec<? extends BiomeModifier>> BIOME_MODIFIER_SERIALIZERS =
            DeferredRegister.create(ForgeRegistries.Keys.BIOME_MODIFIER_SERIALIZERS, MOD_ID);
    public static final DeferredRegister<PlacementModifierType<?>> PLACEMENT_MODIFIERS =
            DeferredRegister.create(Registries.PLACEMENT_MODIFIER_TYPE, MOD_ID);

    public static final RegistryObject<Codec<ScaleOresBiomeModifier>> SCALE_ORES =
            BIOME_MODIFIER_SERIALIZERS.register("scale_ores", () -> Codec.unit(ScaleOresBiomeModifier::new));
    public static final RegistryObject<PlacementModifierType<FrequencyPlacement>> FREQUENCY =
            PLACEMENT_MODIFIERS.register("frequency", () -> () -> FrequencyPlacement.CODEC);

    public OreConfigMod() {
        IEventBus modBus = FMLJavaModLoadingContext.get().getModEventBus();
        BIOME_MODIFIER_SERIALIZERS.register(modBus);
        PLACEMENT_MODIFIERS.register(modBus);

        ModLoadingContext.get().registerConfig(ModConfig.Type.COMMON, OreConfig.SPEC);

        // Rebuild the scaled features from the (possibly edited) config for the next world.
        MinecraftForge.EVENT_BUS.addListener((ServerStoppedEvent event) -> OreScaler.reset());
    }
}
