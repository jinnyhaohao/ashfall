# Initial repository snapshot

This upload preserves the complete current Unity source project, including unfinished combat/expedition work. It is not a verified beta release.

Use Unity **6000.3.24f1** with Windows Build Support. Add this repository in Unity Hub and open `Assets/Scenes/Ashfall.unity`. All sprite assets, source, package manifests, and settings are included. Unity regenerates its cache on first open.

## In-progress integration

`ExpeditionObjectives` calls `AshfallBeta.SaveJourney()`, which has not yet been added and currently prevents compilation. Objective initialization, encounter/objective save persistence, safe-travel UI, enemy attack cues, and regression testing remain unfinished. The new `CombatRules.cs`, `EncounterJournal.cs`, and `ExpeditionObjectives.cs` files are intentionally preserved in this source snapshot.

The previously tested local DepthAndMotion executable predates these changes. After completing integration, use **Ashfall > Build Windows**; the current script outputs `Builds/DepthAndMotion/Ashfall.exe`.

Generated builds, Unity installers, caches, logs, machine settings, and personal saves are excluded from version control. Historical notes in this repository describe earlier iterations; the original README's build path and content counts are outdated. The existing beta has eight areas, twelve enemy types, directional character and weapon animations, and expanded equipment/crafting.
