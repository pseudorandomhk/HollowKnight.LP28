using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Attribute = System.Attribute;

namespace LP28;

[Serializable]
public class GlobalSettings
{
    internal readonly Dictionary<FieldInfo, Type> fields = new();

    public Dictionary<string, bool> enabled = new();
    
    public Dictionary<string, bool> bools = new();
    public Dictionary<string, float> floats = new();
    public Dictionary<string, int> ints = new();

    public static GlobalSettings instance = new();

    public GlobalSettings()
    {
        foreach (var field in typeof(GlobalSettings).Assembly.DefinedTypes.SelectMany(t => t.GetFields())
                     .Where(SerializableSettingAttribute.ShouldSerialize))
        {
            fields.Add(field, field.DeclaringType);
        }

        instance = this;
    }

    [OnSerializing]
    public void BeforeSerialize(StreamingContext _)
    {
        foreach (var (field, type) in fields)
        {
            if (field.FieldType == typeof(bool))
            {
                bools[$"{type.Name}::{field.Name}"] =
                    (bool)field.GetValue(field.IsStatic ? null : GlitchModule.InitializedModules[type]);
            }
            else if (field.FieldType == typeof(float))
            {
                floats[$"{type.Name}::{field.Name}"] =
                    (float)field.GetValue(field.IsStatic ? null : GlitchModule.InitializedModules[type]);
            }
            else if (field.FieldType == typeof(int))
            {
                ints[$"{type.Name}::{field.Name}"] =
                    (int)field.GetValue(field.IsStatic ? null : GlitchModule.InitializedModules[type]);
            }
            // else
            // {
            //     LP28.instance.LogError($"Field {field.Name} on type {type.Name} has unsupported type {field.FieldType}");
            // }
        }

        // foreach (Type t in typeof(LP28).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GlitchModule))))
        // {
        //     bools[$"{t}::_enabled"] = GlitchModule.InitializedModules[t].IsEnabled;
        // }
    }

    [OnDeserialized]
    public void AfterDeserialized(StreamingContext _)
    {
        foreach (var (field, type) in fields)
        {
            if (field.FieldType == typeof(bool) && bools.TryGetValue($"{type.Name}::{field.Name}", out bool b))
            {
                field.SetValue(field.IsStatic ? null : GlitchModule.InitializedModules[type], b);
            }
            else if (field.FieldType == typeof(float) && floats.TryGetValue($"{type.Name}::{field.Name}", out float f))
            {
                field.SetValue(field.IsStatic ? null : GlitchModule.InitializedModules[type], f);
            }
            else if (field.FieldType == typeof(int) && ints.TryGetValue($"{type.Name}::{field.Name}", out int i))
            {
                field.SetValue(field.IsStatic ? null : GlitchModule.InitializedModules[type], i);
            }
            // else
            // {
            //     LP28.instance.LogError($"Could not load serialized setting {field.Name} on type {type.Name}");
            // }
        }
        
        // foreach (var (s, b) in bools.Where(p => p.Key.Split(':')[2] == "_enabled"))
        // {
        //     typeof(LP28).Assembly.GetType("")
        // }
    }
}

public class SerializableSettingAttribute : Attribute
{
    public static bool ShouldSerialize(FieldInfo field) =>
        Attribute.IsDefined(field, typeof(SerializableSettingAttribute));
}