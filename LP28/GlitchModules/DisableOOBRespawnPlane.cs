using UnityEngine;

namespace LP28.GlitchModules;

public class DisableOOBRespawnPlane : GlitchModule
{
    public override string GetDescription() => "Disable the respawn plane beneath all levels";

    protected override void Enable()
    {
        On.KillOnContact.OnCollisionEnter2D += DisableHCBoundCheck;
    }

    protected override void Disable()
    {
        On.KillOnContact.OnCollisionEnter2D -= DisableHCBoundCheck;
    }

    private void DisableHCBoundCheck(On.KillOnContact.orig_OnCollisionEnter2D orig, KillOnContact self,
        Collision2D collision)
    {
        GameObject boundscage = GameObject.Find("Bounds Cage");
        var koc = boundscage.GetComponent<KillOnContact>();
        if (!ReferenceEquals(self, koc))
        {
            orig(self, collision);
            return;
        }

        var colliderGo = collision.gameObject;
        if (!ReferenceEquals(colliderGo, HeroController.instance.gameObject))
        {
            orig(self, collision);
        }
    }
}