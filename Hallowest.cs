using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;

namespace Hallowest
{
    public class Hallowest : Mod
    {
        internal static Hallowest Instance;
        public override string GetVersion() => "1.0.2";

        public override void Initialize()
        {
            Instance = this;

            ModHooks.OnEnableEnemyHook += OnEnableEnemy;

            FalseKnightMod.HookFalseKnight();
            PrimalAspidOverhaul.Initialize();
        }

        private bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            if (isAlreadyDead) return isAlreadyDead;

            HealthManager hpManager = enemy.GetComponent<HealthManager>();

            if (hpManager != null)
            {
                hpManager.hp = (int)Math.Round(hpManager.hp * 1.3f);

                foreach (var fsm in enemy.GetComponents<PlayMakerFSM>())
                {
                    foreach (var state in fsm.FsmStates)
                    {
                        foreach (var action in state.Actions)
                        {
                            if (action is Wait waitAction)
                                waitAction.time.Value *= 0.6f;

                            if (action is WaitRandom wr)
                            {
                                wr.timeMin.Value *= 0.6f;
                                wr.timeMax.Value *= 0.6f;
                            }
                        }
                    }

                    foreach (var fsmFloat in fsm.FsmVariables.FloatVariables)
                    {
                        if (fsmFloat.Name.Contains("Speed"))
                            fsmFloat.Value *= 1.5f;
                    }
                }
            }

            return isAlreadyDead;
        }
    }
}