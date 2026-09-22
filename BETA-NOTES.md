# Ashfall — visual beta 0.2

Launch `Builds/Beta/Ashfall.exe`. This build replaces the placeholder blocks with original dark-fantasy artwork, textured terrain, ruins, vegetation, a pixel-resolution camera, combat trails, damage numbers, and a redesigned interface.

Controls: WASD move; mouse aims; hold left mouse to attack; 1/2/3 sword/bow/staff; Space dodge; E collect; Q potion; C craft; I equipment; M map; H help; Escape pause/close.

Travel is via the map. Mountain opens after 6 kills, Wasteland after 12, Dungeon after 18. Progress saves every 30 seconds and on exit; Continue returns to the village. There are five craft recipes, two ring slots, and a two-phase boss.

Known beta limitations: character animation currently uses sprite motion and mirroring, not full directional animation sheets. Scenery is decorative rather than collision geometry. Mountain and Wasteland reuse stronger variants of the core enemy roster. Weapon upgrades are selected automatically within each weapon category. Audio and controller support are not implemented.

## Art provenance

Original atlas generated with the built-in image-generation tool, then imported with point filtering and rendered through a 640 x 360 game camera. Source asset: `Assets/Resources/AshfallAtlas.png`.

Prompt: Create a production game sprite atlas for Ashfall, grounded realistic dark fantasy pixel art, top down three-quarter RPG camera, crisp deliberate pixel clusters, muted natural colors, textured material shading, no outlines like cartoons. Transparent background. Square canvas divided into exactly 4 columns and 4 rows equal cells, generous transparent padding per cell, no grid lines, no labels, no text. Each object completely contained in its cell. Row 1 left to right: full-body hooded leather-armored human ranger holding steel sword red scarf; crouching realistic gray wolf; skeletal warrior with rusty sword; green goblin with cleaver. Row 2: glossy translucent green slime; huge antlered forest beast brown fur moss on shoulders; black armored hollow knight with violet eyes and huge sword; tall richly detailed evergreen pine. Row 3: ruined roofless stone cottage with broken timber beams; moss-covered stone boulder cluster; campfire with glowing orange flames and logs; ancient stone archway with violet portal. Row 4: red health potion glass bottle; steel longsword; wooden recurve bow; ember crystal staff. Consistent overhead perspective and warm upper-left lighting. Actual transparent alpha, no ground squares or checkerboard. Assets for an actual playable game, not a mockup.
