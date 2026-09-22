using System;
using System.Collections.Generic;
using UnityEngine;

// Authored room-and-trail geometry is shared by terrain, collision, spawning and maps.
public static class WorldAtlas
{
    public static readonly string[] Names={"Ruined Village","Forest","Mountain","Wasteland","Dungeon","Sunken Marsh","Crystal Caverns","Ashen Citadel"};
    public static readonly Vector2[] Centers={Vector2.zero,new Vector2(100,0),new Vector2(200,0),new Vector2(300,0),new Vector2(400,0),new Vector2(500,0),new Vector2(600,0),new Vector2(700,0)};
    public static readonly Vector2[] MapPositions={new Vector2(350,210),new Vector2(160,210),new Vector2(350,90),new Vector2(550,210),new Vector2(350,365),new Vector2(130,355),new Vector2(550,90),new Vector2(570,365)};
    public static readonly int[] Requirements={0,0,6,12,18,8,16,30};
    public static readonly string[] Landmarks={"Ember refuge","Old oak crossroads","Splitstone pass","Scorpion basin","Hollow throne","Drowned sanctuary","Prism heart","Fallen crown"};
    public static readonly string[] Hints={"Recover at the village fire.","Wolves, goblins and mushroom shamans.","Golems guard ore; bats swarm the pass.","Scorpions and dark knights hunt the dunes.","Undead guards surround the Hollow Knight.","Shamans carry Venom Sacs for poison weapons.","Wraiths carry Storm Crystals for lightning gear.","Dark knights carry Sunsteel for the final forge."};
    public static readonly Color[] Colors={new Color(.30f,.44f,.31f),new Color(.19f,.38f,.25f),new Color(.37f,.42f,.54f),new Color(.55f,.34f,.23f),new Color(.24f,.21f,.34f),new Color(.17f,.35f,.34f),new Color(.22f,.29f,.47f),new Color(.39f,.24f,.28f)};
    public static readonly int[,] Links={{0,1},{0,2},{0,3},{0,4},{1,5},{5,4},{2,6},{6,3},{3,7},{4,7}};
    public static readonly Vector2[][] Rooms={
        new[]{V(0,0),V(-12,0),V(0,12),V(12,0),V(0,-12)},
        new[]{V(0,0),V(-12,0),V(-12,12),V(0,12),V(12,12),V(12,0),V(-12,-12)},
        new[]{V(0,0),V(-12,0),V(-12,12),V(0,12),V(12,12),V(12,-2)},
        new[]{V(0,0),V(12,0),V(12,-12),V(0,-12),V(-12,-12),V(-12,0),V(-12,12)},
        new[]{V(0,0),V(-12,0),V(12,0),V(0,12),V(-12,12),V(12,12),V(0,-12)},
        new[]{V(0,0),V(-12,0),V(-12,12),V(0,12),V(12,12),V(12,0),V(12,-12)},
        new[]{V(0,0),V(12,0),V(12,12),V(0,12),V(-12,12),V(-12,0),V(0,-12)},
        new[]{V(0,0),V(-12,0),V(12,0),V(-12,12),V(12,12),V(0,12),V(0,-12)}
    };
    // Deliberate branches, optional loops and terminal chambers, not a closed chain of circles.
    public static readonly int[][] Edges={
        new[]{0,1,0,2,0,3,0,4},
        new[]{0,1,1,2,2,3,3,4,4,5,5,0,1,6},
        new[]{0,1,1,2,2,3,3,4,4,5},
        new[]{0,1,1,2,2,3,3,4,4,5,5,0,5,6},
        new[]{0,1,0,2,0,3,3,4,3,5,0,6},
        new[]{0,1,1,2,2,3,3,4,4,5,5,0,5,6},
        new[]{0,1,1,2,2,3,3,4,4,5,5,0,0,6},
        new[]{0,1,0,2,1,3,2,4,3,5,4,5,0,6}
    };
    static readonly List<Rect>[] floors=new List<Rect>[8];
    static readonly bool[,,] tiles=new bool[8,44,44];
    static bool ready;
    static void Initialize(){if(ready)return;ready=true;for(int z=0;z<8;z++){
        floors[z]=new List<Rect>();for(int i=0;i<Rooms[z].Length;i++){Vector2 p=Rooms[z][i];float w=i==0?10:i==Rooms[z].Length-1?10:8;float h=z==4||z==7?8:6;floors[z].Add(new Rect(p.x-w/2,p.y-h/2,w,h));}
        var edges=Edges[z];for(int i=0;i<edges.Length;i+=2){Vector2 a=Rooms[z][edges[i]],b=Rooms[z][edges[i+1]],corner=new Vector2(b.x,a.y);AddCorridor(z,a,corner);AddCorridor(z,corner,b);}
        for(int y=0;y<44;y++)for(int x=0;x<44;x++){Vector2 p=new Vector2(x-21.5f,y-21.5f);foreach(var r in floors[z])if(r.Contains(p)){tiles[z,x,y]=true;break;}}
    }}
    static void AddCorridor(int z,Vector2 a,Vector2 b){floors[z].Add(new Rect(Mathf.Min(a.x,b.x)-2,Mathf.Min(a.y,b.y)-2,Mathf.Abs(a.x-b.x)+4,Mathf.Abs(a.y-b.y)+4));}
    public static bool Floor(int z,Vector2 p){Initialize();int x=Mathf.FloorToInt(p.x+22),y=Mathf.FloorToInt(p.y+22);return x>=0&&x<44&&y>=0&&y<44&&tiles[z,x,y];}
    static Vector2 V(float x,float y){return new Vector2(x,y);}
    public static int Index(string name){return Mathf.Max(0,Array.IndexOf(Names,name));}
    public static bool Unlocked(int z,AshfallDirector d){return d.kills>=Requirements[z]&&(z!=7||d.bossState==2);}
    public static List<int> Neighbors(int z){var a=new List<int>();for(int i=0;i<Links.GetLength(0);i++){if(Links[i,0]==z)a.Add(Links[i,1]);if(Links[i,1]==z)a.Add(Links[i,0]);}return a;}
    public static Vector2 Gate(int z,int n){var neighbors=Neighbors(z);int index=neighbors.IndexOf(n);var rooms=Rooms[z];return rooms[1+index%(rooms.Length-1)];}
    public static float Segment(Vector2 p,Vector2 a,Vector2 b){Vector2 v=b-a;return Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/Mathf.Max(.001f,v.sqrMagnitude)));}
    public static float Trail(int z,Vector2 p){float d=100;var r=Rooms[z];var edges=Edges[z];for(int i=0;i<edges.Length;i+=2){Vector2 a=r[edges[i]],b=r[edges[i+1]],c=new Vector2(b.x,a.y);d=Mathf.Min(d,Mathf.Min(Segment(p,a,c),Segment(p,c,b)));}return d;}
    public static float Field(int z,Vector2 p){Initialize();float d=100;foreach(var r in floors[z]){Vector2 q=new Vector2(Mathf.Abs(p.x-r.center.x)-r.width/2,Mathf.Abs(p.y-r.center.y)-r.height/2);float v=new Vector2(Mathf.Max(0,q.x),Mathf.Max(0,q.y)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0);d=Mathf.Min(d,v);}return d;}
    // Footprint checks use the UNION of floor tiles, avoiding false seams between overlapping rooms.
    public static bool Walkable(int z,Vector2 p){const float r=.28f;return Floor(z,p)&&Floor(z,p+V(r,0))&&Floor(z,p+V(-r,0))&&Floor(z,p+V(0,r))&&Floor(z,p+V(0,-r))&&Floor(z,p+V(.2f,.2f))&&Floor(z,p+V(-.2f,.2f))&&Floor(z,p+V(.2f,-.2f))&&Floor(z,p+V(-.2f,-.2f));}
    public static Vector2 Recover(int z,Vector2 p){if(Walkable(z,p))return p;float best=float.MaxValue;Vector2 result=Vector2.zero;for(int y=0;y<44;y++)for(int x=0;x<44;x++){if(!tiles[z,x,y])continue;Vector2 candidate=V(x-21.5f,y-21.5f);float d=(candidate-p).sqrMagnitude;if(d<best){best=d;result=candidate;}}return result;}
    public static Vector2 Constrain(int z,Vector2 before,Vector2 next){Vector2 delta=next-before,p=Recover(z,before);int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.12f));Vector2 step=delta/steps;for(int i=0;i<steps;i++){Vector2 target=p+step;if(Walkable(z,target)){p=target;continue;}Vector2 x=p+V(step.x,0);if(Walkable(z,x))p=x;Vector2 y=p+V(0,step.y);if(Walkable(z,y))p=y;}return p;}
    public static Vector2 SpawnPoint(int z,int ordinal){var rooms=Rooms[z];Vector2 center=rooms[1+ordinal%(rooms.Length-1)];for(int i=0;i<20;i++){Vector2 p=center+UnityEngine.Random.insideUnitCircle*2.7f;if(Walkable(z,p))return p;}return center;}
    static bool Clear(int z,Vector2 a,Vector2 b){int steps=Mathf.CeilToInt(Vector2.Distance(a,b)*2);for(int i=1;i<=steps;i++)if(!Walkable(z,Vector2.Lerp(a,b,i/(float)steps)))return false;return true;}
    static int flowZone=-1,flowTarget=-1;static readonly int[] flow=new int[1936],queue=new int[1936];
    static int Cell(Vector2 p){return Mathf.Clamp(Mathf.FloorToInt(p.y+22),0,43)*44+Mathf.Clamp(Mathf.FloorToInt(p.x+22),0,43);}
    static Vector2 CellPoint(int i){return V(i%44-21.5f,i/44-21.5f);}
    public static Vector2 Chase(int z,Vector2 from,Vector2 to){if(Clear(z,from,to))return (to-from).normalized;int target=Cell(Recover(z,to));if(flowZone!=z||flowTarget!=target){flowZone=z;flowTarget=target;for(int i=0;i<flow.Length;i++)flow[i]=9999;int head=0,tail=0;queue[tail++]=target;flow[target]=0;while(head<tail){int c=queue[head++];for(int k=0;k<4;k++){int n=c+(k==0?1:k==1?-1:k==2?44:-44);if(n<0||n>=1936||Mathf.Abs(n%44-c%44)+Mathf.Abs(n/44-c/44)!=1||!Floor(z,CellPoint(n))||flow[n]!=9999)continue;flow[n]=flow[c]+1;queue[tail++]=n;}}}int cell=Cell(from),best=cell;for(int k=0;k<4;k++){int n=cell+(k==0?1:k==1?-1:k==2?44:-44);if(n>=0&&n<1936&&Mathf.Abs(n%44-cell%44)+Mathf.Abs(n/44-cell/44)==1&&flow[n]<flow[best]&&Clear(z,from,CellPoint(n)))best=n;}return (CellPoint(best)-from).normalized;}
    public static Texture2D MapTexture(int z,int size=192){var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;var pixels=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++){Vector2 p=new Vector2(x/(float)size*44-22,y/(float)size*44-22);float f=Field(z,p);pixels[y*size+x]=f<0?(Trail(z,p)<.65f?new Color(.79f,.67f,.43f):Colors[z]*1.5f):f<.45f?new Color(.72f,.65f,.45f):Color.clear;}t.SetPixels(pixels);t.Apply();return t;}
    public static Vector2 MapPoint(Vector2 p,Rect r){return new Vector2(r.x+(p.x+22)/44*r.width,r.y+(22-p.y)/44*r.height);}
}
