using UnityEngine;

public enum MasterworkPath { None, Force, Technique }
public enum WeaponMasteryTrait { None, SwordShockwave, SwordTempo, BowPierce, BowTwinShot, StaffSplitBlast, StaffGreaterNova }

// Rank three is a permanent play-style choice. Force preserves the original
// masterworks while Technique offers a bounded alternative for every weapon family.
public static class WeaponMastery
{
    public const int UnlockRank=BlacksmithStation.MaxLevel;

    public static WeaponMasteryTrait Trait(ItemId item,int forgeRank,MasterworkPath path)
    {
        if(forgeRank<UnlockRank)return WeaponMasteryTrait.None;
        if(path==MasterworkPath.None)path=MasterworkPath.Force; // Version 7 and older.
        int type=ItemCatalog.WeaponType(item);
        if(type==0)return path==MasterworkPath.Technique?WeaponMasteryTrait.SwordTempo:WeaponMasteryTrait.SwordShockwave;
        if(type==1)return path==MasterworkPath.Technique?WeaponMasteryTrait.BowTwinShot:WeaponMasteryTrait.BowPierce;
        if(type==2)return path==MasterworkPath.Technique?WeaponMasteryTrait.StaffGreaterNova:WeaponMasteryTrait.StaffSplitBlast;
        return WeaponMasteryTrait.None;
    }

    public static WeaponMasteryTrait Trait(ItemId item,int forgeRank){return Trait(item,forgeRank,MasterworkPath.Force);}
    public static WeaponMasteryTrait Trait(PlayerHero player)
    {
        if(!player||!player.director||!player.director.blacksmith)return WeaponMasteryTrait.None;
        ItemId item=ItemCatalog.Equipped(player);var forge=player.director.blacksmith;
        return Trait(item,forge.Level(item),forge.Path(item));
    }

    public static bool Has(PlayerHero player,WeaponMasteryTrait trait){return Trait(player)==trait;}
    public static bool IsMasterworked(ItemId item,int forgeRank){return forgeRank>=UnlockRank&&ItemCatalog.WeaponType(item)>=0;}

    static string Signature(ItemId item)
    {
        switch(item){
            case ItemId.IronSword:return "Iron";case ItemId.GoblinCleaver:return "Goblin";case ItemId.FangSword:return "Fang";case ItemId.HollowBlade:return "Hollow";
            case ItemId.EmberSword:return "Ember";case ItemId.VenomSaber:return "Venom";case ItemId.ThunderAxe:return "Thunder";case ItemId.SunsteelBlade:return "Sunsteel";
            case ItemId.BoneBow:return "Bone";case ItemId.FrostBow:return "Frost";case ItemId.ThornBow:return "Thorn";case ItemId.StormBow:return "Storm";
            case ItemId.CrystalStaff:return "Crystal";case ItemId.DuskStaff:return "Dusk";default:return "Forged";
        }
    }

    public static string PathName(ItemId item,MasterworkPath path)
    {
        int type=ItemCatalog.WeaponType(item);string suffix=type==0?(path==MasterworkPath.Technique?"Tempo":"Wave"):type==1?(path==MasterworkPath.Technique?"Volley":"Piercer"):(path==MasterworkPath.Technique?"Nova":"Burst");
        return Signature(item)+" "+suffix;
    }

    public static string Name(WeaponMasteryTrait trait)
    {
        switch(trait){case WeaponMasteryTrait.SwordShockwave:return "Cinder Wave";case WeaponMasteryTrait.SwordTempo:return "Duelist Tempo";case WeaponMasteryTrait.BowPierce:return "Piercing Flight";case WeaponMasteryTrait.BowTwinShot:return "Twin Flight";case WeaponMasteryTrait.StaffSplitBlast:return "Prismatic Burst";case WeaponMasteryTrait.StaffGreaterNova:return "Greater Nova";default:return "No masterwork trait";}
    }

    public static string Description(WeaponMasteryTrait trait)
    {
        switch(trait){
            case WeaponMasteryTrait.SwordShockwave:return "Combo finishers release a short wall-aware shockwave.";
            case WeaponMasteryTrait.SwordTempo:return "Basic sword attacks recover 18% faster for a flowing combo.";
            case WeaponMasteryTrait.BowPierce:return "Arrows pierce one target, dealing reduced damage after the first hit.";
            case WeaponMasteryTrait.BowTwinShot:return "Basic shots loose a second angled arrow for 55% damage.";
            case WeaponMasteryTrait.StaffSplitBlast:return "Staff impacts split sideways and make a larger final explosion.";
            case WeaponMasteryTrait.StaffGreaterNova:return "Staff impacts create one focused explosion with 55% more reach.";
            default:return "Reach forge rank 3 to awaken this weapon's combat trait.";
        }
    }

    public static string Preview(ItemId item,int rank,MasterworkPath path)
    {
        var chosen=path==MasterworkPath.None?MasterworkPath.Force:path;
        return (rank>=UnlockRank?"MASTERWORK / ":"RANK 3 / ")+PathName(item,chosen)+": "+Description(Trait(item,UnlockRank,chosen));
    }

    public static int SwordShockwaveDamage(PlayerHero player){return Has(player,WeaponMasteryTrait.SwordShockwave)?Mathf.Max(1,Mathf.CeilToInt(player.Damage*.45f)):0;}
    public static float SwordShockwaveRange(PlayerHero player){return Has(player,WeaponMasteryTrait.SwordShockwave)?2.8f:0;}
    public static float AttackCooldownMultiplier(PlayerHero player){return Has(player,WeaponMasteryTrait.SwordTempo)?.82f:1;}
    public static int BowExtraPierces(PlayerHero player){return Has(player,WeaponMasteryTrait.BowPierce)?1:0;}
    public static float BowDamageAfterPierce(PlayerHero player){return Has(player,WeaponMasteryTrait.BowPierce)?.72f:0;}
    public static float BowTwinShotMultiplier(PlayerHero player){return Has(player,WeaponMasteryTrait.BowTwinShot)?.55f:0;}
    public static int StaffSplitCount(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?2:0;}
    public static float StaffSplitAngle(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?24f:0;}
    public static float StaffSplitDamage(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?.55f:0;}
    public static float StaffExplosionScale(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffGreaterNova)?1.55f:Has(player,WeaponMasteryTrait.StaffSplitBlast)?1.35f:1;}
}
