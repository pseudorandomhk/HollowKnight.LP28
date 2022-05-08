using System;
using HutongGames.PlayMaker.Actions;
using Vasi;

// TODO: don't allow ungrounded rocket jump
namespace LP28.GlitchModules;

public class RocketJump : GlitchModule
{
    public override string GetDescription() => "Allow rocket jumps";

    protected override void Enable()
    {
        MoreHooks.HCStartHook += AllowRocketJump;
        if (HeroController.instance != null)
        {
            AllowRocketJump(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= AllowRocketJump;
    }

    private void AllowRocketJump(HeroController HC)
    {
        HC.gameObject.LocateMyFSM("Superdash").GetState("Cancelable").RemoveAllOfType<SetVelocity2d>();
    }
}