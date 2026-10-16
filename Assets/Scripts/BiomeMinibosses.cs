using System;
using UnityEngine;

// One authored champion per non-boss dangerous biome. State is separate from the
// encounter snapshot so older visited areas can gain a champion exactly once.
public class BiomeMinibosses : MonoBehaviour
{
    public int[] states=new int[8]; // 0 unseen, 1 alive, 2 defeated
    AshfallDirector d;
    public void Initialize(AshfallDirector director){d=director;}
    public void Load(int[] saved){states=new int[8];if(saved!=null)Array.Copy(saved,states,Mathf.Min(saved.Length,states.Length));for(int i=0;i<states.Length;i++)states[i]=Mathf.Clamp(states[i],0,2);}
    public int[] Capture(){return states;}
    public bool Supported(int zone){return zone>0&&zone<8&&zone!=4;}
    public void Deploy(int zone)
    {
        if(!Supported(zone)||states[zone]==2)return;
        var existing=d.enemies.Find(e=>e&&e.IsMiniboss);
        if(existing){states[zone]=1;return;}
        int before=d.enemies.Count;d.Spawn(Kind(zone),1,EliteModifier.None);var champion=d.enemies[before];champion.PromoteMiniboss(zone);champion.transform.position=WorldAtlas.Centers[zone]+WorldAtlas.Recover(zone,WorldAtlas.Rooms[zone][WorldAtlas.Rooms[zone].Length-1]);states[zone]=1;
    }
    public void Defeated(EnemyActor enemy)
    {
        int zone=Mathf.Clamp(enemy.minibossZone,1,7);states[zone]=2;d.Drop(enemy.transform.position,ItemId.Gold,40+zone*7);d.Drop(enemy.transform.position+Vector3.right*.35f,Reward(zone),zone>=6?3:2);d.Tell(Name(zone).ToUpperInvariant()+" DEFEATED / champion cache dropped.");
    }
    public static EnemyKind Kind(int zone){switch(zone){case 1:return EnemyKind.ForestBeast;case 2:return EnemyKind.StoneGolem;case 3:return EnemyKind.Scorpion;case 5:return EnemyKind.MushroomShaman;case 6:return EnemyKind.IceWraith;default:return EnemyKind.DarkKnight;}}
    public static string Name(int zone){switch(zone){case 1:return "Thornmaw";case 2:return "Cragheart";case 3:return "Duneclaw";case 5:return "Mire Oracle";case 6:return "Prism Wraith";case 7:return "Cinder Castellan";default:return "Champion";}}
    public static string Epithet(int zone){switch(zone){case 1:return "ANCIENT PREDATOR";case 2:return "LIVING AVALANCHE";case 3:return "VENOM TYRANT";case 5:return "DROWNED SEER";case 6:return "SHATTERED SOUL";case 7:return "KEEPER OF ASH";default:return "BIOME CHAMPION";}}
    public static ItemId Reward(int zone){if(zone==1)return ItemId.EmberShard;if(zone==2)return ItemId.AncientShard;if(zone==3||zone==5)return ItemId.VenomSac;if(zone==6)return ItemId.StormCrystal;return ItemId.Sunsteel;}
    public static Color Color(int zone){return GameSettings.CueColor(zone==1?new Color(.45f,1,.3f):zone==2?new Color(.55f,.8f,1):zone==3?new Color(1,.55f,.18f):zone==5?new Color(.3f,1,.78f):zone==6?new Color(.45f,.8f,1):new Color(1,.35f,.2f));}
}

// Gold crown plus full name: readable independently of palette and elite symbols.
public class MinibossBadge : MonoBehaviour
{
    EnemyActor enemy;Transform badge;TextMesh crown,nameplate;float age;
    public void Initialize(EnemyActor e){enemy=e;var root=new GameObject("Biome champion nameplate");root.transform.SetParent(transform,false);root.transform.localPosition=new Vector3(0,1.45f,0);badge=root.transform;crown=Label("Crown","^",TextAnchor.MiddleCenter,.035f);crown.transform.localPosition=new Vector3(0,.18f,0);nameplate=Label("Champion name",e.MinibossName.ToUpperInvariant(),TextAnchor.MiddleCenter,.024f);}
    TextMesh Label(string title,string value,TextAnchor anchor,float size){var g=new GameObject(title);g.transform.SetParent(badge,false);var t=g.AddComponent<TextMesh>();t.text=value;t.anchor=anchor;t.alignment=TextAlignment.Center;t.fontSize=36;t.fontStyle=FontStyle.Bold;t.characterSize=size;t.GetComponent<MeshRenderer>().sortingOrder=1910;return t;}
    void Update(){if(!enemy||!badge)return;age+=Time.deltaTime;float inverse=1/Mathf.Max(.01f,transform.lossyScale.x);badge.localScale=Vector3.one*inverse;crown.color=BiomeMinibosses.Color(enemy.minibossZone);nameplate.color=Color.Lerp(Color.white,BiomeMinibosses.Color(enemy.minibossZone),.25f+.1f*Mathf.Sin(age*4));}
    void OnDestroy(){if(badge)Destroy(badge.gameObject);}
}
