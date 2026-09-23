using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Presentation, navigation and save layer for the beta. Art is loaded from the shipped atlas.
public class AshfallBeta : MonoBehaviour
{
    public static bool Paused;
    public static AshfallBeta Instance;
    AshfallDirector d;
    public Sprite[] art=new Sprite[16];
    Texture2D atlas, vignette;
    RenderTexture canvas;
    bool title=true, pause, map;
    int tab, ringSlot;
    string lastZone="";
    Vector2 scroll,craftScroll;
    ItemId selectedItem=ItemId.HealthPotion;
    float saveAt, shake;
    GUIStyle label, small, heading, button;
    readonly Color ink=new Color(.075f,.08f,.18f,.97f), gold=new Color(1,.78f,.32f), muted=new Color(.57f,.64f,.61f);
    readonly string[] zones=WorldAtlas.Names;
    readonly Vector2[] centers=WorldAtlas.Centers;
    WorldPresentation world;
    public Vector2 Center { get {return centers[Mathf.Max(0,Array.IndexOf(zones,d.zone))];} }
    bool RigTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-rigTest")>=0;}}
    bool ItemTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-itemTest")>=0;}}
    bool WeaponTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-weaponTest")>=0;}}
    bool WorldTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-worldTest")>=0;}}
    bool DetailTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-detailTest")>=0;}}
    bool CaptureTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-betaCapture")>=0;}}
    bool BetaTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-betaTest")>=0;}}
    bool ManualTesting {get{return Array.IndexOf(Environment.GetCommandLineArgs(),"-manualTest")>=0||Application.dataPath.IndexOf("ManualPlaytest",StringComparison.OrdinalIgnoreCase)>=0;}}
    bool Testing {get{return ManualTesting||CaptureTesting||DetailTesting||WorldTesting||WeaponTesting||RigTesting||ItemTesting||BetaTesting;}}
    string SavePath { get {return Path.Combine(Application.persistentDataPath,Testing?"ashfall-qa.json":"ashfall-beta.json");} }
    string BackupPath { get {return SavePath+".last-good.bak";} }

    void Start()
    {
        Instance=this;d=GetComponent<AshfallDirector>();
        atlas=Resources.Load<Texture2D>("AshfallAtlasArcade");
        gameObject.AddComponent<CombatFX>();
        QualitySettings.vSyncCount=1;Application.targetFrameRate=-1;Application.runInBackground=true;
        atlas.filterMode=FilterMode.Point;
        for(int i=0;i<16;i++) {
            int cw=atlas.width/4,ch=atlas.height/4;
            art[i]=Sprite.Create(atlas,new Rect((i%4)*cw,(3-i/4)*ch,cw,ch),new Vector2(.5f,.12f),cw);
        }
        world=new WorldPresentation(this,d,art);
        d.objectives=gameObject.AddComponent<ExpeditionObjectives>();d.objectives.Initialize(d,art);
        d.cam.orthographicSize=5.8f;
        d.cam.backgroundColor=new Color(.075f,.095f,.095f);
        // Render at native resolution; point-filtered sprites retain pixel edges without camera quantization.
        d.cam.targetTexture=null;
        vignette=new Texture2D(128,72,TextureFormat.RGBA32,false);
        for(int y=0;y<72;y++)for(int x=0;x<128;x++){
            float v=Mathf.Clamp01((new Vector2((x-64f)/64,(y-36f)/36).magnitude-.42f)*.8f);
            vignette.SetPixel(x,y,new Color(.015f,.022f,.028f,v*.18f));
        }vignette.Apply();
        d.cam.transform.position=new Vector3(0,1,-10);
        Paused=true;Time.timeScale=0;
        if(CaptureTesting){title=false;Paused=false;Time.timeScale=1;StartCoroutine(Capture());}
        if(DetailTesting)StartCoroutine(TestDetails());else if(WorldTesting)StartCoroutine(TestWorld());else if(WeaponTesting)StartCoroutine(TestWeapons());else if(ItemTesting)StartCoroutine(TestItems());else if(RigTesting)StartCoroutine(TestRig());else if(BetaTesting)StartCoroutine(TestBeta());
    }
    IEnumerator TestDetails(){
        yield return new WaitForSecondsRealtime(1);title=false;pause=true;d.player.enabled=false;NavigationQA.Run();
        var frames=SpriteFrames.Load("AshfallHeroFrames",8,4,.82f);Check(frames.Length==32,"32 hero frames");var unique=new HashSet<Sprite>();foreach(EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))for(int frame=0;frame<4;frame++)unique.Add(SpriteFrames.Mob(kind,frame));Check(unique.Count==48,"48 monster locomotion frames");
        foreach(var node in WorldDetails.Nodes)if(node){int before=d.bag[node.item];Check(node.Harvest(d)&&d.bag[node.item]==before+2,"Harvest adds resources");Check(!node.Harvest(d),"Harvest cooldown prevents repeat");break;}
        for(int group=0;group<2;group++){foreach(var e in d.enemies)if(e)Destroy(e.gameObject);d.enemies.Clear();for(int i=0;i<6;i++){d.Spawn((EnemyKind)(group*6+i),1);var e=d.enemies[d.enemies.Count-1];e.enabled=false;e.transform.position=new Vector3(-4+i%3*4,-1.8f+i/3*3,0);}yield return null;yield return null;
            d.player.transform.position=new Vector3(0,-4,0);d.cam.orthographicSize=4.5f;d.cam.transform.position=new Vector3(0,0,-10);
            for(int frame=0;frame<4;frame++){foreach(var e in d.enemies)e.GetComponent<BetaActor>().PreviewFrame=frame;pause=false;yield return null;pause=true;yield return new WaitForEndOfFrame();foreach(var e in d.enemies)Check(e.GetComponent<BetaActor>().AnimationFrame==frame,"Monster frame playback "+e.kind+"/"+frame);ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../monster-frames-"+group+"-"+frame+".png"));yield return new WaitForSecondsRealtime(.1f);}
        }
        Debug.Log("DETAIL AND ANIMATION QA: ALL CHECKS PASSED");yield return new WaitForSecondsRealtime(.5f);Application.Quit();
    }
    IEnumerator TestWorld(){
        yield return new WaitForSecondsRealtime(1);title=false;pause=true;d.player.enabled=false;d.kills=60;d.bossState=2;
        Check(d.bag.Count==39&&d.recipes.Count==20,"Expanded content counts");
        for(int z=0;z<8;z++){
            TravelForTest(z);pause=false;yield return null;pause=true;
            Check(d.enemies.Count>=4,"Populated "+zones[z]);
            foreach(var e in d.enemies)Check(WorldAtlas.Walkable(z,(Vector2)e.transform.position-centers[z]),"Valid spawn "+e.kind);
            var exits=new HashSet<Vector2>();foreach(int n in WorldAtlas.Neighbors(z)){Check(WorldAtlas.Walkable(z,WorldAtlas.Gate(z,n)),"Reachable exit "+n);Check(exits.Add(WorldAtlas.Gate(z,n)),"Separate exit "+n);}
            Check(!WorldAtlas.Walkable(z,new Vector2(20,20)),"Irregular boundary "+z);
            var edges=WorldAtlas.Edges[z];for(int j=0;j<edges.Length;j+=2){var room=WorldAtlas.Rooms[z][edges[j]];var next=WorldAtlas.Rooms[z][edges[j+1]];var bend=new Vector2(next.x,room.y);for(int step=0;step<=20;step++){Check(WorldAtlas.Walkable(z,Vector2.Lerp(room,bend,step/20f)),"Connected trail");Check(WorldAtlas.Walkable(z,Vector2.Lerp(bend,next,step/20f)),"Connected trail");}}
            pause=false;d.cam.orthographicSize=10;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../world-"+z+".png"));pause=true;
        }
        foreach(var item in ExpansionContent.Weapons){d.bag[item]=1;d.player.EquipItem(item);Check(d.player.Damage>=19,"New weapon stats "+item);}
        var target=d.enemies[0];d.player.ModifiedDamage(target,20,ItemId.VenomSaber);Check(target.poisonUntil>Time.time,"Poison effect");d.player.ModifiedDamage(target,20,ItemId.ThunderAxe);Check(target.shockUntil>Time.time,"Shock effect");
        d.player.hp=50;target.hp=target.maxHp;target.TakeDamage(d.player.ModifiedDamage(target,20,ItemId.DuskStaff),ItemId.DuskStaff);Check(d.player.hp>50,"Drain uses dealt damage");
        pause=false;map=true;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../world-atlas.png"));
        world.local=false;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../world-routes.png"));
        Debug.Log("WORLD EXPANSION QA: ALL CHECKS PASSED");yield return new WaitForSecondsRealtime(.5f);Application.Quit();
    }
    IEnumerator TestWeapons(){
        yield return new WaitForSecondsRealtime(1);title=false;pause=false;d.player.enabled=false;
        foreach(var e in d.enemies)if(e)Destroy(e.gameObject);d.enemies.Clear();
        d.cam.orthographicSize=2.7f;var p=d.player;var rig=p.GetComponent<HeroRig>();var distinct=new HashSet<Sprite>();
        foreach(var item in WeaponArt.Items){
            d.bag[item]=1;p.EquipItem(item);rig.SetPreview(p.weapon,Vector2.right,.48f);
            yield return null;yield return new WaitForEndOfFrame();
            Check(rig.DisplayedWeapon==QuickbarSprite((int)p.weapon),"Held and hotbar match "+item);
            Check(rig.DisplayedWeapon==ItemCatalog.LootArt(item,art),"Drop artwork matches "+item);
            Check(rig.DisplayedWeapon.name==item.ToString(),"Distinct skin "+item);
            Check(rig.GripError<.001f,"Grip attached "+item);distinct.Add(rig.DisplayedWeapon);
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../skin-"+item+".png"));yield return new WaitForSecondsRealtime(.15f);
        }
        Check(distinct.Count==14,"Fourteen unique weapon sprites");p.swordSlot=ItemId.HollowBlade;p.bowSlot=ItemId.FrostBow;p.staffSlot=ItemId.CrystalStaff;Save();
        p.swordSlot=p.bowSlot=p.staffSlot=ItemId.Gold;Load();yield return null;
        Check(QuickbarSprite(0).name=="HollowBlade"&&QuickbarSprite(1).name=="FrostBow"&&QuickbarSprite(2).name=="CrystalStaff","Saved gear updates all icons");
        p.swordSlot=ItemId.Gold;Check(QuickbarSprite(0).name=="Starter Sword","Starter fallback");p.EquipItem(ItemId.HollowBlade);rig.SetPreview(WeaponKind.Sword,Vector2.right,.48f);
        yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../weapon-bar-updated.png"));
        Debug.Log("WEAPON VISUAL QA: ALL CHECKS PASSED");yield return new WaitForSecondsRealtime(.5f);Application.Quit();
    }
    IEnumerator TestItems(){
        yield return new WaitForSecondsRealtime(1);title=false;pause=true;d.player.enabled=false;
        foreach(ItemId i in Enum.GetValues(typeof(ItemId)))d.bag[i]=1000;d.bag[ItemId.Gold]=10000;
        Check(d.bag.Count==39&&d.recipes.Count==20,"39 item types and 20 recipes");
        foreach(var r in d.recipes){int before=d.bag[r.output];d.Craft(r);Check(d.bag[r.output]==before+r.amount,"Craft "+r.output);}
        var p=d.player;p.EquipItem(ItemId.IronSword);Check(p.Damage==12,"Selected gear overrides stronger owned gear");
        p.EquipItem(ItemId.EmberSword);Check(p.Damage==18,"Ember Sword damage");var target=d.enemies[0];p.ModifiedDamage(target,18);Check(target.burnUntil>Time.time,"Burn applied");
        p.EquipItem(ItemId.FrostBow);p.ModifiedDamage(target,17);Check(p.Damage==17&&target.chillUntil>Time.time,"Frost slow applied");
        p.EquipItem(ItemId.CrystalStaff);Check(p.Damage==20,"Crystal Staff damage");
        ItemCatalog.Equip(p,ItemId.IronArmor,0);Check(p.MaxHp==160,"Iron Armor health");p.hp=160;p.Hurt(20);Check(Mathf.Abs(p.hp-143)<.01f,"Armor damage reduction");
        p.stamina=10;int pots=d.bag[ItemId.StaminaPotion];Check(ItemCatalog.Use(p,ItemId.StaminaPotion)&&p.stamina==70&&d.bag[ItemId.StaminaPotion]==pots-1,"Stamina potion consumption");
        ItemCatalog.Use(p,ItemId.FuryPotion);Check(p.Damage==26,"Fury damage bonus");p.furyUntil=0;
        ItemCatalog.Equip(p,ItemId.RingHaste,0);ItemCatalog.Equip(p,ItemId.RingGuard,1);Save();string modern=File.ReadAllText(SavePath);
        p.swordSlot=ItemId.Gold;p.armorSlot=ItemId.Gold;Load();Check(p.swordSlot==ItemId.EmberSword&&p.armorSlot==ItemId.IronArmor&&p.ringA==ItemId.RingHaste,"Expanded gear save/load");
        var legacy=new SaveData{items=new int[18],armor=true};legacy.items[(int)ItemId.IronSword]=1;legacy.items[(int)ItemId.LeatherArmor]=1;legacy.items[0]=80;File.WriteAllText(SavePath,JsonUtility.ToJson(legacy));Load();Check(d.bag[ItemId.EmberShard]==0&&p.swordSlot==ItemId.IronSword&&p.MaxHp==125,"Legacy 18-item save migration");File.WriteAllText(SavePath,modern);Load();File.WriteAllText(BackupPath,modern);File.WriteAllText(SavePath,"{broken save");p.swordSlot=ItemId.Gold;Load();Check(p.swordSlot==ItemId.EmberSword&&File.ReadAllText(SavePath)==modern,"Corrupt save recovers from last good backup");
        d.Spawn(EnemyKind.Goblin,1);d.enemies[d.enemies.Count-1].TakeDamage(9999);Check(d.pickups.Exists(x=>x&&x.item==ItemId.EmberShard),"New material drops");
        pause=false;d.inventoryOpen=true;selectedItem=ItemId.EmberSword;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../expanded-inventory.png"));
        d.inventoryOpen=false;d.craftOpen=true;craftScroll=new Vector2(0,500);yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../expanded-crafting.png"));
        Debug.Log("ITEM EXPANSION QA: ALL CHECKS PASSED");yield return new WaitForSecondsRealtime(.5f);Application.Quit();
    }
    IEnumerator TestRig(){
        yield return new WaitForSecondsRealtime(1);title=false;pause=true;d.player.enabled=false;
        foreach(var e in d.enemies)if(e)Destroy(e.gameObject);d.enemies.Clear();
        d.cam.orthographicSize=1.6f;d.cam.transform.position=new Vector3(0,.55f,-10);
        yield return null;var rig=d.player.GetComponent<HeroRig>();Check(rig!=null,"Articulated rig exists");
        Vector3 first=Vector3.zero;
        Vector2[] directions={Vector2.down,Vector2.up,Vector2.right,Vector2.left};
        for(int direction=0;direction<4;direction++)for(int weapon=0;weapon<3;weapon++)for(int pose=0;pose<3;pose++){
            rig.SetPreview((WeaponKind)weapon,directions[direction],new[]{.20f,.48f,.85f}[pose]);
            yield return null;yield return new WaitForEndOfFrame();
            Check(rig.GripError<.001f,"Handle attached weapon "+weapon+" pose "+pose);
            Check(rig.FacingDirection==direction,"Correct body facing "+direction);
            Check(rig.WeaponBehindBody==(direction==1),"Correct depth layering "+direction);
            if(weapon==0&&pose==0)first=rig.GripPosition;if(weapon==0&&pose==1)Check(Vector3.Distance(first,rig.GripPosition)>.1f,"Sword hand visibly swings");
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../direction-"+direction+"-"+weapon+"-"+pose+".png"));yield return new WaitForSecondsRealtime(.1f);
        }
        for(int direction=0;direction<4;direction++)for(int frame=0;frame<8;frame++){rig.SetPreview(WeaponKind.Sword,directions[direction],1);rig.PreviewWalk=frame;yield return null;yield return new WaitForEndOfFrame();Check(rig.WalkFrame==frame,"Walk frame "+direction+"/"+frame);}
        rig.PreviewWalk=-1;
        rig.SetPreview(WeaponKind.Sword,Vector2.left,.48f);yield return null;yield return new WaitForEndOfFrame();Check(rig.GripError<.001f,"Left-facing grip");ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../rig-left.png"));
        Debug.Log("HERO RIG QA: ALL CHECKS PASSED");yield return new WaitForSecondsRealtime(.3f);Application.Quit();
    }
    IEnumerator TestBeta(){
        yield return new WaitForSecondsRealtime(2);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../beta-title.png"));
        yield return new WaitForSecondsRealtime(1);title=false;pause=true;
        foreach(ItemId i in Enum.GetValues(typeof(ItemId)))d.bag[i]=100;
        int ore=d.bag[ItemId.IronOre];d.Craft(d.recipes[0]);Check(d.bag[ItemId.IronOre]==ore-8,"Crafting consumes materials");
        var victim=d.enemies[0];d.player.ringA=ItemId.RingVampire;d.player.hp=50;victim.hp=10;victim.TakeDamage(999,ItemId.IronSword);Check(Mathf.Abs(d.player.hp-50.3f)<.01f,"Ring lifesteal excludes overkill");
        d.kills=18;d.zoneTier=3;d.objectives.seals[1]=3;d.objectives.claimed[2]=true;Save();d.kills=0;d.objectives.seals[1]=0;d.objectives.claimed[2]=false;Load();Check(d.kills==18&&d.objectives.seals[1]==3&&d.objectives.claimed[2],"Version 3 save and objective persistence");
        pause=false;d.craftOpen=true;yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../beta-crafting.png"));
        d.craftOpen=false;TravelForTest(1);pause=true;yield return new WaitForSecondsRealtime(.5f);Check(d.objectives.Guarded(1,0)&&d.objectives.Guarded(1,3),"Dedicated seal and cache guards deploy");
        var persisted=d.enemies.Find(e=>e&&e.hp>0);persisted.hp=7;EnemyKind persistedKind=persisted.kind;d.RestoreLoot(persisted.transform.position+Vector3.right,ItemId.VenomSac,2);Save();TravelForTest(0);TravelForTest(1);Check(d.enemies.Exists(e=>e&&e.kind==persistedKind&&Mathf.Abs(e.hp-7)<.01f)&&d.pickups.Exists(p=>p&&p.item==ItemId.VenomSac&&p.amount==2),"Enemy health and loose loot persist between visits");
        bool occluded=false;for(int a=0;a<WorldAtlas.Rooms[1].Length;a++)for(int b=a+1;b<WorldAtlas.Rooms[1].Length;b++)if(!CombatRules.Clear(d,centers[1]+WorldAtlas.Rooms[1][a],centers[1]+WorldAtlas.Rooms[1][b]))occluded=true;Check(occluded,"Wall trace rejects occluded room attacks");
        var cue=d.enemies.Find(e=>e&&e.hp>0);d.player.hp=10000;d.player.transform.position=centers[1];cue.transform.position=d.player.transform.position+Vector3.right*.8f;pause=false;yield return null;yield return null;pause=true;yield return null;Check(cue.AttackStage==EnemyAttackStage.Windup,"Enemy wind-up cue");pause=false;yield return new WaitForSeconds(.25f);pause=true;yield return null;Check(cue.AttackStage==EnemyAttackStage.Strike,"Enemy strike cue");pause=false;yield return new WaitForSeconds(.13f);pause=true;yield return null;Check(cue.AttackStage==EnemyAttackStage.Recovery,"Enemy recovery cue");
        cue.hp=0;d.player.transform.position=centers[1];d.combatUntil=0;string travelReason;Check(!CanTravel(0,out travelReason)&&travelReason.Contains("route marker"),"Remote map travel blocked");d.player.transform.position=centers[1]+WorldAtlas.Gate(1,0);d.combatUntil=Time.time+10;Check(!CanTravel(0,out travelReason)&&travelReason.Contains("engaged"),"Combat escape blocked");foreach(var e in d.enemies)if(e)e.hp=0;d.combatUntil=0;Check(CanTravel(0,out travelReason),"Safe gate travel allowed");
        d.player.transform.position=ExpeditionObjectives.Position(1,2);Check(d.objectives.Interact()&&d.objectives.seals[1]==7,"Final expedition seal restores");int rewardGold=d.bag[ItemId.Gold];d.player.transform.position=ExpeditionObjectives.Position(1,3);Check(d.objectives.Interact()&&d.objectives.claimed[1]&&d.bag[ItemId.Gold]>rewardGold,"Guarded expedition cache rewards");
        pause=false;map=true;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../beta-map.png"));map=false;
        TravelForTest(4);pause=true;yield return new WaitForSecondsRealtime(.5f);
        var boss=d.enemies.Find(e=>e&&e.kind==EnemyKind.HollowKnight);Check(boss!=null,"Boss spawns");
        boss.TakeDamage(170);pause=false;yield return null;yield return null;pause=true;Check(d.enemies.Count>=10,"Boss phase two summons");
        float phaseHp=boss.hp;TravelForTest(1);TravelForTest(4);boss=d.enemies.Find(e=>e&&e.kind==EnemyKind.HollowKnight);Check(boss&&boss.PhaseTwo&&Mathf.Abs(boss.hp-phaseHp)<.01f,"Boss phase and health persist between visits");
        boss.TakeDamage(1000);Check(d.bossState==2&&d.pickups.Exists(p=>p&&p.item==ItemId.HollowBlade),"Boss legendary loot");
        TravelForTest(1);pause=false;yield return null;yield return null;
        d.player.stamina=100;d.player.weapon=WeaponKind.Sword;d.player.Special();Check(d.player.stamina<70,"Whirlwind stamina cost");
        yield return new WaitForSecondsRealtime(.06f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../arcade-combat.png"));
        yield return new WaitForSeconds(2.5f);d.player.stamina=100;d.player.weapon=WeaponKind.Bow;d.player.Special();Check(FindObjectsByType<HeroProjectile>(FindObjectsSortMode.None).Length>=5,"Bow fan shot");
        yield return new WaitForSeconds(2.5f);d.player.stamina=100;d.player.weapon=WeaponKind.Staff;d.player.Special();Check(FindObjectsByType<HeroProjectile>(FindObjectsSortMode.None).Length>=8,"Staff nova");
        d.player.enabled=false;d.player.hp=100000;d.player.weapon=WeaponKind.Sword;d.Spawn(EnemyKind.Goblin,24);
        var frames=new System.Collections.Generic.List<float>();
        for(int n=0;n<600;n++){d.player.transform.position=(Vector3)Center+new Vector3(Mathf.Sin(n*.025f)*3,Mathf.Cos(n*.025f)*2,0);d.player.Attack();if(n%100==0){d.player.stamina=100;d.player.Special();}yield return null;if(n>60)frames.Add(Time.unscaledDeltaTime*1000);}
        frames.Sort();Debug.Log("ARCADE FRAME TEST: median="+frames[frames.Count/2].ToString("F2")+"ms p95="+frames[(int)(frames.Count*.95f)].ToString("F2")+"ms max="+frames[frames.Count-1].ToString("F2")+"ms");
        Debug.Log("ASHFALL BETA QA: ALL CHECKS PASSED");yield return new WaitForSecondsRealtime(1);Application.Quit();
    }
    void Check(bool ok,string name){if(!ok)throw new Exception("BETA QA FAILED: "+name);Debug.Log("BETA QA PASS: "+name);}
    IEnumerator Capture(){yield return new WaitForSecondsRealtime(1);d.kills=60;d.bossState=2;TravelForTest(1);yield return new WaitForSecondsRealtime(1);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../hud-preview.png"));var cue=d.enemies.Find(e=>e&&e.hp>0);d.player.hp=10000;d.player.transform.position=centers[1];cue.transform.position=d.player.transform.position+Vector3.right*.8f;yield return null;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../attack-windup.png"));map=true;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../hud-map.png"));yield return new WaitForSecondsRealtime(.5f);Application.Quit();}
    void Update()
    {
        if(!title&&!pause){if(Input.GetKeyDown(KeyCode.C))d.craftOpen=!d.craftOpen;if(Input.GetKeyDown(KeyCode.I))d.inventoryOpen=!d.inventoryOpen;if(Input.GetKeyDown(KeyCode.H))d.helpOpen=!d.helpOpen;}
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(d.craftOpen||d.inventoryOpen||d.helpOpen||map){d.craftOpen=d.inventoryOpen=d.helpOpen=map=false;}
            else if(!title)pause=!pause;
        }
        if(!title&&!pause&&Input.GetKeyDown(KeyCode.M))map=!map;
        Paused=title||pause||map||d.craftOpen||d.inventoryOpen||d.helpOpen;
        Time.timeScale=Paused?0:1;
        if(!Paused&&Time.time>saveAt){Save();saveAt=Time.time+30;}
        foreach(var e in d.enemies)if(e&&!e.GetComponent<BetaActor>()){
            int idx=e.kind==EnemyKind.Slime?4:e.kind==EnemyKind.Wolf?1:e.kind==EnemyKind.Skeleton?2:e.kind==EnemyKind.Goblin?3:e.kind==EnemyKind.ForestBeast?5:6;
            Dress(e.gameObject,idx,e.kind==EnemyKind.ForestBeast||e.kind==EnemyKind.HollowKnight?2.25f:1.3f);if((int)e.kind>=6){e.GetComponent<SpriteRenderer>().sprite=ExpansionContent.Monster(e.kind);e.GetComponent<BetaActor>().size=e.kind==EnemyKind.StoneGolem?1.8f:e.kind==EnemyKind.CaveBat?.85f:1.4f;}
        }
        if(!d.player.GetComponent<BetaActor>())Dress(d.player.gameObject,0,1.6f);
        foreach(var p in d.pickups)if(p&&!p.GetComponent<BetaActor>()){Dress(p.gameObject,12,.40f);p.GetComponent<SpriteRenderer>().sprite=ItemCatalog.LootArt(p.item,art);}
        world.Update();
        if(lastZone!=d.zone){lastZone=d.zone;d.Tell(d.zone=="Ruined Village"?"The last embers still burn. Explore the forest to the west.":"Entering "+d.zone);}
    }
    Vector3 cameraVelocity;
    void LateUpdate(){if(!Paused){Vector3 target=d.player.transform.position+new Vector3(0,.45f,-10);d.cam.transform.position=Vector3.SmoothDamp(d.cam.transform.position,target,ref cameraVelocity,.075f);if(CombatFX.shake>0){d.cam.transform.position+=(Vector3)UnityEngine.Random.insideUnitCircle*CombatFX.shake;CombatFX.shake=Mathf.MoveTowards(CombatFX.shake,0,Time.deltaTime*.8f);}}}
    void Dress(GameObject g,int index,float size){var r=g.GetComponent<SpriteRenderer>();r.sprite=art[index];r.color=Color.white;g.transform.localScale=Vector3.one*size;var a=g.AddComponent<BetaActor>();a.size=size;a.hero=g.GetComponent<PlayerHero>()!=null;}
    SpriteRenderer Prop(int index,Vector2 p,float size,bool flip=false){var g=new GameObject("Scenery "+index);g.transform.position=p;g.transform.localScale=Vector3.one*size;var r=g.AddComponent<SpriteRenderer>();r.sprite=art[index];r.flipX=flip;r.sortingOrder=100-(int)(p.y*10);return r;}
    public static void Slash(Vector3 p,bool magic){CombatFX.Slash(p,Vector2.right,1.7f,false,magic);}
    public static void DamageNumber(Vector3 p,int amount){CombatFX.Number(p,amount);}
    [Serializable]class SaveData{public int[] items;public int kills,tier,boss;public bool armor;public int ringA,ringB;public int version,sword,bow,staff,armorItem,weaponMode;public ZoneRecord[] encounters;public int[] seals;public bool[] claimed;}
    SaveData ReadSave(string path){var s=JsonUtility.FromJson<SaveData>(File.ReadAllText(path));if(s==null||s.items==null)throw new Exception("Invalid save data.");return s;}
    void WriteSave(string json){string temp=SavePath+".tmp";File.WriteAllText(temp,json);if(File.Exists(SavePath))File.Replace(temp,SavePath,BackupPath,true);else File.Move(temp,SavePath);}
    void Save(){try{d.journal.Capture(d);var p=d.player;var s=new SaveData{version=3,items=new int[Enum.GetValues(typeof(ItemId)).Length],kills=d.kills,tier=d.zoneTier,boss=d.bossState==2?2:0,armor=p.leatherArmor,ringA=(int)p.ringA,ringB=(int)p.ringB,sword=(int)p.swordSlot,bow=(int)p.bowSlot,staff=(int)p.staffSlot,armorItem=(int)p.armorSlot,weaponMode=(int)p.weapon,encounters=d.journal.records,seals=d.objectives?d.objectives.seals:null,claimed=d.objectives?d.objectives.claimed:null};foreach(var kv in d.bag)s.items[(int)kv.Key]=kv.Value;WriteSave(JsonUtility.ToJson(s));}catch(Exception e){Debug.LogWarning("Save unavailable: "+e.Message);}}
    void Load(){try{SaveData s;bool recovered=false;try{s=ReadSave(SavePath);}catch(Exception primary){if(!File.Exists(BackupPath))throw new Exception("Invalid save and no recovery backup.",primary);s=ReadSave(BackupPath);recovered=true;}if(recovered){if(File.Exists(SavePath)&&!File.Exists(SavePath+".corrupt.bak"))File.Copy(SavePath,SavePath+".corrupt.bak");File.Copy(BackupPath,SavePath,true);}if(s.version<2&&!File.Exists(SavePath+".pre-expansion.bak"))File.Copy(SavePath,SavePath+".pre-expansion.bak");if(s.version<3&&!File.Exists(SavePath+".pre-expedition.bak"))File.Copy(SavePath,SavePath+".pre-expedition.bak");foreach(ItemId i in Enum.GetValues(typeof(ItemId)))d.bag[i]=(int)i<s.items.Length?Mathf.Max(0,s.items[(int)i]):0;d.kills=Mathf.Max(0,s.kills);d.zoneTier=Mathf.Max(0,s.tier);d.bossState=s.boss==2?2:0;var p=d.player;p.leatherArmor=s.armor;p.ringA=ValidRing(s.ringA);p.ringB=ValidRing(s.ringB);if(s.version<2)ItemCatalog.MigrateGear(p);else{p.swordSlot=ValidWeapon(s.sword,0);p.bowSlot=ValidWeapon(s.bow,1);p.staffSlot=ValidWeapon(s.staff,2);p.armorSlot=ItemCatalog.IsArmor((ItemId)s.armorItem)&&d.Has((ItemId)s.armorItem,1)?(ItemId)s.armorItem:ItemId.Gold;p.weapon=(WeaponKind)Mathf.Clamp(s.weaponMode,0,2);}d.journal.Load(s.encounters);if(d.objectives)d.objectives.Load(s.seals,s.claimed);d.SpawnZone(zones[0]);p.transform.position=centers[0];d.cam.transform.position=new Vector3(centers[0].x,centers[0].y,-10);if(recovered)d.Tell("Recovered journey from the last good save.");}catch(Exception e){d.Tell("Could not load save: "+e.Message);}}
    public void SaveJourney(){Save();}
    ItemId ValidWeapon(int i,int kind){return ItemCatalog.WeaponType((ItemId)i)==kind&&d.Has((ItemId)i,1)?(ItemId)i:ItemId.Gold;}
    ItemId ValidRing(int i){return ItemCatalog.IsRing((ItemId)i)&&d.Has((ItemId)i,1)?(ItemId)i:ItemId.Gold;}
    void OnApplicationQuit(){if(!title)Save();Time.timeScale=1;}
    void Styles(){if(label!=null)return;label=new GUIStyle(GUI.skin.label){fontSize=17};label.normal.textColor=new Color(.87f,.87f,.78f);small=new GUIStyle(label){fontSize=13};heading=new GUIStyle(label){fontSize=25,fontStyle=FontStyle.Bold};heading.normal.textColor=gold;button=new GUIStyle(GUI.skin.button){fontSize=15,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(15,12,8,8)};}
    void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Panel(Rect r){Fill(new Rect(r.x-1,r.y-1,r.width+2,r.height+2),gold*.6f);Fill(r,ink);}
    void Text(float x,float y,string s,GUIStyle style=null,float width=650){GUI.Label(new Rect(x,y,width,s.Contains("\n")?280:Mathf.Max(40,(style??label).fontSize+15)),s,style??label);}
    public static Vector3 MouseWorld(){return AshfallDirector.I.cam.ViewportToWorldPoint(new Vector3(Input.mousePosition.x/Screen.width,Input.mousePosition.y/Screen.height,10));}
    void Bar(float x,float y,float w,float v,Color c){Fill(new Rect(x,y,w,8),new Color(.16f,.17f,.17f));Fill(new Rect(x,y,w*Mathf.Clamp01(v),8),c);}
    void Icon(int n,Rect r){GUI.DrawTextureWithTexCoords(r,atlas,new Rect((n%4)/4f,(3-n/4)/4f,.25f,.25f));}
    bool Button(Rect r,string s){return GUI.Button(r,s,button);}
    public Sprite QuickbarSprite(int slot){return WeaponArt.Get(WeaponArt.Slot(d.player,slot),(WeaponKind)slot);}
    void DrawWeaponBar(){
        Panel(new Rect(405,653,470,53));
        for(int i=0;i<3;i++){float x=412+i*116;bool active=(int)d.player.weapon==i;if(active){Fill(new Rect(x,659,112,40),gold);Fill(new Rect(x+2,661,108,36),new Color(.16f,.17f,.28f));}WeaponArt.Draw(QuickbarSprite(i),new Rect(x+5,662,29,33));Text(x+39,660,(i+1)+(active?" ACTIVE":""),small,72);Text(x+39,679,WeaponArt.SlotName(d.player,i),small,72);}
        Icon(12,new Rect(766,662,28,34));Text(800,660,"Q  "+d.bag[ItemId.HealthPotion],small,65);Text(800,679,"potions",small,65);
    }
    void OnGUI()
    {
        if(RigTesting||DetailTesting)return;
        if(!atlas)return;Styles();GUI.depth=0;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),vignette);
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
        if(title){DrawTitle();GUI.matrix=Matrix4x4.identity;return;}
        if(map){DrawMap();GUI.matrix=Matrix4x4.identity;return;}if(d.craftOpen){DrawCraft();GUI.matrix=Matrix4x4.identity;return;}if(d.inventoryOpen){DrawPack();GUI.matrix=Matrix4x4.identity;return;}if(d.helpOpen){DrawHelp();GUI.matrix=Matrix4x4.identity;return;}if(pause){DrawPause();GUI.matrix=Matrix4x4.identity;return;}
        Panel(new Rect(20,20,252,90));Text(32,28,"ASHEN   Lv. "+(1+d.kills/6),small,150);Text(197,28,Mathf.CeilToInt(d.player.hp)+" / "+d.player.MaxHp,small,70);Bar(32,51,225,d.player.hp/d.player.MaxHp,new Color(.72f,.25f,.22f));Bar(32,68,225,d.player.stamina/100,new Color(.37f,.62f,.57f));Text(32,84,d.bag[ItemId.Gold]+" gold   R "+d.bag[ItemId.StaminaPotion]+"   F "+d.bag[ItemId.FuryPotion],small,225);
        Text(540,20,d.zone.ToUpper(),small,220);Text(450,626,d.player.SkillRemaining>0?"SKILL  "+d.player.SkillRemaining.ToString("0.0")+"s":"RMB  "+(d.player.weapon==WeaponKind.Sword?"WHIRLWIND":d.player.weapon==WeaponKind.Bow?"FAN SHOT":"EMBER NOVA")+"  /  35 stamina",small,380);
        if(d.player.furyUntil>Time.time)Text(32,119,"FURY +30%  /  "+Mathf.CeilToInt(d.player.furyUntil-Time.time)+"s",small);
        Panel(new Rect(986,20,271,84));Text(999,29,"EXPEDITION",small);Text(999,51,d.objectives?d.objectives.Status():"Explore the Hollow Road",small,246);Text(999,77,"M Map   "+d.kills+" defeated",small,246);
        DrawWeaponBar();world.Mini(small);
        Text(20,699,"WASD Move  •  SPACE Dash  •  E Interact",small,375);Text(900,699,"C Craft  •  I Gear  •  ESC Pause",small,350);
        if(Time.time<d.toastUntil){Panel(new Rect(340,579,600,35));Text(355,587,d.toast,small,570);}
        foreach(var e in d.enemies)if(e&&e.hp<e.maxHp&&Vector2.Distance(e.transform.position,d.player.transform.position)<9){var p=d.cam.WorldToViewportPoint(e.transform.position+Vector3.up*1.7f);if(p.z>0)Bar(p.x*1280-24,(1-p.y)*720,48,e.hp/e.maxHp,new Color(.73f,.22f,.25f));}
        if(d.bossState==1){var boss=d.enemies.Find(e=>e&&e.kind==EnemyKind.HollowKnight);if(boss){Panel(new Rect(390,50,500,45));Text(407,56,"THE HOLLOW KNIGHT",small);Bar(407,80,466,boss.hp/boss.maxHp,new Color(.58f,.3f,.68f));}}
        GUI.matrix=Matrix4x4.identity;
    }
    void DrawTitle(){Fill(new Rect(0,0,1280,720),new Color(.015f,.025f,.026f,.48f));Panel(new Rect(72,90,442,540));Text(112,122,"A DARK FANTASY SURVIVAL RPG",small);var big=new GUIStyle(heading){fontSize=58};Text(108,167,"ASHFALL",big);Text(113,246,"FROM THE ASHES, YOU RISE.",small);Text(113,298,"A ruined village. A forest of hungry things.\nForge what you need to face the darkness.",label,365);if(Button(new Rect(113,397,355,48),"Begin journey")){title=false;d.Tell("Welcome to Ashfall. Hold left mouse to attack. Press M to travel.");}if((File.Exists(SavePath)||File.Exists(BackupPath))&&Button(new Rect(113,456,355,48),"Continue saved journey")){Load();title=false;}if(Button(new Rect(113,526,355,40),"Exit game"))Application.Quit();Text(112,590,"BETA 0.7  /  EXPEDITIONS",small);}
    void DrawPause(){Panel(new Rect(440,220,400,262));Text(471,247,"REST A MOMENT",heading);if(Button(new Rect(470,304,340,43),"Resume"))pause=false;if(Button(new Rect(470,357,340,43),"Save journey")){Save();d.Tell("Journey saved.");}if(Button(new Rect(470,410,340,43),"Save and exit")){Save();Application.Quit();}}
    void DrawCraft(){
        Panel(new Rect(230,145,820,435));Text(254,163,"FIELD CRAFTING  /  "+d.recipes.Count+" RECIPES",heading);
        Text(254,203,"Scroll to browse. Costs show owned / required. Equip crafted gear in I.",small);
        craftScroll=GUI.BeginScrollView(new Rect(252,243,774,305),craftScroll,new Rect(0,0,748,d.recipes.Count*67));
        for(int k=0;k<d.recipes.Count;k++){var r=d.recipes[k];bool can=true;string needs="";foreach(var c in r.cost){needs+=d.bag[c.Key]+"/"+c.Value+" "+d.ItemName(c.Key)+"   ";if(d.bag[c.Key]<c.Value)can=false;}GUI.enabled=can;if(Button(new Rect(2,k*67,738,59),r.title+"\n"+needs))d.Craft(r);GUI.enabled=true;}
        GUI.EndScrollView();Text(254,553,"C / ESC to close",small);
    }
    void DrawPack(){
        Panel(new Rect(180,144,920,448));Text(204,161,"PACK & LOADOUT",heading);
        Text(204,206,"Weapon: "+d.player.WeaponName+" / "+d.player.Damage+" damage",small,370);
        Text(204,232,"Armor: "+(d.player.armorSlot==ItemId.Gold?"Worn cloth":d.ItemName(d.player.armorSlot)),small,370);
        Text(204,258,"Ring I: "+RingName(d.player.ringA),small,370);Text(204,284,"Ring II: "+RingName(d.player.ringB),small,370);
        if(Button(new Rect(204,323,330,32),"Ring destination: "+(ringSlot==0?"I":"II")))ringSlot=1-ringSlot;
        Text(204,365,d.ItemName(selectedItem),heading,270);if(ItemCatalog.WeaponType(selectedItem)>=0)WeaponArt.Draw(WeaponArt.Get(selectedItem,(WeaponKind)ItemCatalog.WeaponType(selectedItem)),new Rect(492,356,38,47));Text(204,404,ItemCatalog.Description(selectedItem),new GUIStyle(small){wordWrap=true},330);
        bool usable=ItemCatalog.IsPotion(selectedItem)||ItemCatalog.IsArmor(selectedItem)||ItemCatalog.IsRing(selectedItem)||ItemCatalog.WeaponType(selectedItem)>=0;
        GUI.enabled=usable&&d.Has(selectedItem,1);
        if(Button(new Rect(204,469,330,36),ItemCatalog.IsPotion(selectedItem)?"Use selected item":"Equip selected item"))ItemCatalog.Equip(d.player,selectedItem,ringSlot);
        GUI.enabled=true;
        if(Button(new Rect(204,515,159,31),"Unequip ring")){if(ringSlot==0)d.player.ringA=ItemId.Gold;else d.player.ringB=ItemId.Gold;}
        if(Button(new Rect(375,515,159,31),"Starter weapon")){if(d.player.weapon==WeaponKind.Sword)d.player.swordSlot=ItemId.Gold;else if(d.player.weapon==WeaponKind.Bow)d.player.bowSlot=ItemId.Gold;else d.player.staffSlot=ItemId.Gold;}
        string[] filters={"All","Gear","Materials","Potions"};
        for(int i=0;i<4;i++)if(Button(new Rect(565+i*125,203,120,32),(tab==i?"> ":"")+filters[i])){tab=i;scroll=Vector2.zero;}
        scroll=GUI.BeginScrollView(new Rect(565,248,509,299),scroll,new Rect(0,0,482,Enum.GetValues(typeof(ItemId)).Length*42));
        int row=0;foreach(ItemId id in Enum.GetValues(typeof(ItemId))){if(!d.Has(id,1))continue;bool gear=ItemCatalog.IsArmor(id)||ItemCatalog.IsRing(id)||ItemCatalog.WeaponType(id)>=0,potion=ItemCatalog.IsPotion(id);if(tab==1&&!gear||tab==2&&(gear||potion)||tab==3&&!potion)continue;GUI.color=ItemCatalog.Tint(id);if(Button(new Rect(0,row++*42,480,36),(selectedItem==id?"> ":"")+d.ItemName(id)+"  x"+d.bag[id]))selectedItem=id;GUI.color=Color.white;}
        GUI.EndScrollView();Text(204,560,"Q Health potion   R Stamina potion   F Fury potion    |    I / ESC closes",small,850);
    }
    string RingName(ItemId i){return i==ItemId.Gold?"Empty":d.ItemName(i);}
    void DrawHelp(){Panel(new Rect(280,174,720,366));Text(305,194,"SURVIVAL GUIDE",heading);Text(305,247,"WASD to move. Aim with the mouse. Hold left mouse to attack.\n1 / 2 / 3 switch weapons. Space dashes. E collects loot (also picked up nearby).\nRMB: sword whirlwind / bow fan / staff nova (35 stamina).\nQ health. R stamina potion. F fury potion. C craft. I gear. M map.\n\nHunt six enemies to open the Mountain; twelve for the Wasteland;\neighteen for the Dungeon. Marsh: 8 kills; Caverns: 16.\nCitadel: 30 kills + Hollow Knight. Follow E route gates or M atlas.\n\nProgress saves every 30 seconds and on exit. Continue at the village.\nPress H or ESC to return.",label,676);}
    public void OpenMap(){map=true;}public void CloseMap(){map=false;}
    void DrawMap(){world.Draw(heading,small);}
    public bool CanTravel(int i,out string reason){if(i<0||i>=zones.Length){reason="Unknown route.";return false;}int current=WorldAtlas.Index(d.zone);if(i==current){reason="You are already in "+zones[i]+".";return false;}if(!WorldAtlas.Unlocked(i,d)){reason="Sealed: requires "+WorldAtlas.Requirements[i]+" kills"+(i==7?" and the Hollow Knight defeated.":".");return false;}if(!WorldAtlas.Neighbors(current).Contains(i)){reason="Travel through a connected route first.";return false;}if(CombatRules.InCombat(d)){reason="Cannot travel while enemies are engaged.";return false;}Vector2 gate=WorldAtlas.Centers[current]+WorldAtlas.Gate(current,i);if(Vector2.Distance(d.player.transform.position,gate)>1.8f){reason="Reach the cyan route marker to travel.";return false;}reason="Route is clear.";return true;}
    public bool TryTravel(int i){string reason;if(!CanTravel(i,out reason)){d.Tell(reason);return false;}CompleteTravel(i);return true;}
    void TravelForTest(int i){if(!Testing)throw new InvalidOperationException("Test travel is only available during QA runs.");CompleteTravel(i);}
    void CompleteTravel(int i){d.SpawnZone(zones[i]);d.player.transform.position=centers[i];if(i==0)d.player.hp=d.player.MaxHp;d.cam.transform.position=new Vector3(centers[i].x,centers[i].y,-10);Save();}

}

public class BetaActor : MonoBehaviour
{
    public float size;public bool hero;
    Vector3 last;float phase,flashUntil,dustAt;
    SpriteRenderer sprite;PlayerHero player;LootPickup loot;EnemyActor enemy;public HeroRig rig;float frameClock;public int PreviewFrame=-1;public int AnimationFrame {get;private set;}
    void Start(){last=transform.position;sprite=GetComponent<SpriteRenderer>();player=GetComponent<PlayerHero>();loot=GetComponent<LootPickup>();enemy=GetComponent<EnemyActor>();if(hero||enemy)gameObject.AddComponent<ActorShadow>();if(hero){rig=gameObject.AddComponent<HeroRig>();rig.Initialize(sprite,player);}}
    public void Flash(){flashUntil=Time.time+.12f;}
    public void Swing(Vector2 aim,bool back,bool big,bool spin=false){if(rig)rig.Play(aim,back,big,spin);}
    void LateUpdate(){
        if(!sprite)return;
        Vector3 motion=transform.position-last;bool walking=motion.sqrMagnitude>.00001f;phase+=Time.deltaTime*(walking?16:3);
        if(enemy){frameClock+=Time.deltaTime*(walking?9:2);AnimationFrame=PreviewFrame>=0?PreviewFrame%4:enemy.AttackStage==EnemyAttackStage.Windup?1:enemy.AttackStage==EnemyAttackStage.Strike?3:enemy.AttackStage==EnemyAttackStage.Recovery?0:walking||enemy.kind==EnemyKind.CaveBat||enemy.kind==EnemyKind.Slime?(int)frameClock%4:(int)frameClock%2;sprite.sprite=SpriteFrames.Mob(enemy.kind,AnimationFrame);}
        float bob=Mathf.Sin(phase)*(walking?.035f:.008f);
        float scaleX=size*(1-bob*.3f),scaleY=size*(1+bob);if(enemy&&enemy.AttackStage==EnemyAttackStage.Windup){float pulse=.04f*Mathf.Sin(Time.time*35);scaleX*=1.08f+pulse;scaleY*=.88f-pulse;}else if(enemy&&enemy.AttackStage==EnemyAttackStage.Strike){scaleX*=1.22f;scaleY*=.82f;}else if(enemy&&enemy.AttackStage==EnemyAttackStage.Recovery){scaleX*=.96f;scaleY*=1.04f;}transform.localScale=new Vector3(scaleX,scaleY,1);
        sprite.color=Time.time<flashUntil?new Color(2,1.8f,2):enemy&&enemy.AttackStage==EnemyAttackStage.Windup?new Color(1.35f,.72f,.48f):enemy&&enemy.AttackStage==EnemyAttackStage.Strike?new Color(1.6f,1.15f,.8f):enemy&&enemy.AttackStage==EnemyAttackStage.Recovery?new Color(.72f,.76f,.8f):loot&&ItemCatalog.WeaponType(loot.item)<0?ItemCatalog.Tint(loot.item):Color.white;
        sprite.sortingOrder=110-(int)(transform.position.y*10);
        if(hero&&!AshfallBeta.Paused)sprite.flipX=player.Aim.x<0;
        else if(enemy&&enemy.AttackStage!=EnemyAttackStage.None&&Mathf.Abs(enemy.AttackDirection.x)>.01f)sprite.flipX=enemy.AttackDirection.x<0;
        else if(Mathf.Abs(motion.x)>.001f)sprite.flipX=motion.x<0;
        if(walking&&!AshfallBeta.Paused&&(!enemy||enemy.AttackStage==EnemyAttackStage.None)&&Time.time>dustAt){dustAt=Time.time+.14f;CombatFX.Spark(transform.position,Vector2.up*.2f,new Color(.75f,.72f,.5f,.5f),.24f,.13f);}
        last=transform.position;
    }
}
public class BetaFloat : MonoBehaviour {float age;TextMesh text;void Start(){text=GetComponent<TextMesh>();}void Update(){if(AshfallBeta.Paused)return;age+=Time.deltaTime;transform.position+=Vector3.up*Time.deltaTime*1.1f;if(text){Color c=text.color;c.a=1-age/.55f;text.color=c;}if(age>.55f)Destroy(gameObject);}}
