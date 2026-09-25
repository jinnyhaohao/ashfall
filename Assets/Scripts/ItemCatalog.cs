using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ItemCatalog
{
    static Sprite ringSprite;
    public static Sprite LootArt(ItemId i,Sprite[] art){if(IsPotion(i))return art[12];int kind=WeaponType(i);if(kind>=0)return WeaponArt.Get(i,(WeaponKind)kind);if(IsRing(i)){if(!ringSprite){var t=new Texture2D(12,12,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;for(int y=0;y<12;y++)for(int x=0;x<12;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(5.5f,5.5f));t.SetPixel(x,y,r>3&&r<5.5f?new Color(1,.78f,.25f):Color.clear);}t.Apply();ringSprite=Sprite.Create(t,new Rect(0,0,12,12),Vector2.one*.5f,12);}return ringSprite;}return i==ItemId.Gold?CombatFX.Orb:CombatFX.Dot;}
    public static bool IsRing(ItemId i){return i==ItemId.RingFlames||i==ItemId.RingHunter||i==ItemId.RingVampire||i==ItemId.RingHaste||i==ItemId.RingGuard;}
    public static bool IsArmor(ItemId i){return i==ItemId.LeatherArmor||i==ItemId.BoneArmor||i==ItemId.IronArmor;}
    public static bool IsPotion(ItemId i){return i==ItemId.HealthPotion||i==ItemId.StaminaPotion||i==ItemId.FuryPotion;}
    public static int WeaponType(ItemId i){if(i==ItemId.BoneBow||i==ItemId.FrostBow||i==ItemId.ThornBow||i==ItemId.StormBow)return 1;if(i==ItemId.CrystalStaff||i==ItemId.DuskStaff)return 2;if(i==ItemId.VenomSaber||i==ItemId.ThunderAxe||i==ItemId.SunsteelBlade||i==ItemId.GoblinCleaver||i==ItemId.IronSword||i==ItemId.FangSword||i==ItemId.HollowBlade||i==ItemId.EmberSword)return 0;return -1;}
    public static string Name(ItemId i){return Regex.Replace(i.ToString(),"([a-z])([A-Z])","$1 $2");}
    public static ItemId Equipped(PlayerHero p){return p.weapon==WeaponKind.Sword?p.swordSlot:p.weapon==WeaponKind.Bow?p.bowSlot:p.staffSlot;}
    public static string WeaponName(PlayerHero p){var i=Equipped(p);return i!=ItemId.Gold?Name(i):p.weapon==WeaponKind.Sword?"Rusty Sword":p.weapon==WeaponKind.Bow?"Hunter Bow":"Ember Staff";}
    public static int Damage(PlayerHero p){ItemId item=Equipped(p);return BaseDamage(item,p.weapon)+(p.director.blacksmith?p.director.blacksmith.Bonus(item):0);}
    public static int BaseDamage(ItemId item,WeaponKind fallback){switch(item){case ItemId.VenomSaber:return 20;case ItemId.ThunderAxe:return 26;case ItemId.SunsteelBlade:return 32;case ItemId.ThornBow:return 19;case ItemId.StormBow:return 24;case ItemId.DuskStaff:return 27;case ItemId.IronSword:return 12;case ItemId.GoblinCleaver:return 14;case ItemId.FangSword:return 17;case ItemId.HollowBlade:return 23;case ItemId.BoneBow:return 15;case ItemId.EmberSword:return 18;case ItemId.FrostBow:return 17;case ItemId.CrystalStaff:return 20;default:return fallback==WeaponKind.Sword?8:fallback==WeaponKind.Bow?9:11;}}
    public static Color Tint(ItemId i){switch(i){case ItemId.EmberSword:case ItemId.EmberShard:return new Color(1,.55f,.28f);case ItemId.FrostBow:case ItemId.StaminaPotion:case ItemId.Moonleaf:return new Color(.45f,.95f,1);case ItemId.CrystalStaff:case ItemId.AncientShard:case ItemId.FuryPotion:return new Color(.8f,.55f,1);case ItemId.Gold:return new Color(1,.85f,.3f);default:return Color.white;}}
    public static string Description(ItemId i){switch(i){
case ItemId.VenomSaber:return "20 damage / poison: 3 damage every 0.6s for 4s";
case ItemId.ThunderAxe:return "26 damage / briefly stuns enemies on hit";
case ItemId.SunsteelBlade:return "32 damage / +50% against undead and dark knights";
case ItemId.ThornBow:return "19 damage / poison arrows for 4s";
case ItemId.StormBow:return "24 damage / arrows briefly stun enemies";
case ItemId.DuskStaff:return "27 magic damage / heals 5% of direct damage";
case ItemId.VenomSac:return "Scorpions and mushroom shamans / poison weapons";
case ItemId.StormCrystal:return "Bats and ice wraiths / storm weapons";
case ItemId.Sunsteel:return "Dark knights / forge Sunsteel Blade";
        case ItemId.EmberSword:return "18 damage / burns for 2 damage every 0.6s for 3s";
        case ItemId.FrostBow:return "17 damage / slows enemies by 45% for 2 seconds";
        case ItemId.CrystalStaff:return "20 magic damage / area-damage projectiles";
        case ItemId.BoneArmor:return "+40 max HP / 8% damage reduction";
        case ItemId.IronArmor:return "+60 max HP / 15% damage reduction";
        case ItemId.LeatherArmor:return "+25 maximum HP";
        case ItemId.StaminaPotion:return "Use: restore 60 stamina / hotkey R";
        case ItemId.FuryPotion:return "Use: +30% damage for 30 seconds / hotkey F";
        case ItemId.HealthPotion:return "Use: restore 45 HP / hotkey Q";
        case ItemId.RingHaste:return "+15% movement speed / +5 stamina regeneration";
        case ItemId.RingGuard:return "15% less incoming damage";
        case ItemId.RingFlames:return "+15% staff projectile damage";
        case ItemId.RingHunter:return "+10% damage against wolves and forest beasts";
        case ItemId.RingVampire:return "Heal for 3% of damage dealt";
        case ItemId.FangSword:return "17 damage / 15% chance to critically hit";
        case ItemId.HollowBlade:return "23 damage / +40% damage against undead";
        case ItemId.IronSword:return "12 melee damage";
        case ItemId.GoblinCleaver:return "14 melee damage / rare goblin drop";
        case ItemId.BoneBow:return "15 ranged damage";
        case ItemId.EmberShard:return "Goblins drop this / forge Ember Sword";
        case ItemId.AncientShard:return "Skeletons and forest beasts / advanced equipment";
        case ItemId.Moonleaf:return "Wolves and slimes / potions and Frost Bow";
        default:return "Crafting material / see recipes with C";
    }}
    public static void Recipes(AshfallDirector d){
        d.recipes.Add(new Recipe(ItemId.EmberSword,1,"Ember Sword / burn",ItemId.EmberShard,6,ItemId.IronOre,8,ItemId.Gold,120));
        d.recipes.Add(new Recipe(ItemId.FrostBow,1,"Frost Bow / slow",ItemId.Moonleaf,6,ItemId.Bone,8,ItemId.Gold,100));
        d.recipes.Add(new Recipe(ItemId.CrystalStaff,1,"Crystal Staff / 20 damage",ItemId.AncientShard,5,ItemId.EmberShard,3,ItemId.Gold,160));
        d.recipes.Add(new Recipe(ItemId.BoneArmor,1,"Bone Armor / +40 HP, 8% defense",ItemId.Bone,14,ItemId.WolfHide,6,ItemId.Gold,80));
        d.recipes.Add(new Recipe(ItemId.IronArmor,1,"Iron Armor / +60 HP, 15% defense",ItemId.IronOre,16,ItemId.RustedMetal,8,ItemId.Gold,140));
        d.recipes.Add(new Recipe(ItemId.StaminaPotion,2,"Stamina Potions x2",ItemId.Moonleaf,2,ItemId.SlimeGel,1));
        d.recipes.Add(new Recipe(ItemId.FuryPotion,1,"Fury Potion / 30 second boost",ItemId.EmberShard,2,ItemId.WolfFang,2,ItemId.RedHerb,1));
        d.recipes.Add(new Recipe(ItemId.RingHaste,1,"Ring of Haste",ItemId.WolfFang,6,ItemId.Moonleaf,4,ItemId.Gold,90));
        d.recipes.Add(new Recipe(ItemId.RingGuard,1,"Ring of Guarding",ItemId.AncientShard,3,ItemId.RustedMetal,6,ItemId.Gold,100));
    }
    public static bool Use(PlayerHero p,ItemId i){var d=p.director;if(!d.Has(i,1))return false;if(i==ItemId.HealthPotion){if(p.hp>=p.MaxHp)return false;p.hp=Mathf.Min(p.MaxHp,p.hp+45);}else if(i==ItemId.StaminaPotion){if(p.stamina>=p.MaxStamina)return false;p.stamina=Mathf.Min(p.MaxStamina,p.stamina+60);}else if(i==ItemId.FuryPotion)p.furyUntil=Time.time+30;else return false;d.bag[i]--;d.Tell(Name(i)+" used.");CombatFX.Ring(p.transform.position,.8f,Tint(i));return true;}
    public static void Equip(PlayerHero p,ItemId i,int ringSlot){if(!p.director.Has(i,1))return;if(IsPotion(i)){Use(p,i);return;}if(IsArmor(i)){p.armorSlot=i;p.leatherArmor=i==ItemId.LeatherArmor;p.hp=Mathf.Min(p.hp,p.MaxHp);}else if(IsRing(i)){var other=ringSlot==0?p.ringB:p.ringA;if(other==i&&p.director.bag[i]<2){p.director.Tell("You need another copy for both ring slots.");return;}if(ringSlot==0)p.ringA=i;else p.ringB=i;}else if(WeaponType(i)>=0)p.EquipItem(i);else return;p.director.Tell(Name(i)+" equipped.");}
    public static void MigrateGear(PlayerHero p){p.swordSlot=p.bowSlot=p.staffSlot=p.armorSlot=ItemId.Gold;ItemId[] swords={ItemId.IronSword,ItemId.GoblinCleaver,ItemId.FangSword,ItemId.EmberSword,ItemId.HollowBlade};foreach(var i in swords)if(p.director.Has(i,1))p.swordSlot=i;if(p.director.Has(ItemId.BoneBow,1))p.bowSlot=ItemId.BoneBow;if(p.leatherArmor)p.armorSlot=ItemId.LeatherArmor;}
}
