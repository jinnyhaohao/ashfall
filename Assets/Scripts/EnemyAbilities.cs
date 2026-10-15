using UnityEngine;

// A local, unscaled clock freezes combat actors without touching Time.timeScale. Pause and autosave remain authoritative.
public static class CombatHitStop
{
    static float until;
    public static bool Active {get{return Time.unscaledTime<until;}}
    public static float Remaining {get{return Mathf.Max(0,until-Time.unscaledTime);}}
    public static void Trigger(float seconds){until=Mathf.Max(until,Time.unscaledTime+Mathf.Clamp(seconds,0,.08f));}
    public static void Reset(){until=0;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Initialize(){Reset();}
}

// Wall-clipped warning lanes make multi-projectile attacks readable before any damage exists.
public static class EnemyVolleyWarning
{
    public static int ActiveWarnings {get;private set;}
    public static void Show(AshfallDirector director,Vector2 origin,Vector2 aim,int count,float spread,Color color,float duration)
    {
        var root=new GameObject("Enemy volley warning");ActiveWarnings++;
        var life=root.AddComponent<EnemyVolleyWarningLife>();life.duration=duration;
        for(int i=0;i<count;i++){
            float angle=(i-(count-1)*.5f)*spread;Vector2 direction=Quaternion.Euler(0,0,angle)*aim;
            Vector2 traceEnd;CombatRules.Trace(director,origin-Vector2.up*.3f,origin+direction*8-Vector2.up*.3f,out traceEnd);Vector2 end=traceEnd+Vector2.up*.3f;
            var lane=new GameObject("Warning lane "+(i+1));lane.transform.SetParent(root.transform,false);var line=lane.AddComponent<LineRenderer>();line.sharedMaterial=CombatFX.Material;line.positionCount=2;line.SetPosition(0,origin);line.SetPosition(1,end);line.startWidth=.12f;line.endWidth=.055f;Color faded=color;faded.a=.58f;line.startColor=line.endColor=faded;line.sortingOrder=1749;
        }
    }
    public static void Released(){ActiveWarnings=Mathf.Max(0,ActiveWarnings-1);}
}

public class EnemyVolleyWarningLife : MonoBehaviour
{
    public float duration;float age;
    void Update(){if(AshfallBeta.Paused)return;age+=Time.deltaTime;if(age>=duration)Destroy(gameObject);}
    void OnDestroy(){EnemyVolleyWarning.Released();}
}

// Shared wall-aware projectile for the scorpion poison spread and ice-wraith fan.
public class EnemyVolleyProjectile : MonoBehaviour
{
    public static int ActiveProjectiles {get;private set;}
    public AshfallDirector director;public EnemyActor owner;public Vector2 direction;public float speed,damage;public bool poisonous;public Color color;
    float life=2.2f;bool counted;
    public static EnemyVolleyProjectile Launch(AshfallDirector director,EnemyActor owner,Vector2 origin,Vector2 direction,float speed,float damage,Color color,bool poisonous)
    {
        var g=new GameObject(poisonous?"Telegraphed poison barb":"Telegraphed ice shard");g.transform.position=origin;
        var projectile=g.AddComponent<EnemyVolleyProjectile>();projectile.director=director;projectile.owner=owner;projectile.direction=direction.normalized;projectile.speed=speed;projectile.damage=damage;projectile.color=color;projectile.poisonous=poisonous;return projectile;
    }
    void Start(){ActiveProjectiles++;counted=true;var renderer=gameObject.AddComponent<SpriteRenderer>();renderer.sprite=CombatFX.Orb;renderer.color=color;renderer.sortingOrder=1600;transform.localScale=Vector3.one*(poisonous?.24f:.27f);}
    void Update()
    {
        if(AshfallBeta.Paused||CombatHitStop.Active)return;
        Vector2 before=transform.position,next=before+direction*speed*Time.deltaTime,hit;bool clear=CombatRules.Trace(director,before-Vector2.up*.3f,next-Vector2.up*.3f,out hit);transform.position=hit+Vector2.up*.3f;life-=Time.deltaTime;
        if(life<=0){Destroy(gameObject);return;}
        if(WorldAtlas.Segment(director.player.transform.position,before,transform.position)<.47f){float hp=director.player.hp;if(owner)owner.HitPlayer(damage);else director.player.Hurt(damage);if(poisonous&&director.player.hp<hp)director.player.ApplyPoison(2.45f,3);CombatFX.Burst(transform.position,color,5);Destroy(gameObject);return;}
        if(!clear){CombatFX.Burst(transform.position,color,3);Destroy(gameObject);}
    }
    void OnDestroy(){if(counted)ActiveProjectiles=Mathf.Max(0,ActiveProjectiles-1);}
}
