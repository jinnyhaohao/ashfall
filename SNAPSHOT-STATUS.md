# Current repository status

This repository contains the complete Unity source for the Ashfall work-in-progress beta. Use Unity **6000.3.24f1** with Windows Build Support and open `Assets/Scenes/Ashfall.unity`. Unity regenerates its cache on first open.

## Objective, elite, and forge sprint completed

The priority replayability sprint is integrated as of 2026-09-25. The seven dangerous areas now rotate among four reusable, data-driven room objectives: corrupted anchors, three survival waves, a timed relic defense, and an elite hunt. Every objective exposes contextual progress, persists partial/completed state, and unlocks a biome-scaled cache guarded by elite enemies. Version 3 seal saves migrate into the new version 5 objective schema, while current encounter records also persist elite modifiers and objective-target identity.

Five readable elite modifiers reuse the existing enemy roster: Swift, Armored, Volatile, Vampiric, and Warden. Each has an overhead label and tint plus distinct gameplay behavior and reward bonuses. Volatile deaths use a long warning ring before damage; Warden protection uses a visible aura; attack wind-up, wall-aware damage, encounter persistence, and travel restrictions remain intact.

The Ruined Village is now a safe hub with an Ember & Iron blacksmith. Owned weapons can be upgraded through three bounded ranks using existing currency and materials. The forge shows owned/required costs and exact current-to-resulting damage before purchase. Weapon ranks are stored in backward-compatible saves.

## Relic-defense spawn hotfix

The 2026-09-28 playtest exposed an unfair relic-defense start: Wasteland scorpions were created inside the relic's damage radius, so integrity could drain before the player had a reasonable chance to intercept them. Fresh defense waves now enter from walkable outer approach chambers, show a cyan approach warning and `INCOMING` HUD state, and receive a two-second no-drain grace period. Restored in-progress defenses also relocate any legacy objective attackers found on top of the relic, so existing saves recover safely without resetting progress.

The build entry **Ashfall > Build Windows** outputs `Builds/DefenseFixCandidate/Ashfall.exe`; **Ashfall > Build Manual Playtest** remains separate. The desktop `Play Ashfall Beta.cmd` launcher targets the fully tested DefenseFixCandidate build. The prior `Builds/ObjectiveForgeCandidate`, `Builds/ExpeditionIntegration`, and `Builds/DepthAndMotion` builds remain preserved as fallbacks.

## Verification

The exact DefenseFixCandidate build passed all executable QA suites:

- `-sprintTest` (all four objectives, perimeter relic spawns, warning grace, fair drain rate, five elite modifiers/behaviors, encounter persistence, forge costs/damage, version 3 migration)
- `-betaTest`
- `-worldTest`
- `-itemTest`
- `-detailTest`
- `-weaponTest`
- `-rigTest`

Release captures were visually reviewed at 1280x720 for objective/HUD feedback, the five labeled/tinted elite variants, the blacksmith cost/damage preview, and the fixed relic encounter. The relic capture shows attackers staged in outer rooms, the relic clear, a cyan warning ring, and `Relic 100% / INCOMING`. Automated runs used the isolated `ashfall-qa.json`. The actively played real `ashfall-beta.json` was preserved and only read for verification; its post-playtest SHA-256 is `A49FE255A5A00C80357E4642BBDA0FB0996F7F2E1EB7D090258944020CE3762F`. The older `playtest-save-20260922.json` backup remains untouched.

The spawn bug itself was found through the user's hands-on playtest. Post-fix mouse-and-keyboard combat feel remains outstanding: the Windows computer-control service returned no controllable apps, so native input could not be honestly exercised through automation.

Generated builds, Unity installers, caches, logs, machine settings, and personal saves are excluded from version control. Historical notes and the original README describe older versions and may contain outdated build paths or content counts.
