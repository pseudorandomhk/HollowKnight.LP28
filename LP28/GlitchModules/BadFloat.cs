using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace LP28.GlitchModules;

public class BadFloat : GlitchModule
{
    public override string GetDescription() => "Allow bad float";
    
    private ILHook _noRecoilCheck, _noNoInputCheck, _cancelRecoilNotTouchingGround;

    protected override void Enable()
    {
        _noRecoilCheck = new ILHook(Util.GetMethodInfo<HeroController>("orig_Update"), DisableUpdateRecoilCheck);
        _noNoInputCheck = new ILHook(Util.GetMethodInfo<HeroController>("FailSafeChecks"), FailSafeChecks);
        _cancelRecoilNotTouchingGround = new ILHook(Util.GetMethodInfo<HeroController>("FailSafeChecks"), CancelRecoil);
    }

    protected override void Disable()
    {
        _noRecoilCheck?.Dispose();
        _noNoInputCheck?.Dispose();
        _cancelRecoilNotTouchingGround?.Dispose();
    }

    private void FailSafeChecks(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);

        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("rb2d"),
                i => i.MatchCallvirt<Rigidbody2D>("get_velocity"),
                i => i.MatchLdfld<Vector2>("y")))
        {
            if (c.TryGotoNext(i => i.MatchLdarg(0),
                    i => i.MatchLdfld<HeroController>("hero_state"),
                    i => i.MatchLdcI4(7),
                    i => i.MatchBeq(out _)))
            {
                c.RemoveRange(4);
            }
        }
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("floatingBufferTimer"),
                i => i.MatchCall<Time>("get_deltaTime")))
        {
            c.Index += 3;
            c.Next.Operand = Util.GetMethodInfo<Time>("get_unscaledDeltaTime", false);
        }

        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("cState"),
                i => i.MatchLdfld<HeroControllerStates>("recoiling"),
                i => i.MatchBrfalse(out _),
                i => i.MatchLdarg(0),
                i => i.MatchCallvirt<HeroController>("CancelDamageRecoil")))
        {
            c.RemoveRange(6);
        }
    }
    
    private void DisableUpdateRecoilCheck(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        ILLabel afterRecoilCheck = null;
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchCallvirt<HeroController>("LookForInput"),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("cState"),
                i => i.MatchLdfld<HeroControllerStates>("recoiling"),
                i => i.MatchBrfalse(out afterRecoilCheck)))
        {
            c.Index += 2;
            c.RemoveRange(3);
            c.Next.OpCode = OpCodes.Br_S;
            c.Next.Operand = afterRecoilCheck;
        }
    }

    private void CancelRecoil(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        ILLabel l = null;
        c.TryGotoNext(i => i.MatchLdarg(0),
            i => i.MatchCallvirt<HeroController>("CheckTouchingGround"),
            i => i.MatchBrfalse(out l));
        c.GotoLabel(l);
        c.EmitDelegate(() =>
        {
            if (HeroController.instance.cState.recoiling)
            {
                ReflectionHelper.CallMethod(HeroController.instance, "CancelDamageRecoil");
            }
        });
    }
}