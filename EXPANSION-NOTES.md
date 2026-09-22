# Ashfall 0.4 — Forge & Fury

Launch `Builds/Expanded/Ashfall.exe`, or the updated desktop launcher.

Twelve additions bring the inventory to 30 item types and crafting to 14 recipes:

- Ember Shard (goblins), Ancient Shard (skeletons/forest beasts), Moonleaf (wolves/slimes).
- Ember Sword: 18 damage, burning hits. Frost Bow: 17 damage, 45% slow. Crystal Staff: 20 damage, splash projectiles.
- Bone Armor: +40 HP and 8% damage reduction. Iron Armor: +60 HP and 15% reduction.
- Stamina Potion: restores 60 stamina (R). Fury Potion: +30% damage for 30 seconds (F).
- Ring of Haste: +15% movement speed, +5 stamina regeneration. Ring of Guarding: 15% damage reduction.

New equipment is craftable using the enemy materials above. Open C for recipes and scroll down. Open I, select an item, then click Equip or Use. Each weapon category remembers its selected item; owning stronger gear no longer silently overrides your selection. Ring effects occupy the existing two slots. Damage reduction from armor and the guarding ring stacks multiplicatively.

The inventory now has gear/material/potion filters, descriptions and explicit equipment actions. Bow animation has a moving string and nocked arrow. New weapons reuse existing silhouettes with elemental tinting, not unique sprite sheets.

Older saves load with zero quantities for new items and migrate existing equipment. Before loading a pre-expansion save, the game preserves a `.pre-expansion.bak` copy beside it. New equipment selections persist in saves. Temporary Fury buffs do not persist across restarting the game.

Other controls remain unchanged: WASD move; mouse aim; left mouse attacks; right mouse skill; Space dash; 1/2/3 weapons; Q health potion; M map; Escape pause.

Verification: all 14 crafting recipes, explicit equipment selection, burn/slow application, armor defense, potion use, expanded save/load, old-save migration and new material drops passed automated checks. Directional rig checks and existing boss/combat checks also passed. Logs: `../expanded-itemTest-final.log`, `../expanded-rigTest-final.log`, `../expanded-betaTest-final.log`.
