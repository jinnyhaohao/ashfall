using System.Collections.Generic;
using UnityEngine;

// Imported artwork remains untouched. Runtime slices tolerate atlas export padding.
public static class SpriteFrames
{
    static Sprite[][] mobs;
    public static Sprite[] Load(string resource,int columns,int rows,float height){
        var t=Resources.Load<Texture2D>(resource);var pixels=t.GetPixels32();var bands=new List<Vector2Int>();int start=-1,last=-1;
        for(int y=0;y<t.height;y++){int count=0;for(int x=0;x<t.width;x++)if(pixels[y*t.width+x].a>230)count++;if(count>12){if(start<0)start=y;last=y;}else if(start>=0&&y-last>5){if(last-start>t.height/rows/3)bands.Add(new Vector2Int(start,last));start=-1;}}
        if(start>=0&&last-start>t.height/rows/3)bands.Add(new Vector2Int(start,last));
        if(bands.Count!=rows){bands.Clear();for(int row=0;row<rows;row++)bands.Add(new Vector2Int(row*t.height/rows,(row+1)*t.height/rows-1));}
        bands.Reverse();var result=new Sprite[rows*columns];int cw=t.width/columns;
        for(int row=0;row<rows;row++){var band=bands[row];int bh=band.y-band.x+1;for(int col=0;col<columns;col++){int ox=col*cw,lx=cw,hx=-1;for(int y=band.x;y<=band.y;y++)for(int x=0;x<cw;x++)if(pixels[y*t.width+ox+x].a>230){lx=Mathf.Min(lx,x);hx=Mathf.Max(hx,x);}if(hx<lx){lx=0;hx=cw-1;}result[row*columns+col]=Sprite.Create(t,new Rect(ox+lx,band.x,hx-lx+1,bh),new Vector2(.5f,.035f),bh/height);result[row*columns+col].name=resource+"/"+row+"/"+col;}}
        return result;
    }
    public static Sprite Mob(EnemyKind kind,int frame){if(mobs==null)mobs=new[]{Load("AshfallAtlasMobFramesA",4,6,1),Load("AshfallAtlasMobFramesB",4,6,1)};int k=(int)kind;return mobs[k/6][k%6*4+frame%4];}
}

public class ActorShadow : MonoBehaviour
{
    static Sprite ellipse;SpriteRenderer shadow;
    void Start(){if(!ellipse){var t=new Texture2D(32,16,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;for(int y=0;y<16;y++)for(int x=0;x<32;x++){float d=new Vector2((x-15.5f)/15.5f,(y-7.5f)/7.5f).magnitude;t.SetPixel(x,y,new Color(.015f,.025f,.04f,Mathf.Clamp01(1-d)*.48f));}t.Apply();ellipse=Sprite.Create(t,new Rect(0,0,32,16),Vector2.one*.5f,32);}var g=new GameObject("Contact shadow");g.transform.SetParent(transform,false);g.transform.localPosition=new Vector3(0,.015f,.1f);g.transform.localScale=new Vector3(.65f,.38f,1);shadow=g.AddComponent<SpriteRenderer>();shadow.sprite=ellipse;}
    void LateUpdate(){if(shadow){shadow.sortingOrder=95-(int)(transform.position.y*10);shadow.transform.rotation=Quaternion.identity;}}
}
