using UnityEngine;

public enum WeaponMasteryTrait { None, SwordShockwave, BowPierce, StaffSplitBlast }

// Forge rank three changes how a weapon plays instead of only increasing its number.
// Combat code can use these small queries without knowing anything about forge storage.
public static class WeaponMastery
{
    public const int UnlockRank=BlacksmithStation.MaxLevel;

    public static WeaponMasteryTrait Trait(ItemId item,int forgeRank)
    {
        if(forgeRank<UnlockRank)return WeaponMasteryTrait.None;
        int type=ItemCatalog.WeaponType(item);
        return type==0?WeaponMasteryTrait.SwordShockwave:type==1?WeaponMasteryTrait.BowPierce:type==2?WeaponMasteryTrait.StaffSplitBlast:WeaponMasteryTrait.None;
    }

    public static WeaponMasteryTrait Trait(PlayerHero player)
    {
        if(!player||!player.director||!player.director.blacksmith)return WeaponMasteryTrait.None;
        ItemId item=ItemCatalog.Equipped(player);
        return Trait(item,player.director.blacksmith.Level(item));
    }

    public static bool Has(PlayerHero player,WeaponMasteryTrait trait){return Trait(player)==trait;}
    public static bool IsMasterworked(ItemId item,int forgeRank){return Trait(item,forgeRank)!=WeaponMasteryTrait.None;}

    public static string Name(WeaponMasteryTrait trait)
    {
        switch(trait){case WeaponMasteryTrait.SwordShockwave:return "Cinder Wave";case WeaponMasteryTrait.BowPierce:return "Piercing Flight";case WeaponMasteryTrait.StaffSplitBlast:return "Prismatic Burst";default:return "No masterwork trait";}
    }

    public static string Description(WeaponMasteryTrait trait)
    {
        switch(trait){
            case WeaponMasteryTrait.SwordShockwave:return "Combo finishers release a short wall-aware shockwave.";
            case WeaponMasteryTrait.BowPierce:return "Arrows pierce one target, dealing reduced damage after the first hit.";
            case WeaponMasteryTrait.StaffSplitBlast:return "Staff impacts split sideways and make a larger final explosion.";
            default:return "Reach forge rank 3 to awaken this weapon's combat trait.";
        }
    }

    public static string Preview(ItemId item,int currentRank)
    {
        var trait=Trait(item,UnlockRank);
        if(trait==WeaponMasteryTrait.None)return "This item has no forge mastery.";
        if(currentRank>=UnlockRank)return "MASTERWORK / "+Name(trait)+": "+Description(trait);
        return "RANK 3 UNLOCK / "+Name(trait)+": "+Description(trait);
    }

    // Tuned outputs for the combat integration. These remain bounded and derive from
    // the player's already-upgraded damage rather than creating another progression stat.
    public static int SwordShockwaveDamage(PlayerHero player){return Has(player,WeaponMasteryTrait.SwordShockwave)?Mathf.Max(1,Mathf.CeilToInt(player.Damage*.45f)):0;}
    public static float SwordShockwaveRange(PlayerHero player){return Has(player,WeaponMasteryTrait.SwordShockwave)?2.8f:0;}
    public static int BowExtraPierces(PlayerHero player){return Has(player,WeaponMasteryTrait.BowPierce)?1:0;}
    public static float BowDamageAfterPierce(PlayerHero player){return Has(player,WeaponMasteryTrait.BowPierce)?0.72f:0;}
    public static int StaffSplitCount(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?2:0;}
    public static float StaffSplitAngle(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?24f:0;}
    public static float StaffSplitDamage(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?0.55f:0;}
    public static float StaffExplosionScale(PlayerHero player){return Has(player,WeaponMasteryTrait.StaffSplitBlast)?1.35f:1;}
}
