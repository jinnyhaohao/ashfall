# Ashfall 0.3 — arcade combat pass

Launch `Builds/Arcade/Ashfall.exe` or the desktop Play Ashfall Beta launcher.

## Changes

- Colorful, outlined, chibi pixel sprites replace the realistic atlas. Simpler terrain palette.
- Native-resolution rendering of point-filtered sprites, late-update camera smoothing, display-synced frame pacing, and Direct3D 11.
- Shared effect materials, 256 pooled particles, and no enemy-list copies in projectile frame loops.
- Hold left mouse for a three-hit sword combo; the third strike is wider and stronger. Attacks face the cursor. Visible held weapons sweep through attacks.
- Right mouse: sword whirlwind, bow five-arrow fan, or staff ember nova. Costs 35 stamina; 2.4-second cooldown.
- Space dashes in the movement direction (toward the cursor when stationary), with afterimages and invulnerability. Costs 24 stamina.
- Knockback, impact flashes, sparks, attack sounds, pickup sounds, and automatic nearby loot pickup.
- Wolves and the boss show a warning before lunging. Enemies separate instead of completely stacking.

Still early beta: sprite motion is tweened rather than full directional animation sheets; scenery collision and enemy variety remain limited. The final local 600-frame scripted combat run measured median 16.67 ms, 95th percentile 16.99 ms, and maximum 17.80 ms. Recorded in `../arcade-qa-final.log`; these figures describe this test, not all possible gameplay. Crafting, save/load, boss summons and rewards, and all three weapon skills passed their automated checks.

## Generated artwork

Built-in image-generation tool used. Project asset: `Assets/Resources/AshfallAtlasArcade.png`. Previous atlas retained for reference.

Final prompt: Use case: stylized-concept. Production sprite atlas for a colorful top-down pixel action RPG inspired by the chunky readability of Soul Knight and Terraria, NOT realistic. Exactly 4 by 4 equal cells on a square transparent canvas. Each sprite centered completely inside its cell with 15 percent clear padding. Bold dark purple outlines, tiny chibi proportions, large heads, expressive eyes, bright limited palette, flat two-tone shading, extremely simple chunky pixel clusters as if every cell is only 32x32 pixels enlarged with nearest neighbor. No painterly detail, no gradients, no text, no grid. Row1: cute red-scarf blue-armored adventurer WITHOUT held weapon; small blue-gray wolf; cute skeleton warrior; green goblin. Row2: lime slime; chunky horned brown forest beast; purple armored evil knight; round leafy emerald tree. Row3: small ruined stone cottage; purple-gray rock cluster; orange campfire; purple portal stone arch. Row4: red potion; broad silver sword with blue hilt pointing up; golden wooden bow; fire gem magic staff. All sixteen isolated sprites actual transparent alpha background, consistent top-down three-quarter view, crisp pixel stair-step edges. Game assets not a mockup.
