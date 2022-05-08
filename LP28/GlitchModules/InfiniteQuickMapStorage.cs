using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Vasi;

namespace LP28.GlitchModules;

public class InfiniteQuickMapStorage : GlitchModule
{
    private static SortedDictionary<int, FsmStateAction> bdcChecks, doubleChecks;

    public override string GetDescription() => "Infinite quick map storage attempts, open quick map without having it";

    protected override void Enable()
    {
        MoreHooks.HCStartHook += RemoveQMapChecks;
        MoreHooks.HCStartHook += CacheInv;

        if (HeroController.instance != null)
        {
            RemoveQMapChecks(HeroController.instance);
            CacheInv(HeroController.instance);
        }
    }

    protected override void Disable()
    {
        MoreHooks.HCStartHook -= RemoveQMapChecks;
        MoreHooks.HCStartHook -= CacheInv;

        if (HeroController.instance != null)
        {
            var mapCtrl = HeroController.instance.gameObject.LocateMyFSM("Map Control");
            mapCtrl.GetState("Button Down Check").RemoveAction<CacheInventory>();

            if (bdcChecks != null)
            {
                mapCtrl.GetState("Button Down Check").RestoreActions(bdcChecks);
            }
            else
            {
                var actions = mapCtrl.GetState("Button Down Check").Actions.ToList();
                actions.Insert(0, new PlayerDataBoolTest
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    boolName = "disablePause",
                    isTrue = new FsmEvent("CANCEL"),
                    isFalse = null
                });
                actions.Insert(3, new CallMethodProper
                {
                    gameObject = new FsmOwnerDefault(),
                    behaviour = "HeroController",
                    methodName = "CanQuickMap",
                    parameters = Array.Empty<FsmVar>(),
                    storeResult = new FsmVar(mapCtrl.FsmVariables.GetFsmBool("Return Bool"))
                });
                actions.Insert(4, new BoolTest
                {
                    boolVariable = mapCtrl.FsmVariables.GetFsmBool("Return Bool"),
                    isTrue = null,
                    isFalse = new FsmEvent("CANCEL"),
                    everyFrame = false
                });
                mapCtrl.GetState("Button Down Check").Actions = actions.ToArray();
            }

            if (doubleChecks != null)
            {
                mapCtrl.GetState("Double!").RestoreActions(doubleChecks);
            }
            else
            {
                var actions = mapCtrl.GetState("Double!").Actions.ToList();
                actions.Insert(1, new PlayerDataBoolTest
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    boolName = "disablePause",
                    isTrue = new FsmEvent("CANCEL"),
                    isFalse = null
                });
                actions.Insert(2, new CallMethodProper
                {
                    gameObject = new FsmOwnerDefault(),
                    behaviour = "HeroController",
                    methodName = "CanQuickMap",
                    parameters = Array.Empty<FsmVar>(),
                    storeResult = new FsmVar(mapCtrl.FsmVariables.GetFsmBool("Return Bool"))
                });
                actions.Insert(3, new BoolTest
                {
                    boolVariable = mapCtrl.FsmVariables.GetFsmBool("Return Bool"),
                    isTrue = null,
                    isFalse = new FsmEvent("CANCEL"),
                    everyFrame = false
                });
                mapCtrl.GetState("Double!").Actions = actions.ToArray();
            }
        }
    }

    private void CacheInv(HeroController HC)
    {
        HC.gameObject.LocateMyFSM("Map Control").GetState("Button Down Check").AddAction(new CacheInventory());
    }

    private static void RemoveQMapChecks(HeroController HC)
    {
        var mapCtrl = HC.gameObject.LocateMyFSM("Map Control");

        bdcChecks = mapCtrl.GetState("Button Down Check").ExtractActions(a =>
            (a is PlayerDataBoolTest pdbt && pdbt.boolName.Value != "disablePause") ||
            (a is CallMethodProper or BoolTest));

        
        doubleChecks = mapCtrl.GetState("Double!")
            .ExtractActions(a => a is PlayerDataBoolTest or CallMethodProper or BoolTest);
    }
    
    private class CacheInventory : FsmStateAction
    {
        public override void OnEnter()
        {
            var invVar = Fsm.FsmComponent.FsmVariables.GetFsmGameObject("Inventory");
            if (invVar.Value == null)
            {
                GameObject inv = GameObject.FindGameObjectWithTag("Inventory Top");
                if (inv == null)
                {
                    LogError("Unable to cache Inventory Top: game object is null");
                    return;
                }

                invVar.Value = inv;
            }
        }
    }
}