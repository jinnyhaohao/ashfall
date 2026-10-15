using System;
using System.Collections.Generic;
using UnityEngine;

public enum AshShrineEvent { GuardedEmbers, BloodOffering, ScavengerOffering }

// One deterministic secret in a dead-end chamber of every dangerous area.
// State is represented as 0 hidden, 1 revealed, 2 claimed for compact persistence.
public class WorldEvents : MonoBehaviour
{
    public static WorldEvents I {get;private set;}
    public int[] state=new int[8];
    readonly SpriteRenderer[] markers=new SpriteRenderer[8];
    readonly bool[] guardianDeployed=new bool[8];
    AshfallDirector d;Sprite markerArt;

    public static WorldEvents Ensure(AshfallDirector director,Sprite art)
    {
        if(I){I.Initialize(director,art);return I;}
        var events=director.GetComponent<WorldEvents>();if(!events)events=director.gameObject.AddComponent<WorldEvents>();events.Initialize(director,art);return events;
    }

    public void Initialize(AshfallDirector director,Sprite art)
    {
        I=this;d=director;markerArt=art;
        for(int z=1;z<8;z++)if(!markers[z]){
            var g=new GameObject("Hidden ash shrine / "+WorldAtlas.Names[z]);g.transform.position=Position(z);g.transform.localScale=Vector3.one*.48f;
            var r=g.AddComponent<SpriteRenderer>();r.sprite=markerArt;r.color=new Color(.78f,.36f,1);r.sortingOrder=112-(int)(g.transform.position.y*10);r.enabled=state[z]>0;markers[z]=r;
        }
    }

    public static AshShrineEvent Type(int zone){return (AshShrineEvent)((zone-1)%3);}
    public static Vector2 Position(int zone)
    {
        var rooms=WorldAtlas.Rooms[zone];var deadEnds=new List<int>();int[] edges=WorldAtlas.Edges[zone];
        for(int room=1;room<rooms.Length;room++){int degree=0;for(int i=0;i<edges.Length;i++)if(edges[i]==room)degree++;if(degree==1)deadEnds.Add(room);}
        int selected=deadEnds.Count>0?deadEnds[(zone*5+1)%deadEnds.Count]:rooms.Length-1;
        Vector2 local=rooms[selected]+new Vector2(2.25f,-1.15f);return WorldAtlas.Centers[zone]+WorldAtlas.Recover(zone,local);
    }

    public bool Revealed(int zone){return zone>0&&zone<state.Length&&state[zone]>=1;}
    public bool Claimed(int zone){return zone>0&&zone<state.Length&&state[zone]>=2;}
    public int[] Capture(){var copy=new int[8];Array.Copy(state,copy,8);return copy;}
    public void Load(int[] saved){state=new int[8];if(saved!=null)for(int z=1;z<Mathf.Min(8,saved.Length);z++)state[z]=Mathf.Clamp(saved[z],0,2);for(int z=1;z<8;z++)if(markers[z])markers[z].enabled=state[z]>0;}

    void Update()
    {
        if(!d||!d.player||AshfallBeta.Paused)return;int z=WorldAtlas.Index(d.zone);if(z<=0)return;
        if(!Revealed(z)&&Vector2.Distance(d.player.transform.position,Position(z))<4.2f)Reveal(z);
        if(markers[z])markers[z].color=Claimed(z)?new Color(.32f,.34f,.38f):Color.Lerp(new Color(.55f,.2f,.8f),new Color(1,.66f,.35f),.5f+.5f*Mathf.Sin(Time.time*3));
    }

    void Reveal(int zone)
    {
        state[zone]=1;if(markers[zone])markers[zone].enabled=true;CombatFX.Ring(Position(zone),1.4f,new Color(.78f,.35f,1),.65f);
        d.Tell("A hidden ash shrine answers nearby.");
        if(Type(zone)==AshShrineEvent.GuardedEmbers&&!Guarded(zone))DeployGuardian(zone);
    }

    void DeployGuardian(int zone)
    {
        if(guardianDeployed[zone])return;guardianDeployed[zone]=true;int start=d.enemies.Count;d.Spawn(GuardKind(zone),1,EliteRules.ForZone(zone));
        if(d.enemies.Count>start){var e=d.enemies[start];Vector2 local=Position(zone)-WorldAtlas.Centers[zone]+new Vector2(-1.7f,.55f);e.transform.position=WorldAtlas.Centers[zone]+WorldAtlas.Recover(zone,local);}
    }

    static EnemyKind GuardKind(int zone){switch(zone){case 1:return EnemyKind.Goblin;case 2:return EnemyKind.StoneGolem;case 3:return EnemyKind.Scorpion;case 4:return EnemyKind.Skeleton;case 5:return EnemyKind.MushroomShaman;case 6:return EnemyKind.IceWraith;default:return EnemyKind.DarkKnight;}}
    static ItemId Material(int zone){return zone==1?ItemId.WolfFang:zone==2?ItemId.IronOre:zone==3||zone==5?ItemId.VenomSac:zone==6?ItemId.StormCrystal:zone==7?ItemId.Sunsteel:ItemId.AncientShard;}
    bool Guarded(int zone){Vector2 p=Position(zone);foreach(var e in d.enemies)if(e&&e.hp>0&&Vector2.Distance(e.transform.position,p)<4.8f&&CombatRules.Clear(d,e.transform.position,p))return true;return false;}

    public int Nearby()
    {
        if(!d||!d.player)return -1;int z=WorldAtlas.Index(d.zone);return Revealed(z)&&Vector2.Distance(d.player.transform.position,Position(z))<1.55f?z:-1;
    }

    public string Prompt()
    {
        int z=Nearby();if(z<0)return "";if(Claimed(z))return "Ash shrine spent";
        if(Type(z)==AshShrineEvent.GuardedEmbers)return Guarded(z)?"Defeat the hidden shrine guardian":"E  Claim guarded embers";
        if(Type(z)==AshShrineEvent.BloodOffering)return "E  Offer 20 health for rare materials";
        int cost=35+z*5;return "E  Offer "+cost+" gold for a hidden cache";
    }

    public bool Interact()
    {
        int z=Nearby();if(z<0)return false;if(Claimed(z)){d.Tell("Only cold ash remains.");return true;}
        var type=Type(z);
        if(type==AshShrineEvent.GuardedEmbers){if(Guarded(z)){d.Tell("The shrine guardian still stands.");return true;}Reward(z,55+z*8,3,"The guarded embers yield their secret.");return true;}
        if(type==AshShrineEvent.BloodOffering){if(d.player.hp<=25){d.Tell("The shrine refuses a fatal offering.");return true;}d.player.hp-=20;Reward(z,20+z*5,4,"Blood wakes the buried ash.");return true;}
        int cost=35+z*5;if(!d.Has(ItemId.Gold,cost)){d.Tell("The shrine demands "+cost+" gold.");return true;}d.bag[ItemId.Gold]-=cost;Reward(z,0,5,"The hidden cache opens beneath the shrine.");return true;
    }

    void Reward(int zone,int gold,int materials,string message)
    {
        state[zone]=2;if(gold>0)d.Add(ItemId.Gold,gold);d.Add(Material(zone),materials);d.Add(ItemId.HealthPotion,1);
        CombatFX.Burst(Position(zone),new Color(1,.58f,.24f),14);d.Tell(message);if(AshfallBeta.Instance)AshfallBeta.Instance.SaveJourney();
    }
}
