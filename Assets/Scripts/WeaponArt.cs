using System.Collections.Generic;
using UnityEngine;

// One source of truth for held, dropped, inventory and quickbar weapon artwork.
public static class WeaponArt
{
    static readonly Dictionary<ItemId,Sprite> skins=new Dictionary<ItemId,Sprite>();
    static Sprite[] starters;
    public static readonly ItemId[] Items={ItemId.IronSword,ItemId.FangSword,ItemId.GoblinCleaver,ItemId.HollowBlade,ItemId.EmberSword,ItemId.BoneBow,ItemId.FrostBow,ItemId.CrystalStaff,ItemId.VenomSaber,ItemId.ThunderAxe,ItemId.SunsteelBlade,ItemId.ThornBow,ItemId.StormBow,ItemId.DuskStaff};
    public static void Initialize(){
        if(starters!=null)return;
        starters=new Sprite[3];Vector2[] grips={new Vector2(.50f,.30f),new Vector2(.36f,.49f),new Vector2(.50f,.28f)};
        for(int i=0;i<3;i++){var original=AshfallBeta.Instance.art[13+i];starters[i]=Sprite.Create(original.texture,original.rect,grips[i],original.pixelsPerUnit);starters[i].name="Starter "+(WeaponKind)i;}
        var texture=Resources.Load<Texture2D>("AshfallWeaponSkins");int cw=texture.width/4,ch=texture.height/2;
        for(int i=0;i<8;i++){
            int ox=i%4*cw,oy=(1-i/4)*ch;var pixels=texture.GetPixels(ox,oy,cw,ch);int minX=cw,minY=ch,maxX=0,maxY=0;
            for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(pixels[y*cw+x].a>.2f){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
            int type=ItemCatalog.WeaponType(Items[i]);Vector2 pivot=type==0?new Vector2(.5f,.18f):type==1?new Vector2(.25f,.50f):new Vector2(.5f,.25f);
            var sprite=Sprite.Create(texture,new Rect(ox+minX,oy+minY,maxX-minX+1,maxY-minY+1),pivot,maxY-minY+1);sprite.name=Items[i].ToString();skins.Add(Items[i],sprite);
        }
        LoadExpansion();
    }
    static void LoadExpansion(){var t=Resources.Load<Texture2D>("AshfallWeaponExpansion");for(int i=0;i<6;i++){var item=ExpansionContent.Weapons[i];int kind=ItemCatalog.WeaponType(item);var sprite=ExpansionContent.Slice(t,i,3,2,kind==0?new Vector2(.5f,.18f):kind==1?new Vector2(.25f,.5f):new Vector2(.5f,.25f));sprite.name=item.ToString();skins.Add(item,sprite);}}
    public static Sprite Get(ItemId item,WeaponKind kind){Initialize();Sprite sprite;return skins.TryGetValue(item,out sprite)?sprite:starters[(int)kind];}
    public static ItemId Slot(PlayerHero p,int slot){return slot==0?p.swordSlot:slot==1?p.bowSlot:p.staffSlot;}
    public static string SlotName(PlayerHero p,int slot){ItemId item=Slot(p,slot);return item!=ItemId.Gold?ItemCatalog.Name(item):slot==0?"Rusty Sword":slot==1?"Hunter Bow":"Ember Staff";}
    public static void Draw(Sprite sprite,Rect box){Rect source=sprite.rect;float scale=Mathf.Min(box.width/source.width,box.height/source.height);Rect target=new Rect(box.center.x-source.width*scale*.5f,box.center.y-source.height*scale*.5f,source.width*scale,source.height*scale);GUI.DrawTextureWithTexCoords(target,sprite.texture,new Rect(source.x/sprite.texture.width,source.y/sprite.texture.height,source.width/sprite.texture.width,source.height/sprite.texture.height));}
}
