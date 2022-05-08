using Vasi;

namespace LP28.GlitchModules;

public class DiveInvuln : GlitchModule
{
    public override string GetDescription() => "Allow dive invulnerability";

    protected override void Enable()
    {
        MoreHooks.HCStartHook += SkipResetDamageMode;
        if (HeroController.instance != null)
        {
            SkipResetDamageMode(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= SkipResetDamageMode;
        if (HeroController.instance != null)
        {
            var cancelAll = HeroController.instance.gameObject.LocateMyFSM("Spell Control").GetState("Cancel All");
            cancelAll.ChangeTransition("FINISHED", "Reset Damage Mode");
        }
    }

    private void SkipResetDamageMode(HeroController HC)
    {
        var cancelAll = HC.gameObject.LocateMyFSM("Spell Control").GetState("Cancel All");
        cancelAll.ChangeTransition("FINISHED", "Inactive");
    }
}