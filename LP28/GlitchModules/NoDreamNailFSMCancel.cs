using HutongGames.PlayMaker;
using Vasi;

namespace LP28.GlitchModules;

public class NoDreamNailFSMCancel : GlitchModule
{
    public override string GetDescription() => "Dream nail doesn't cancel other FSMs";

    protected override void Enable()
    {
        MoreHooks.HCStartHook += ChangeCheckFinishTransition;
        if (HeroController.instance)
        {
            ChangeCheckFinishTransition(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= ChangeCheckFinishTransition;
        if (HeroController.instance)
        {
            FsmState canDN = HeroController.instance.gameObject.LocateMyFSM("Dream Nail").GetState("Can Dream Nail?");
            canDN.RemoveTransitionEvent("FINISHED");
            canDN.AddTransition("FINISHED", "Fsm Cancel");
        }
    }

    private void ChangeCheckFinishTransition(HeroController hc)
    {
        FsmState canDN = hc.gameObject.LocateMyFSM("Dream Nail").GetState("Can Dream Nail?");
        canDN.RemoveTransitionEvent("FINISHED");
        canDN.AddTransition("FINISHED", "Take Control");
    }
}