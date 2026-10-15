using UnityEngine;

public enum EliteModifier { None, Swift, Armored, Volatile, Vampiric, Warden }

public static class EliteRules
{
    public static EliteModifier ForZone(int zone){return (EliteModifier)(1+(Mathf.Max(1,zone)-1)%5);}
    public static string Name(EliteModifier m){return m==EliteModifier.None?"":m.ToString().ToUpperInvariant();}
    public static string Symbol(EliteModifier m){switch(m){case EliteModifier.Swift:return ">>";case EliteModifier.Armored:return "#";case EliteModifier.Volatile:return "!";case EliteModifier.Vampiric:return "+";case EliteModifier.Warden:return "O";default:return "";}}
    public static Color Color(EliteModifier m){switch(m){case EliteModifier.Swift:return new Color(1,.82f,.18f);case EliteModifier.Armored:return new Color(.35f,.68f,1);case EliteModifier.Volatile:return new Color(1,.35f,.12f);case EliteModifier.Vampiric:return new Color(.9f,.2f,.62f);case EliteModifier.Warden:return new Color(.25f,1,.72f);default:return UnityEngine.Color.white;}}
}

// Compact floating badge: readable without introducing another atlas or obscuring attack cues.
public class EliteBadge : MonoBehaviour
{
    static Sprite square;EnemyActor enemy;Transform badge;SpriteRenderer border,back;TextMesh symbol,label;float pulse;
    public void Initialize(EnemyActor e){enemy=e;if(!square){var texture=new Texture2D(1,1,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.SetPixel(0,0,Color.white);texture.Apply();square=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.one*.5f,1);}
        var root=new GameObject("Elite symbol badge");root.transform.SetParent(transform,false);root.transform.localPosition=new Vector3(0,1.05f,0);badge=root.transform;
        border=Piece("Modifier color frame",new Vector3(1.38f,.34f,1),1897);back=Piece("Dark badge backplate",new Vector3(1.30f,.26f,1),1898);back.color=new Color(.025f,.025f,.045f,.94f);
        symbol=Text("Modifier symbol",EliteRules.Symbol(e.modifier),TextAnchor.MiddleCenter);symbol.transform.localPosition=new Vector3(-.50f,-.015f,0);symbol.characterSize=.028f;
        label=Text("Modifier label",EliteRules.Name(e.modifier),TextAnchor.MiddleLeft);label.transform.localPosition=new Vector3(-.34f,-.015f,0);label.characterSize=.022f;label.color=Color.white;RefreshColors();}
    SpriteRenderer Piece(string title,Vector3 scale,int order){var g=new GameObject(title);g.transform.SetParent(badge,false);g.transform.localScale=scale;var r=g.AddComponent<SpriteRenderer>();r.sprite=square;r.sortingOrder=order;return r;}
    TextMesh Text(string title,string value,TextAnchor anchor){var g=new GameObject(title);g.transform.SetParent(badge,false);var t=g.AddComponent<TextMesh>();t.text=value;t.anchor=anchor;t.alignment=anchor==TextAnchor.MiddleLeft?TextAlignment.Left:TextAlignment.Center;t.fontSize=32;t.fontStyle=FontStyle.Bold;t.GetComponent<MeshRenderer>().sortingOrder=1900;return t;}
    void RefreshColors(){if(!enemy)return;Color cue=GameSettings.CueColor(EliteRules.Color(enemy.modifier));border.color=cue;symbol.color=cue;}
    void Update(){if(!enemy||!badge)return;pulse+=Time.deltaTime;float parentScale=Mathf.Max(.01f,transform.lossyScale.x),breath=1+.035f*Mathf.Sin(pulse*5);badge.localScale=Vector3.one*(breath/parentScale);RefreshColors();}
    void OnDestroy(){if(badge)Destroy(badge.gameObject);}
}

// Volatile deaths are dangerous only after a large, 0.8-second warning ring.
public class VolatileBurst : MonoBehaviour
{
    public AshfallDirector director;float age;
    void Start(){CombatFX.Ring(transform.position,2.25f,EliteRules.Color(EliteModifier.Volatile),.8f);}
    void Update(){if(AshfallBeta.Paused)return;age+=Time.deltaTime;if(age<.8f)return;CombatFX.Burst(transform.position,EliteRules.Color(EliteModifier.Volatile),18);if(director&&director.player&&Vector2.Distance(transform.position,director.player.transform.position)<2.25f&&CombatRules.Clear(director,transform.position,director.player.transform.position))director.player.Hurt(18);Destroy(gameObject);}
}
