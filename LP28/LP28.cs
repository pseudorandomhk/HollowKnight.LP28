using System;
using System.Linq;
using Modding;
using UnityEngine;
using Vasi;
using Object = UnityEngine.Object;

namespace LP28;

public class LP28 : Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings>
{
    private static LP28 _instance;
    public static LP28 instance => _instance ??= new LP28();

    public override string GetVersion() => "0.1.0";
    public bool ToggleButtonInsideMenu => true;

    public override void Initialize()
    {
        GameObject go = new GameObject();
        go.AddComponent<InfoDisplay>();
        Object.DontDestroyOnLoad(go);

        MoreHooks.HookHooks();

        DoEnables();

        MoreHooks.KeyPressedHook += () =>
        {
            foreach (var a in HeroController.instance.gameObject.LocateMyFSM("Map Control")
                         .GetState("Button Down Check").Actions)
            {
                Log($"{a.GetType()}");
            }
            Log("----");
            foreach (var a in HeroController.instance.gameObject.LocateMyFSM("Map Control")
                          .GetState("Double!").Actions)
            {
                Log($"{a.GetType()}");
            }
        };
    }

    private void DoEnables()
    {
        foreach (Type t in typeof(LP28).Assembly.GetTypes()
                     .Where(t => t.IsSubclassOf(typeof(GlitchModule))))
        {
            if (!GlitchModule.InitializedModules.TryGetValue(t, out var module))
            {
                module = (GlitchModule)Activator.CreateInstance(t);
                GlitchModule.InitializedModules[t] = module;
            }
            if (!GlobalSettings.instance.enabled.TryGetValue(t.Name, out bool en) || en)
            {
                module.SetEnabled(true);
                GlobalSettings.instance.enabled[t.Name] = true;
            }
        }
    }

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        return MenuHelper.GetMenuScreen(modListMenu, toggleDelegates!);
    }

    public void OnLoadGlobal(GlobalSettings s)
    {
        GlobalSettings.instance = s ?? GlobalSettings.instance;
    }

    public GlobalSettings OnSaveGlobal()
    {
        return GlobalSettings.instance;
    }
}