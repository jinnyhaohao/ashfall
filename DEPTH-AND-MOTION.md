# Ashfall beta 0.6 — Depth & Motion

## Layout and movement

Replaces circular-room chains and diagonal strips with authored, branching chamber layouts and straight four-unit-wide passages. The village is a crossroads, Mountain a climbing route, Dungeon branching chambers, Citadel twin approaches; other biomes include side chambers and alternate routes. The overall levels remain nonrectangular.

Rendering and collision share the same union of floor tiles. A small foot footprint handles corners, movement is subdivided into steps no longer than 0.12 units, and blocked motion slides along walls. Invalid starting positions recover to a nearby valid floor tile. Enemies use a cached tile-distance field when direct movement is blocked, rather than assuming every area is a ring.

## Presentation and interaction

Raised, shaded wall faces and caps provide 2.5D depth. Nearby foreground walls fade to keep the player visible. New floor patterns, actor contact shadows, softly pulsing lamps, and sixteen biome-specific scenery sprites add detail. Large trees and rocks stay beyond corridor boundaries. Plants and banners sway subtly.

Press E near highlighted herbs, ore, mushrooms or crystals to gather two resources. Nodes have a 90-second in-session cooldown. Route gates take priority over gathering. Gathered inventory is saved by the existing save system; node cooldowns reset on restarting the game.

Hero: eight locomotion frames per facing, 32 total, retaining the articulated weapon-hand rig. All twelve enemy types: four locomotion frames each, 48 total, plus existing impact flashes, attack telegraphs and motion. These are locomotion sheets, not a claim of separate four-direction attack sheets for every monster.

## Design references

- Soul Knight's connected-room structure: https://soul-knight.fandom.com/wiki/Levels
- Core Keeper developer overview, exploration and gathering: https://news.xbox.com/en-us/2024/07/24/core-keeper-coming-day-one-to-xbox-game-pass-august-27/

Original Ashfall art is retained and extended; no game assets were copied from these references. The imagegen skill guided the new raster assets. Built-in image generation was used, not CLI.

## Saved art and prompts

- `Assets/Resources/AshfallHeroFrames.png`: exactly eight columns/four rows; same brown-haired, blue-tunic, red-scarf hero without arms/hands or weapons; front, back, right and left rows; eight distinct contact/down/passing/up walk phases per row; consistent identity, scale and baseline, transparent alpha, crisp chunky pixel clusters. Reference: existing hero directional atlas.
- `Assets/Resources/AshfallAtlasMobFramesA.png`: four columns/six rows; green slime, blue-gray wolf, skeleton, goblin, horned forest beast, purple Hollow Knight; four distinct locomotion poses each, same facing and scale, transparent background, dark outlines, 3-tone pixel shading. Reference: existing arcade atlas.
- `Assets/Resources/AshfallAtlasMobFramesB.png`: four columns/six rows; stone golem, purple bat, gold scorpion, mushroom shaman, ice wraith, dark knight; alternating foot/fist steps, bat wing flaps, stinger sway, staff motion, floating robes and marching poses; transparent alpha, detailed chunky pixel style, no haze or scenery. Reference: existing monster atlas.
- `Assets/Resources/AshfallAtlasDetails.png`: four columns/four rows, isolated top-down 2.5D pixel props; mossy stump, glowing mushrooms, flowering fern, iron ore boulder; blue crystal, frosty grass, gravestone, broken column; crate, anvil, brazier, bones; reeds, lily pads, cactus, crimson banner. Visible top/front faces, dark-purple outlines, three-tone shading, transparent background, no text/grid. Reference: existing arcade atlas.

## Verification

`-detailTest` checks route walking/dashing in both directions, navigation to each chamber, floor/render agreement, invalid-position recovery, thousands of boundary sweeps, harvesting/cooldown, 32 hero frame assets and 48 monster frame playback checks. It captures eight monster pose screenshots. `-worldTest` captures every region and both atlas views. `-rigTest` checks the articulated grip, facing, depth and all 32 hero walk frames. Existing crafting/save and combat tests remain available. All test modes use the isolated QA save.
