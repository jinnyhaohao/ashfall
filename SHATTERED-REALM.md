# Ashfall beta 0.5 — The Shattered Realm

Eight playable areas: Ruined Village, Forest, Mountain, Wasteland, Dungeon, Sunken Marsh, Crystal Caverns, Ashen Citadel. Authored room-and-trail geometry replaces rectangular movement clamps and terrain. Terrain silhouettes, collision, spawns, local maps and the minimap use the same geometry. Monsters steer around bends. Route gates connect neighboring regions; M opens a world route chart and detailed terrain atlas. Fast travel remains available to unlocked areas.

Unlocks: Forest immediately; Mountain 6 kills; Marsh 8; Wasteland 12; Caverns 16; Dungeon 18; Citadel 30 plus Hollow Knight defeated. Returning to the village restores health. Regions repopulate when entered.

Six new enemies: Stone Golem (ground slam), Cave Bat (fast swoop), Scorpion (charge), Mushroom Shaman (ranged poison-colored spell), Ice Wraith (ranged ice spell), Dark Knight (charge and slam). There are 12 enemy types total. Later regions contain 12–20 enemies, plus the dungeon boss when undefeated.

Six new weapons, all craftable and with unique sprites: Venom Saber (20, poison), Thorn Bow (19, poison), Thunder Axe (26, brief stun), Storm Bow (24, brief stun), Dusk Staff (27, 5% direct-damage healing), Sunsteel Blade (32, +50% against undead/dark knights). There are 14 upgraded weapons and 20 recipes total. Axe uses the melee slot and existing swing/whirlwind actions. New crafting resources are Venom Sac, Storm Crystal and Sunsteel. New monsters can also drop selected weapons directly.

Existing item enum values are preserved; new entries are appended. Existing saves initialize missing materials to zero. Automated tests use a separate QA save and never grant items to the real save.

## Art provenance

The imagegen skill guided reference-based sprite generation, using the built-in image-generation tool (not CLI). Assets are saved in `Assets/Resources/AshfallAtlasMonsters.png` and `Assets/Resources/AshfallWeaponExpansion.png`. Original arcade atlas used only as a style reference.

Monster prompt: Production pixel RPG monster sprite atlas, exactly 3 columns by 2 rows six equal cells. Chunky colorful pixel art, dark purple outlines, simple 2–3 tone shading, cute dangerous monsters. Genuine transparent alpha background. No text, grid, scenery, shadows, detached particles. 15% padding per cell. Full bodies centered, three-quarter front view. Row 1: stone golem with cyan rune core and huge rocky fists; purple cave bat with wide wings and red eyes; golden desert scorpion with raised stinger and pincers. Row 2: green mushroom shaman holding twisted wooden wand; floating cyan ice wraith in torn dark robes; black armored dark knight with red visor and massive axe. Crisp pixel clusters, no realism. Follow-up: remove only background and exterior haze, preserve all six monsters and their positions, genuine transparent alpha.

Weapon prompt: Production pixel RPG weapon atlas on genuine transparent alpha background, exactly 3 columns by 2 rows equal cells. Six upright weapons with tips top grips bottom, consistent scale, 20 percent padding inside cells. Chunky colorful pixel art with dark outlines and simple crisp shading, not realism. Row 1: Venom Saber curved green saber with fang guard; Thunder Axe broad silver double axe with blue lightning rune and brown handle; Sunsteel Blade golden straight sword with orange sun-shaped guard. Row 2: Thorn Bow green wooden recurve bow with leaves, curve left string right; Storm Bow blue silver recurve bow, curve left string right; Dusk Staff long dark staff topped with crescent and violet orb. No text, grid, scenery, shadows, detached effects or glow outside silhouettes. Each object completely isolated in its own cell.

## Tests

`-worldTest`: all eight populated zones, walkable spawns/exits, connected trails, irregular boundaries, new weapon stats and poison/stun/drain effects; terrain and map screenshots.

`-weaponTest`: fourteen unique weapon sprites; held/drop/hotbar identity; attachment; save/load and starter fallback.

`-itemTest`: all twenty recipes, equipment, consumables and legacy save migration. `-betaTest`: boss, loot, actions and combat frame timings. `-rigTest`: directional poses and walk frames.
