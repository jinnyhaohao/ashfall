using System;
using UnityEngine;
public static class NavigationQA
{
    static void Require(bool ok,string name){if(!ok)throw new Exception("NAVIGATION QA FAILED: "+name);}
    public static void Run(){int traversals=0,sweeps=0;for(int z=0;z<8;z++){
        Require(WorldAtlas.Walkable(z,WorldAtlas.Constrain(z,new Vector2(-21,-21),new Vector2(-20,-20))),"Recover invalid position "+z);
        var edges=WorldAtlas.Edges[z];for(int e=0;e<edges.Length;e+=2){Vector2 a=WorldAtlas.Rooms[z][edges[e]],b=WorldAtlas.Rooms[z][edges[e+1]],corner=new Vector2(b.x,a.y);for(int reverse=0;reverse<2;reverse++)for(int speed=0;speed<2;speed++){Vector2 p=reverse==0?a:b;Vector2[] targets=reverse==0?new[]{corner,b}:new[]{corner,a};foreach(var target in targets){int steps=0;while(Vector2.Distance(p,target)>.06f&&steps++<500){Vector2 next=Vector2.MoveTowards(p,target,speed==0?.0867f:.2333f);p=WorldAtlas.Constrain(z,p,next);Require(WorldAtlas.Walkable(z,p),"Stayed on floor");}Require(Vector2.Distance(p,target)<.07f,"Walk/dash corridor "+z+"/"+e);}traversals++;}}
        foreach(Vector2 target in WorldAtlas.Rooms[z]){Vector2 p=Vector2.zero;int steps=0;while(Vector2.Distance(p,target)>.2f&&steps++<1800){Vector2 direction=WorldAtlas.Chase(z,p,target);p=WorldAtlas.Constrain(z,p,p+direction*.12f);}Require(Vector2.Distance(p,target)<.21f,"Enemy route reaches chamber "+z+" "+target);}
        for(int y=-19;y<19;y++)for(int x=-19;x<19;x++){Vector2 p=new Vector2(x+.5f,y+.5f);Require(WorldAtlas.Floor(z,p)==(WorldAtlas.Field(z,p)<0),"Render/collision agreement "+z);if(!WorldAtlas.Walkable(z,p))continue;for(int k=0;k<8;k++){Vector2 direction=new Vector2(Mathf.Cos(k*Mathf.PI/4),Mathf.Sin(k*Mathf.PI/4));Vector2 end=WorldAtlas.Constrain(z,p,p+direction*3);Require(WorldAtlas.Walkable(z,end),"Dash stays in bounds "+z);sweeps++;}}
        Debug.Log("NAVIGATION QA PASS: "+WorldAtlas.Names[z]+" routes, wall sliding, recovery and collision alignment");
    }Debug.Log("NAVIGATION QA: ALL CHECKS PASSED / "+traversals+" route traversals / "+sweeps+" boundary sweeps");}
}
