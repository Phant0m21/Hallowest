using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;

namespace Hallowest
{
    public static class PrimalAspidOverhaul
    {
        private const float SpeedMultiplier = 8.0f;
        private const float ShootSpeedMultiplier = 0.04f;

        public static void Initialize()
        {
            ModHooks.OnEnableEnemyHook += OnEnableEnemy;
            ModHooks.ObjectPoolSpawnHook += OnSpawnObject;
        }

        private static bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            if (isAlreadyDead || enemy == null)
                return isAlreadyDead;

            if (!enemy.name.ToLower().Contains("aspid"))
                return isAlreadyDead;

            BoostAspid(enemy);

            return isAlreadyDead;
        }

        private static void BoostAspid(GameObject enemy)
        {
            foreach (var fsm in enemy.GetComponents<PlayMakerFSM>())
            {
                foreach (var state in fsm.FsmStates)
                {
                    string name = state.Name.ToLower();

                    if (name.Contains("shoot") || name.Contains("fire") || name.Contains("attack"))
                    {
                        foreach (var action in state.Actions)
                        {
                            if (action is Wait wait)
                                wait.time.Value *= ShootSpeedMultiplier;

                            if (action is WaitRandom wr)
                            {
                                wr.timeMin.Value *= ShootSpeedMultiplier;
                                wr.timeMax.Value *= ShootSpeedMultiplier;
                            }
                        }
                    }
                }

                foreach (var fsmFloat in fsm.FsmVariables.FloatVariables)
                {
                    string n = fsmFloat.Name.ToLower();

                    if (n.Contains("speed") ||
                        n.Contains("run") ||
                        n.Contains("move") ||
                        n.Contains("accel"))
                    {
                        fsmFloat.Value *= SpeedMultiplier;
                    }
                }
            }
        }

        private static GameObject OnSpawnObject(GameObject obj)
        {
            if (obj == null)
                return obj;

            string n = obj.name.ToLower();

            if (n.Contains("spit") || n.Contains("acid") || n.Contains("shot"))
            {
                var dmg = obj.GetComponent<DamageHero>();

                if (dmg != null)
                {
                }
            }

            return obj;
        }
    }
}