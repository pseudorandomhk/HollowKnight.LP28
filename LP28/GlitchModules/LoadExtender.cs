using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using UnityEngine;

namespace LP28.GlitchModules;

public class LoadExtender : GlitchModule
{
    [SerializableSetting]
    public static float DELAY = 1.5f;
    
    public override string GetDescription() => "Extends all loads, aka toaster simulator";

    private ILHook _increaseVerticalWait, _delaySceneFetch;

    protected override void Enable()
    {
        _increaseVerticalWait = new ILHook(Util.GetMethodInfo<HeroController>("EnterScene").GetStateMachineTarget(),
            IncreaseVerticalWait);
        _delaySceneFetch = new ILHook(
            Util.GetMethodInfo<GameManager>("BeginSceneTransitionRoutine").GetStateMachineTarget(),
            DelaySceneFetch);
    }

    protected override void Disable()
    {
        _increaseVerticalWait?.Dispose();
        _delaySceneFetch?.Dispose();
    }

    public override List<IMenuMod.MenuEntry> RegisterMoreOptions()
    {
        var res = new List<IMenuMod.MenuEntry>
        {
            new()
            {
                Name = "Delay",
                Description = "Amount of time to add to each load",
                Values = Enumerable.Range(0, 101).Select(x => (x * 0.1).ToString(CultureInfo.InvariantCulture)).ToArray(),
                Saver = i => DELAY = 0.1f * i,
                Loader = () => (int) (DELAY / 0.1)
            }
        };
        return res;
    }

    private void DelaySceneFetch(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdarg(0),
                i => i.MatchLdcR4(0.5f)))
        {
            c.Index++;
            c.Next.Operand = 0.5f + DELAY;
        }
    }

    private static void IncreaseVerticalWait(ILContext ctx)
    {
        ILCursor c = new ILCursor(ctx).Goto(0);
        if (c.TryGotoNext(i => i.MatchLdcR4(0.33f),
                i => i.MatchNewobj<WaitForSeconds>()))
        {
            c.Next.Operand = 0.66f;
        }
    }
}