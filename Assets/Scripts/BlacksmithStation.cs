using System;
using UnityEngine;

// Safe-hub forge with three bounded weapon ranks and explicit material costs.
public class BlacksmithStation : MonoBehaviour
{
    public const int MaxLevel=3;
    public int[] levels=new int[Enum.GetValues(typeof(ItemId)).Length];
    public int[] paths=new int[Enum.GetValues(typeof(ItemId)).Length];
    public bool open;
    public ItemId selected=ItemId.IronSword;
    AshfallDirector d;SpriteRenderer marker;
    public static Vector2 Position {get{return WorldAtlas.Centers[0]+new Vector2(-3.1f,1.15f);}}
    public void Initialize(AshfallDirector director,Sprite art){d=director;var g=new GameObject("Village blacksmith");g.transform.position=Position;g.transform.localScale=Vector3.one*.82f;marker=g.AddComponent<SpriteRenderer>();marker.sprite=art;marker.color=new Color(1,.58f,.22f);marker.sortingOrder=115-(int)(g.transform.position.y*10);}
    public bool Nearby(){return WorldAtlas.Index(d.zone)==0&&Vector2.Distance(d.player.transform.position,Position)<1.8f;}
    public string Prompt(){return Nearby()?"E  Blacksmith / improve weapons":"";}
    public bool Interact(){if(!Nearby())return false;open=true;if(ItemCatalog.WeaponType(selected)<0||!d.Has(selected,1))foreach(ItemId i in Enum.GetValues(typeof(ItemId)))if(ItemCatalog.WeaponType(i)>=0&&d.Has(i,1)){selected=i;break;}return true;}
    public int Level(ItemId item){int i=(int)item;return i>=0&&i<levels.Length?Mathf.Clamp(levels[i],0,MaxLevel):0;}
    public int Bonus(ItemId item){int baseDamage=ItemCatalog.BaseDamage(item,(WeaponKind)Mathf.Max(0,ItemCatalog.WeaponType(item)));return Level(item)*Mathf.Max(2,Mathf.CeilToInt(baseDamage*.1f));}
    public MasterworkPath Path(ItemId item){int i=(int)item;var path=i>=0&&i<paths.Length?(MasterworkPath)Mathf.Clamp(paths[i],0,2):MasterworkPath.None;return path==MasterworkPath.None&&Level(item)>=MaxLevel?MasterworkPath.Force:path;}
    public WeaponMasteryTrait Trait(ItemId item){return WeaponMastery.Trait(item,Level(item),Path(item));}
    public bool Masterworked(ItemId item){return WeaponMastery.IsMasterworked(item,Level(item));}
    public string TraitPreview(ItemId item){return WeaponMastery.Preview(item,Level(item),Path(item));}
    public string PathPreview(ItemId item,MasterworkPath path){return WeaponMastery.Preview(item,Level(item),path);}
    public bool ChoosePath(ItemId item,MasterworkPath path){int i=(int)item;if(path==MasterworkPath.None||ItemCatalog.WeaponType(item)<0||!d.Has(item,1)||Level(item)!=MaxLevel-1||Path(item)!=MasterworkPath.None)return false;paths[i]=(int)path;d.Tell(WeaponMastery.PathName(item,path)+" selected. The final forge will make it permanent.");if(AshfallBeta.Instance)AshfallBeta.Instance.SaveJourney();return true;}
    public ItemId Material(ItemId item){int next=Level(item)+1;if(next==1)return ItemId.IronOre;if(next==2)return ItemId.AncientShard;int type=ItemCatalog.WeaponType(item);return type==0?ItemId.Sunsteel:type==1?ItemId.StormCrystal:ItemId.VenomSac;}
    public int MaterialCost(ItemId item){return Level(item)+1==1?4:3;}
    public int GoldCost(ItemId item){return new[]{0,60,130,220}[Mathf.Clamp(Level(item)+1,0,3)];}
    public bool CanUpgrade(ItemId item){return ItemCatalog.WeaponType(item)>=0&&d.Has(item,1)&&Level(item)<MaxLevel&&(Level(item)!=MaxLevel-1||Path(item)!=MasterworkPath.None)&&d.Has(ItemId.Gold,GoldCost(item))&&d.Has(Material(item),MaterialCost(item));}
    public string Cost(ItemId item){if(Level(item)>=MaxLevel)return "MASTERWORK / "+WeaponMastery.PathName(item,Path(item));if(Level(item)==MaxLevel-1&&Path(item)==MasterworkPath.None)return "Choose a masterwork path before the final forge.";var mat=Material(item);return d.bag[ItemId.Gold]+" / "+GoldCost(item)+" gold   "+d.bag[mat]+" / "+MaterialCost(item)+" "+ItemCatalog.Name(mat);}
    public bool Upgrade(ItemId item){if(!CanUpgrade(item)){d.Tell(Level(item)>=MaxLevel?"That weapon is already masterworked.":Level(item)==MaxLevel-1&&Path(item)==MasterworkPath.None?"Choose its masterwork path first.":"The forge needs more materials.");return false;}d.bag[ItemId.Gold]-=GoldCost(item);d.bag[Material(item)]-=MaterialCost(item);levels[(int)item]++;CombatFX.Burst(Position,new Color(1,.58f,.18f),14);d.Tell(Masterworked(item)?ItemCatalog.Name(item)+" awakened: "+WeaponMastery.PathName(item,Path(item))+".":ItemCatalog.Name(item)+" forged to rank "+Level(item)+".");if(AshfallBeta.Instance)AshfallBeta.Instance.SaveJourney();return true;}
    public void Load(int[] saved,int[] savedPaths=null,int saveVersion=8){levels=new int[Enum.GetValues(typeof(ItemId)).Length];paths=new int[levels.Length];if(saved!=null)for(int i=0;i<Mathf.Min(saved.Length,levels.Length);i++)levels[i]=Mathf.Clamp(saved[i],0,MaxLevel);if(savedPaths!=null)for(int i=0;i<Mathf.Min(savedPaths.Length,paths.Length);i++)paths[i]=Mathf.Clamp(savedPaths[i],0,2);for(int i=0;i<levels.Length;i++)if(levels[i]>=MaxLevel&&paths[i]==0)paths[i]=(int)MasterworkPath.Force;}
    void Update(){if(marker)marker.color=Color.Lerp(new Color(1,.38f,.12f),new Color(1,.86f,.3f),.5f+.5f*Mathf.Sin(Time.unscaledTime*3));}
}
