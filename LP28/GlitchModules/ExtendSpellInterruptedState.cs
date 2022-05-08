using HutongGames.PlayMaker.Actions;
using Vasi;
using OnTk2dWatchAnimationEvents = On.HutongGames.PlayMaker.Actions.Tk2dWatchAnimationEvents;

namespace LP28.GlitchModules;

public class ExtendSpellInterruptedState : GlitchModule
{
    public override string GetDescription() => "Float and dive invuln persist through turnaround";
    private static bool delayWatch = true;

    protected override void Enable()
    {
        // Unity update order meme, I hate it here
        OnTk2dWatchAnimationEvents.OnUpdate += DelayWatchUpdate;
        On.HeroAnimationController.Update += DoWatchUpdate;
    }

    protected override void Disable()
    {
        OnTk2dWatchAnimationEvents.OnUpdate -= DelayWatchUpdate;
        On.HeroAnimationController.Update -= DoWatchUpdate;
    }

    private static void DoWatchUpdate(On.HeroAnimationController.orig_Update orig, HeroAnimationController self)
    {
        orig(self);
        var spellctrl = HeroController.instance.gameObject.LocateMyFSM("Spell Control");
        if (spellctrl.ActiveStateName is "Fireball Recoil" or "Q2 Pillar")
        {
            delayWatch = false;
            spellctrl.GetState(spellctrl.ActiveStateName).GetAction<Tk2dWatchAnimationEvents>().OnUpdate();
            delayWatch = true;
        }
    }

    private static void DelayWatchUpdate(OnTk2dWatchAnimationEvents.orig_OnUpdate orig, Tk2dWatchAnimationEvents self)
    {
        if (HeroController.instance != null && HeroController.instance.gameObject != null &&
            (ReferenceEquals(self, GetSpellControl.GetState("Fireball Recoil").GetAction<Tk2dWatchAnimationEvents>())
             || ReferenceEquals(self, GetSpellControl.GetState("Q2 Pillar").GetAction<Tk2dWatchAnimationEvents>())) &&
            delayWatch)
        {
            return;
        }

        orig(self);
    }

    private static PlayMakerFSM GetSpellControl => HeroController.instance.gameObject.LocateMyFSM("Spell Control");
}