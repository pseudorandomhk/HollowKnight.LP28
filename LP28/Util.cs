using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using Vasi;

namespace LP28;

public static class Util
{
    public static GameObject FindRecursive(this GameObject go, string name)
    {
        if (go == null) 
            return null;

        int count = go.transform.childCount;
        return Enumerable.Range(0, count).Select(i => go.transform.GetChild(i).gameObject)
                   .FirstOrDefault(o => o.name == name) ??
               Enumerable.Range(0, count)
                   .Select(i => go.transform.GetChild(i).gameObject.FindRecursive(name)).FirstOrDefault();
    }
    
    public static SortedDictionary<int, FsmStateAction> ExtractActions(this FsmState state,
        Func<FsmStateAction, bool> removePredicate)
    {
        SortedDictionary<int, FsmStateAction> res = new();
        var actions = state.Actions.ToList();

        for (int i = actions.Count - 1; i >= 0; i--)
        {
            if (removePredicate(actions[i]))
            {
                res.Add(i, actions[i]);
                actions.RemoveAt(i);
            }
        }

        state.Actions = actions.ToArray();

        return res;
    }

    public static void RestoreActions(this FsmState state, SortedDictionary<int, FsmStateAction> actions)
    {
        var l = state.Actions.ToList();
        
        foreach (var (i, action) in actions)
        {
            l.Insert(i, action);
        }

        state.Actions = l.ToArray();
    }

    public static void RemoveTransitionEvent(this FsmState state, string eventName)
    {
        state.Transitions = state.Transitions.Where(t => t.EventName != eventName).ToArray();
    }
    
    public static MethodInfo[] GetMethodInfos<T>(string methodName, bool instance = true)
    {
        var res = typeof(T)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                        (instance ? BindingFlags.Instance : BindingFlags.Static)).Where(m => m.Name == methodName)
            .ToArray();
        if (res == null || !res.Any())
        {
            throw new MissingMethodException(
                $"Cannot find {(instance ? "instance" : "static")} methods {methodName} on type {typeof(T)}");
        }

        return res;
    }

    public static MethodInfo GetMethodInfo<T>(string methodName, bool instance = true)
    {
        return GetMethodInfo(typeof(T), methodName, instance);
    }

    public static MethodInfo GetMethodInfo(Type t, string methodName, bool instance = true)
    {
        MethodInfo res = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public |
                                (instance ? BindingFlags.Instance : BindingFlags.Static));
        if (res == null)
        {
            throw new MissingMethodException(
                $"Cannot find {(instance ? "instance" : "static")} method {methodName} on type {t}");
        }

        return res;
    }
    
    public static void RaiseEventNoArgs<T>(T source, string eventName)
    {
        var delegateField = typeof(T).GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (delegateField == null)
        {
            LP28.instance.LogError($"Unable to find event {eventName} on type {typeof(T)}");
            return;
        }
        var eventDelegate = (MulticastDelegate) delegateField.GetValue(source);
        if (eventDelegate != null)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
            {
                handler.Method.Invoke(handler.Target, new object[] { });
            }
        }
    }

    public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }

        public bool Equals(T left, T right)
        {
            return ReferenceEquals(left, right);
        }
    }
}