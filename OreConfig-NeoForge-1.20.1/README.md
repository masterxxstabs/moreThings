# Ore Config (NeoForge 1.20.1)

Set how often ores generate (**frequency**) and how big their veins are (**density**).
Works on vanilla ores and on modded ores that use the standard ore feature.

## Build

Requires JDK 17.

```
./gradlew build
```

The jar is written to `build/libs/oreconfig-1.0.0.jar`. Put it in your `mods` folder.

## Configure

Start the game once, then edit `config/oreconfig-common.toml`:

```toml
[general]
    enabled = true
    # Veins per chunk for all ores. 1.0 = vanilla, 2.0 = twice as many, 0 = none
    globalFrequency = 1.0
    # Blocks per vein for all ores. 1.0 = vanilla, 2.0 = twice as big
    globalDensity = 1.0
    # Also apply the global values to dirt/gravel/granite/etc. blobs
    globalAffectsNonOres = false

[ores]
    # "<block or #tag>;<frequency>;<density>", multiplied with the global values
    overrides = ["#forge:ores/diamond;2.0;1.5", "minecraft:emerald_ore;0;1", ...]
```

- Tags (`#forge:ores/iron`) cover stone, deepslate and modded variants. Plain block ids work too.
- Changes affect **newly generated chunks** only, and apply the next time a world is loaded.
