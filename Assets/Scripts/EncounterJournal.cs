using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class EnemyRecord {public int kind;public float hp,maxHp,speed,damage;public Vector2 position;}
[Serializable] public class LootRecord {public int item,amount;public Vector2 position;}
[Serializable] public class ZoneRecord {public bool visited;public EnemyRecord[] enemies;public LootRecord[] loot;}
public class EncounterJournal
{
    public ZoneRecord[] records=new ZoneRecord[8];public int active=-1;
    public void Capture(AshfallDirector d){if(active<0)return;var enemies=new List<EnemyRecord>();foreach(var e in d.enemies)if(e&&e.hp>0)enemies.Add(new EnemyRecord{kind=(int)e.kind,hp=e.hp,maxHp=e.maxHp,speed=e.speed,damage=e.damage,position=(Vector2)e.transform.position-WorldAtlas.Centers[active]});var loot=new List<LootRecord>();foreach(var p in d.pickups)if(p)loot.Add(new LootRecord{item=(int)p.item,amount=p.amount,position=(Vector2)p.transform.position-WorldAtlas.Centers[active]});records[active]=new ZoneRecord{visited=true,enemies=enemies.ToArray(),loot=loot.ToArray()};}
    public bool Restore(AshfallDirector d,int area){active=area;var record=records[area];if(record==null||!record.visited)return false;foreach(var r in record.enemies??new EnemyRecord[0]){if(r.hp<=0||!Enum.IsDefined(typeof(EnemyKind),r.kind))continue;d.Spawn((EnemyKind)r.kind,1);var e=d.enemies[d.enemies.Count-1];e.hp=r.hp;e.maxHp=r.maxHp;e.speed=r.speed;e.damage=r.damage;e.transform.position=WorldAtlas.Centers[area]+WorldAtlas.Recover(area,r.position);if(e.kind==EnemyKind.HollowKnight)d.bossState=1;}foreach(var r in record.loot??new LootRecord[0])if(Enum.IsDefined(typeof(ItemId),r.item)&&r.amount>0)d.RestoreLoot(WorldAtlas.Centers[area]+WorldAtlas.Recover(area,r.position),(ItemId)r.item,r.amount);return true;}
    public void Load(ZoneRecord[] source){records=new ZoneRecord[8];if(source!=null)Array.Copy(source,records,Mathf.Min(8,source.Length));active=-1;}
}
