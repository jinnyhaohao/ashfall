using System;
using System.Text.RegularExpressions;
using UnityEngine;

// Compact discovery journal. Sightings are one-bit discoveries, elite sightings are a
// modifier bit mask, and kill totals are capped before load/save to reject bad data.
public class BestiaryJournal : MonoBehaviour
{
    public const int MaxKills=999999;
    public static BestiaryJournal I {get;private set;}
    public int[] sightings {get;private set;}
    public int[] kills {get;private set;}
    public int[] eliteSightings {get;private set;}
    AshfallDirector d;float scanAt;

    public static BestiaryJournal Ensure(AshfallDirector director)
    {
        if(I){if(!I.d)I.d=director;return I;}
        var journal=director.GetComponent<BestiaryJournal>();
        if(!journal)journal=director.gameObject.AddComponent<BestiaryJournal>();
        journal.Initialize(director);return journal;
    }

    public void Initialize(AshfallDirector director)
    {
        I=this;d=director;int count=Enum.GetValues(typeof(EnemyKind)).Length;
        if(sightings==null||sightings.Length!=count)sightings=new int[count];
        if(kills==null||kills.Length!=count)kills=new int[count];
        if(eliteSightings==null||eliteSightings.Length!=count)eliteSightings=new int[count];
    }

    void Update()
    {
        if(!d||!d.player||AshfallBeta.Paused||Time.unscaledTime<scanAt)return;scanAt=Time.unscaledTime+.2f;
        foreach(var enemy in d.enemies)if(enemy&&Vector2.Distance(d.player.transform.position,enemy.transform.position)<12)RegisterSighting(enemy.kind,enemy.modifier);
    }

    public void RegisterSighting(EnemyKind kind,EliteModifier modifier=EliteModifier.None)
    {
        int i=(int)kind;if(!Valid(i))return;sightings[i]=1;
        if(modifier!=EliteModifier.None)eliteSightings[i]|=1<<Mathf.Clamp((int)modifier,1,30);
    }

    // Call once from the enemy-death path before the actor is destroyed.
    public void RegisterKill(EnemyKind kind,EliteModifier modifier=EliteModifier.None)
    {
        int i=(int)kind;if(!Valid(i))return;RegisterSighting(kind,modifier);kills[i]=Mathf.Min(MaxKills,kills[i]+1);
    }

    public bool Discovered(EnemyKind kind){int i=(int)kind;return Valid(i)&&sightings[i]!=0;}
    public int KillCount(EnemyKind kind){int i=(int)kind;return Valid(i)?kills[i]:0;}
    public int EliteMask(EnemyKind kind){int i=(int)kind;return Valid(i)?eliteSightings[i]:0;}
    public int DiscoveredCount {get{int n=0;for(int i=0;i<sightings.Length;i++)if(sightings[i]!=0)n++;return n;}}
    bool Valid(int i){return sightings!=null&&i>=0&&i<sightings.Length;}

    public int[] CaptureSightings(){return Copy(sightings);}
    public int[] CaptureKills(){return Copy(kills);}
    public int[] CaptureEliteSightings(){return Copy(eliteSightings);}
    static int[] Copy(int[] source){var result=new int[source.Length];Array.Copy(source,result,source.Length);return result;}

    public void Load(int[] savedSightings,int[] savedKills,int[] savedEliteSightings)
    {
        int count=Enum.GetValues(typeof(EnemyKind)).Length;sightings=new int[count];kills=new int[count];eliteSightings=new int[count];
        for(int i=0;i<count;i++){
            sightings[i]=savedSightings!=null&&i<savedSightings.Length&&savedSightings[i]!=0?1:0;
            kills[i]=savedKills!=null&&i<savedKills.Length?Mathf.Clamp(savedKills[i],0,MaxKills):0;
            eliteSightings[i]=savedEliteSightings!=null&&i<savedEliteSightings.Length?savedEliteSightings[i]&0x3e:0;
            if(kills[i]>0||eliteSightings[i]!=0)sightings[i]=1;
        }
    }

    public string Entry(EnemyKind kind)
    {
        if(!Discovered(kind))return "UNKNOWN CREATURE\nExplore dangerous chambers to reveal this entry.";
        return DisplayName(kind)+"\nDefeated: "+KillCount(kind)+"   Elites seen: "+EliteNames(kind)+"\nTactic: "+Weakness(kind)+"\nKnown drops: "+Drops(kind);
    }

    public string EliteNames(EnemyKind kind)
    {
        int mask=EliteMask(kind);if(mask==0)return "none";string result="";
        foreach(EliteModifier modifier in Enum.GetValues(typeof(EliteModifier)))if(modifier!=EliteModifier.None&&(mask&(1<<(int)modifier))!=0)result+=(result.Length>0?", ":"")+EliteRules.Name(modifier);
        return result;
    }

    public static string DisplayName(EnemyKind kind){return Regex.Replace(kind.ToString(),"([a-z])([A-Z])","$1 $2").ToUpperInvariant();}
    public static string Weakness(EnemyKind kind)
    {
        switch(kind){
            case EnemyKind.Slime:return "Keep distance; its slow approach leaves it open to ranged attacks.";
            case EnemyKind.Wolf:return "Sidestep the dash tell, then punish its recovery.";
            case EnemyKind.Skeleton:return "Hollow and Sunsteel weapons exploit the undead.";
            case EnemyKind.Goblin:return "Interrupt its approach with knockback or a combo finisher.";
            case EnemyKind.ForestBeast:return "Circle its heavy body and attack after committed swings.";
            case EnemyKind.HollowKnight:return "Save stamina for Gravefall and its faster second phase.";
            case EnemyKind.StoneGolem:return "Leave the marked slam circle before committing to damage.";
            case EnemyKind.CaveBat:return "Bait its dash into a clear lane and counter during recovery.";
            case EnemyKind.Scorpion:return "Its straight charge is quick but clearly lined; dodge sideways.";
            case EnemyKind.MushroomShaman:return "Close the gap after its cyan casting wind-up.";
            case EnemyKind.IceWraith:return "Use walls to break spell lines, then pressure its recovery.";
            default:return "Respect the dash and slam tells; Sunsteel bites into dark armor.";
        }
    }

    public static string Drops(EnemyKind kind)
    {
        switch(kind){
            case EnemyKind.Slime:return "Slime Gel, Moonleaf, Ring of Flames";
            case EnemyKind.Wolf:return "Wolf Hide, Wolf Fang, Moonleaf";
            case EnemyKind.Skeleton:return "Bone, Rusted Metal, Ancient Shard";
            case EnemyKind.Goblin:return "Iron Ore, Ember Shard, Goblin Cleaver";
            case EnemyKind.ForestBeast:return "Wolf Hide, Ancient Shard, Ring of Hunting";
            case EnemyKind.HollowKnight:return "Hollow Blade, Ring of Vampirism";
            case EnemyKind.StoneGolem:return "Iron Ore, Ancient Shard, Thunder Axe";
            case EnemyKind.CaveBat:return "Storm Crystal, Storm Bow";
            case EnemyKind.Scorpion:return "Venom Sac, Venom Saber";
            case EnemyKind.MushroomShaman:return "Venom Sac, Venom Saber";
            case EnemyKind.IceWraith:return "Storm Crystal, Storm Bow";
            default:return "Sunsteel, Rusted Metal, Dusk Staff";
        }
    }
}
