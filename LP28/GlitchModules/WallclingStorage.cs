using System.Linq;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace LP28.GlitchModules;

// TODO: fix wcs after sliding down off walls
public class WallclingStorage : GlitchModule
{
    public override string GetDescription() => "Allow pre-lifeblood wallcling storage effects";
    
    private ILHook _finishDashingHook, _setMotionStateHook, _cancelWallslideHook, _noWCSOnRegainControl;

    protected override void Enable()
    {
        _finishDashingHook =
            new ILHook(Util.GetMethodInfo<HeroController>("FinishedDashing"), RemoveWallTouchLRCheck);
        _setMotionStateHook =
            new ILHook(
                Util.GetMethodInfos<HeroController>("SetStartingMotionState").First(m => m.GetParameters().Any()),
                RemoveStartingMotionSetTouchWall);
        _cancelWallslideHook =
            new ILHook(Util.GetMethodInfo<HeroController>("CancelWallsliding"), MaintainTouchingWallState);
        _noWCSOnRegainControl =
            new ILHook(Util.GetMethodInfo<HeroController>("RegainControl"), RemoveWallTouchSet);
    }

    protected override void Disable()
    {
        _finishDashingHook?.Dispose();
        _setMotionStateHook?.Dispose();
        _cancelWallslideHook?.Dispose();
        _noWCSOnRegainControl?.Dispose();
    }

    private void RemoveWallTouchSet(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("cState"),
                i => i.MatchLdcI4(1),
                i => i.MatchStfld<HeroControllerStates>("touchingWall")))
        {
            c.RemoveRange(4);
        }

        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("transform"),
                i => i.MatchCallvirt<Transform>("get_localScale"),
                i => i.MatchLdfld<Vector3>("x"),
                i => i.MatchLdcR4(0.0f),
                i => i.MatchBgeUn(out _)))
        {
            c.RemoveRange(19);
        }
    }

    private void MaintainTouchingWallState(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdarg(0), 
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<HeroController>("touchingWallL"),
                i => i.MatchLdarg(0),
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<HeroController>("touchingWallR")))
        {
            c.RemoveRange(6);
        }
    }

    private void RemoveStartingMotionSetTouchWall(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("cState"), 
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<HeroControllerStates>("touchingWall")))
        {
            c.RemoveRange(4);
        }
    }
    
    private void RemoveWallTouchLRCheck(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("touchingWallL"),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>("touchingWallR"),
                i => i.MatchBr(out _),
                i => i.MatchLdcI4(1),
                i => i.MatchAnd()))
        {
            c.RemoveRange(8);
        }
    }
}