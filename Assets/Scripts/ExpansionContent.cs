using UnityEngine;
public static class ExpansionContent
{
    public static readonly ItemId[] Weapons={ItemId.VenomSaber,ItemId.ThunderAxe,ItemId.SunsteelBlade,ItemId.ThornBow,ItemId.StormBow,ItemId.DuskStaff};
    static Sprite[] monsters;
    public static Sprite Slice(Texture2D t,int i,int columns,int rows,Vector2 pivot){int w=t.width/columns,h=t.height/rows,ox=i%columns*w,oy=(rows-1-i/columns)*h;var pixels=t.GetPixels(ox,oy,w,h);int lx=w,ly=h,hx=0,hy=0;for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(pixels[y*w+x].a>.65f){lx=Mathf.Min(lx,x);ly=Mathf.Min(ly,y);hx=Mathf.Max(hx,x);hy=Mathf.Max(hy,y);}return Sprite.Create(t,new Rect(ox+lx,oy+ly,hx-lx+1,hy-ly+1),pivot,hy-ly+1);}
    public static Sprite Monster(EnemyKind kind){if(monsters==null){monsters=new Sprite[6];var t=Resources.Load<Texture2D>("AshfallAtlasMonsters");for(int i=0;i<6;i++)monsters[i]=Slice(t,i,3,2,new Vector2(.5f,.05f));}return monsters[(int)kind-6];}
    public static void Recipes(AshfallDirector d){
        d.recipes.Add(new Recipe(ItemId.VenomSaber,1,"Venom Saber / poison",ItemId.VenomSac,6,ItemId.IronOre,8,ItemId.Gold,140));
        d.recipes.Add(new Recipe(ItemId.ThornBow,1,"Thorn Bow / poison arrows",ItemId.VenomSac,4,ItemId.WolfFang,6,ItemId.Gold,130));
        d.recipes.Add(new Recipe(ItemId.ThunderAxe,1,"Thunder Axe / stunning strikes",ItemId.StormCrystal,6,ItemId.IronOre,12,ItemId.Gold,220));
        d.recipes.Add(new Recipe(ItemId.StormBow,1,"Storm Bow / shocking arrows",ItemId.StormCrystal,5,ItemId.Bone,10,ItemId.Gold,200));
        d.recipes.Add(new Recipe(ItemId.DuskStaff,1,"Dusk Staff / draining magic",ItemId.StormCrystal,4,ItemId.AncientShard,8,ItemId.Gold,250));
        d.recipes.Add(new Recipe(ItemId.SunsteelBlade,1,"Sunsteel Blade / undead slayer",ItemId.Sunsteel,8,ItemId.EmberShard,8,ItemId.Gold,300));
    }
    public static void Setup(EnemyActor e){switch(e.kind){case EnemyKind.StoneGolem:e.maxHp=140;e.speed=1.1f;e.damage=19;break;case EnemyKind.CaveBat:e.maxHp=28;e.speed=3.7f;e.damage=7;break;case EnemyKind.Scorpion:e.maxHp=65;e.speed=2.1f;e.damage=14;break;case EnemyKind.MushroomShaman:e.maxHp=58;e.speed=1.6f;e.damage=12;break;case EnemyKind.IceWraith:e.maxHp=85;e.speed=2.2f;e.damage=16;break;case EnemyKind.DarkKnight:e.maxHp=160;e.speed=1.8f;e.damage=22;break;}}
}

public class EnemyBolt : MonoBehaviour
{
    public AshfallDirector director;public Vector2 velocity;public float damage;public Color color;float life=4;
    void Start(){var r=gameObject.AddComponent<SpriteRenderer>();r.sprite=CombatFX.Orb;r.color=color;r.sortingOrder=1600;transform.localScale=Vector3.one*.3f;}
    void Update(){if(AshfallBeta.Paused)return;Vector2 before=transform.position;Vector2 next=before+velocity*Time.deltaTime,hit;bool clear=CombatRules.Trace(director,before-Vector2.up*.3f,next-Vector2.up*.3f,out hit);transform.position=hit+Vector2.up*.3f;life-=Time.deltaTime;if(life<=0){Destroy(gameObject);return;}if(WorldAtlas.Segment(director.player.transform.position,before,transform.position)<.45f){director.player.Hurt(damage);CombatFX.Burst(transform.position,color,5);Destroy(gameObject);}if(!clear)Destroy(gameObject);}
}
