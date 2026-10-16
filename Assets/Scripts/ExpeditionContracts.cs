using System;
using UnityEngine;

public enum ExpeditionContract { None, HuntersMark, IronTrial, GlassOath }

// A single, readable risk/reward choice is locked to each dangerous area.
// Choices persist with the journey and modify the existing authored rooms.
public class ExpeditionContracts : MonoBehaviour
{
    public int[] selected=new int[8];
    public bool[] applied=new bool[8];
    AshfallDirector d;

    public void Initialize(AshfallDirector director){d=director;}
    public ExpeditionContract Selected(int zone){return zone>0&&zone<selected.Length&&Enum.IsDefined(typeof(ExpeditionContract),selected[zone])?(ExpeditionContract)selected[zone]:ExpeditionContract.None;}
    public bool NeedsSelection(int zone){return zone>0&&zone<8&&Selected(zone)==ExpeditionContract.None&&(!d.objectives||!d.objectives.claimed[zone]);}
    public bool Select(int zone,ExpeditionContract contract){if(!NeedsSelection(zone)||contract==ExpeditionContract.None||!Enum.IsDefined(typeof(ExpeditionContract),contract))return false;selected[zone]=(int)contract;applied[zone]=false;return true;}
    public int[] Capture(){var copy=new int[8];Array.Copy(selected,copy,8);return copy;}
    public bool[] CaptureApplied(){var copy=new bool[8];Array.Copy(applied,copy,8);return copy;}
    public void Load(int[] saved,bool[] savedApplied=null){selected=new int[8];applied=new bool[8];if(saved==null)return;for(int z=1;z<Mathf.Min(8,saved.Length);z++){selected[z]=Enum.IsDefined(typeof(ExpeditionContract),saved[z])?saved[z]:0;applied[z]=savedApplied!=null&&z<savedApplied.Length&&savedApplied[z];}}

    public static string Name(ExpeditionContract contract){switch(contract){case ExpeditionContract.HuntersMark:return "HUNTER'S MARK";case ExpeditionContract.IronTrial:return "IRON TRIAL";case ExpeditionContract.GlassOath:return "GLASS OATH";default:return "NO CONTRACT";}}
    public static string Risk(ExpeditionContract contract){switch(contract){case ExpeditionContract.HuntersMark:return "An additional biome elite hunts this expedition.";case ExpeditionContract.IronTrial:return "Enemies gain 20% health and 15% damage.";case ExpeditionContract.GlassOath:return "You deal 25% more damage, but enemies also deal 25% more.";default:return "Choose a contract before entering this area.";}}
    public static string Reward(ExpeditionContract contract){switch(contract){case ExpeditionContract.HuntersMark:return "Cache bonus: 60 gold.";case ExpeditionContract.IronTrial:return "Cache bonus: 2 extra biome materials.";case ExpeditionContract.GlassOath:return "Cache bonus: 35 gold and 1 Ancient Shard.";default:return "";}}
    public string Summary(int zone){var contract=Selected(zone);return contract==ExpeditionContract.None?"No expedition contract":Name(contract);}

    public float PlayerDamageMultiplier(int zone){return Selected(zone)==ExpeditionContract.GlassOath?1.25f:1;}
    public float EnemyHealthMultiplier(int zone){return Selected(zone)==ExpeditionContract.IronTrial?1.2f:1;}
    public float EnemyDamageMultiplier(int zone){var contract=Selected(zone);return contract==ExpeditionContract.IronTrial?1.15f:contract==ExpeditionContract.GlassOath?1.25f:1;}

    public void ApplyEnemy(EnemyActor enemy,int zone)
    {
        if(!enemy||zone<=0)return;float hp=EnemyHealthMultiplier(zone),damage=EnemyDamageMultiplier(zone);
        if(hp!=1){enemy.maxHp=Mathf.Ceil(enemy.maxHp*hp);enemy.hp=enemy.maxHp;}
        if(damage!=1)enemy.damage*=damage;
    }

    public void DeployFresh(int zone)
    {
        if(!d||Selected(zone)!=ExpeditionContract.HuntersMark||d.objectives&&d.objectives.claimed[zone])return;
        int before=d.enemies.Count;d.Spawn(GuardKind(zone),1,EliteRules.ForZone(zone%7+1));
        if(d.enemies.Count>before)d.enemies[d.enemies.Count-1].gameObject.name="Contract elite / Hunter's Mark";
    }

    public void MarkApplied(int zone){if(zone>0&&zone<applied.Length&&Selected(zone)!=ExpeditionContract.None)applied[zone]=true;}
    public void ApplyRestored(int zone)
    {
        if(zone<=0||zone>=applied.Length||applied[zone]||Selected(zone)==ExpeditionContract.None)return;
        if(Selected(zone)==ExpeditionContract.HuntersMark)DeployFresh(zone);else foreach(var enemy in d.enemies)ApplyEnemy(enemy,zone);
        applied[zone]=true;
    }

    public void GrantReward(int zone)
    {
        switch(Selected(zone)){
            case ExpeditionContract.HuntersMark:d.Add(ItemId.Gold,60);break;
            case ExpeditionContract.IronTrial:d.Add(Material(zone),2);break;
            case ExpeditionContract.GlassOath:d.Add(ItemId.Gold,35);d.Add(ItemId.AncientShard,1);break;
        }
    }

    static EnemyKind GuardKind(int zone){switch(zone){case 1:return EnemyKind.Goblin;case 2:return EnemyKind.StoneGolem;case 3:return EnemyKind.Scorpion;case 4:return EnemyKind.Skeleton;case 5:return EnemyKind.MushroomShaman;case 6:return EnemyKind.IceWraith;default:return EnemyKind.DarkKnight;}}
    static ItemId Material(int zone){return zone==1?ItemId.WolfFang:zone==2?ItemId.IronOre:zone==3||zone==5?ItemId.VenomSac:zone==6?ItemId.StormCrystal:zone==7?ItemId.Sunsteel:ItemId.AncientShard;}
}
