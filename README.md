# Ashfall

> Current upload is a work-in-progress source snapshot. See [SNAPSHOT-STATUS.md](SNAPSHOT-STATUS.md) for setup, known compilation blocker, and current build path. The notes below describe the original MVP.

Top-down pixel-action RPG MVP built in Unity 6.3 LTS.

Open `Assets/Scenes/Ashfall.unity` in Unity and press Play. The project generates its world and pixel art at runtime, so it has no external asset dependencies. To create a Windows build, use **Ashfall → Build Windows**; the executable is written to `Builds/Windows/Ashfall.exe`.

Controls: `WASD` move, left-click attack, `1/2/3` sword/bow/staff, `Space` dodge, `E` collect loot, `Q` potion, `C` craft, `I` equipment, `Tab` travel, `H` help.

The core loop is fully playable: combat, enemy-specific loot, a 5% Goblin Cleaver roll, crafting, four equipment slots, build-altering rings, four connected zones, gating by kills, and a two-phase Hollow Knight boss.
