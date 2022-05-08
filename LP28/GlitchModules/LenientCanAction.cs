using Modding;

namespace LP28.GlitchModules;

public class LenientCanAction : GlitchModule
{
    public override string GetDescription() => "Less restrictions on quick map, inventory, interaction";

    protected override void Enable()
    {
        On.HeroController.CanQuickMap += CanQuickMap;
        On.HeroController.CanOpenInventory += CanOpenInventory;
        On.HeroController.CanInteract += CanInteract;
    }

    protected override void Disable()
    {
        On.HeroController.CanQuickMap -= CanQuickMap;
        On.HeroController.CanOpenInventory -= CanOpenInventory;
        On.HeroController.CanInteract -= CanInteract;
    }

    private static bool CanInteract(On.HeroController.orig_CanInteract orig, HeroController self)
    {
        return self.CanInput() && self.cState.onGround && !(self.cState.attacking || self.cState.downAttacking ||
                                                            self.cState.upAttacking || self.cState.dashing ||
                                                            self.cState.backDashing);
    }

    private static bool CanOpenInventory(On.HeroController.orig_CanOpenInventory orig, HeroController HC)
    {
        return (!GameManager.instance.isPaused && !HC.controlReqlinquished && !HC.cState.transitioning &&
                !HC.cState.hazardDeath && !HC.cState.hazardRespawning && !HC.playerData.GetBool("disablePause") &&
                HC.CanInput()) || HC.playerData.atBench;
    }

    private static bool CanQuickMap(On.HeroController.orig_CanQuickMap orig, HeroController HC)
    {
        float attack_time = ReflectionHelper.GetField<HeroController, float>(HC, "attack_time");
        return !GameManager.instance.isPaused && !HC.cState.onConveyor && !HC.cState.dashing &&
               !HC.cState.backDashing &&
               (!HC.cState.attacking || attack_time >= HC.ATTACK_RECOVERY_TIME) &&
               !HC.cState.recoiling && !HC.cState.transitioning && !HC.cState.hazardDeath &&
               !HC.cState.hazardRespawning && !HC.cState.recoilFrozen && HC.cState.onGround &&
               HC.CanInput();
    }
}