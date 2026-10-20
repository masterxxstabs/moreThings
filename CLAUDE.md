# Notes for Claude

- Minecraft mods live in their own top-level folders (e.g. `OreConfig-NeoForge-1.20.1/`).
- The user always wants the built `.jar` for mods. After pushing mod changes, wait for the
  "Build Minecraft mods" GitHub Action, `git pull` the jar it commits to `jars/`, and send it to the user.
