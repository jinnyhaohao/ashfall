using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerHero : MonoBehaviour
{
    public AshfallDirector director;
    public float hp=100,stamina=100,MaxStamina=100;
    public bool leatherArmor;
    public ItemId swordSlot=ItemId.Gold,bowSlot=ItemId.Gold,staffSlot=ItemId.Gold,armorSlot=ItemId.Gold;
    public float furyUntil;
    public ItemId ringA=ItemId.Gold,ringB=ItemId.Gold;
    public WeaponKind weapon=WeaponKind.Sword;
    float cooldown,dodgeUntil,invulnerableUntil,skillUntil,lastAttack,ghostAt;
    int combo;
    Vector2 facing=Vector2.right,dodgeDirection,velocity;
    Vector2 attackStep;float attackStepUntil;
    public Vector2 Aim {get {Vector2 a=AshfallBeta.MouseWorld()-transform.position;return a.sqrMagnitude>.01f?a.normalized:facing;}}
    public float SkillRemaining {get{return Mathf.Max(0,skillUntil-Time.time);}}
    public bool Dodging {get{return Time.time<dodgeUntil;}}
    public float MaxHp {get{return armorSlot==ItemId.IronArmor?160:armorSlot==ItemId.BoneArmor?140:armorSlot==ItemId.LeatherArmor||leatherArmor?125:100;}}
    public string WeaponName {get{return ItemCatalog.WeaponName(this);}}
    public int Damage {get{return Mathf.CeilToInt(ItemCatalog.Damage(this)*(Time.time<furyUntil?1.3f:1));}}
    void Update()
    {
        if(AshfallBeta.Paused)return;
        Vector2 input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")).normalized;
        if(input.sqrMagnitude>.01f)facing=input;
        if(Input.GetKeyDown(KeyCode.R))ItemCatalog.Use(this,ItemId.StaminaPotion);
        if(Input.GetKeyDown(KeyCode.F))ItemCatalog.Use(this,ItemId.FuryPotion);
        if(Input.GetKeyDown(KeyCode.Alpha1))weapon=WeaponKind.Sword;
        if(Input.GetKeyDown(KeyCode.Alpha2))weapon=WeaponKind.Bow;
        if(Input.GetKeyDown(KeyCode.Alpha3))weapon=WeaponKind.Staff;
        if(Input.GetKeyDown(KeyCode.Space))Dodge(input.sqrMagnitude>.01f?input:Aim);
        velocity=Vector2.MoveTowards(velocity,input*5.2f*(HasRing(ItemId.RingHaste)?1.15f:1),Time.deltaTime*65);
        Vector2 beforeMove=transform.position;
        transform.position+=(Vector3)(Dodging?dodgeDirection*14:velocity)*Time.deltaTime;
        if(!Dodging&&Time.time<attackStepUntil)transform.position+=(Vector3)attackStep*Time.deltaTime;
        int area=WorldAtlas.Index(director.zone);Vector2 home=WorldAtlas.Centers[area];transform.position=WorldAtlas.Constrain(area,beforeMove-home,(Vector2)transform.position-home)+home;
        if(Dodging&&Time.time>ghostAt){ghostAt=Time.time+.035f;CombatFX.Ghost(GetComponent<SpriteRenderer>());}
        stamina=Mathf.Min(MaxStamina,stamina+(HasRing(ItemId.RingHaste)?33:28)*Time.deltaTime);
        hp=Mathf.Min(hp,MaxHp);
        if(!Dodging){
            if(Input.GetMouseButtonDown(1))Special();
            else if(Input.GetMouseButton(0))Attack();
        }
        if(hp<=0){hp=MaxHp;transform.position=Vector3.zero;director.zone="Ruined Village";director.SpawnZone(director.zone);director.Tell("Back at the fire. Try dodging through attacks!");}
    }
    public void Dodge(Vector2 direction){if(stamina<24||Dodging)return;stamina-=24;dodgeDirection=direction.normalized;dodgeUntil=Time.time+.19f;invulnerableUntil=dodgeUntil+.08f;CombatFX.Burst(transform.position,new Color(.4f,.85f,1),6);CombatFX.Sound(2);}
    public void EquipItem(ItemId i){int type=ItemCatalog.WeaponType(i);if(type<0||!director.Has(i,1))return;weapon=(WeaponKind)type;if(type==0)swordSlot=i;else if(type==1)bowSlot=i;else staffSlot=i;director.Tell(WeaponName+" readied.");}
    public void Attack()
    {
        if(Time.time<cooldown||AshfallBeta.Paused)return;
        Vector2 aim=Aim;
        if(weapon==WeaponKind.Sword){
            if(Time.time-lastAttack>.8f)combo=0;
            combo=combo%3+1;lastAttack=Time.time;
            bool finisher=combo==3;cooldown=Time.time+(finisher?.42f:.31f);
            var anim=GetComponent<BetaActor>();if(anim)anim.Swing(aim,combo==2,finisher);
            StartCoroutine(MeleeContact(aim,finisher,combo==2));
        }else{
            cooldown=Time.time+(weapon==WeaponKind.Bow?.31f:.35f);
            var anim=GetComponent<BetaActor>();if(anim)anim.Swing(aim,false,false);
            StartCoroutine(ReleaseShot(aim,weapon));
        }
    }
    IEnumerator MeleeContact(Vector2 aim,bool finisher,bool reverse){yield return new WaitForSeconds(finisher?.16f:.12f);if(Dodging||weapon!=WeaponKind.Sword)yield break;attackStep=aim*(finisher?3.0f:1.65f);attackStepUntil=Time.time+.10f;CombatFX.Slash(transform.position+Vector3.up*.45f,aim,finisher?2.1f:1.65f,reverse,finisher);HitArc(aim,finisher?2.4f:1.95f,finisher?1.8f:1,finisher?7:3.7f,false);CombatFX.Sound(finisher?1:0);}
    IEnumerator ReleaseShot(Vector2 aim,WeaponKind kind){yield return new WaitForSeconds(.13f);if(Dodging||weapon!=kind)yield break;Fire(aim,kind==WeaponKind.Staff,1);CombatFX.Sound(kind==WeaponKind.Bow?3:4);}
    public void Special()
    {
        if(Time.time<skillUntil||stamina<35||AshfallBeta.Paused)return;
        stamina-=35;skillUntil=Time.time+2.4f;cooldown=Time.time+.32f;
        Vector2 aim=Aim;
        var rigAnim=GetComponent<BetaActor>();if(rigAnim)rigAnim.Swing(aim,false,true,weapon==WeaponKind.Sword);
        if(weapon==WeaponKind.Sword){
            CombatFX.Slash(transform.position+Vector3.up*.3f,aim,2.8f,false,true,true);
            HitArc(aim,3.05f,2.25f,9,true);
            director.Tell("WHIRLWIND!");
        }else if(weapon==WeaponKind.Bow){
            for(int i=-2;i<=2;i++)Fire(Quaternion.Euler(0,0,i*12)*aim,false,1.2f);
            director.Tell("FAN SHOT!");
        }else{
            CombatFX.Ring(transform.position,3.2f,new Color(1,.42f,.15f));
            HitArc(aim,3.4f,2.4f,8,true);
            for(int i=0;i<8;i++)Fire(new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4)),true,.7f);
            director.Tell("EMBER NOVA!");
        }
        CombatFX.Kick(.1f);CombatFX.Sound(1);
    }
    void Fire(Vector2 direction,bool magic,float multiplier){
        var rig=GetComponent<HeroRig>();Vector2 origin=(rig?rig.GripPosition:transform.position+Vector3.up*.45f)+(Vector3)direction*(magic?.65f:.28f);if(!CombatRules.Clear(director,transform.position,origin-Vector2.up*.3f))return;var g=new GameObject(magic?"Ember":"Arrow");g.transform.position=origin;
        var p=g.AddComponent<HeroProjectile>();p.director=director;p.direction=direction;p.magic=magic;p.sourceItem=ItemCatalog.Equipped(this);p.damage=Mathf.CeilToInt(Damage*multiplier*(magic&&HasRing(ItemId.RingFlames)?1.15f:1));
    }
    public bool HasRing(ItemId x){return ringA==x||ringB==x;}
    public int ModifiedDamage(EnemyActor e,int value,ItemId? source=null){ItemId used=source??ItemCatalog.Equipped(this);if((e.kind==EnemyKind.Wolf||e.kind==EnemyKind.ForestBeast)&&HasRing(ItemId.RingHunter))value=Mathf.CeilToInt(value*1.1f);if(used==ItemId.HollowBlade&&(e.kind==EnemyKind.Skeleton||e.kind==EnemyKind.HollowKnight))value=Mathf.CeilToInt(value*1.4f);if(used==ItemId.FangSword&&UnityEngine.Random.value<.15f)value*=2;if(used==ItemId.EmberSword)e.burnUntil=Time.time+3;if(used==ItemId.FrostBow)e.chillUntil=Time.time+2;if(used==ItemId.VenomSaber||used==ItemId.ThornBow)e.poisonUntil=Time.time+4;if(used==ItemId.ThunderAxe||used==ItemId.StormBow)e.shockUntil=Time.time+.3f;if(used==ItemId.SunsteelBlade&&(e.kind==EnemyKind.Skeleton||e.kind==EnemyKind.IceWraith||e.kind==EnemyKind.DarkKnight||e.kind==EnemyKind.HollowKnight))value=Mathf.CeilToInt(value*1.5f);return value;}
    void HitArc(Vector2 aim,float range,float multiplier,float force,bool radial){
        for(int i=director.enemies.Count-1;i>=0;i--){
            var e=director.enemies[i];if(!e)continue;Vector2 delta=e.transform.position-transform.position;
            if(delta.magnitude>range||(!radial&&delta.sqrMagnitude>.2f&&Vector2.Dot(delta.normalized,aim)<-.1f))continue;
            if(!CombatRules.Clear(director,transform.position,e.transform.position))continue;
            e.Impact(delta.normalized*force);
            e.TakeDamage(ModifiedDamage(e,Mathf.CeilToInt(Damage*multiplier)),ItemCatalog.Equipped(this));
        }
    }
    public void Hurt(float damage){if(Time.time<invulnerableUntil)return;director.combatUntil=Time.time+6;hp-=damage*(HasRing(ItemId.RingGuard)?.85f:1)*(armorSlot==ItemId.IronArmor?.85f:armorSlot==ItemId.BoneArmor?.92f:1);invulnerableUntil=Time.time+.5f;var a=GetComponent<BetaActor>();if(a)a.Flash();CombatFX.Burst(transform.position+Vector3.up*.4f,new Color(1,.25f,.4f),8);CombatFX.Kick(.07f);CombatFX.Sound(5);}
    public void Lifesteal(float damage){if(HasRing(ItemId.RingVampire))hp=Mathf.Min(MaxHp,hp+damage*.03f);}
}

public class HeroProjectile : MonoBehaviour
{
    public AshfallDirector director;public Vector2 direction;public int damage;public bool magic;public ItemId sourceItem;
    float life=1.3f,trailAt;
    void Start(){var r=gameObject.AddComponent<SpriteRenderer>();r.sprite=magic?CombatFX.Orb:CombatFX.Arrow;r.sortingOrder=1800;transform.localScale=Vector3.one*(magic?.4f:.65f);transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);}
    void Update(){
        if(AshfallBeta.Paused)return;
        Vector2 before=transform.position;transform.position+=(Vector3)direction*(magic?12:17)*Time.deltaTime;
        Vector2 clipped;bool clear=CombatRules.Trace(director,before-Vector2.up*.3f,(Vector2)transform.position-Vector2.up*.3f,out clipped);transform.position=clipped+Vector2.up*.3f;
        life-=Time.deltaTime;if(life<0){Destroy(gameObject);return;}
        if(magic&&Time.time>trailAt){trailAt=Time.time+.035f;CombatFX.Spark(transform.position,-direction*1.5f,new Color(1,.4f,.15f),.18f,.14f);}
        Vector2 segment=(Vector2)transform.position-before;
        for(int i=director.enemies.Count-1;i>=0;i--){
            var e=director.enemies[i];if(!e)continue;Vector2 target=(Vector2)e.transform.position+Vector2.up*.3f;
            float t=Mathf.Clamp01(Vector2.Dot(target-before,segment)/Mathf.Max(.00001f,segment.sqrMagnitude));
            if(Vector2.Distance(before+segment*t,target)>.55f)continue;
            e.Impact(direction*(magic?3:4));e.TakeDamage(director.player.ModifiedDamage(e,damage,sourceItem),sourceItem);
            if(magic){CombatFX.Ring(transform.position,1.15f,new Color(1,.55f,.15f));for(int j=director.enemies.Count-1;j>=0;j--){var other=director.enemies[j];if(other&&other!=e&&Vector2.Distance(other.transform.position,transform.position)<1.3f&&CombatRules.Clear(director,(Vector2)transform.position-Vector2.up*.3f,other.transform.position))other.TakeDamage(Mathf.CeilToInt(damage*.45f));}}
            Destroy(gameObject);return;
        }
        if(!clear){CombatFX.Burst(transform.position,new Color(.6f,.7f,.8f),3);Destroy(gameObject);}
    }
}

public enum EnemyAttackStage { None, Windup, Strike, Recovery }

public class EnemyActor : MonoBehaviour
{
    public AshfallDirector director;public EnemyKind kind;public float hp,maxHp,speed,damage;
    public float burnUntil,chillUntil,poisonUntil,shockUntil;float nextBurn,nextPoison,nextRanged,nextSlam;
    float nextAttack,nextDash,nextGround,stunUntil,windup,dashUntil,stageUntil;bool phaseTwo,preparing,dashHit;Vector2 knock,dash;int attackToken;
    public EnemyAttackStage AttackStage {get;private set;}public Vector2 AttackDirection {get;private set;}public bool PhaseTwo {get{return phaseTwo;}}
    public void Setup(bool elite){switch(kind){case EnemyKind.Slime:maxHp=24;speed=1.5f;damage=6;break;case EnemyKind.Wolf:maxHp=38;speed=2.8f;damage=9;break;case EnemyKind.Skeleton:maxHp=48;speed=1.7f;damage=11;break;case EnemyKind.Goblin:maxHp=34;speed=2.2f;damage=8;break;case EnemyKind.ForestBeast:maxHp=110;speed=1.3f;damage=17;break;default:maxHp=300;speed=2.3f;damage=18;break;}ExpansionContent.Setup(this);if(elite&&kind!=EnemyKind.HollowKnight){maxHp*=1.35f;damage*=1.18f;}hp=maxHp;nextDash=Time.time+2;}
    void Cue(EnemyAttackStage stage,float duration,Vector2 direction){AttackStage=stage;stageUntil=Time.time+duration;if(direction.sqrMagnitude>.01f)AttackDirection=direction.normalized;}
    public void RestorePhase(bool value){phaseTwo=value&&kind==EnemyKind.HollowKnight;if(phaseTwo){speed=Mathf.Max(speed,3.1f);damage=Mathf.Max(damage,24);nextGround=Time.time+1.2f;}}
    void Update(){
        if(AshfallBeta.Paused||!director||!director.player)return;
        if(AttackStage==EnemyAttackStage.Recovery&&Time.time>=stageUntil)AttackStage=EnemyAttackStage.None;
        if(Time.time<burnUntil&&Time.time>=nextBurn){nextBurn=Time.time+.6f;TakeDamage(2);if(hp<=0)return;CombatFX.Spark(transform.position,Vector2.up,new Color(1,.4f,.2f));}
        Vector2 prior=transform.position;
        if(Time.time<poisonUntil&&Time.time>=nextPoison){nextPoison=Time.time+.6f;TakeDamage(3);if(hp<=0)return;CombatFX.Spark(transform.position,Vector2.up,Color.green);}
        if(Time.time<shockUntil)return;
        transform.position+=(Vector3)knock*Time.deltaTime;knock=Vector2.MoveTowards(knock,Vector2.zero,30*Time.deltaTime);
        float dist=Vector2.Distance(transform.position,director.player.transform.position);
        int navArea=WorldAtlas.Index(director.zone);Vector2 toward=WorldAtlas.Chase(navArea,(Vector2)transform.position-WorldAtlas.Centers[navArea],(Vector2)director.player.transform.position-WorldAtlas.Centers[navArea]);
        if(kind==EnemyKind.HollowKnight&&hp<maxHp*.5f&&!phaseTwo){phaseTwo=true;speed=3.1f;damage=24;nextGround=Time.time+1.2f;director.Tell("HOLLOW KNIGHT: PHASE TWO");director.Spawn(EnemyKind.Skeleton,3,true);}
        if(Time.time<stunUntil){Vector2 c=WorldAtlas.Centers[navArea];transform.position=WorldAtlas.Constrain(navArea,prior-c,(Vector2)transform.position-c)+c;return;}
        if(dashUntil>0&&Time.time<dashUntil){transform.position+=(Vector3)dash*Time.deltaTime*11;if(!dashHit&&Vector2.Distance(transform.position,director.player.transform.position)<1.05f&&CombatRules.Clear(director,transform.position,director.player.transform.position)){dashHit=true;nextAttack=Time.time+.65f;director.player.Hurt(damage);}}
        else if(dashUntil>0){dashUntil=0;Cue(EnemyAttackStage.Recovery,.34f,dash);}
        else if(!preparing&&AttackStage==EnemyAttackStage.None&&dist<11&&dist>((kind==EnemyKind.MushroomShaman||kind==EnemyKind.IceWraith)?4:.7f)){
            Vector2 separation=Vector2.zero;
            foreach(var other in director.enemies)if(other&&other!=this){Vector2 delta=transform.position-other.transform.position;if(delta.sqrMagnitude<.65f&&delta.sqrMagnitude>.001f)separation+=delta.normalized*.8f;}
            transform.position+=(Vector3)(toward*speed*(Time.time<chillUntil?.55f:1)+separation)*Time.deltaTime;
        }
        if((kind==EnemyKind.Wolf||kind==EnemyKind.HollowKnight||kind==EnemyKind.CaveBat||kind==EnemyKind.Scorpion||kind==EnemyKind.DarkKnight)&&Time.time>nextDash&&dist>2&&dist<7&&!preparing&&AttackStage==EnemyAttackStage.None){preparing=true;windup=Time.time+.45f;dash=toward;nextDash=Time.time+3.2f;Cue(EnemyAttackStage.Windup,.45f,dash);CombatFX.Telegraph(transform.position,(Vector2)transform.position+dash*3.8f,.45f);}
        if(preparing&&Time.time>windup){preparing=false;dashHit=false;dashUntil=Time.time+.3f;Cue(EnemyAttackStage.Strike,.3f,dash);}
        if(kind==EnemyKind.HollowKnight&&phaseTwo&&Time.time>nextGround&&AttackStage==EnemyAttackStage.None){nextGround=Time.time+2.8f;StartCoroutine(Slam());}
        if((kind==EnemyKind.MushroomShaman||kind==EnemyKind.IceWraith)&&dist<9&&Time.time>nextRanged&&AttackStage==EnemyAttackStage.None){nextRanged=Time.time+2.2f;StartCoroutine(Cast(toward));}
        if((kind==EnemyKind.StoneGolem||kind==EnemyKind.DarkKnight)&&dist<3.5f&&Time.time>nextSlam&&AttackStage==EnemyAttackStage.None){nextSlam=Time.time+4;StartCoroutine(Slam());}
        int area=WorldAtlas.Index(director.zone);Vector2 home=WorldAtlas.Centers[area];transform.position=WorldAtlas.Constrain(area,prior-home,(Vector2)transform.position-home)+home;
        if(dist<1&&Time.time>nextAttack&&AttackStage==EnemyAttackStage.None&&!preparing&&dashUntil<=0){nextAttack=Time.time+(kind==EnemyKind.Wolf?.9f:1.25f);StartCoroutine(MeleeStrike(toward));}
    }
    System.Collections.IEnumerator MeleeStrike(Vector2 aim){int token=++attackToken;Cue(EnemyAttackStage.Windup,.24f,aim);CombatFX.Ring(transform.position,.75f,new Color(1,.35f,.2f),.24f);yield return new WaitForSeconds(.24f);if(hp<=0||token!=attackToken)yield break;Cue(EnemyAttackStage.Strike,.11f,aim);if(Vector2.Distance(transform.position,director.player.transform.position)<1.25f&&CombatRules.Clear(director,transform.position,director.player.transform.position))director.player.Hurt(damage);yield return new WaitForSeconds(.11f);if(hp>0&&token==attackToken)Cue(EnemyAttackStage.Recovery,.3f,aim);}
    System.Collections.IEnumerator Cast(Vector2 aim){int token=++attackToken;Cue(EnemyAttackStage.Windup,.45f,aim);CombatFX.Ring(transform.position,.9f,Color.cyan,.45f);yield return new WaitForSeconds(.45f);if(hp<=0||token!=attackToken)yield break;Cue(EnemyAttackStage.Strike,.12f,aim);var g=new GameObject("Enemy spell");g.transform.position=transform.position+Vector3.up*.3f;var b=g.AddComponent<EnemyBolt>();b.director=director;b.velocity=aim*5.5f;b.damage=damage;b.color=kind==EnemyKind.IceWraith?Color.cyan:new Color(.5f,1,.2f);yield return new WaitForSeconds(.12f);if(hp>0&&token==attackToken)Cue(EnemyAttackStage.Recovery,.32f,aim);}
    System.Collections.IEnumerator Slam(){int token=++attackToken;Vector2 aim=(director.player.transform.position-transform.position).normalized;Cue(EnemyAttackStage.Windup,.5f,aim);CombatFX.Ring(transform.position,3.1f,new Color(1,.2f,.55f),.5f);yield return new WaitForSeconds(.5f);if(hp<=0||token!=attackToken)yield break;Cue(EnemyAttackStage.Strike,.14f,aim);CreateGroundPulse();yield return new WaitForSeconds(.14f);if(hp>0&&token==attackToken)Cue(EnemyAttackStage.Recovery,.4f,aim);}
    public void Impact(Vector2 force){knock=force*(kind==EnemyKind.HollowKnight?.35f:1);stunUntil=Time.time+.13f;preparing=false;dashUntil=0;attackToken++;AttackStage=EnemyAttackStage.None;var a=GetComponent<BetaActor>();if(a)a.Flash();}
    void CreateGroundPulse(){var g=new GameObject("Gravefall");g.transform.position=transform.position;var p=g.AddComponent<GroundPulse>();p.director=director;p.damage=20;CombatFX.Ring(transform.position,3.1f,new Color(.9f,.25f,1),.75f);director.Tell("GRAVEFALL! Dodge out of the circle.");}
    public void TakeDamage(int d,ItemId? source=null){if(hp<=0||d<=0)return;float dealt=Mathf.Min(hp,d);hp-=dealt;director.combatUntil=Time.time+6;if(source==ItemId.DuskStaff)director.player.hp=Mathf.Min(director.player.MaxHp,director.player.hp+dealt*.05f);AshfallBeta.DamageNumber(transform.position,Mathf.CeilToInt(dealt));CombatFX.Burst(transform.position+Vector3.up*.4f,new Color(1,.85f,.35f),7);CombatFX.Kick(.025f);CombatFX.Sound(6);director.player.Lifesteal(dealt);if(hp<=0){CombatFX.Burst(transform.position,new Color(.6f,.45f,.85f),12);director.EnemyDied(this);}}
}
public class GroundPulse : MonoBehaviour
{
    public AshfallDirector director;public float damage;float age;
    void Update(){if(AshfallBeta.Paused)return;age+=Time.deltaTime;if(age>.7f){CombatFX.Ring(transform.position,3.1f,new Color(1,.4f,.8f),.25f);if(Vector2.Distance(transform.position,director.player.transform.position)<3.1f&&CombatRules.Clear(director,transform.position,director.player.transform.position))director.player.Hurt(damage);Destroy(gameObject);}}
}
public class LootPickup : MonoBehaviour
{
    public AshfallDirector director;public ItemId item;public int amount;
    void Update(){if(AshfallBeta.Paused)return;float dist=Vector2.Distance(transform.position,director.player.transform.position);if(dist<1.6f)transform.position=Vector3.MoveTowards(transform.position,director.player.transform.position,Time.deltaTime*8);if(dist<.45f){director.Add(item,amount);director.pickups.Remove(this);CombatFX.Sound(7);Destroy(gameObject);}}
}
// Ashfall is intentionally self-contained: it creates its pixel-styled world and UI at runtime.
// Drop this into a blank Unity 6 2D project, press Play, and no external art or packages are needed.
public static class AshfallBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateGame()
    {
        if (UnityEngine.Object.FindFirstObjectByType<AshfallDirector>() == null)
            new GameObject("ASHFALL — Game Director").AddComponent<AshfallDirector>();
    }
}

public enum ItemId { Gold, SlimeGel, RedHerb, WolfHide, WolfFang, Bone, RustedMetal, IronOre, GoblinCleaver, IronSword, FangSword, BoneBow, LeatherArmor, HealthPotion, HollowBlade, RingFlames, RingHunter, RingVampire, EmberShard, AncientShard, Moonleaf, EmberSword, FrostBow, CrystalStaff, BoneArmor, IronArmor, StaminaPotion, FuryPotion, RingHaste, RingGuard, VenomSac, StormCrystal, Sunsteel, VenomSaber, ThunderAxe, SunsteelBlade, ThornBow, StormBow, DuskStaff }
public enum WeaponKind { Sword, Bow, Staff }
public enum EnemyKind { Slime, Wolf, Skeleton, Goblin, ForestBeast, HollowKnight, StoneGolem, CaveBat, Scorpion, MushroomShaman, IceWraith, DarkKnight }

[Serializable] public class Recipe
{
    public ItemId output; public int amount; public string title; public Dictionary<ItemId, int> cost;
    public Recipe(ItemId o, int a, string t, params object[] values) { output=o; amount=a; title=t; cost=new Dictionary<ItemId,int>(); for(int i=0;i<values.Length;i+=2) cost[(ItemId)values[i]]=(int)values[i+1]; }
}

public class AshfallDirector : MonoBehaviour
{
    public static AshfallDirector I;
    public Sprite pixel;
    public PlayerHero player;
    public Camera cam;
    public Dictionary<ItemId,int> bag = new Dictionary<ItemId,int>();
    public List<Recipe> recipes = new List<Recipe>();
    public List<EnemyActor> enemies = new List<EnemyActor>();
    public List<LootPickup> pickups = new List<LootPickup>();
    public string zone = "Ruined Village";
    public string toast = "Wake, Ashen. The village is not safe.";
    public float toastUntil;
    public bool craftOpen, inventoryOpen, helpOpen;
    public EncounterJournal journal=new EncounterJournal();public float combatUntil;public ExpeditionObjectives objectives;
    public int kills, zoneTier, bossState; // bossState: 0 not spawned, 1 alive, 2 defeated
    void Awake()
    {
        I=this;
        Application.targetFrameRate=60;
        pixel=MakePixel();
        SetupCamera(); SetupInventory(); BuildWorld(); SpawnZone("Ruined Village");
        player = Make<PlayerHero>("Ashen", Vector2.zero, new Color(0.95f,0.75f,0.35f), 0.65f);
        player.director=this;
        gameObject.AddComponent<AshfallBeta>();
        Tell("WASD move • Click attack • 1/2/3 weapons • Space dodge • E loot");
    }

    Sprite MakePixel()
    {
        var t=new Texture2D(1,1,TextureFormat.RGBA32,false); t.SetPixel(0,0,Color.white); t.filterMode=FilterMode.Point; t.wrapMode=TextureWrapMode.Clamp; t.Apply();
        return Sprite.Create(t,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
    }
    void SetupCamera()
    {
        var g=new GameObject("Pixel Camera"); g.tag="MainCamera"; cam=g.AddComponent<Camera>(); cam.orthographic=true; cam.orthographicSize=8.5f; cam.backgroundColor=new Color(.035f,.045f,.075f); g.transform.position=new Vector3(0,0,-10);
    }
    void SetupInventory()
    {
        foreach(ItemId i in Enum.GetValues(typeof(ItemId))) bag[i]=0;
        bag[ItemId.Gold]=45; bag[ItemId.HealthPotion]=2; bag[ItemId.WolfFang]=2; bag[ItemId.WolfHide]=4; bag[ItemId.SlimeGel]=3; bag[ItemId.RedHerb]=3; bag[ItemId.Bone]=3; bag[ItemId.IronOre]=4;
        recipes.Add(new Recipe(ItemId.IronSword,1,"Iron Sword  •  damage 12",ItemId.IronOre,8,ItemId.WolfFang,2,ItemId.Gold,100));
        recipes.Add(new Recipe(ItemId.LeatherArmor,1,"Leather Armor  •  +25 max HP",ItemId.WolfHide,10,ItemId.Bone,5));
        recipes.Add(new Recipe(ItemId.HealthPotion,2,"Health Potions ×2",ItemId.RedHerb,2,ItemId.SlimeGel,1));
        recipes.Add(new Recipe(ItemId.FangSword,1,"Fang Sword  •  damage 17 / crit",ItemId.WolfFang,5,ItemId.IronOre,4,ItemId.Gold,75));
        recipes.Add(new Recipe(ItemId.BoneBow,1,"Bone Bow  •  damage 15",ItemId.Bone,8,ItemId.RustedMetal,3,ItemId.Gold,60));
        ItemCatalog.Recipes(this);ExpansionContent.Recipes(this);
    }
    T Make<T>(string title,Vector2 pos,Color c,float scale) where T:MonoBehaviour
    {
        var g=new GameObject(title); g.transform.position=pos; var r=g.AddComponent<SpriteRenderer>(); r.sprite=pixel; r.color=c; r.sortingOrder=4; g.transform.localScale=Vector3.one*scale; return g.AddComponent<T>();
    }
    void Block(string n,Vector2 p,Vector2 size,Color c,int order=0)
    {
        var g=new GameObject(n);g.transform.position=p;var r=g.AddComponent<SpriteRenderer>();r.sprite=pixel;r.color=c;r.sortingOrder=order;g.transform.localScale=size;
    }
    void Label(string words,Vector2 at,Color color)
    {
        var g=new GameObject(words);g.transform.position=at;var t=g.AddComponent<TextMesh>();t.text=words;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.fontSize=48;t.characterSize=.11f;t.color=color;t.fontStyle=FontStyle.Bold;t.GetComponent<MeshRenderer>().sortingOrder=7;
    }
    void BuildWorld()
    {
    }
    void Update()
    {
        if(player==null)return;
        if(AshfallBeta.Paused)return;
        if(Input.GetKeyDown(KeyCode.E)) CollectNearby();
        if(Input.GetKeyDown(KeyCode.Q)) DrinkPotion();
        if(Input.GetKeyDown(KeyCode.Tab)) AshfallBeta.Instance.OpenMap();

    }
    public void SpawnZone(string z)
    {
        journal.Capture(this);if(bossState==1)bossState=0;
        foreach(var e in new List<EnemyActor>(enemies))if(e)Destroy(e.gameObject);
        enemies.Clear();
        foreach(var p in new List<LootPickup>(pickups))if(p)Destroy(p.gameObject);
        pickups.Clear();
        zone=z;int area=WorldAtlas.Index(z);
        foreach(var bolt in FindObjectsByType<EnemyBolt>(FindObjectsSortMode.None))Destroy(bolt.gameObject);
        foreach(var shot in FindObjectsByType<HeroProjectile>(FindObjectsSortMode.None))Destroy(shot.gameObject);
        foreach(var pulse in FindObjectsByType<GroundPulse>(FindObjectsSortMode.None))Destroy(pulse.gameObject);
        if(journal.Restore(this,area))return;
        EnemyKind[][] roster={
            new[]{EnemyKind.Slime,EnemyKind.Goblin},
            new[]{EnemyKind.Wolf,EnemyKind.Slime,EnemyKind.Goblin,EnemyKind.MushroomShaman,EnemyKind.ForestBeast},
            new[]{EnemyKind.StoneGolem,EnemyKind.CaveBat,EnemyKind.Skeleton},
            new[]{EnemyKind.Scorpion,EnemyKind.DarkKnight,EnemyKind.Goblin,EnemyKind.Skeleton},
            new[]{EnemyKind.Skeleton,EnemyKind.IceWraith,EnemyKind.DarkKnight},
            new[]{EnemyKind.MushroomShaman,EnemyKind.Slime,EnemyKind.Scorpion,EnemyKind.Wolf},
            new[]{EnemyKind.IceWraith,EnemyKind.CaveBat,EnemyKind.StoneGolem},
            new[]{EnemyKind.DarkKnight,EnemyKind.IceWraith,EnemyKind.StoneGolem,EnemyKind.Scorpion}};
        foreach(var kind in roster[area])Spawn(kind,area==0?2:area==7?5:4,area==4||area==7);
        if(z=="Dungeon"&&bossState==0){Spawn(EnemyKind.HollowKnight,1,true);bossState=1;Tell("THE HOLLOW KNIGHT AWAKENS.");}
        foreach(var bolt in FindObjectsByType<EnemyBolt>(FindObjectsSortMode.None))Destroy(bolt.gameObject);
    }

    public void Spawn(EnemyKind kind,int count,bool elite=false)
    {
        int area=WorldAtlas.Index(zone);Vector2 home=WorldAtlas.Centers[area]; for(int i=0;i<count;i++){ Vector2 p=home+WorldAtlas.SpawnPoint(area,enemies.Count); var e=Make<EnemyActor>(kind.ToString(),p,EnemyColor(kind),kind==EnemyKind.ForestBeast?1.25f:kind==EnemyKind.HollowKnight?1.5f:.7f);e.director=this;e.kind=kind;e.Setup(elite);enemies.Add(e); }
    }
    Color EnemyColor(EnemyKind e){ switch(e){case EnemyKind.Slime:return new Color(.3f,.9f,.5f);case EnemyKind.Wolf:return new Color(.55f,.45f,.35f);case EnemyKind.Skeleton:return new Color(.85f,.85f,.75f);case EnemyKind.Goblin:return new Color(.55f,.8f,.22f);case EnemyKind.ForestBeast:return new Color(.2f,.5f,.25f);default:return new Color(.72f,.35f,.9f);} }
    public void Add(ItemId item,int amount){ bag[item]+=amount; Tell("+"+amount+" "+ItemName(item)); }
    public bool Has(ItemId item,int n){return bag.ContainsKey(item)&&bag[item]>=n;}
    public string ItemName(ItemId i){return ItemCatalog.Name(i);}
    public void CollectNearby(){ foreach(var p in new List<LootPickup>(pickups))if(p&&Vector2.Distance(player.transform.position,p.transform.position)<2){Add(p.item,p.amount);pickups.Remove(p);Destroy(p.gameObject);} }
    public void DrinkPotion(){if(!ItemCatalog.Use(player,ItemId.HealthPotion))Tell("No potion needed or none available.");}
    public void Craft(Recipe r){ foreach(var c in r.cost) if(!Has(c.Key,c.Value)){Tell("Missing "+ItemName(c.Key)+" ×"+(c.Value-bag[c.Key]));return;} foreach(var c in r.cost)bag[c.Key]-=c.Value;bag[r.output]+=r.amount;Tell("Crafted "+ItemName(r.output)+(r.amount>1?" ×"+r.amount:"")); }
    public void Tell(string message){toast=message;toastUntil=Time.time+3.5f;}
    public void EnemyDied(EnemyActor e)
    {
        enemies.Remove(e); kills++; if(kills>=6)zoneTier=Mathf.Max(zoneTier,1);if(kills>=12)zoneTier=Mathf.Max(zoneTier,2);if(kills>=18)zoneTier=Mathf.Max(zoneTier,3);
        if(e.kind==EnemyKind.HollowKnight){bossState=2; Drop(e.transform.position,ItemId.HollowBlade,1);Drop(e.transform.position+Vector3.right,ItemId.RingVampire,1);Tell("VICTORY — Hollow Blade gained: +40% damage to undead.");}
        else { DropTable(e); if(UnityEngine.Random.value<.18f)Drop((Vector2)e.transform.position+UnityEngine.Random.insideUnitCircle,ItemId.RedHerb,1); }
        Destroy(e.gameObject);
    }
    void DropTable(EnemyActor e)
    {
        if(e.kind==EnemyKind.Scorpion||e.kind==EnemyKind.MushroomShaman){Drop(e.transform.position,ItemId.VenomSac,UnityEngine.Random.Range(1,3));if(UnityEngine.Random.value<.07f)Drop(e.transform.position,ItemId.VenomSaber,1);}
        if(e.kind==EnemyKind.IceWraith||e.kind==EnemyKind.CaveBat){Drop(e.transform.position,ItemId.StormCrystal,e.kind==EnemyKind.IceWraith?2:1);if(UnityEngine.Random.value<.05f)Drop(e.transform.position,ItemId.StormBow,1);}
        if(e.kind==EnemyKind.StoneGolem){Drop(e.transform.position,ItemId.IronOre,3);Drop(e.transform.position,ItemId.AncientShard,1);if(UnityEngine.Random.value<.05f)Drop(e.transform.position,ItemId.ThunderAxe,1);}
        if(e.kind==EnemyKind.DarkKnight){Drop(e.transform.position,ItemId.Sunsteel,2);Drop(e.transform.position,ItemId.RustedMetal,2);if(UnityEngine.Random.value<.04f)Drop(e.transform.position,ItemId.DuskStaff,1);}

        if(e.kind==EnemyKind.Goblin)Drop(e.transform.position,ItemId.EmberShard,1);
        if(e.kind==EnemyKind.Wolf||(e.kind==EnemyKind.Slime&&UnityEngine.Random.value<.35f))Drop(e.transform.position,ItemId.Moonleaf,1);
        if(e.kind==EnemyKind.ForestBeast)Drop(e.transform.position,ItemId.AncientShard,2);
        if(e.kind==EnemyKind.Skeleton&&UnityEngine.Random.value<.45f)Drop(e.transform.position,ItemId.AncientShard,1);
        int gold=UnityEngine.Random.Range(e.kind==EnemyKind.Skeleton?10:5,e.kind==EnemyKind.Skeleton?26:16); Drop(e.transform.position,ItemId.Gold,gold);
        if(e.kind==EnemyKind.Slime){Drop(e.transform.position+Vector3.right*.5f,ItemId.SlimeGel,UnityEngine.Random.Range(1,3));if(UnityEngine.Random.value<.04f)Drop(e.transform.position+Vector3.left*.4f,ItemId.RingFlames,1);}
        if(e.kind==EnemyKind.Wolf){Drop(e.transform.position+Vector3.right*.4f,ItemId.WolfHide,UnityEngine.Random.Range(1,3));Drop(e.transform.position+Vector3.left*.4f,ItemId.WolfFang,UnityEngine.Random.Range(1,3));}
        if(e.kind==EnemyKind.Skeleton){Drop(e.transform.position+Vector3.right*.4f,ItemId.Bone,UnityEngine.Random.Range(1,3));Drop(e.transform.position+Vector3.left*.4f,ItemId.RustedMetal,UnityEngine.Random.Range(1,3));}
        if(e.kind==EnemyKind.Goblin){Drop(e.transform.position+Vector3.right*.4f,ItemId.IronOre,UnityEngine.Random.Range(1,3));if(UnityEngine.Random.value<=.05f)Drop(e.transform.position+Vector3.left*.4f,ItemId.GoblinCleaver,1);}
        if(e.kind==EnemyKind.ForestBeast){Drop(e.transform.position+Vector3.right*.4f,ItemId.WolfHide,3);Drop(e.transform.position+Vector3.left*.4f,ItemId.RingHunter,1);}
    }
    public void RestoreLoot(Vector2 p,ItemId item,int amount){var l=Make<LootPickup>(ItemName(item),p,ItemColor(item),.35f);l.item=item;l.amount=amount;l.director=this;pickups.Add(l);}
    void Drop(Vector2 p,ItemId item,int amount){var l=Make<LootPickup>(ItemName(item),p+UnityEngine.Random.insideUnitCircle*.35f,ItemColor(item),.35f);l.item=item;l.amount=amount;l.director=this;pickups.Add(l);}
    Color ItemColor(ItemId i){if(i==ItemId.Gold)return new Color(1,.78f,.12f);if(i==ItemId.HealthPotion||i==ItemId.RedHerb)return new Color(.9f,.15f,.24f);if(i==ItemId.SlimeGel)return new Color(.25f,.9f,.45f);if(i==ItemId.HollowBlade)return new Color(.8f,.35f,1);return new Color(.78f,.78f,.85f);}
    void LegacyGUI()
    {
        GUI.color=new Color(.04f,.05f,.09f,.9f);GUI.Box(new Rect(14,14,300,110),"");GUI.color=Color.white;
        GUI.Label(new Rect(28,25,270,24),"ASHFALL  •  "+zone.ToUpper(),Title());
        Bar(new Rect(28,55,200,14),player.hp,player.MaxHp,new Color(.85f,.18f,.23f),"HP "+Mathf.Ceil(player.hp)+" / "+player.MaxHp);
        Bar(new Rect(28,78,200,12),player.stamina,player.MaxStamina,new Color(.20f,.72f,.95f),"STAMINA");
        GUI.Label(new Rect(28,97,275,20),"Weapon: "+player.WeaponName+"   Damage "+player.Damage+"   Gold "+bag[ItemId.Gold],Small());
        GUI.Label(new Rect(Screen.width-230,18,215,20),"Kills "+kills+"   Paths unlocked "+(zoneTier+1)+" / 4",Small());
        GUI.Label(new Rect(Screen.width-360,Screen.height-43,345,25),"[1] Sword  [2] Bow  [3] Staff    [C] Craft  [I] Gear  [H] Help",Small());
        if(Time.time<toastUntil){GUI.color=new Color(.04f,.04f,.08f,.92f);GUI.Box(new Rect(Screen.width/2-230,Screen.height-92,460,38),"");GUI.color=new Color(1,.9f,.55f);GUI.Label(new Rect(Screen.width/2-215,Screen.height-83,430,25),toast,Small(TextAnchor.MiddleCenter));GUI.color=Color.white;}
        if(craftOpen) DrawCraft(); if(inventoryOpen)DrawInventory(); if(helpOpen)DrawHelp();
    }
    GUIStyle Title(){var s=new GUIStyle(GUI.skin.label);s.fontSize=17;s.fontStyle=FontStyle.Bold;s.normal.textColor=new Color(1,.76f,.35f);return s;}
    GUIStyle Small(TextAnchor a=TextAnchor.UpperLeft){var s=new GUIStyle(GUI.skin.label);s.fontSize=13;s.alignment=a;s.normal.textColor=Color.white;return s;}
    void Bar(Rect r,float v,float max,Color c,string text){GUI.color=new Color(.1f,.1f,.13f);GUI.Box(r,"");GUI.color=c;GUI.Box(new Rect(r.x,r.y,r.width*Mathf.Clamp01(v/max),r.height),"");GUI.color=Color.white;GUI.Label(new Rect(r.x+4,r.y-3,r.width,r.height+6),text,Small(TextAnchor.MiddleCenter));}
    void Window(Rect r,string title){GUI.color=new Color(.055f,.065f,.10f,.97f);GUI.Box(r,"");GUI.color=new Color(1,.76f,.35f);GUI.Label(new Rect(r.x+16,r.y+12,r.width-32,26),title,Title());GUI.color=Color.white;}
    void DrawCraft(){var r=new Rect(Screen.width/2-265,105,530,420);Window(r,"FIELD CRAFTING  —  click a recipe");float y=r.y+55;foreach(var recipe in recipes){string needs="";foreach(var c in recipe.cost)needs+=ItemName(c.Key)+" ×"+c.Value+"   ";if(GUI.Button(new Rect(r.x+18,y,494,43),recipe.title+"\n"+needs)){Craft(recipe);}y+=54;}if(GUI.Button(new Rect(r.x+18,r.y+370,110,30),"Close [C]"))craftOpen=false;}
    void DrawInventory(){var r=new Rect(Screen.width/2-265,120,530,390);Window(r,"PACK & EQUIPMENT");GUI.Label(new Rect(r.x+18,r.y+55,240,22),"WEAPON  "+player.WeaponName,Small());GUI.Label(new Rect(r.x+18,r.y+83,240,22),"ARMOR    "+(player.leatherArmor?"Leather Armor (+25 HP)":"Tattered Cloth"),Small());GUI.Label(new Rect(r.x+18,r.y+111,440,22),"RING I   "+(player.ringA==ItemId.Gold?"Empty":ItemName(player.ringA)),Small());GUI.Label(new Rect(r.x+18,r.y+139,440,22),"RING II  "+(player.ringB==ItemId.Gold?"Empty":ItemName(player.ringB)),Small());
        GUI.Label(new Rect(r.x+18,r.y+180,470,22),"EQUIP: click an owned weapon, armor, or ring",Small());float y=r.y+210;ItemId[] gear={ItemId.GoblinCleaver,ItemId.IronSword,ItemId.FangSword,ItemId.BoneBow,ItemId.HollowBlade,ItemId.LeatherArmor,ItemId.RingFlames,ItemId.RingHunter,ItemId.RingVampire};foreach(var id in gear)if(bag[id]>0){if(GUI.Button(new Rect(r.x+18,y,235,28),ItemName(id)+" ×"+bag[id]))Equip(id);y+=33;}if(GUI.Button(new Rect(r.x+18,r.y+350,110,28),"Close [I]"))inventoryOpen=false;}
    void Equip(ItemId i){if(i==ItemId.LeatherArmor){player.leatherArmor=true;Tell("Leather Armor equipped.");return;}if(i==ItemId.RingFlames||i==ItemId.RingHunter||i==ItemId.RingVampire){if(player.ringA==ItemId.Gold)player.ringA=i;else player.ringB=i;Tell(ItemName(i)+" equipped.");return;} player.EquipItem(i);}
    void DrawHelp(){var r=new Rect(Screen.width/2-260,130,520,335);Window(r,"ASHFALL — SURVIVAL GUIDE");string text="Fight → Loot → Craft → Get stronger → Explore\n\nWASD  Move     SPACE  Dodge (uses stamina)\nLEFT CLICK  Attack     1 / 2 / 3  Sword / Bow / Staff\nE  Collect nearby drops     Q  Drink health potion\nC  Field crafting     I  Pack and equipment     TAB  travel\n\nForest opens first. Defeat 6 enemies for Mountain, 12 for Wasteland,\nand 18 for Dungeon. The Hollow Knight changes phase at 50% health.\n\nGoblin Cleaver has a 5% Goblin drop chance. Rings create build choices:\nFlames +15% staff damage • Hunter +10% beast damage • Vampire heals 3%.";GUI.Label(new Rect(r.x+18,r.y+54,r.width-36,240),text,Small());if(GUI.Button(new Rect(r.x+18,r.y+285,110,30),"Close [H]"))helpOpen=false;}
}
