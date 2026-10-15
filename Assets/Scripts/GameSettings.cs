using System;
using UnityEngine;

// Serializable payload for save/UI integration. The runtime copy is kept here so
// presentation systems can react immediately without depending on a menu or save format.
[Serializable]
public class GameSettingsState
{
    public float shakeStrength=1;
    public float hitFlashIntensity=1;
    public bool damageNumbers=true;
    public bool highContrastCues;
}

public static class GameSettings
{
    static GameSettingsState current=new GameSettingsState();
    public static event Action Changed;

    public static GameSettingsState Current { get {return current;} }
    public static float ShakeStrength { get {return current.shakeStrength;} set {Set(value,current.hitFlashIntensity,current.damageNumbers,current.highContrastCues);} }
    public static float HitFlashIntensity { get {return current.hitFlashIntensity;} set {Set(current.shakeStrength,value,current.damageNumbers,current.highContrastCues);} }
    public static bool DamageNumbers { get {return current.damageNumbers;} set {Set(current.shakeStrength,current.hitFlashIntensity,value,current.highContrastCues);} }
    public static bool HighContrastCues { get {return current.highContrastCues;} set {Set(current.shakeStrength,current.hitFlashIntensity,current.damageNumbers,value);} }

    public static void Apply(GameSettingsState state)
    {
        if(state==null){ResetDefaults();return;}
        Set(state.shakeStrength,state.hitFlashIntensity,state.damageNumbers,state.highContrastCues);
    }

    public static GameSettingsState Capture()
    {
        return new GameSettingsState{shakeStrength=current.shakeStrength,hitFlashIntensity=current.hitFlashIntensity,damageNumbers=current.damageNumbers,highContrastCues=current.highContrastCues};
    }

    public static void ResetDefaults(){Set(1,1,true,false);}

    public static void Set(float shakeStrength,float hitFlashIntensity,bool damageNumbers,bool highContrastCues)
    {
        current.shakeStrength=Mathf.Clamp01(shakeStrength);
        current.hitFlashIntensity=Mathf.Clamp01(hitFlashIntensity);
        current.damageNumbers=damageNumbers;
        current.highContrastCues=highContrastCues;
        if(current.shakeStrength<=0)CombatFX.shake=0;
        if(Changed!=null)Changed();
    }

    // High contrast keeps modifier colors recognizable while lifting them away
    // from dark terrain. Shape and labels remain the primary language.
    public static Color CueColor(Color color)
    {
        if(!current.highContrastCues)return color;
        float h,s,v;Color.RGBToHSV(color,out h,out s,out v);
        Color result=Color.HSVToRGB(h,Mathf.Max(.72f,s),1);
        result.a=Mathf.Max(.92f,color.a);
        return result;
    }

    // Hook for BetaActor: replace its fixed hit-flash tint with this property.
    public static Color HitFlashColor { get {return Color.Lerp(Color.white,new Color(2,1.8f,2),current.hitFlashIntensity);} }
}
