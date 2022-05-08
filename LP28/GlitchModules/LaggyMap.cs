using System.Linq;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Vasi;

namespace LP28.GlitchModules;

public class LaggyMap : GlitchModule
{
    protected override void Enable()
    {
        MoreHooks.HCStartHook += IncreaseCloseWait;

        if (HeroController.instance != null)
        {
            IncreaseCloseWait(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= IncreaseCloseWait;

        if (HeroController.instance != null)
        {
            GameCameras.instance.gameObject.FindRecursive("Inventory").LocateMyFSM("Inventory Control").GetState("Close")!.Actions.OfType<Wait>()
                .FirstOrDefault()!.time = 0.2f;
        }
    }

    private void IncreaseCloseWait(HeroController hc)
    {
        GameCameras.instance.gameObject.FindRecursive("Inventory").LocateMyFSM("Inventory Control").GetState("Close")!.Actions.OfType<Wait>()
            .FirstOrDefault()!.time = 0.3f;
    }
}