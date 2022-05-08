using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using UnityEngine;
using Vasi;

namespace LP28.GlitchModules;

public class NoExtraBossScenes : GlitchModule
{
    public override string GetDescription() => "No extra rooms from boss scenes";
    
    private ILHook _skipLoadedAdditionalLoads;

    protected override void Enable()
    {
        _skipLoadedAdditionalLoads =
            new ILHook(Util.GetMethodInfo<SceneAdditiveLoadConditional>("LoadAll", false).GetStateMachineTarget(),
                SkipLoadIfLoaded);
    }

    protected override void Disable()
    {
        _skipLoadedAdditionalLoads?.Dispose();
    }

    private void SkipLoadIfLoaded(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        ILLabel loopContinue = null;
        if (c.TryGotoNext(i => i.MatchLdloc(2),
                i => i.MatchCall<Object>("op_Implicit"),
                i => i.MatchBrfalse(out loopContinue)))
        {
            c.Index += 3;
            c.Emit(OpCodes.Ldloc_2);
            c.Emit(OpCodes.Ldfld, Mirror.GetFieldInfo(typeof(SceneAdditiveLoadConditional), "sceneLoaded"));
            c.Emit(OpCodes.Brtrue_S, loopContinue);
        }
    }
}