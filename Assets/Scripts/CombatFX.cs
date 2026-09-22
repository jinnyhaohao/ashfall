using System;
using System.Collections.Generic;
using UnityEngine;

// Shared effect resources and a bounded particle pool: no per-particle allocation during combat.
public class CombatFX : MonoBehaviour
{
    public static CombatFX I;
    public static Material Material;
    public static Sprite Orb,Arrow,Dot;
    public static float shake;
    struct Particle {public Transform t;public SpriteRenderer r;public Vector2 velocity;public float age,life,size;public Color color;}
    Particle[] particles=new Particle[256];int cursor;
    TextMesh[] numbers=new TextMesh[48];float[] numberAge=new float[48];int numberCursor;
    AudioSource source;AudioClip[] clips=new AudioClip[8];float soundAt;
    void Awake(){
        I=this;Material=new Material(Shader.Find("Sprites/Default"));
        Dot=Make(8,(x,y)=>Mathf.Abs(x-3.5f)+Mathf.Abs(y-3.5f)<4?Color.white:Color.clear);
        Orb=Make(16,(x,y)=>{float d=Vector2.Distance(new Vector2(x,y),new Vector2(7.5f,7.5f));return d<3?new Color(1,1,.8f):d<5?new Color(1,.8f,.2f):d<7?new Color(1,.3f,.08f):Color.clear;});
        Arrow=Make(16,(x,y)=> (x>9&&Mathf.Abs(y-7)<=15-x)||(x<12&&Mathf.Abs(y-7)<1.5f)?new Color(1,.91f,.65f):Color.clear);
        for(int i=0;i<particles.Length;i++){var g=new GameObject("Pooled spark");g.transform.SetParent(transform);var r=g.AddComponent<SpriteRenderer>();r.sprite=Dot;r.sharedMaterial=Material;r.sortingOrder=1900;r.enabled=false;particles[i]=new Particle{t=g.transform,r=r};}
        var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");font.RequestCharactersInTexture("0123456789",36,FontStyle.Normal);
        for(int i=0;i<numbers.Length;i++){var g=new GameObject("Pooled damage number");g.transform.SetParent(transform);var t=g.AddComponent<TextMesh>();t.font=font;t.fontSize=36;t.characterSize=.045f;t.anchor=TextAnchor.MiddleCenter;var renderer=t.GetComponent<MeshRenderer>();renderer.sharedMaterial=font.material;renderer.sortingOrder=2000;numbers[i]=t;g.SetActive(false);}
        gameObject.AddComponent<AudioListener>();source=gameObject.AddComponent<AudioSource>();source.spatialBlend=0;source.volume=.19f;
        for(int k=0;k<clips.Length;k++){int count=k==1?9000:4000;float[] data=new float[count];var rng=new System.Random(k+44);for(int n=0;n<count;n++){float t=n/22050f,env=1-n/(float)count;float f=k==7?850+n*.12f:k==4?430-n*.03f:k==1?130:240;float tone=Mathf.Sin(t*f*Mathf.PI*2);float noise=(float)rng.NextDouble()*2-1;data[n]=(k==0||k==2?noise*.5f:k==6?noise*.3f+tone*.3f:tone*.5f)*env*env;}clips[k]=AudioClip.Create("Arcade cue "+k,count,1,22050,false);clips[k].SetData(data,0);}
    }
    static Sprite Make(int size,Func<int,int,Color> pixel){var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;var p=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++)p[y*size+x]=pixel(x,y);t.SetPixels(p);t.Apply();return Sprite.Create(t,new Rect(0,0,size,size),Vector2.one*.5f,size);}
    public static void Sound(int id){if(!I||Time.unscaledTime<I.soundAt)return;I.soundAt=Time.unscaledTime+.025f;I.source.PlayOneShot(I.clips[id]);}
    public static void Kick(float amount){shake=Mathf.Max(shake,amount);}
    public static void Number(Vector3 position,int value){if(!I)return;int i=I.numberCursor++%I.numbers.Length;var t=I.numbers[i];t.transform.position=position+Vector3.up;t.text=value.ToString();t.color=new Color(1,.91f,.5f);I.numberAge[i]=0;t.gameObject.SetActive(true);}
    void LateUpdate(){if(AshfallBeta.Paused)return;for(int i=0;i<numbers.Length;i++){var t=numbers[i];if(!t.gameObject.activeSelf)continue;numberAge[i]+=Time.deltaTime;t.transform.position+=Vector3.up*Time.deltaTime*1.2f;t.color=new Color(1,.91f,.5f,1-numberAge[i]/.6f);if(numberAge[i]>.6f)t.gameObject.SetActive(false);}}
    public static void Spark(Vector3 p,Vector2 velocity,Color color,float life=.3f,float size=.13f){if(!I)return;int i=I.cursor++%I.particles.Length;var a=I.particles[i];a.t.position=p;a.t.rotation=Quaternion.identity;a.t.localScale=Vector3.one*size;a.r.sprite=Dot;a.r.color=color;a.r.enabled=true;a.r.flipX=false;a.velocity=velocity;a.age=0;a.life=life;a.size=size;a.color=color;I.particles[i]=a;}
    public static void Burst(Vector3 p,Color color,int count){for(int i=0;i<count;i++)Spark(p,UnityEngine.Random.insideUnitCircle*5,color,UnityEngine.Random.Range(.15f,.35f),UnityEngine.Random.Range(.07f,.17f));}
    public static void Ghost(SpriteRenderer original){if(!I)return;int i=I.cursor++%I.particles.Length;var a=I.particles[i];a.t.position=original.transform.position;a.t.rotation=original.transform.rotation;a.t.localScale=original.transform.lossyScale;a.r.sprite=original.sprite;a.r.flipX=original.flipX;a.color=new Color(.35f,.8f,1,.48f);a.r.color=a.color;a.r.enabled=true;a.velocity=Vector2.zero;a.age=0;a.life=.2f;a.size=-1;I.particles[i]=a;}
    void Update(){if(AshfallBeta.Paused)return;float dt=Time.deltaTime;for(int i=0;i<particles.Length;i++){var a=particles[i];if(!a.r.enabled)continue;a.age+=dt;if(a.age>=a.life)a.r.enabled=false;else{a.t.position+=(Vector3)a.velocity*dt;a.velocity*=Mathf.Exp(-5*dt);Color c=a.color;c.a*=1-a.age/a.life;a.r.color=c;if(a.size>0)a.t.localScale=Vector3.one*a.size*(1-a.age/a.life*.7f);}particles[i]=a;}}
    public static void Slash(Vector3 p,Vector2 aim,float radius,bool reverse,bool heavy,bool full=false){var g=new GameObject("Sweeping slash");g.transform.position=p;var a=g.AddComponent<ArcSweep>();a.angle=Mathf.Atan2(aim.y,aim.x);a.radius=radius;a.reverse=reverse;a.heavy=heavy;a.full=full;}
    public static void Ring(Vector3 p,float radius,Color color,float duration=.32f){var g=new GameObject("Expanding ring");g.transform.position=p;var a=g.AddComponent<ArcSweep>();a.radius=radius;a.full=true;a.ring=true;a.tint=color;a.duration=duration;}
    public static void Telegraph(Vector2 from,Vector2 to,float duration){var g=new GameObject("Dash warning");var l=g.AddComponent<LineRenderer>();l.sharedMaterial=Material;l.positionCount=2;l.SetPosition(0,from);l.SetPosition(1,to);l.startWidth=l.endWidth=.15f;l.startColor=l.endColor=new Color(1,.3f,.4f,.5f);l.sortingOrder=30;Destroy(g,duration);}
}
public class ArcSweep : MonoBehaviour
{
    public float angle,radius=2,duration=.2f;public bool reverse,heavy,full,ring;public Color tint=Color.white;
    Mesh mesh;MeshRenderer render;float age;const int N=32;Vector3[] vertices=new Vector3[(N+1)*2];Color[] colors=new Color[(N+1)*2];int[] triangles=new int[N*6];
    void Start(){mesh=new Mesh();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;render=gameObject.AddComponent<MeshRenderer>();render.sharedMaterial=CombatFX.Material;render.sortingOrder=1750;for(int i=0;i<N;i++){int j=i*6,k=i*2;triangles[j]=k;triangles[j+1]=k+1;triangles[j+2]=k+2;triangles[j+3]=k+1;triangles[j+4]=k+3;triangles[j+5]=k+2;}if(!ring)tint=heavy?new Color(1,.77f,.24f):new Color(.55f,.95f,1);Draw();}
    void Update(){if(AshfallBeta.Paused)return;age+=Time.deltaTime;if(age>=duration){Destroy(gameObject);return;}Draw();}
    void Draw(){float progress=Mathf.Clamp01(age/duration),span=full?Mathf.PI*2:2.5f;float sweep=ring?0:progress*1.8f;float r=radius*(ring?Mathf.Lerp(.5f,1,progress):1);for(int i=0;i<=N;i++){float t=i/(float)N,a=angle+(t-.5f)*span+(reverse?-sweep:sweep);float thickness=ring?.075f:(.05f+Mathf.Sin(t*Mathf.PI)*.35f)*(heavy?1.6f:1);Vector3 dir=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);vertices[i*2]=dir*(r-thickness);vertices[i*2+1]=dir*r;Color inner=Color.Lerp(tint,Color.white,.7f);inner.a=(1-progress)*(ring?.8f:1);Color outer=tint;outer.a=inner.a*.65f;colors[i*2]=inner;colors[i*2+1]=outer;}mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();}
    void OnDestroy(){if(mesh)Destroy(mesh);}
}
