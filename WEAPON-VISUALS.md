# Weapon visuals update

Eight unique weapon sprites now share one item-based lookup for the character's held weapon, dropped loot, inventory preview, and bottom weapon bar. The bar shows the equipped item name and active category. Starter weapons retain their original artwork. Existing saves and equipment are preserved.

## Artwork

Created with the imagegen skill using the built-in image generation tool, reference-guided generation mode. Saved asset: `Assets/Resources/AshfallWeaponSkins.png`.

Prompt: Production weapon sprite atlas for Ashfall. Use reference ONLY for chunky colorful outlined pixel-art style. Make exactly 4 columns by 2 rows, eight equal cells on a wide canvas. Actual transparent alpha background, no text or grid lines. All objects fully inside their cells with ample 15 percent padding, no crossing cell boundaries. Upright weapons, tips at top, grips at bottom, no diagonal rotation, consistent scale. Row 1 left to right: Iron Sword, simple straight silver blade and plain steel crossguard with brown wrapped handle; Fang Sword, curved ivory bone blade with two fang barbs and leather handle; Goblin Cleaver, broad chipped greenish steel rectangular cleaver with dark brown handle; Hollow Blade, unmistakably legendary black-violet jagged sword with a glowing violet rune channel, skull-shaped crossguard and dark purple wrapped handle. Row 2 left to right: Ember Sword, orange-red glowing blade with angular flame-shaped tip and dark bronze crossguard; Bone Bow, ivory bone recurve bow with skull ornament, bow curve on left and string on right; Frost Bow, icy cyan crystalline recurve bow, curve on left and string on right; Crystal Staff, slim dark shaft with large purple diamond crystal at top. Swords' handles aligned to horizontal center and lowest 20% of each sprite. Simple clean crisp pixel clusters, thick dark purple outlines, 2 or 3 shades per color, readable small game sprites matching reference. No ground shadows, no particles detached from weapons, no photorealism. Eight distinct silhouettes, not recolors.

## Verification

`-weaponTest` checks eight distinct skins, held/hotbar/drop consistency, grip attachment, save/load icon restoration, and starter fallback. It writes screenshots per weapon and uses only the isolated QA save. Existing `-itemTest`, `-rigTest`, and `-betaTest` cover crafting, equipment, directional animation, gameplay and combat performance.
