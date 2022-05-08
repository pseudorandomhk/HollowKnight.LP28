using Vasi;

namespace LP28.GlitchModules;

public class Underwater : GlitchModule
{
    public override string GetDescription() => "Allow the knight to enter water";

    protected override void Enable()
    {
        MoreHooks.HCStartHook += RemoveCloseInvCancelTransition;
        if (HeroController.instance != null)
        {
            RemoveCloseInvCancelTransition(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= RemoveCloseInvCancelTransition;
        if (GameManager.instance != null)
        {
            GameManager.instance.inventoryFSM.GetState("Close").AddTransition("INVENTORY CANCEL", "Regain Control 2");
        }
    }
    
    private void RemoveCloseInvCancelTransition(HeroController hc)
    {
        GameManager.instance.inventoryFSM.GetState("Close").RemoveTransitionEvent("INVENTORY CANCEL");
    }
}