using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace LP28.GlitchModules;

public class TransitionMultiLoad : GlitchModule
{
    public override string GetDescription() => "Allow room dupes from hitting a transition multiple times";
    
    private ILHook _fieldSceneLoad;
    private List<ILHook> _closureMethodHooks = new();
    
    private static Dictionary<GameManager.SceneLoadInfo, SceneLoad> currentLoads;
    private static FieldInfo routineSceneLoadInfo, closureSceneLoadInfo, closureThis, closureLastSceneName;
    private static Type routineType, closureType;

    protected override void Enable()
    {
        currentLoads =
            new Dictionary<GameManager.SceneLoadInfo, SceneLoad>(
                new Util.IdentityEqualityComparer<GameManager.SceneLoadInfo>());
        var smTarget = MonoMod.Utils.Extensions.GetStateMachineTarget(
            Util.GetMethodInfo<GameManager>("BeginSceneTransitionRoutine"));
        routineType = smTarget.DeclaringType;
        routineSceneLoadInfo = routineType?.GetField("info");
        _fieldSceneLoad = new ILHook(smTarget, RoutineUseLocalSceneLoad);
        _closureMethodHooks = new List<ILHook>();
        closureType = routineType?.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType.Name.Contains("DisplayClass"))?.FieldType;
        if (closureType == null)
        {
            throw new TypeLoadException("Can't find closure type for BeginSceneTransitionRoutine");
        }

        closureSceneLoadInfo = closureType.GetField("info");
        closureThis = closureType.GetFields().FirstOrDefault(fi => fi.Name.Contains("this"));
        closureLastSceneName = closureType.GetField("lastSceneName");
        foreach (var method in closureType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                     .Where(m => m.DeclaringType == closureType))
        {
            _closureMethodHooks.Add(new ILHook(method, DelegateUseLocalSceneLoad));
        }

        _closureMethodHooks.Add(new ILHook(Util.GetMethodInfo(closureType, "<BeginSceneTransitionRoutine>b__2"),
            SkipUnloadSceneIfNotValid));
        _closureMethodHooks.Add(new ILHook(Util.GetMethodInfo(closureType, "<BeginSceneTransitionRoutine>b__4"),
            DisposeSceneLoad));
    }

    protected override void Disable()
    {
        _fieldSceneLoad?.Dispose();
        foreach (var hook in _closureMethodHooks)
        {
            hook?.Dispose();
        }

        currentLoads?.Clear();
    }

    private void DisposeSceneLoad(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchRet()))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, closureSceneLoadInfo);
            c.EmitDelegate<Action<GameManager.SceneLoadInfo>>(info => currentLoads.Remove(info));
        }
    }

    private void SkipUnloadSceneIfNotValid(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        while (c.TryGotoNext(i => i.MatchLdarg(0),
                   i => i.MatchLdfld(closureLastSceneName),
                   i => i.MatchCall<UnityEngine.SceneManagement.SceneManager>("UnloadScene"),
                   i => i.MatchPop()))
        {
            c.EmitDelegate<Action>(() => GameManager.instance.gameState = GameState.ENTERING_LEVEL);
            int start = c.Index;
            c.Index += 4;
            ILLabel post = c.DefineLabel();
            c.MarkLabel(post);
            c.Index = start;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, closureLastSceneName);
            c.EmitDelegate<Func<string, bool>>(lastSceneName =>
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(lastSceneName).IsValid());
            c.Emit(OpCodes.Brfalse_S, post);
            c.Index += 4;
        }
    }

    private void DelegateUseLocalSceneLoad(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        while (c.TryGotoNext(i => i.MatchLdarg(0),
                   i => i.MatchLdfld(closureThis),
                   i => i.MatchLdfld<GameManager>("sceneLoad")))
        {
            c.Index++;
            c.RemoveRange(2);
            c.Emit(OpCodes.Ldfld, closureSceneLoadInfo);
            c.EmitDelegate<Func<GameManager.SceneLoadInfo, SceneLoad>>(info => currentLoads[info]);
        }
    }

    private void RoutineUseLocalSceneLoad(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        ILLabel afterMultipleLoadCheck = null;
        if (c.TryGotoNext(i => i.MatchLdloc(1),
                i => i.MatchLdfld<GameManager>("sceneLoad"),
                i => i.MatchBrfalse(out afterMultipleLoadCheck)))
        {
            c.RemoveRange(3); //remove check for multiple concurrent loads
            c.Emit(OpCodes.Br_S, afterMultipleLoadCheck);
            if (c.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdfld<GameManager.SceneLoadInfo>("SceneName"),
                    i => i.MatchNewobj<SceneLoad>(),
                    i => i.MatchStfld<GameManager>("sceneLoad")))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0); // push 'this' onto stack
                c.EmitDelegate<Func<SceneLoad, IEnumerator, SceneLoad>>((load, routine) =>
                {
                    GameManager.SceneLoadInfo
                        slinfo = (GameManager.SceneLoadInfo)routineSceneLoadInfo.GetValue(routine);
                    currentLoads[slinfo] = load;
                    return load;
                });
                while (c.TryGotoNext(i => i.MatchLdloc(1),
                           i => i.MatchLdfld<GameManager>("sceneLoad")))
                {
                    c.Next.OpCode = OpCodes.Ldarg_0;
                    c.Index++;
                    c.Remove();
                    c.EmitDelegate<Func<IEnumerator, SceneLoad>>(routine =>
                    {
                        var slinfo = (GameManager.SceneLoadInfo)routineSceneLoadInfo.GetValue(routine);
                        SceneLoad sl = currentLoads[slinfo];
                        return sl;
                    });
                }
            }
        }
    }
}