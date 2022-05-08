using System;
using System.Collections.Generic;
using Modding;

namespace LP28;

public abstract class GlitchModule
{
    public static readonly Dictionary<Type, GlitchModule> InitializedModules = new();
        
    public virtual string GetName() => this.GetType().ToString().Split('.')[2];
    public virtual string GetDescription() => "";

    protected abstract void Enable();
    protected abstract void Disable();
    public virtual void SetEnabled(bool enable, bool silentUpdate = false)
    {
        if (enable)
        {
            this.Disable();
            this.Enable();
            if (!silentUpdate)
                this.IsEnabled = true;
        }
        else
        {
            this.Disable();
            if (!silentUpdate)
                this.IsEnabled = false;
        }
    }
    public bool IsEnabled { 
        get => GlobalSettings.instance.enabled[this.GetType().Name]; 
        private set => GlobalSettings.instance.enabled[this.GetType().Name] = value;
    }

    public virtual List<IMenuMod.MenuEntry> RegisterMoreOptions() => new();

    protected void Log(object msg)
    {
        LP28.instance.Log($"[{this.GetName()}]: {msg}");
    }
    protected void LogWarn(object msg)
    {
        LP28.instance.LogWarn($"[{this.GetName()}]: {msg}");
    }
    protected void LogError(object msg)
    {
        LP28.instance.LogError($"[{this.GetName()}]: {msg}");
    }
}