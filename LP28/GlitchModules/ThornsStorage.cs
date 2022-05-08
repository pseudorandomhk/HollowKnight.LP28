using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Vasi;

namespace LP28.GlitchModules;

public class ThornsStorage : GlitchModule
{
    public override string GetDescription() => "Thorns of agony hitbox can persist";
    private FsmStateAction deactivateThorns;

    protected override void Enable()
    {
        MoreHooks.HCStartHook += RemoveThornDeactivate;
        MoreHooks.GMBeginSceneTransitionHook += ResetWhenLevelUnloaded;

        if (HeroController.instance != null)
        {
            RemoveThornDeactivate(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= RemoveThornDeactivate;
        MoreHooks.GMBeginSceneTransitionHook -= ResetWhenLevelUnloaded;
        if (HeroController.instance != null)
        {
            HeroController.instance.fsm_thornCounter.GetState("Idle").AddAction(deactivateThorns);
        }
    }

    private void RemoveThornDeactivate(HeroController HC)
    {
        deactivateThorns = HC.fsm_thornCounter.GetState("Idle").GetAction<ActivateGameObject>();
        HC.fsm_thornCounter.GetState("Idle").RemoveAction<ActivateGameObject>();
    }

    private void ResetWhenLevelUnloaded(GameManager gm, GameManager.SceneLoadInfo info)
    {
        HeroController.instance.fsm_thornCounter.SendEvent("LEVEL LOADED");
    }
}