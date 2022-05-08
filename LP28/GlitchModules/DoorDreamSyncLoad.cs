using HutongGames.PlayMaker.Actions;
using OnBeginSceneTransition = On.HutongGames.PlayMaker.Actions.BeginSceneTransition;

namespace LP28.GlitchModules;

public class DoorDreamSyncLoad : GlitchModule
{
    public override string GetDescription() => "Make doors and dreams sync loads";

    protected override void Enable()
    {
        OnBeginSceneTransition.OnEnter += DoSyncLoad;
    }

    protected override void Disable()
    {
        OnBeginSceneTransition.OnEnter -= DoSyncLoad;
    }

    private void DoSyncLoad(OnBeginSceneTransition.orig_OnEnter orig, BeginSceneTransition self)
    {
        if ((self.Owner.name == "Dream Enter" && self.Fsm.Name == "Control") || (self.Fsm.Name == "Door Control"))
        {
            GameManager.instance.inputHandler.acceptingInput = false;
            GameManager.instance.ChangeToScene(self.sceneName.Value, self.entryGateName.Value, self.entryDelay.Value);
            return;
        }

        orig(self);
    }
}