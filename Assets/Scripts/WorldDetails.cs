using System.Collections.Generic;
using UnityEngine;

public static class WorldDetails
{
    static Sprite[] props,walls;static Sprite glow;
    public static readonly List<HarvestNode> Nodes=new List<HarvestNode>();
    static readonly int[][] palettes={new[]{0,2,8,9,10},new[]{0,1,2,0,2},new[]{3,3,5,4},new[]{14,11,3,14},new[]{6,7,10,11},new[]{12,1,13,2},new[]{4,4,5,1},new[]{7,15,10,11}};
    public static void Initialize(){if(props!=null)return;props=new Sprite[16];var t=Resources.Load<Texture2D>("AshfallAtlasDetails");for(int i=0;i<16;i++)props[i]=ExpansionContent.Slice(t,i,4,4,new Vector2(.5f,.04f));walls=new Sprite[8];for(int z=0;z<8;z++)walls[z]=Wall(z);var light=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16;light.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2)*.17f));}light.Apply();glow=Sprite.Create(light,new Rect(0,0,32,32),Vector2.one*.5f,16);}
    static Sprite Wall(int zone){var t=new Texture2D(24,36,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;Color stone=zone==5?new Color(.20f,.33f,.31f):zone==1||zone==0?new Color(.28f,.35f,.24f):zone==3?new Color(.51f,.34f,.25f):zone==6?new Color(.32f,.40f,.57f):new Color(.32f,.29f,.38f);for(int y=0;y<36;y++)for(int x=0;x<24;x++){bool top=y>=23;float shade=top?1.22f:.7f+y*.009f;bool mortar=top?(x==0||y==35):y%8==0||(x+(y/8%2)*12)%24==0;if(mortar)shade*=.67f;if(y==23)shade=1.5f;if((x*17+y*13)%43==0)shade*=1.12f;t.SetPixel(x,y,stone*shade);}t.Apply();return Sprite.Create(t,new Rect(0,0,24,36),new Vector2(.5f,0),24);}
    public static void Build(int z){Initialize();Vector2 home=WorldAtlas.Centers[z];
        for(int y=-20;y<20;y++)for(int x=-20;x<20;x++){Vector2 p=new Vector2(x+.5f,y+.5f);if(WorldAtlas.Floor(z,p))continue;bool edge=WorldAtlas.Floor(z,p+Vector2.up)||WorldAtlas.Floor(z,p+Vector2.down)||WorldAtlas.Floor(z,p+Vector2.left)||WorldAtlas.Floor(z,p+Vector2.right);if(!edge)continue;var g=new GameObject("Raised wall / "+z);g.transform.position=home+p-Vector2.up*.5f;var r=g.AddComponent<SpriteRenderer>();r.sprite=walls[z];r.sortingOrder=100-(int)((home.y+y)*10);var f=g.AddComponent<WallFade>();f.zone=z;}
        var random=new System.Random(9300+z*97);int ordinal=0;
        foreach(Vector2 room in WorldAtlas.Rooms[z]){
            for(int k=0;k<8;k++){Vector2 p=room+new Vector2(k%2==0?-2.8f:2.8f,-1.9f+(k/2)*1.2f);if(WorldAtlas.Trail(z,p)<1.65f||NearGate(z,p)||!WorldAtlas.Walkable(z,p))continue;int id=palettes[z][random.Next(palettes[z].Length)];Place(z,p,id,.55f+(float)random.NextDouble()*.6f,ordinal++);}
            // A paired lit threshold makes each chamber entrance legible without blocking it.
            Place(z,room+new Vector2(-3.0f,2.2f),z==6?4:z==5?1:10,.65f,ordinal++);
            Place(z,room+new Vector2(3.0f,2.2f),z==6?4:z==5?1:10,.65f,ordinal++);
        }
        for(int k=0;k<110;k++){Vector2 p=new Vector2((float)random.NextDouble()*38-19,(float)random.NextDouble()*38-19);float f=WorldAtlas.Field(z,p);if(f<1.4f||f>4)continue;Place(z,p,palettes[z][random.Next(palettes[z].Length)],.45f+(float)random.NextDouble()*.65f,-1);}
        if(z==0){Place(z,new Vector2(-3.1f,1.6f),9,1.15f,-1);Place(z,new Vector2(-3.2f,-1.8f),8,.9f,-1);}
    }
    static bool NearGate(int z,Vector2 p){foreach(int n in WorldAtlas.Neighbors(z))if(Vector2.Distance(p,WorldAtlas.Gate(z,n))<2)return true;return false;}
    static void Place(int z,Vector2 p,int id,float size,int ordinal){var g=new GameObject("Biome detail "+id);g.transform.position=WorldAtlas.Centers[z]+p;g.transform.localScale=Vector3.one*size;var r=g.AddComponent<SpriteRenderer>();r.sprite=props[id];r.sortingOrder=103-(int)(g.transform.position.y*10);
        if(id==1||id==4||id==10){var lamp=new GameObject("Soft pixel glow");lamp.transform.SetParent(g.transform,false);lamp.transform.localPosition=Vector3.up*.35f;lamp.transform.localScale=Vector3.one*3;var lr=lamp.AddComponent<SpriteRenderer>();lr.sprite=glow;lr.color=id==10?new Color(1,.6f,.15f):new Color(.25f,.8f,1);lr.sortingOrder=-800;g.AddComponent<DetailMotion>().glow=lr;}
        else if(id==2||id==5||id==12||id==15)g.AddComponent<DetailMotion>();
        if(ordinal>=0&&(id==2||id==3||id==4||id==1)){var node=g.AddComponent<HarvestNode>();node.zone=z;node.item=id==2?ItemId.RedHerb:id==3?ItemId.IronOre:id==4?ItemId.StormCrystal:ItemId.Moonleaf;Nodes.Add(node);}
    }
    public static HarvestNode Nearest(AshfallDirector d){HarvestNode best=null;float distance=1.5f;int z=WorldAtlas.Index(d.zone);foreach(var n in Nodes){if(!n||n.zone!=z||!n.Ready)continue;float dist=Vector2.Distance(d.player.transform.position,n.transform.position);if(dist<distance){distance=dist;best=n;}}return best;}
}

public class HarvestNode : MonoBehaviour
{
    public int zone;public ItemId item;float readyAt;public bool Ready {get{return Time.time>=readyAt;}}
    public bool Harvest(AshfallDirector d){if(!Ready)return false;d.Add(item,2);readyAt=Time.time+90;GetComponent<SpriteRenderer>().color=new Color(.42f,.45f,.43f,.7f);CombatFX.Burst(transform.position,ItemCatalog.Tint(item),6);CombatFX.Sound(7);return true;}
    void Update(){if(Ready)GetComponent<SpriteRenderer>().color=Color.white;}
}
public class WallFade : MonoBehaviour
{
    public int zone;SpriteRenderer sprite;void Start(){sprite=GetComponent<SpriteRenderer>();}
    void LateUpdate(){var d=AshfallDirector.I;if(!d||!d.player||WorldAtlas.Index(d.zone)!=zone)return;Vector2 delta=d.player.transform.position-transform.position;float alpha=Mathf.Abs(delta.x)<1.5f&&delta.y>-.15f&&delta.y<2?.32f:1;sprite.color=new Color(1,1,1,alpha);}
}
public class DetailMotion : MonoBehaviour
{
    public SpriteRenderer glow;float phase;void Start(){phase=transform.position.x*1.7f+transform.position.y;}
    void Update(){if(AshfallBeta.Paused)return;float wave=Mathf.Sin(Time.time*2+phase);if(glow){var c=glow.color;c.a=.75f+wave*.2f;glow.color=c;}else transform.rotation=Quaternion.Euler(0,0,wave*2);}
}
