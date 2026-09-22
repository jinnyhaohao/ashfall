using UnityEngine;
public static class CombatRules
{
    // Sweep through terrain at sub-tile intervals so fast shots cannot tunnel through walls.
    public static bool Trace(AshfallDirector d,Vector2 from,Vector2 to,out Vector2 end){int z=WorldAtlas.Index(d.zone);Vector2 home=WorldAtlas.Centers[z];end=from;int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)/.08f));if(!WorldAtlas.Floor(z,from-home))return false;for(int i=1;i<=steps;i++){Vector2 p=Vector2.Lerp(from,to,i/(float)steps);if(!WorldAtlas.Floor(z,p-home))return false;end=p;}return true;}
    public static bool Clear(AshfallDirector d,Vector2 from,Vector2 to){Vector2 end;return Trace(d,from,to,out end);}
    public static bool InCombat(AshfallDirector d){if(Time.time<d.combatUntil)return true;foreach(var e in d.enemies)if(e&&e.hp>0&&Vector2.Distance(e.transform.position,d.player.transform.position)<8&&Clear(d,e.transform.position,d.player.transform.position))return true;return false;}
}
