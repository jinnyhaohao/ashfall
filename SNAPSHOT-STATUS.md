# Current repository status

This repository contains the complete Unity source for the Ashfall work-in-progress beta. Use Unity **6000.3.24f1** with Windows Build Support and open `Assets/Scenes/Ashfall.unity`. Unity regenerates its cache on first open.

## Weapon identity paths completed

The 2026-10-16 weapon-identity sprint turns forge rank three into a permanent two-path choice for every weapon. Force preserves the existing sword shockwave, bow pierce, and staff split-blast masterworks. Technique adds an 18% faster sword combo, a second angled bow shot at 55% damage, or a focused staff explosion with 55% more reach. Each individual weapon receives flavored path names while retaining its existing elemental or enemy-specific passive.

The blacksmith now compares both behaviors before the final purchase, requires a selection at rank two, locks the chosen path, and keeps large weapon collections inside a readable scroll pane. Saves use schema version 8 and persist the path per weapon. Version 7 rank-three weapons migrate to Force so their established behavior does not change; a one-time `.pre-weapon-identity.bak` is created before an older real save is migrated.

## Expedition choice layer completed

The 2026-10-16 expedition-choice sprint adds one permanent, save-backed risk/reward decision before first entering each dangerous area. Hunter's Mark adds a biome-specific elite for 60 bonus cache gold. Iron Trial gives enemies 20% more health and 15% more damage for two additional biome materials. Glass Oath raises both player and enemy damage by 25% for 35 gold and an Ancient Shard. Choices are shown in a dedicated three-card comparison screen, remain visible in the expedition HUD, affect fresh and restored encounter behavior safely, and pay out through the existing guarded cache loop.

Saves now use schema version 7. Version 6 and older journeys migrate with contracts unchosen, allowing the player to make each decision naturally on the next qualifying route. A one-time `.pre-contracts.bak` is created before an older real save is migrated.

## Adventure polish sprint completed

The 2026-10-15 adventure-polish sprint strengthens combat feel, enemy variety, progression, discovery, and readability without adding a new biome. Attacks and specials now have a short input buffer, heavy impacts apply brief local hit-stop, and enemies use visible poise/stagger. Scorpions gained a telegraphed three-lane poison volley and ice wraiths gained a telegraphed five-lane fan volley; both retain wall-aware damage and projectile behavior.

Rank-three forged weapons now unlock bounded behavioral masterworks: sword combo finishers release a wall-aware shockwave, arrows pierce an additional target, and staff impacts split into two smaller blasts. A twelve-entry bestiary records sightings, kills, elite variants, tactics, and known drops. Death drops a recoverable gold satchel containing 20% of carried gold, capped at 100. Deterministic hidden ash shrines add small guarded discoveries to existing layouts.

Combat and objective readability now has persisted accessibility controls for screen shake, hit flash, damage numbers, and high-contrast warnings. Elite variants use non-color symbols and stronger badge backplates; objective types use distinct silhouettes, ground glyphs, and lock feedback. Saves use schema version 6, migrate older bestiary/event/settings data safely, and retain the existing atomic replacement and recovery behavior.

## Objective, elite, and forge sprint completed

The priority replayability sprint is integrated as of 2026-09-25. The seven dangerous areas now rotate among four reusable, data-driven room objectives: corrupted anchors, three survival waves, a timed relic defense, and an elite hunt. Every objective exposes contextual progress, persists partial/completed state, and unlocks a biome-scaled cache guarded by elite enemies. Version 3 seal saves migrate into the new version 5 objective schema, while current encounter records also persist elite modifiers and objective-target identity.

Five readable elite modifiers reuse the existing enemy roster: Swift, Armored, Volatile, Vampiric, and Warden. Each has an overhead label and tint plus distinct gameplay behavior and reward bonuses. Volatile deaths use a long warning ring before damage; Warden protection uses a visible aura; attack wind-up, wall-aware damage, encounter persistence, and travel restrictions remain intact.

The Ruined Village is now a safe hub with an Ember & Iron blacksmith. Owned weapons can be upgraded through three bounded ranks using existing currency and materials. The forge shows owned/required costs and exact current-to-resulting damage before purchase. Weapon ranks are stored in backward-compatible saves.

## Relic-defense spawn hotfix

The 2026-09-28 playtest exposed an unfair relic-defense start: Wasteland scorpions were created inside the relic's damage radius, so integrity could drain before the player had a reasonable chance to intercept them. Fresh defense waves now enter from walkable outer approach chambers, show a cyan approach warning and `INCOMING` HUD state, and receive a two-second no-drain grace period. Restored in-progress defenses also relocate any legacy objective attackers found on top of the relic, so existing saves recover safely without resetting progress.

The build entry **Ashfall > Build Windows** outputs `Builds/WeaponIdentityCandidate/Ashfall.exe`; **Ashfall > Build Manual Playtest** remains separate. The desktop `Play Ashfall Beta.cmd` launcher targets the fully tested WeaponIdentityCandidate build. The prior `Builds/ExpeditionChoiceCandidate`, `Builds/AdventurePolishCandidate`, `Builds/DefenseFixCandidate`, `Builds/ObjectiveForgeCandidate`, `Builds/ExpeditionIntegration`, and `Builds/DepthAndMotion` builds remain preserved as fallbacks.

## Verification

The exact WeaponIdentityCandidate build passed all executable QA suites:

- `-masteryTest` (path gating and locking, three Technique behaviors, distinct weapon naming, persistence, and version 7 migration)
- `-contractTest` (choice locking, additional elite deployment, bounded enemy/player modifiers, cache bonuses, persistence, and version 6 migration)
- `-adventureTest` (combat buffering, hit-stop, poise, enemy abilities, three masterwork traits, bestiary, hidden shrines, death satchel, settings persistence, version 5 migration, objective cues, and elite symbols)
- `-sprintTest` (all four objectives, perimeter relic spawns, warning grace, fair drain rate, five elite modifiers/behaviors, encounter persistence, forge costs/damage, and version 3 migration)
- `-betaTest`
- `-worldTest`
- `-itemTest`
- `-detailTest`
- `-weaponTest`
- `-rigTest`

Release captures were visually reviewed at 1280x720 for the two-path masterwork comparison, scrollable weapon list, three-card contract comparison, bestiary, accessibility settings, objective silhouettes/glyphs, HUD readability, five symbol-marked elite variants, and the fixed relic encounter. Both mastery descriptions and actions fit without clipping or overlap. Automated runs used the isolated `ashfall-qa.json`. The actively played real `ashfall-beta.json` was preserved and only read for verification; its SHA-256 after all QA remained `9FD99683504DEA534CC40226F29A0885FE162705B5384A30B7D4E4E3DBDB4E61` with its original 2026-09-28 modification time. The older `playtest-save-20260922.json` backup remains untouched.

The spawn bug itself was found through the user's hands-on playtest. Post-fix mouse-and-keyboard combat feel remains outstanding: the Windows computer-control service returned no controllable apps, so native input could not be honestly exercised through automation.

Generated builds, Unity installers, caches, logs, machine settings, and personal saves are excluded from version control. Historical notes and the original README describe older versions and may contain outdated build paths or content counts.
