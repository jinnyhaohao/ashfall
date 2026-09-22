using UnityEngine;

// Two-bone arms. Each weapon's actual handle pivot is placed exactly at the gripping fist.
[DefaultExecutionOrder(100)]
public class HeroRig : MonoBehaviour
{
    static Sprite bodyArt,upperArt,foreArt,handArt;
    static Sprite[] directionalBodies;
    Vector3 lastPosition;float walkCycle;
    public int FacingDirection {get;private set;}
    public int WalkFrame {get;private set;}
    public bool WeaponBehindBody {get;private set;}
    public float PreviewWalk=-1;
    SpriteRenderer body,weapon;
    SpriteRenderer nockedArrow;LineRenderer bowString;
    SpriteRenderer[] upper=new SpriteRenderer[2],fore=new SpriteRenderer[2],hands=new SpriteRenderer[2];
    PlayerHero player;GameObject root;
    float start=-100,duration=.3f;Vector2 attackAim;bool reverse,heavy,spin;WeaponKind activeWeapon;
    public float PreviewPhase=-1;public Vector2 PreviewAim=Vector2.right;
    public Vector3 GripPosition {get{return hands[0].transform.position;}}
    public float GripError {get{return Vector3.Distance(weapon.transform.position,GripPosition);}}
    public Sprite DisplayedWeapon {get{return weapon?weapon.sprite:null;}}
    public string PoseName {get {float t=(Time.time-start)/duration;return t<.25f?"wind-up":t<.65f?"strike":"recovery";}}
    public void Initialize(SpriteRenderer renderer,PlayerHero hero){
        body=renderer;player=hero;
        lastPosition=transform.position;
        if(!bodyArt){var t=Resources.Load<Texture2D>("AshfallHeroRig");bodyArt=Part(t,0,new Vector2(.5f,.05f),false);upperArt=Part(t,1,new Vector2(.5f,1),true);foreArt=Part(t,2,new Vector2(.5f,1),true);handArt=Part(t,3,new Vector2(.5f,.5f),true);}
        body.sprite=bodyArt;root=new GameObject("Hero articulated arms");
        if(directionalBodies==null)LoadDirections();
        for(int i=0;i<2;i++){upper[i]=Piece("Upper arm "+i,upperArt);fore[i]=Piece("Forearm "+i,foreArt);hands[i]=Piece("Gripping hand "+i,handArt);}
        weapon=Piece("Weapon anchored at handle",null);
        nockedArrow=Piece("Nocked arrow",CombatFX.Arrow);var stringObject=new GameObject("Animated bow string");stringObject.transform.SetParent(root.transform);bowString=stringObject.AddComponent<LineRenderer>();bowString.sharedMaterial=CombatFX.Material;bowString.positionCount=3;bowString.startWidth=bowString.endWidth=.012f;bowString.startColor=bowString.endColor=new Color(.95f,.88f,.67f);
        WeaponArt.Initialize();
    }
    static void LoadDirections(){
        directionalBodies=SpriteFrames.Load("AshfallHeroFrames",8,4,.82f);
    }
    static RectInt MainIsland(Color[] pixels,int size){
        // Ignore isolated export flecks from neighboring cells when choosing sprite bounds.
        bool[] seen=new bool[pixels.Length];int[] stack=new int[pixels.Length];int largest=0;RectInt best=new RectInt(0,0,size,size);
        for(int seed=0;seed<pixels.Length;seed++){
            if(seen[seed]||pixels[seed].a<=.15f)continue;
            int count=0,top=0,minX=size,minY=size,maxX=0,maxY=0;stack[top++]=seed;seen[seed]=true;
            while(top>0){int p=stack[--top],x=p%size,y=p/size;count++;minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
                for(int k=0;k<4;k++){int nx=x+(k==0?-1:k==1?1:0),ny=y+(k==2?-1:k==3?1:0);if(nx<0||ny<0||nx>=size||ny>=size)continue;int n=ny*size+nx;if(!seen[n]&&pixels[n].a>.15f){seen[n]=true;stack[top++]=n;}}
            }
            if(count>largest){largest=count;best=new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);}
        }return best;
    }
    public static int DirectionIndex(Vector2 aim){return Mathf.Abs(aim.y)>Mathf.Abs(aim.x)?(aim.y>0?1:0):(aim.x>=0?2:3);}
    static Sprite Part(Texture2D texture,int cell,Vector2 pivot,bool normalize){
        int w=texture.width/2,h=Mathf.RoundToInt(texture.height*(cell<2?.58f:.40f)),ox=cell%2*w,oy=cell<2?texture.height-h:0;
        int minX=w,minY=h,maxX=0,maxY=0;
        // Alpha bounds remove export padding, not artwork. Keep source texture untouched.
        var pixels=texture.GetPixels(ox,oy,w,h);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(pixels[y*w+x].a>.15f){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
        return Sprite.Create(texture,new Rect(ox+minX,oy+minY,maxX-minX+1,maxY-minY+1),pivot,normalize?maxY-minY+1:(maxY-minY+1)/.82f);
    }
    SpriteRenderer Piece(string name,Sprite sprite){var g=new GameObject(name);g.transform.SetParent(root.transform);var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;return r;}
    public void Play(Vector2 aim,bool back,bool big,bool whirl=false){start=Time.time;attackAim=aim;reverse=back;heavy=big;spin=whirl;activeWeapon=player.weapon;duration=whirl?.42f:big?.4f:activeWeapon==WeaponKind.Bow?.30f:activeWeapon==WeaponKind.Staff?.34f:.30f;}
    public void SetPreview(WeaponKind kind,Vector2 aim,float phase,bool back=false){activeWeapon=kind;player.weapon=kind;PreviewAim=aim;PreviewPhase=phase;reverse=back;heavy=false;spin=false;}
    void LateUpdate(){
        if(!body||!weapon)return;
        float t=(Time.time-start)/duration;
        bool attacking=t<1&&activeWeapon==player.weapon;
        Vector2 aim=attacking?attackAim:player.Aim;
        if(PreviewPhase>=0){t=PreviewPhase;aim=PreviewAim;attacking=true;}
        Vector3 moved=transform.position-lastPosition;lastPosition=transform.position;
        bool walking=!AshfallBeta.Paused&&moved.sqrMagnitude>.000001f&&moved.sqrMagnitude<1;
        if(walking)walkCycle+=moved.magnitude*.65f;
        FacingDirection=DirectionIndex(aim);
        WalkFrame=walking?(int)(walkCycle*8)%8:0;
        if(attacking&&!walking)WalkFrame=t<.25f?1:t<.65f?3:5;
        if(PreviewWalk>=0)WalkFrame=(int)PreviewWalk%8;
        body.sprite=directionalBodies[FacingDirection*8+WalkFrame];
        bool sword=player.weapon==WeaponKind.Sword,bow=player.weapon==WeaponKind.Bow;
        float facing=aim.x>=0?1:-1,angle=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
        float wind=attacking?Mathf.Clamp01(t/.25f):0,strike=attacking?Mathf.SmoothStep(0,1,(t-.25f)/.4f):0,recover=attacking?Mathf.SmoothStep(0,1,(t-.65f)/.35f):0;
        float pose=attacking?(t<.25f?Mathf.Lerp(25,-110,wind):t<.65f?Mathf.Lerp(-110,105,strike):Mathf.Lerp(105,25,recover)):25;
        if(reverse)pose=-pose;
        float lean=attacking?(sword?Mathf.Sin(t*Mathf.PI*2)*-13:Mathf.Sin(t*Mathf.PI)*-7)*aim.x:walking?Mathf.Sin(walkCycle*Mathf.PI*2)*2:0;
        transform.rotation=Quaternion.Euler(0,0,lean);
        body.flipX=false;
        if(attacking){float recoil=Mathf.Sin(t*Mathf.PI*2);var scale=transform.localScale;transform.localScale=new Vector3(scale.x*(1+recoil*.035f),scale.y*(1-recoil*.04f),1);}
        var torsoRotation=Quaternion.Euler(0,0,lean);
        Vector3 chest=transform.position+torsoRotation*new Vector3(0,.48f,0);
        bool side=FacingDirection>=2;
        Vector3 shoulder=chest+torsoRotation*new Vector3((side?.08f:.20f)*facing,.02f,0),offShoulder=chest+torsoRotation*new Vector3((side?-.08f:-.20f)*facing,.02f,0);
        Vector2 radial;
        float weaponAngle;
        if(sword){weaponAngle=angle+(spin&&attacking?t*360-90:pose);radial=Direction(weaponAngle);}
        else{weaponAngle=angle;radial=aim;}
        float reach=sword?.35f:bow?.43f:.35f;
        if(attacking&&!sword)reach+=Mathf.Sin(t*Mathf.PI)*(bow?-.10f:.10f);
        Vector3 grip=chest+(Vector3)radial*reach;
        // Restrict the wrist to the two-bone arm's reach before placing the handle.
        grip=shoulder+Vector3.ClampMagnitude(grip-shoulder,.48f);
        Vector3 offGrip=offShoulder+new Vector3(-.025f*facing,-.25f,0);
        if(bow){float draw=attacking?(t<.43f?Mathf.Lerp(.15f,.34f,t/.43f):t<.56f?Mathf.Lerp(.34f,.09f,(t-.43f)/.13f):Mathf.Lerp(.09f,.15f,(t-.56f)/.44f)):.15f;offGrip=grip-(Vector3)aim*draw;}
        else if(sword&&heavy&&attacking)offGrip=grip-(Vector3)radial*.09f;
        else if(!sword)offGrip=chest+new Vector3(-facing*.24f,-.14f,0)+(Vector3)aim*(attacking?Mathf.Sin(t*Mathf.PI)*.1f:0);
        // North-facing attacks belong behind the torso; the far arm is hidden in side views.
        WeaponBehindBody=FacingDirection==1;
        int order=body.sortingOrder+(WeaponBehindBody?-12:0);
        Arm(0,shoulder,grip,grip.x>=shoulder.x?-1:1,order+3);
        Arm(1,offShoulder,offGrip,-facing,side?body.sortingOrder-5:order+1);
        weapon.sprite=WeaponArt.Get(ItemCatalog.Equipped(player),player.weapon);weapon.transform.position=grip;
        weapon.color=Color.white;
        weapon.transform.rotation=Quaternion.Euler(0,0,bow?weaponAngle:weaponAngle-90);
        weapon.flipX=bow;weapon.transform.localScale=Vector3.one*(sword?1.20f:bow?1.0f:1.15f);weapon.sortingOrder=order+5;
        hands[0].sortingOrder=order+6;hands[1].sortingOrder=bow||heavy?order+7:side?body.sortingOrder-2:order+7;
        hands[0].transform.rotation=Quaternion.Euler(0,0,weaponAngle-90);
        bowString.enabled=bow;nockedArrow.enabled=bow&&(!attacking||t<.43f);
        if(bow){Vector3 sideAxis=new Vector3(-aim.y,aim.x,0);bowString.SetPosition(0,grip-(Vector3)aim*.19f+sideAxis*.38f);bowString.SetPosition(1,offGrip);bowString.SetPosition(2,grip-(Vector3)aim*.19f-sideAxis*.38f);bowString.sortingOrder=order+5;nockedArrow.transform.position=offGrip+(Vector3)aim*.24f;nockedArrow.transform.rotation=Quaternion.Euler(0,0,angle);nockedArrow.transform.localScale=new Vector3(.55f,.13f,1);nockedArrow.sortingOrder=order+6;nockedArrow.color=weapon.color;}
    }
    static Vector2 Direction(float degrees){return new Vector2(Mathf.Cos(degrees*Mathf.Deg2Rad),Mathf.Sin(degrees*Mathf.Deg2Rad));}
    void Arm(int i,Vector3 shoulder,Vector3 wrist,float bend,int order){
        const float length=.25f;
        Vector2 delta=wrist-shoulder;float distance=Mathf.Clamp(delta.magnitude,.001f,length*2-.001f);
        Vector2 dir=delta.sqrMagnitude>.0001f?delta.normalized:Vector2.down;
        Vector3 elbow=shoulder+(Vector3)(dir*(distance*.5f)+new Vector2(-dir.y,dir.x)*Mathf.Sqrt(length*length-distance*distance*.25f)*bend);
        Segment(upper[i],shoulder,elbow,.19f,order);Segment(fore[i],elbow,wrist,.145f,order+1);
        hands[i].transform.position=wrist;hands[i].transform.localScale=Vector3.one*.17f;hands[i].color=body.color;
    }
    void Segment(SpriteRenderer r,Vector3 from,Vector3 to,float width,int order){Vector2 delta=to-from;r.transform.position=from;r.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg+90);r.transform.localScale=new Vector3(width/r.sprite.bounds.size.x,delta.magnitude+.035f,1);r.sortingOrder=order;r.color=body.color;}
    void OnDestroy(){if(root)Destroy(root);}
}
