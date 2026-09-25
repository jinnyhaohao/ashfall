using UnityEngine;

public enum EliteModifier { None, Swift, Armored, Volatile, Vampiric, Warden }

public static class EliteRules
{
    public static EliteModifier ForZone(int zone){return (EliteModifier)(1+(Mathf.Max(1,zone)-1)%5);}
    public static string Name(EliteModifier m){return m==EliteModifier.None?"":m.ToString().ToUpperInvariant();}
    public static Color Color(EliteModifier m){switch(m){case EliteModifier.Swift:return new Color(1,.82f,.18f);case EliteModifier.Armored:return new Color(.35f,.68f,1);case EliteModifier.Volatile:return new Color(1,.35f,.12f);case EliteModifier.Vampiric:return new Color(.9f,.2f,.62f);case EliteModifier.Warden:return new Color(.25f,1,.72f);default:return UnityEngine.Color.white;}}
}

// Compact floating badge: readable without introducing another atlas or obscuring attack cues.
public class EliteBadge : MonoBehaviour
{
    EnemyActor enemy;TextMesh text;float pulse;
    public void Initialize(EnemyActor e){enemy=e;var g=new GameObject("Elite tell");g.transform.SetParent(transform,false);g.transform.localPosition=new Vector3(0,1.05f,0);text=g.AddComponent<TextMesh>();text.text=EliteRules.Name(e.modifier);text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=32;text.characterSize=.035f;text.fontStyle=FontStyle.Bold;text.color=EliteRules.Color(e.modifier);text.GetComponent<MeshRenderer>().sortingOrder=1900;}
    void Update(){if(!enemy||!text)return;pulse+=Time.deltaTime;text.transform.localScale=Vector3.one*(1+.06f*Mathf.Sin(pulse*5));}
}

// Volatile deaths are dangerous only after a large, 0.8-second warning ring.
public class VolatileBurst : MonoBehaviour
{
    public AshfallDirector director;float age;
    void Start(){CombatFX.Ring(transform.position,2.25f,EliteRules.Color(EliteModifier.Volatile),.8f);}
    void Update(){if(AshfallBeta.Paused)return;age+=Time.deltaTime;if(age<.8f)return;CombatFX.Burst(transform.position,EliteRules.Color(EliteModifier.Volatile),18);if(director&&director.player&&Vector2.Distance(transform.position,director.player.transform.position)<2.25f&&CombatRules.Clear(director,transform.position,director.player.transform.position))director.player.Hurt(18);Destroy(gameObject);}
}
