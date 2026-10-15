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
    readonly float[] defenseClock=new float[8],defenseDamage=new float[8],defenseGraceUntil=new float[8];
    AshfallDirector d;
    public static readonly ObjectiveType[] Types={ObjectiveType.None,ObjectiveType.CorruptedAnchors,ObjectiveType.SurvivalWaves,ObjectiveType.DefendRelic,ObjectiveType.EliteHunt,ObjectiveType.CorruptedAnchors,ObjectiveType.SurvivalWaves,ObjectiveType.DefendRelic};
    public static readonly string[] Titles={"Village refuge","Shatter corrupted anchors","Survive the deep ambush","Defend the caravan relic","Hunt the grave champion","Sever the drowned roots","Endure the prism assault","Hold the fallen crown"};

    public static Vector2 Position(int zone,int slot){var rooms=WorldAtlas.Rooms[zone];int room=slot<3?1+slot%(rooms.Length-1):rooms.Length-1;return WorldAtlas.Centers[zone]+rooms[room]+new Vector2(slot==3?-2.5f:2.5f,0);}
    public ObjectiveType Type(int zone){return zone>0&&zone<Types.Length?Types[zone]:ObjectiveType.None;}
    bool UsesMarker(int z,int slot){return slot==3||Type(z)==ObjectiveType.CorruptedAnchors&&slot<3||Type(z)!=ObjectiveType.CorruptedAnchors&&slot==0;}

    public void Initialize(AshfallDirector director,Sprite[] art)
    {
        d=director;var details=Resources.Load<Texture2D>("AshfallAtlasDetails");Sprite cache=ExpansionContent.Slice(details,8,4,4,new Vector2(.5f,.04f)),brazier=ExpansionContent.Slice(details,10,4,4,new Vector2(.5f,.04f)),relic=ExpansionContent.Slice(details,7,4,4,new Vector2(.5f,.04f)),hunt=ExpansionContent.Slice(details,15,4,4,new Vector2(.5f,.04f));
        for(int z=1;z<8;z++)for(int i=0;i<4;i++){
            if(!UsesMarker(z,i))continue;
            string title=i==3?"Guarded reward cache":Type(z)==ObjectiveType.CorruptedAnchors?"Corrupted anchor":Type(z)==ObjectiveType.SurvivalWaves?"Challenge brazier":Type(z)==ObjectiveType.DefendRelic?"Expedition relic":"Hunt sigil";
            var g=new GameObject(title);g.transform.position=Position(z,i);g.transform.localScale=Vector3.one*(i==3?.85f:Type(z)==ObjectiveType.SurvivalWaves?.66f:Type(z)==ObjectiveType.DefendRelic?.74f:Type(z)==ObjectiveType.EliteHunt?.72f:.68f);var r=g.AddComponent<SpriteRenderer>();r.sprite=i==3?cache:Type(z)==ObjectiveType.SurvivalWaves?brazier:Type(z)==ObjectiveType.DefendRelic?relic:Type(z)==ObjectiveType.EliteHunt?hunt:art[11];r.sortingOrder=108-(int)(g.transform.position.y*10);markers[z,i]=r;g.AddComponent<ObjectiveMarkerCue>().Initialize(this,z,i,Type(z),r.sortingOrder-1);
        }
    }

    public void Deploy(int z,bool restored=false)
    {
        if(z<=0||z>=8||claimed[z])return;
        if(Type(z)==ObjectiveType.CorruptedAnchors)for(int slot=0;slot<3;slot++)if((progress[z]&(1<<slot))==0&&(!restored||!Guarded(z,slot)))SpawnAt(z,slot,GuardKind(z),1,EliteModifier.None,false);
        if(restored&&Type(z)==ObjectiveType.DefendRelic&&started[z])RelocateDefenders(z);
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
    Vector2 DefenseApproach(int z,int ordinal){var rooms=WorldAtlas.Rooms[z];int room=ordinal%2==0?0:Mathf.Min(2,rooms.Length-1);Vector2 offset=new Vector2(ordinal<2?-1.35f:1.35f,ordinal%2==0?.8f:-.8f);return WorldAtlas.Centers[z]+WorldAtlas.Recover(z,rooms[room]+offset);}
    void RelocateDefenders(int z){int i=0;foreach(var e in d.enemies)if(e&&e.objectiveTarget&&Vector2.Distance(e.transform.position,Position(z,0))<4)e.transform.position=DefenseApproach(z,i++);defenseGraceUntil[z]=Time.time+2;}
    void SpawnDefenders(int z){int start=d.enemies.Count;d.Spawn(GuardKind(z),4,progress[z]>=15?EliteModifier.Swift:EliteModifier.None);for(int i=0;i<4;i++){var e=d.enemies[start+i];e.transform.position=DefenseApproach(z,i);e.objectiveTarget=true;}defenseGraceUntil[z]=Time.time+2;CombatFX.Ring(Position(z,0),2.6f,new Color(.25f,.9f,1),2);d.Tell("Attackers incoming from the outer chambers!");}
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
        defenseClock[z]+=seconds;int threats=0;if(Time.time>=defenseGraceUntil[z])foreach(var e in d.enemies)if(e&&e.objectiveTarget&&Vector2.Distance(e.transform.position,Position(z,0))<2.6f)threats++;
        if(threats>0){defenseDamage[z]+=threats*7*seconds;int loss=Mathf.FloorToInt(defenseDamage[z]);if(loss>0){aux[z]=Mathf.Max(0,aux[z]-loss);defenseDamage[z]-=loss;}}
        if(defenseClock[z]>=1){int whole=Mathf.FloorToInt(defenseClock[z]);defenseClock[z]-=whole;progress[z]+=whole;if(progress[z]%5==0){CombatFX.Ring(Position(z,0),1.2f,new Color(.3f,.9f,1));Save();}}
        if(aux[z]<=0){started[z]=false;progress[z]=0;aux[z]=100;foreach(var e in d.enemies)if(e&&e.objectiveTarget)e.objectiveTarget=false;d.Tell("The relic was overwhelmed. Regroup and try again.");Save();}
        else if(progress[z]>=30)Complete(z);else if(ObjectiveAlive()==0)SpawnDefenders(z);
    }
    void Update(){if(!d||AshfallBeta.Paused)return;TickDefense(Time.deltaTime);for(int z=1;z<8;z++)for(int i=0;i<4;i++)if(markers[z,i])markers[z,i].color=MarkerColor(z,i);}

    public bool MarkerLocked(int z,int slot){return slot==3&&!completed[z]&&!claimed[z];}
    public bool MarkerResolved(int z,int slot){if(slot==3)return claimed[z];if(Type(z)==ObjectiveType.CorruptedAnchors)return (progress[z]&(1<<slot))!=0;return completed[z];}

    Color MarkerColor(int z,int slot){if(slot==3)return claimed[z]?new Color(.35f,.35f,.35f):completed[z]?Color.white:new Color(.55f,.55f,.6f);if(Type(z)==ObjectiveType.CorruptedAnchors)return (progress[z]&(1<<slot))!=0?new Color(.3f,.9f,1):new Color(.72f,.3f,.92f);return completed[z]?new Color(.3f,.9f,1):ObjectiveColor(Type(z));}
    static Color ObjectiveColor(ObjectiveType t){return t==ObjectiveType.SurvivalWaves?new Color(1,.5f,.18f):t==ObjectiveType.DefendRelic?new Color(.2f,.9f,1):t==ObjectiveType.EliteHunt?new Color(1,.25f,.45f):new Color(.7f,.3f,1);}
    void Complete(int z){started[z]=false;completed[z]=true;CombatFX.Burst(Position(z,0),Color.cyan,18);d.Tell("OBJECTIVE COMPLETE — the guarded cache is unlocked.");Save();}
    void Claim(int z){claimed[z]=true;d.Add(ItemId.Gold,70+z*20);d.Add(ItemId.HealthPotion,2);d.Add(z==1?ItemId.WolfFang:z==2?ItemId.IronOre:z==3||z==5?ItemId.VenomSac:z==6?ItemId.StormCrystal:z==7?ItemId.Sunsteel:ItemId.AncientShard,4);CombatFX.Burst(Position(z,3),Color.yellow,15);d.Tell("Expedition cache claimed: gold, potions and materials.");Save();}
    void Save(){if(AshfallBeta.Instance)AshfallBeta.Instance.SaveJourney();}
    public int AnchorCount(int z){int n=0;for(int i=0;i<3;i++)if((progress[z]&(1<<i))!=0)n++;return n;}
    public int Count(int z){return AnchorCount(z);}
    public string Status(){int z=WorldAtlas.Index(d.zone);if(z==0)return Titles[0];if(claimed[z])return "Expedition cache secured";if(completed[z])return "Objective complete / cache guarded";switch(Type(z)){case ObjectiveType.CorruptedAnchors:return Titles[z]+"  "+AnchorCount(z)+" / 3";case ObjectiveType.SurvivalWaves:return started[z]?Titles[z]+"  wave "+Mathf.Min(3,progress[z]+1)+" / 3":Titles[z];case ObjectiveType.DefendRelic:return started[z]?"Relic "+aux[z]+"%  /  "+(Time.time<defenseGraceUntil[z]?"INCOMING":progress[z]+" / 30s"):Titles[z];default:return started[z]?"Marked champion active":Titles[z];}}

    public void Load(int[] savedProgress,int[] savedAux,bool[] savedStarted,bool[] savedCompleted,bool[] savedClaimed,int[] legacySeals)
    {
        progress=new int[8];aux=new int[8];started=new bool[8];completed=new bool[8];claimed=new bool[8];seals=new int[8];
        if(savedProgress!=null)Array.Copy(savedProgress,progress,Mathf.Min(8,savedProgress.Length));if(savedAux!=null)Array.Copy(savedAux,aux,Mathf.Min(8,savedAux.Length));if(savedStarted!=null)Array.Copy(savedStarted,started,Mathf.Min(8,savedStarted.Length));if(savedCompleted!=null)Array.Copy(savedCompleted,completed,Mathf.Min(8,savedCompleted.Length));if(savedClaimed!=null)Array.Copy(savedClaimed,claimed,Mathf.Min(8,savedClaimed.Length));
        if((savedProgress==null||savedProgress.Length==0)&&legacySeals!=null)for(int z=1;z<Mathf.Min(8,legacySeals.Length);z++){seals[z]=legacySeals[z]&7;if(Type(z)==ObjectiveType.CorruptedAnchors){progress[z]=seals[z];completed[z]=seals[z]==7;}else if(legacySeals[z]==7)completed[z]=true;}
        for(int z=1;z<8;z++){if(Type(z)==ObjectiveType.CorruptedAnchors)seals[z]=progress[z]&7;if(Type(z)==ObjectiveType.DefendRelic&&aux[z]<=0)aux[z]=100;if(claimed[z])completed[z]=true;}
    }
}

// Shape remains readable when colors are muted: triangle=anchor, rays=waves,
// square=relic, diamond=hunt. Caches carry a separate pixel-lock until earned.
public class ObjectiveMarkerCue : MonoBehaviour
{
    static readonly Sprite[] glyphs=new Sprite[5];static Sprite lockSprite;
    ExpeditionObjectives owner;int zone,slot;ObjectiveType type;SpriteRenderer glyph,lockIcon;float phase;
    public void Initialize(ExpeditionObjectives objectives,int z,int markerSlot,ObjectiveType objectiveType,int order){owner=objectives;zone=z;slot=markerSlot;type=objectiveType;var ground=new GameObject(markerSlot==3?"Cache ground seal":"Objective ground glyph");ground.transform.SetParent(transform,false);ground.transform.localPosition=new Vector3(0,.04f,0);glyph=ground.AddComponent<SpriteRenderer>();glyph.sprite=Glyph(markerSlot==3?ObjectiveType.None:objectiveType);glyph.sortingOrder=order;glyph.color=Tint();
        if(markerSlot==3){var locked=new GameObject("Locked cache cue");locked.transform.SetParent(transform,false);locked.transform.localPosition=new Vector3(.48f,.72f,0);locked.transform.localScale=Vector3.one*.46f;lockIcon=locked.AddComponent<SpriteRenderer>();lockIcon.sprite=Lock();lockIcon.sortingOrder=order+3;lockIcon.color=GameSettings.CueColor(new Color(1,.72f,.2f));}}
    static Sprite Glyph(ObjectiveType kind){int index=(int)kind;if(glyphs[index])return glyphs[index];const int w=32,h=16;var texture=new Texture2D(w,h,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;for(int y=0;y<h;y++)for(int x=0;x<w;x++){float nx=(x-15.5f)/15.5f,ny=(y-7.5f)/7.5f,d=Mathf.Sqrt(nx*nx+ny*ny);bool ring=d>.72f&&d<.95f,mark=false;if(kind==ObjectiveType.CorruptedAnchors)mark=y<10&&Mathf.Abs(x-16)<=y/2&&y>3;if(kind==ObjectiveType.SurvivalWaves)mark=(Mathf.Abs(x-16)<2&&y>3)||(Mathf.Abs(x-11)<2&&y>6)||(Mathf.Abs(x-21)<2&&y>6);if(kind==ObjectiveType.DefendRelic)mark=(x>11&&x<21&&(y==4||y==11))||(y>3&&y<12&&(x==11||x==20));if(kind==ObjectiveType.EliteHunt)mark=Mathf.Abs(x-16)+Mathf.Abs(y-8)>=5&&Mathf.Abs(x-16)+Mathf.Abs(y-8)<=7;if(kind==ObjectiveType.None)mark=Mathf.Abs(x-16)<2||Mathf.Abs(y-8)<1;texture.SetPixel(x,y,ring||mark?Color.white:Color.clear);}texture.Apply();glyphs[index]=Sprite.Create(texture,new Rect(0,0,w,h),Vector2.one*.5f,16);return glyphs[index];}
    static Sprite Lock(){if(lockSprite)return lockSprite;const int s=12;var texture=new Texture2D(s,s,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;for(int y=0;y<s;y++)for(int x=0;x<s;x++){bool shackle=y>=6&&y<=10&&(x==3||x==8||y==10),body=y>=1&&y<=6&&x>=2&&x<=9,hole=(x==5||x==6)&&y>=2&&y<=4;texture.SetPixel(x,y,(shackle||body)&&!hole?Color.white:Color.clear);}texture.Apply();lockSprite=Sprite.Create(texture,new Rect(0,0,s,s),Vector2.one*.5f,12);return lockSprite;}
    Color Tint(){if(slot==3)return owner&&owner.MarkerLocked(zone,slot)?new Color(.62f,.58f,.5f):new Color(1,.82f,.3f);return type==ObjectiveType.SurvivalWaves?new Color(1,.5f,.18f):type==ObjectiveType.DefendRelic?new Color(.2f,.9f,1):type==ObjectiveType.EliteHunt?new Color(1,.25f,.45f):new Color(.7f,.3f,1);}
    void Update(){if(!owner||!glyph)return;phase+=Time.deltaTime;bool resolved=owner.MarkerResolved(zone,slot);float pulse=resolved?1:1+.09f*Mathf.Sin(phase*3.5f);glyph.transform.localScale=new Vector3(1.65f,1.25f,1)*pulse;Color color=resolved?new Color(.35f,.9f,1,.34f):GameSettings.CueColor(Tint());color.a=resolved?.34f:.72f+.14f*Mathf.Sin(phase*3.5f);glyph.color=color;if(lockIcon){lockIcon.enabled=owner.MarkerLocked(zone,slot);if(lockIcon.enabled)lockIcon.color=GameSettings.CueColor(new Color(1,.72f,.2f));}}
}
