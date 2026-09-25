using UnityEngine;
public class WorldPresentation
{
    public Texture2D[] maps=new Texture2D[8];
    int selected=-1;public bool local=true;
    AshfallBeta beta;AshfallDirector d;Sprite[] art;
    public WorldPresentation(AshfallBeta b,AshfallDirector director,Sprite[] sprites){beta=b;d=director;art=sprites;Build();}
    SpriteRenderer Prop(int index,Vector2 position,float size,Color color){var g=new GameObject("World landmark");g.transform.position=position;g.transform.localScale=Vector3.one*size;var r=g.AddComponent<SpriteRenderer>();r.sprite=art[index];r.color=color;r.sortingOrder=100-(int)(position.y*10);return r;}
    void Build(){var rng=new System.Random(9137);for(int z=0;z<8;z++){
        maps[z]=WorldAtlas.MapTexture(z);int size=528;var t=new Texture2D(size,size,TextureFormat.RGB24,false);t.filterMode=FilterMode.Point;var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            Vector2 p=new Vector2(x/12f-22,y/12f-22);float field=WorldAtlas.Field(z,p),trail=WorldAtlas.Trail(z,p);float n=Mathf.Floor(Mathf.PerlinNoise(x*.065f+z*41,y*.065f)*5)/5;
            Color c=WorldAtlas.Colors[z]*(.83f+n*.28f);
            if(field>0){c=z==5?new Color(.075f,.19f,.24f):z==1||z==0?new Color(.09f,.18f,.14f):new Color(.085f,.08f,.15f);c*=.83f+n*.35f;if(z==5&&y%13==0&&x%19<8)c*=1.25f;}
            else if(field>-.55f)c*=.58f;
            else if(trail<1.15f){c=z==5?new Color(.40f,.32f,.22f):z==6?new Color(.43f,.48f,.64f):new Color(.52f,.43f,.32f);if((z==0||z==4||z==7)&&(y%12==0||(x+y/12*6)%12==0))c*=.75f;if(z==5&&x%6==0)c*=.7f;}
            else if(rng.NextDouble()>.985)c*=1.2f;
            if(field<-.55f){int hash=(x/3*73+y/3*197+z*811)&255;if(z==4||z==7){if(y%12==0||(x+y/12*6)%12==0)c*=.78f;if(hash<7&&x%3==0)c*=.7f;}else if(z==0||z==1||z==5){if(trail>1.3f&&hash<24&&(x%3==1||y%3==0))c*=1.25f;}else{if(hash<14&&y%3==0)c*=.75f;if(z==6&&hash>250)c=new Color(.35f,.65f,.76f);}}
            pixels[y*size+x]=c;
        }t.SetPixels(pixels);t.Apply();var g=new GameObject(WorldAtlas.Names[z]+" terrain");g.transform.position=WorldAtlas.Centers[z];var r=g.AddComponent<SpriteRenderer>();r.sprite=Sprite.Create(t,new Rect(0,0,size,size),Vector2.one*.5f,12);r.sortingOrder=-1000;
        for(int k=0;k<190;k++){Vector2 p=new Vector2((float)rng.NextDouble()*42-21,(float)rng.NextDouble()*42-21);float f=WorldAtlas.Field(z,p);if(f<2||f>4.5f)continue;int idx=z==0||z==1||z==5?7:9;Color color=z==5?new Color(.6f,.85f,.8f):z==6?new Color(.5f,.85f,1):z==7?new Color(.8f,.55f,.6f):Color.white;Prop(idx,p+WorldAtlas.Centers[z],idx==7?2.6f:1.8f,color);}
        if(z==0){Prop(8,new Vector2(-5,3),3.3f,Color.white);Prop(8,new Vector2(6,6),3,Color.white);Prop(10,new Vector2(1.8f,0),1.1f,Color.white);}
        else if(z==1||z==4||z==6){Vector2 landmark=WorldAtlas.Rooms[z][WorldAtlas.Rooms[z].Length-1]+WorldAtlas.Centers[z];Prop(z==1?7:11,landmark+new Vector2(3,1),z==1?2.5f:1.8f,z==6?Color.cyan:Color.white);}
        WorldDetails.Build(z);
        foreach(int neighbor in WorldAtlas.Neighbors(z)){Vector2 p=WorldAtlas.Centers[z]+WorldAtlas.Gate(z,neighbor);Prop(11,p,1.1f,new Color(.8f,.95f,1));var sign=new GameObject("Route to "+WorldAtlas.Names[neighbor]);sign.transform.position=p+Vector2.up*1.5f;var text=sign.AddComponent<TextMesh>();text.text=WorldAtlas.Names[neighbor];text.fontSize=36;text.characterSize=.05f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(1,.85f,.5f);text.GetComponent<MeshRenderer>().sortingOrder=1700;}
    }}
    public void Update(){if(AshfallBeta.Paused)return;int z=WorldAtlas.Index(d.zone);foreach(int n in WorldAtlas.Neighbors(z))if(Vector2.Distance(d.player.transform.position,WorldAtlas.Centers[z]+WorldAtlas.Gate(z,n))<1.5f&&Input.GetKeyDown(KeyCode.E)){beta.TryTravel(n);return;}if(Input.GetKeyDown(KeyCode.E)){if(d.blacksmith&&d.blacksmith.Interact())return;if(d.objectives&&d.objectives.Interact())return;var node=WorldDetails.Nearest(d);if(node)node.Harvest(d);}}
    void Box(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Dot(Vector2 p,Color c,float size=6){Box(new Rect(p.x-size/2,p.y-size/2,size,size),c);}
    void Line(Vector2 a,Vector2 b,Color c){int count=Mathf.CeilToInt(Vector2.Distance(a,b)/5);for(int i=0;i<count;i++)Dot(Vector2.Lerp(a,b,i/(float)count),c,3);}
    void MapDetails(int area,Rect rect,GUIStyle style){
        GUI.Label(new Rect(rect.x-100,rect.y+5,110,28),"N  ^",style);GUI.Label(new Rect(rect.x-100,rect.y+34,125,80),"CHAMBERS\n& PASSAGES",style);
        for(int i=0;i<WorldAtlas.Rooms[area].Length;i++){Vector2 p=WorldAtlas.MapPoint(WorldAtlas.Rooms[area][i],rect);GUI.Label(new Rect(p.x-6,p.y-19,45,22),(i+1).ToString("00"),new GUIStyle(style){fontSize=11,normal={textColor=new Color(.9f,.85f,.7f)}});}
        foreach(var node in WorldDetails.Nodes)if(node&&node.zone==area){Vector2 p=WorldAtlas.MapPoint((Vector2)node.transform.position-WorldAtlas.Centers[area],rect);Dot(p,node.Ready?new Color(.4f,.95f,.65f):new Color(.25f,.35f,.3f),4);}
        GUI.Label(new Rect(rect.x-100,rect.y+rect.height-55,180,52),"Green: resources\nNumbers: chambers",style);
    }
    public void Mini(GUIStyle text){var node=WorldDetails.Nearest(d);if(node)GUI.Label(new Rect(470,548,340,24),"E  Gather "+ItemCatalog.Name(node.item),text);int z=WorldAtlas.Index(d.zone);Rect rect=new Rect(1121,143,126,126);Box(new Rect(1111,134,146,163),new Color(.055f,.07f,.13f,.9f));GUI.DrawTexture(rect,maps[z]);foreach(int n in WorldAtlas.Neighbors(z))Dot(WorldAtlas.MapPoint(WorldAtlas.Gate(z,n),rect),Color.cyan);Dot(WorldAtlas.MapPoint((Vector2)d.player.transform.position-WorldAtlas.Centers[z],rect),Color.yellow,6);GUI.Label(new Rect(1119,272,133,21),"M MAP / E ROUTE",text);foreach(int n in WorldAtlas.Neighbors(z))if(Vector2.Distance(d.player.transform.position,WorldAtlas.Centers[z]+WorldAtlas.Gate(z,n))<2)GUI.Label(new Rect(470,573,340,24),"E  Travel to "+WorldAtlas.Names[n],text);if(d.blacksmith){string forge=d.blacksmith.Prompt();if(forge!="")GUI.Label(new Rect(470,522,430,24),forge,text);}if(d.objectives){string prompt=d.objectives.Prompt();if(prompt!="")GUI.Label(new Rect(470,522,430,24),prompt,text);}}
    public void Draw(GUIStyle heading,GUIStyle text){int current=WorldAtlas.Index(d.zone);if(selected<0)selected=current;Box(new Rect(88,90,1104,535),new Color(.055f,.065f,.12f,.99f));GUI.Label(new Rect(113,105,700,42),"ASHFALL / EXPLORER'S ATLAS",heading);if(GUI.Button(new Rect(911,112,248,30),local?"Show world routes":"Show local terrain"))local=!local;
        Rect canvas=new Rect(112,166,700,425);Box(canvas,new Color(.08f,.105f,.15f));
        if(local){Rect terrain=new Rect(240,173,410,410);GUI.DrawTexture(terrain,maps[selected]);MapDetails(selected,terrain,text);foreach(int n in WorldAtlas.Neighbors(selected)){Vector2 p=WorldAtlas.MapPoint(WorldAtlas.Gate(selected,n),terrain);Dot(p,Color.cyan,8);GUI.Label(new Rect(p.x+7,p.y-8,170,28),WorldAtlas.Names[n],text);}Vector2 landmark=WorldAtlas.MapPoint(WorldAtlas.Rooms[selected][WorldAtlas.Rooms[selected].Length-1],terrain);Dot(landmark,new Color(1,.5f,.25f),8);GUI.Label(new Rect(landmark.x-55,landmark.y+8,220,28),WorldAtlas.Landmarks[selected],text);if(selected==current){Dot(WorldAtlas.MapPoint((Vector2)d.player.transform.position-WorldAtlas.Centers[current],terrain),Color.yellow,10);foreach(var e in d.enemies)if(e)Dot(WorldAtlas.MapPoint((Vector2)e.transform.position-WorldAtlas.Centers[current],terrain),new Color(1,.3f,.3f),4);}}
        else {Vector2 offset=new Vector2(112,163);for(int i=0;i<WorldAtlas.Links.GetLength(0);i++)Line(offset+WorldAtlas.MapPositions[WorldAtlas.Links[i,0]],offset+WorldAtlas.MapPositions[WorldAtlas.Links[i,1]],new Color(.43f,.47f,.5f));for(int z=0;z<8;z++){Vector2 p=offset+WorldAtlas.MapPositions[z];GUI.color=WorldAtlas.Unlocked(z,d)?Color.white:new Color(.45f,.45f,.5f);GUI.DrawTexture(new Rect(p.x-42,p.y-42,84,84),maps[z]);GUI.color=Color.white;if(GUI.Button(new Rect(p.x-73,p.y+24,146,27),(z==current?"> ":"")+WorldAtlas.Names[z])){selected=z;local=true;}}}
        GUI.Label(new Rect(838,170,325,40),WorldAtlas.Names[selected],heading);GUI.Label(new Rect(838,216,322,65),WorldAtlas.Hints[selected],new GUIStyle(text){wordWrap=true});
        for(int z=0;z<8;z++)if(GUI.Button(new Rect(840,285+z*29,315,26),(z==selected?"> ":"")+WorldAtlas.Names[z]+(WorldAtlas.Unlocked(z,d)?"":" [sealed]"))){selected=z;}
        string travelReason;bool canTravel=beta.CanTravel(selected,out travelReason);GUI.enabled=canTravel;if(GUI.Button(new Rect(840,534,315,38),"Travel to "+WorldAtlas.Names[selected])){if(beta.TryTravel(selected))beta.CloseMap();}GUI.enabled=true;
        GUI.Label(new Rect(113,595,1040,25),canTravel?"Yellow: you   Cyan: exits   Red: monsters   Orange: landmark   M / ESC closes":travelReason,text);
    }
}
