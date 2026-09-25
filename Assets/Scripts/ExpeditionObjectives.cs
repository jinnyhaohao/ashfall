using System;
using UnityEngine;

public enum ObjectiveType { None, CorruptedAnchors, SurvivalWaves, DefendRelic, EliteHunt }

// One data-driven controller supplies four room objectives and a guarded reward cache.
public class ExpeditionObjectives : MonoBehaviour
{
    public int[] progress=new int[8],aux=new int[8];
    public bool[] started=new bool[8],completed=new bool[8],claimed=new bool[8];
    public int[] seals=new int[8]; // Version 3 compatibility view.
    readonly SpriteRenderer[,] markers=new SpriteRenderer[8,4];
    readonly float[] defenseClock=new float[8],defenseDamage=new float[8];
    AshfallDirector d;
    public static readonly ObjectiveType[] Types={ObjectiveType.None,ObjectiveType.CorruptedAnchors,ObjectiveType.SurvivalWaves,ObjectiveType.DefendRelic,ObjectiveType.EliteHunt,ObjectiveType.CorruptedAnchors,ObjectiveType.SurvivalWaves,ObjectiveType.DefendRelic};
    public static readonly string[] Titles={"Village refuge","Shatter corrupted anchors","Survive the deep ambush","Defend the caravan relic","Hunt the grave champion","Sever the drowned roots","Endure the prism assault","Hold the fallen crown"};

    public static Vector2 Position(int zone,int slot){var rooms=WorldAtlas.Rooms[zone];int room=slot<3?1+slot%(rooms.Length-1):rooms.Length-1;return WorldAtlas.Centers[zone]+rooms[room]+new Vector2(slot==3?-2.5f:2.5f,0);}
    public ObjectiveType Type(int zone){return zone>0&&zone<Types.Length?Types[zone]:ObjectiveType.None;}
    bool UsesMarker(int z,int slot){return slot==3||Type(z)==ObjectiveType.CorruptedAnchors&&slot<3||Type(z)!=ObjectiveType.CorruptedAnchors&&slot==0;}

    public void Initialize(AshfallDirector director,Sprite[] art)
    {
        d=director;var details=Resources.Load<Texture2D>("AshfallAtlasDetails");Sprite cache=ExpansionContent.Slice(details,8,4,4,new Vector2(.5f,.04f));
        for(int z=1;z<8;z++)for(int i=0;i<4;i++){
            if(!UsesMarker(z,i))continue;
            string title=i==3?"Guarded reward cache":Type(z)==ObjectiveType.CorruptedAnchors?"Corrupted anchor":Type(z)==ObjectiveType.SurvivalWaves?"Challenge brazier":Type(z)==ObjectiveType.DefendRelic?"Expedition relic":"Hunt sigil";
            var g=new GameObject(title);g.transform.position=Position(z,i);g.transform.localScale=Vector3.one*(i==3?.85f:.68f);var r=g.AddComponent<SpriteRenderer>();r.sprite=i==3?cache:art[11];r.sortingOrder=108-(int)(g.transform.position.y*10);markers[z,i]=r;
        }
    }

    public void Deploy(int z,bool restored=false)
    {
        if(z<=0||z>=8||claimed[z])return;
        if(Type(z)==ObjectiveType.CorruptedAnchors)for(int slot=0;slot<3;slot++)if((progress[z]&(1<<slot))==0&&(!restored||!Guarded(z,slot)))SpawnAt(z,slot,GuardKind(z),1,EliteModifier.None,false);
        if(!completed[z]&&started[z])Resume(z);
        if(!restored||!EliteGuarded(z,3))SpawnAt(z,3,GuardKind(z),2,EliteRules.ForZone(z),false);
    }

    void Resume(int z){if(Type(z)==ObjectiveType.SurvivalWaves&&ObjectiveAlive()==0)SpawnWave(z);else if(Type(z)==ObjectiveType.DefendRelic&&ObjectiveAlive()==0)SpawnDefenders(z);else if(Type(z)==ObjectiveType.EliteHunt&&ObjectiveAlive()==0)SpawnHunt(z);}
    void SpawnAt(int z,int slot,EnemyKind kind,int count,EliteModifier modifier,bool objective)
    {
        int start=d.enemies.Count;d.Spawn(kind,count,modifier);
        for(int i=0;i<count;i++){Vector2 offset=count==1?new Vector2(-1.2f,0):new Vector2(-1.1f+i*2.2f,.75f);Vector2 local=Position(z,slot)-WorldAtlas.Centers[z]+offset;var e=d.enemies[start+i];e.transform.position=WorldAtlas.Centers[z]+WorldAtlas.Recover(z,local);e.objectiveTarget=objective;}
    }
    static EnemyKind GuardKind(int z){switch(z){case 1:return EnemyKind.Goblin;case 2:return EnemyKind.StoneGolem;case 3:return EnemyKind.Scorpion;case 4:return EnemyKind.Skeleton;case 5:return EnemyKind.MushroomShaman;case 6:return EnemyKind.IceWraith;default:return EnemyKind.DarkKnight;}}
    public bool Guarded(int z,int slot){Vector2 p=Position(z,slot);foreach(var e in d.enemies)if(e&&e.hp>0&&Vector2.Distance(e.transform.position,p)<5&&CombatRules.Clear(d,e.transform.position,p))return true;return false;}
    bool EliteGuarded(int z,int slot){Vector2 p=Position(z,slot);foreach(var e in d.enemies)if(e&&e.hp>0&&e.modifier!=EliteModifier.None&&Vector2.Distance(e.transform.position,p)<5&&CombatRules.Clear(d,e.transform.position,p))return true;return false;}
    int ObjectiveAlive(){int n=0;foreach(var e in d.enemies)if(e&&e.hp>0&&e.objectiveTarget)n++;return n;}
    public int Nearby(){int z=WorldAtlas.Index(d.zone);if(z==0)return -1;for(int i=0;i<4;i++)if(UsesMarker(z,i)&&Vector2.Distance(d.player.transform.position,Position(z,i))<1.6f)return i;return -1;}

    public string Prompt()
    {
        int z=WorldAtlas.Index(d.zone),slot=Nearby();if(slot<0)return "";
        if(slot==3){if(claimed[z])return "Cache claimed";return !completed[z]?"Complete the room objective to unlock":Guarded(z,3)?"Defeat the elite cache guards":"E  Claim expedition cache";}
        if(Type(z)==ObjectiveType.CorruptedAnchors){if((progress[z]&(1<<slot))!=0)return "Anchor destroyed";return Guarded(z,slot)?"Defeat the anchor guard":"E  Shatter corrupted anchor";}
        if(completed[z])return "Objective complete";if(started[z])return Type(z)==ObjectiveType.DefendRelic?"Relic defense in progress":"Objective in progress";
        return Type(z)==ObjectiveType.SurvivalWaves?"E  Light brazier: begin 3 waves":Type(z)==ObjectiveType.DefendRelic?"E  Begin relic defense":"E  Reveal the elite target";
    }

    public bool Interact()
    {
        int z=WorldAtlas.Index(d.zone),slot=Nearby();if(slot<0)return false;
        if(slot==3){if(claimed[z])return true;if(!completed[z]||Guarded(z,3)){d.Tell(!completed[z]?"Complete this area's objective first.":"Defeat the elite cache guards.");return true;}Claim(z);return true;}
        if(Type(z)==ObjectiveType.CorruptedAnchors){if((progress[z]&(1<<slot))!=0)return true;if(Guarded(z,slot)){d.Tell("Clear the anchor guard first.");return true;}progress[z]|=1<<slot;seals[z]=progress[z];CombatFX.Burst(Position(z,slot),new Color(.7f,.25f,1),14);if(AnchorCount(z)==3)Complete(z);else d.Tell("Corrupted anchor shattered: "+AnchorCount(z)+" / 3");Save();return true;}
        if(!started[z]&&!completed[z]){started[z]=true;progress[z]=0;aux[z]=Type(z)==ObjectiveType.DefendRelic?100:0;CombatFX.Ring(Position(z,0),1.8f,ObjectiveColor(Type(z)));Resume(z);d.Tell(Type(z)==ObjectiveType.SurvivalWaves?"Wave one approaches!":Type(z)==ObjectiveType.DefendRelic?"Protect the relic for 30 seconds!":"The marked champion has appeared.");Save();}
        return true;
    }

    void SpawnWave(int z){if(progress[z]>=3){Complete(z);return;}SpawnAt(z,0,GuardKind(z),3+progress[z],EliteModifier.None,true);d.Tell("Ambush wave "+(progress[z]+1)+" / 3");}
    void SpawnDefenders(int z){SpawnAt(z,0,GuardKind(z),4,progress[z]>=15?EliteModifier.Swift:EliteModifier.None,true);}
    void SpawnHunt(int z){SpawnAt(z,0,GuardKind(z),1,EliteRules.ForZone(z),true);}

    public void NotifyEnemyDied(EnemyActor e)
    {
        int z=WorldAtlas.Index(d.zone);if(z<=0||completed[z]||!e.objectiveTarget)return;
        if(Type(z)==ObjectiveType.EliteHunt){progress[z]=1;Complete(z);}
        else if(Type(z)==ObjectiveType.SurvivalWaves&&ObjectiveAlive()==0){progress[z]++;if(progress[z]>=3)Complete(z);else SpawnWave(z);}
    }

    public void TickDefense(float seconds)
    {
        int z=WorldAtlas.Index(d.zone);if(z<=0||Type(z)!=ObjectiveType.DefendRelic||!started[z]||completed[z])return;
        defenseClock[z]+=seconds;int threats=0;foreach(var e in d.enemies)if(e&&e.objectiveTarget&&Vector2.Distance(e.transform.position,Position(z,0))<2.6f)threats++;
        if(threats>0){defenseDamage[z]+=threats*7*seconds;int loss=Mathf.FloorToInt(defenseDamage[z]);if(loss>0){aux[z]=Mathf.Max(0,aux[z]-loss);defenseDamage[z]-=loss;}}
        if(defenseClock[z]>=1){int whole=Mathf.FloorToInt(defenseClock[z]);defenseClock[z]-=whole;progress[z]+=whole;if(progress[z]%5==0){CombatFX.Ring(Position(z,0),1.2f,new Color(.3f,.9f,1));Save();}}
        if(aux[z]<=0){started[z]=false;progress[z]=0;aux[z]=100;foreach(var e in d.enemies)if(e&&e.objectiveTarget)e.objectiveTarget=false;d.Tell("The relic was overwhelmed. Regroup and try again.");Save();}
        else if(progress[z]>=30)Complete(z);else if(ObjectiveAlive()==0)SpawnDefenders(z);
    }
    void Update(){if(!d||AshfallBeta.Paused)return;TickDefense(Time.deltaTime);for(int z=1;z<8;z++)for(int i=0;i<4;i++)if(markers[z,i])markers[z,i].color=MarkerColor(z,i);}

    Color MarkerColor(int z,int slot){if(slot==3)return claimed[z]?new Color(.35f,.35f,.35f):completed[z]?Color.white:new Color(.55f,.55f,.6f);if(Type(z)==ObjectiveType.CorruptedAnchors)return (progress[z]&(1<<slot))!=0?new Color(.3f,.9f,1):new Color(.72f,.3f,.92f);return completed[z]?new Color(.3f,.9f,1):ObjectiveColor(Type(z));}
    static Color ObjectiveColor(ObjectiveType t){return t==ObjectiveType.SurvivalWaves?new Color(1,.5f,.18f):t==ObjectiveType.DefendRelic?new Color(.2f,.9f,1):t==ObjectiveType.EliteHunt?new Color(1,.25f,.45f):new Color(.7f,.3f,1);}
    void Complete(int z){started[z]=false;completed[z]=true;CombatFX.Burst(Position(z,0),Color.cyan,18);d.Tell("OBJECTIVE COMPLETE — the guarded cache is unlocked.");Save();}
    void Claim(int z){claimed[z]=true;d.Add(ItemId.Gold,70+z*20);d.Add(ItemId.HealthPotion,2);d.Add(z==1?ItemId.WolfFang:z==2?ItemId.IronOre:z==3||z==5?ItemId.VenomSac:z==6?ItemId.StormCrystal:z==7?ItemId.Sunsteel:ItemId.AncientShard,4);CombatFX.Burst(Position(z,3),Color.yellow,15);d.Tell("Expedition cache claimed: gold, potions and materials.");Save();}
    void Save(){if(AshfallBeta.Instance)AshfallBeta.Instance.SaveJourney();}
    public int AnchorCount(int z){int n=0;for(int i=0;i<3;i++)if((progress[z]&(1<<i))!=0)n++;return n;}
    public int Count(int z){return AnchorCount(z);}
    public string Status(){int z=WorldAtlas.Index(d.zone);if(z==0)return Titles[0];if(claimed[z])return "Expedition cache secured";if(completed[z])return "Objective complete / cache guarded";switch(Type(z)){case ObjectiveType.CorruptedAnchors:return Titles[z]+"  "+AnchorCount(z)+" / 3";case ObjectiveType.SurvivalWaves:return started[z]?Titles[z]+"  wave "+Mathf.Min(3,progress[z]+1)+" / 3":Titles[z];case ObjectiveType.DefendRelic:return started[z]?"Relic "+aux[z]+"%  /  "+progress[z]+" / 30s":Titles[z];default:return started[z]?"Marked champion active":Titles[z];}}

    public void Load(int[] savedProgress,int[] savedAux,bool[] savedStarted,bool[] savedCompleted,bool[] savedClaimed,int[] legacySeals)
    {
        progress=new int[8];aux=new int[8];started=new bool[8];completed=new bool[8];claimed=new bool[8];seals=new int[8];
        if(savedProgress!=null)Array.Copy(savedProgress,progress,Mathf.Min(8,savedProgress.Length));if(savedAux!=null)Array.Copy(savedAux,aux,Mathf.Min(8,savedAux.Length));if(savedStarted!=null)Array.Copy(savedStarted,started,Mathf.Min(8,savedStarted.Length));if(savedCompleted!=null)Array.Copy(savedCompleted,completed,Mathf.Min(8,savedCompleted.Length));if(savedClaimed!=null)Array.Copy(savedClaimed,claimed,Mathf.Min(8,savedClaimed.Length));
        if((savedProgress==null||savedProgress.Length==0)&&legacySeals!=null)for(int z=1;z<Mathf.Min(8,legacySeals.Length);z++){seals[z]=legacySeals[z]&7;if(Type(z)==ObjectiveType.CorruptedAnchors){progress[z]=seals[z];completed[z]=seals[z]==7;}else if(legacySeals[z]==7)completed[z]=true;}
        for(int z=1;z<8;z++){if(Type(z)==ObjectiveType.CorruptedAnchors)seals[z]=progress[z]&7;if(Type(z)==ObjectiveType.DefendRelic&&aux[z]<=0)aux[z]=100;if(claimed[z])completed[z]=true;}
    }
}
