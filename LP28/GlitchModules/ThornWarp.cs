using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Vasi;

namespace LP28.GlitchModules;

public class ThornWarp // : GlitchModule
{
    private static FsmStateAction wait = new Wait
    {
        time = 0.3f,
        finishEvent = FsmEvent.Finished,
        realTime = false
    };

    // private ILHook recoilUnscaledTime;

    // protected override void Enable()
    // {
    //     recoilUnscaledTime = new ILHook(Util.GetMethodInfo<HeroController>("orig_Update"), RecoilUseUnscaledDeltaTime);
    //     
    //     MoreHooks.HCStartHook += RemoveWait;
    //     if (HeroController.instance != null)
    //     {
    //         RemoveWait(HeroController.instance);
    //     }
    // }
    //
    // protected override void Disable()
    // {
    //     recoilUnscaledTime?.Dispose();
    //     
    //     MoreHooks.HCStartHook -= RemoveWait;
    //     if (HeroController.instance != null)
    //     {
    //         HeroController.instance.fsm_thornCounter.GetState("Counter").AddAction(wait);
    //     }
    // }

    private void RecoilUseUnscaledDeltaTime(ILContext ctx)
    {
        
    }

    private void RemoveWait(HeroController hc)
    {
        var counter = hc.fsm_thornCounter.GetState("Counter");
        wait = counter.GetAction<Wait>();
        counter.RemoveAction<Wait>();

        hc.fsm_thornCounter.FsmGlobalTransitions.FirstOrDefault(t => t.EventName == "LEVEL LOADED");
    }
}