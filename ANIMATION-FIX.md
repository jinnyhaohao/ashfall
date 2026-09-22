# Hand and weapon animation fix

Build: `Builds/Animated/Ashfall.exe`.

The player now uses an armless body sprite plus two articulated arms and independent gripping hands. Weapon-specific handle pivots are locked to the main hand every animation frame. The arm solver connects shoulder, elbow and wrist; the body leans into the swing. Sword attacks have anticipation, strike and recovery, including reversed combo swings. Bow attacks pull the off-hand back and release. Staff attacks extend the casting arm. Whirlwind rotates the held sword, not just an effect ring. Melee damage is delayed until the strike begins; basic projectiles release during their attack motion.

Dedicated rig tests capture three poses per weapon plus a left-facing sword pose and check that each handle remains attached to the hand. Existing gameplay tests remain available via `-betaTest`.

Verification: all grip/pose and gameplay checks passed. The final local scripted combat run measured 16.68 ms median, 16.96 ms p95 and 17.40 ms maximum frame time. Logs: `../rig-qa-final.log` and `../rig-gameplay-qa.log`.

## Artwork

Built-in image-generation mode produced the modular body/arm/hand sheet at `Assets/Resources/AshfallHeroRig.png`, using `Assets/Resources/AshfallAtlasArcade.png` as the identity/style reference. The original atlas is retained. Parts are sliced and alpha-trimmed as runtime sprites; source PNGs remain intact.

Final prompt: Use the first character in the reference atlas as identity/style reference only. Create a NEW modular animation sprite sheet, exactly 2 columns by 2 rows, four equally sized cells, transparent alpha background, no labels or grid. Keep matching chunky outlined chibi pixel-art style, brown spiky hair, blue armor and red scarf. Top LEFT cell: that same adventurer's full head, scarf, narrow blue torso, belt, two legs and brown boots, front three-quarter view, but absolutely NO ARMS and NO HANDS attached: a modular body for rigging. Shoulders end neatly at the sides of the chest. Top RIGHT cell: one isolated short blue armored upper arm segment, oriented straight down from shoulder at top to elbow at bottom, no hand; simple rounded pixel shape. Bottom LEFT cell: one isolated short blue-bracer forearm segment, straight down from elbow at top to wrist at bottom, no hand. Bottom RIGHT cell: one isolated small closed peach-colored fist with dark outline, gripping an invisible handle. Center each part inside its own cell with transparent padding. Body fills 85 percent height of its cell; separate arm parts also large in their own cells so I can resize in the game. Strong consistent dark purple outline, crisp pixel clusters, limited palette. This is a sprite animation rig sheet not a character lineup. No weapons, no scenery. Actual transparent alpha.
