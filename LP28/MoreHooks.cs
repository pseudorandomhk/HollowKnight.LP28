using Modding;
using UnityEngine;

namespace LP28;

public static class MoreHooks
{
    public delegate void HCStartDelegate(HeroController hc);

    public delegate void GMBeginSceneTransitionDelegate(GameManager gm, GameManager.SceneLoadInfo info);

    public delegate void KeyPressedDelegate();

    public static event HCStartDelegate HCStartHook;
    public static event GMBeginSceneTransitionDelegate GMBeginSceneTransitionHook;
    public static event KeyPressedDelegate KeyPressedHook;

    public static void HookHooks()
    {
        On.HeroController.Start += (orig, self) =>
        {
            orig(self);
            HCStartHook?.Invoke(self);
        };
        On.GameManager.BeginSceneTransition += delegate(On.GameManager.orig_BeginSceneTransition orig, GameManager self,
            GameManager.SceneLoadInfo info)
        {
            orig(self, info);
            GMBeginSceneTransitionHook?.Invoke(self, info);
        };
        ModHooks.HeroUpdateHook += () =>
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                LP28.instance.Log("key pressed event");
                KeyPressedHook?.Invoke();
            }
        };
    }
}