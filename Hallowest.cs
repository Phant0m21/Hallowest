using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using Vasi;

namespace Hallowest
{
    public class Hallowest : Mod
    {
        internal static Hallowest Instance;
        public override string GetVersion() => "1.0.6";

        public override void Initialize()
        {
            Instance = this;

            FalseKnightMod.HookFalseKnight();
            PrimalAspidOverhaul.Initialize();
            PantheonEditor.Initialize();
            PureVesselOverhaul.Hook();

            ModHooks.OnEnableEnemyHook += OnEnableEnemy;
            ModHooks.AfterSavegameLoadHook += OnSaveLoaded;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        private void OnHeroUpdate()
        {
            if (PlayerData.instance == null) return;
            ApplyCharmCosts();
        }

        private void OnSaveLoaded(SaveGameData data)
        {
            if (PlayerData.instance == null) return;
            ApplyCharmCosts();
        }

        private void ApplyCharmCosts()
        {
            PlayerData.instance.SetInt("charmCost_1", 0);
            PlayerData.instance.SetInt("charmCost_2", 0);
            PlayerData.instance.SetInt("charmCost_37", 0);
            PlayerData.instance.SetInt("charmCost_40", 1);
            PlayerData.instance.SetInt("charmCost_29", 3);
            PlayerData.instance.SetInt("charmCost_6", 1);
            PlayerData.instance.SetInt("charmCost_21", 3);
            PlayerData.instance.SetInt("charmCost_34", 3);
            PlayerData.instance.SetInt("charmCost_28", 1);
            PlayerData.instance.SetInt("charmCost_5", 1);
            PlayerData.instance.SetInt("charmCost_31", 1);
        }

        private bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            if (isAlreadyDead) return isAlreadyDead;

            HealthManager hpManager = enemy.GetComponent<HealthManager>();
            if (hpManager != null)
            {
                hpManager.hp = (int)Math.Round(hpManager.hp * 1.2f);
                foreach (var fsm in enemy.GetComponents<PlayMakerFSM>())
                {
                    foreach (var state in fsm.FsmStates)
                    {
                        foreach (var action in state.Actions)
                        {
                            if (action is Wait waitAction) waitAction.time.Value *= 0.6f;
                            if (action is WaitRandom wr)
                            {
                                wr.timeMin.Value *= 0.6f;
                                wr.timeMax.Value *= 0.6f;
                            }
                        }
                    }
                    foreach (var fsmFloat in fsm.FsmVariables.FloatVariables)
                    {
                        if (fsmFloat.Name.Contains("Speed")) fsmFloat.Value *= 1.4f;
                    }
                }
            }
            return isAlreadyDead;
        }
    }
}