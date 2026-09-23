using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class EnemyRecord {public int kind;public float hp,maxHp,speed,damage;public Vector2 position;public bool phaseTwo;}
[Serializable] public class LootRecord {public int item,amount;public Vector2 position;}
[Serializable] public class ZoneRecord {public bool visited;public EnemyRecord[] enemies;public LootRecord[] loot;public long clearedUtcTicks;}
public class EncounterJournal
{
    const long RestockTicks=3*60*TimeSpan.TicksPerSecond;
    public ZoneRecord[] records=new ZoneRecord[8];public int active=-1;
    public void Capture(AshfallDirector d){if(active<0||active>=records.Length)return;var enemies=new List<EnemyRecord>();foreach(var e in d.enemies)if(e&&e.hp>0)enemies.Add(new EnemyRecord{kind=(int)e.kind,hp=e.hp,maxHp=e.maxHp,speed=e.speed,damage=e.damage,position=(Vector2)e.transform.position-WorldAtlas.Centers[active],phaseTwo=e.PhaseTwo});var loot=new List<LootRecord>();foreach(var p in d.pickups)if(p)loot.Add(new LootRecord{item=(int)p.item,amount=p.amount,position=(Vector2)p.transform.position-WorldAtlas.Centers[active]});long cleared=enemies.Count==0?(records[active]!=null&&records[active].clearedUtcTicks>0?records[active].clearedUtcTicks:DateTime.UtcNow.Ticks):0;records[active]=new ZoneRecord{visited=true,enemies=enemies.ToArray(),loot=loot.ToArray(),clearedUtcTicks=cleared};}
    public bool Restore(AshfallDirector d,int area){active=area;var record=records[area];if(record==null||!record.visited)return false;foreach(var r in record.enemies??new EnemyRecord[0]){if(r.hp<=0||!Enum.IsDefined(typeof(EnemyKind),r.kind))continue;d.Spawn((EnemyKind)r.kind,1);var e=d.enemies[d.enemies.Count-1];e.maxHp=Mathf.Max(1,r.maxHp);e.hp=Mathf.Min(r.hp,e.maxHp);e.speed=r.speed;e.damage=r.damage;e.transform.position=WorldAtlas.Centers[area]+WorldAtlas.Recover(area,r.position);e.RestorePhase(r.phaseTwo);if(e.kind==EnemyKind.HollowKnight)d.bossState=1;}foreach(var r in record.loot??new LootRecord[0])if(Enum.IsDefined(typeof(ItemId),r.item)&&r.amount>0)d.RestoreLoot(WorldAtlas.Centers[area]+WorldAtlas.Recover(area,r.position),(ItemId)r.item,r.amount);if(area>0&&(record.enemies==null||record.enemies.Length==0)&&record.clearedUtcTicks>0&&DateTime.UtcNow.Ticks-record.clearedUtcTicks>=RestockTicks){d.Spawn(RestockKind(area),3,area==7);record.clearedUtcTicks=0;d.Tell("A scavenger patrol has moved into the cleared chambers.");}return true;}
    public void Load(ZoneRecord[] source){records=new ZoneRecord[8];if(source!=null)Array.Copy(source,records,Mathf.Min(8,source.Length));active=-1;}
    static EnemyKind RestockKind(int area){switch(area){case 1:return EnemyKind.Goblin;case 2:return EnemyKind.StoneGolem;case 3:return EnemyKind.Scorpion;case 4:return EnemyKind.Skeleton;case 5:return EnemyKind.MushroomShaman;case 6:return EnemyKind.IceWraith;default:return EnemyKind.DarkKnight;}}
}
