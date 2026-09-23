# Current repository status

This repository contains the complete Unity source for the Ashfall work-in-progress beta. Use Unity **6000.3.24f1** with Windows Build Support and open `Assets/Scenes/Ashfall.unity`. Unity regenerates its cache on first open.

## Expedition integration completed

The combat and expedition work is integrated as of 2026-09-23. Melee, projectile, and area damage respect room walls; route travel requires a nearby safe gate; dangerous areas have persistent three-seal objectives with dedicated guards and guarded caches; encounter health, loose loot, boss phase, seals, and claimed rewards use backward-compatible version 3 saves. Saves use atomic replacement, maintain a last-good backup, and recover from corrupt primary data. Cleared dangerous areas receive delayed resource patrols so crafting materials remain renewable. Enemy attacks expose wind-up, strike, and recovery cues, the HUD is compact, and lifesteal uses damage actually dealt rather than requested damage.

The build entry **Ashfall > Build Windows** outputs `Builds/ExpeditionIntegration/Ashfall.exe`; **Ashfall > Build Manual Playtest** produces a separate QA-save build. The production build passed the beta, world, item, detail/navigation, weapon, and rig executable QA suites, including guard deployment and corrupt-save recovery. Visible 1280x720 captures were reviewed for the compact HUD, atlas, and enemy wind-up cue. Native hands-on input remains outstanding because the Windows computer-control service could not enumerate or bind the running game window.

The prior `Builds/DepthAndMotion/Ashfall.exe` remains preserved as a fallback. The desktop `Play Ashfall Beta.cmd` launcher now targets the tested ExpeditionIntegration build. Automated runs use `ashfall-qa.json`; the real `ashfall-beta.json` save was not modified.

Generated builds, Unity installers, caches, logs, machine settings, and personal saves are excluded from version control. Historical notes and the original README describe older versions and may contain outdated build paths or content counts.
