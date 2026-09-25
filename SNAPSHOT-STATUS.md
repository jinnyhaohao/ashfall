# Current repository status

This repository contains the complete Unity source for the Ashfall work-in-progress beta. Use Unity **6000.3.24f1** with Windows Build Support and open `Assets/Scenes/Ashfall.unity`. Unity regenerates its cache on first open.

## Objective, elite, and forge sprint completed

The priority replayability sprint is integrated as of 2026-09-25. The seven dangerous areas now rotate among four reusable, data-driven room objectives: corrupted anchors, three survival waves, a timed relic defense, and an elite hunt. Every objective exposes contextual progress, persists partial/completed state, and unlocks a biome-scaled cache guarded by elite enemies. Version 3 seal saves migrate into the new version 5 objective schema, while current encounter records also persist elite modifiers and objective-target identity.

Five readable elite modifiers reuse the existing enemy roster: Swift, Armored, Volatile, Vampiric, and Warden. Each has an overhead label and tint plus distinct gameplay behavior and reward bonuses. Volatile deaths use a long warning ring before damage; Warden protection uses a visible aura; attack wind-up, wall-aware damage, encounter persistence, and travel restrictions remain intact.

The Ruined Village is now a safe hub with an Ember & Iron blacksmith. Owned weapons can be upgraded through three bounded ranks using existing currency and materials. The forge shows owned/required costs and exact current-to-resulting damage before purchase. Weapon ranks are stored in backward-compatible saves.

The build entry **Ashfall > Build Windows** outputs `Builds/ObjectiveForgeCandidate/Ashfall.exe`; **Ashfall > Build Manual Playtest** remains separate. The desktop `Play Ashfall Beta.cmd` launcher now targets the fully tested ObjectiveForgeCandidate build. The prior `Builds/ExpeditionIntegration` and `Builds/DepthAndMotion` builds remain preserved as fallbacks.

## Verification

The exact ObjectiveForgeCandidate build passed all executable QA suites:

- `-sprintTest` (all four objectives, five elite modifiers/behaviors, encounter persistence, forge costs/damage, version 3 migration)
- `-betaTest`
- `-worldTest`
- `-itemTest`
- `-detailTest`
- `-weaponTest`
- `-rigTest`

Release captures were visually reviewed at 1280x720 for objective/HUD feedback, the five labeled/tinted elite variants, and the blacksmith cost/damage preview. Automated runs used the isolated `ashfall-qa.json`. The real `ashfall-beta.json` remained byte-for-byte identical to `playtest-save-20260922.json` (SHA-256 `4A85B317702741476564576E44581EF0D80E397AD27675C0C108EFF41343D81C`).

True mouse-and-keyboard hands-on gameplay remains outstanding. The Windows computer-control service again returned no controllable apps, so native input feel could not be honestly verified through automation.

Generated builds, Unity installers, caches, logs, machine settings, and personal saves are excluded from version control. Historical notes and the original README describe older versions and may contain outdated build paths or content counts.
