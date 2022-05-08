using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace LP28.GlitchModules;

public class FlukeDupe : GlitchModule
{
    public override string GetDescription() => "Flukes can kill an enemy multiple times";
    private ILHook _damageHook, _dieHook;

    protected override void Enable()
    {
        _damageHook = new ILHook(Util.GetMethodInfo<SpellFluke>("DoDamage"), FlukeRemoveDeadCheck);
        _dieHook = new ILHook(Util.GetMethodInfo<HealthManager>("Die"), HealthManagerRemoveDeadChecks);
    }

    protected override void Disable()
    {
        _damageHook?.Dispose();
        _dieHook?.Dispose();
    }

    private void HealthManagerRemoveDeadChecks(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        // remove dead check from beginning of Die()
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdfld<HealthManager>("isDead"),
                i => i.MatchBrfalse(out _),
                i => i.MatchRet()))
        {
            c.RemoveRange(4);
        }

        // allow arena kill duping
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdcI4(1),
                i => i.MatchLdfld<HealthManager>("notifiedBattleScene")))
        {
            c.RemoveRange(3);
        }
    }

    private void FlukeRemoveDeadCheck(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdloc(0),
                i => i.MatchCallvirt(Util.GetMethodInfo<HealthManager>("get_IsInvincible")),
                i => i.MatchBrfalse(out _)))
        {
            c.Index += 2;
            c.GotoLabel((ILLabel)c.Next.Operand);
            c.RemoveRange(3);
        }
    }
}