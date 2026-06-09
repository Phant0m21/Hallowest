using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using UnityEngine;
using Vasi;

namespace Hallowest
{
    public static class PureVesselOverhaul
    {
        public static void Hook()
        {
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
        }

        private static void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self == null || self.gameObject == null)
                return;

            if (self.gameObject.name != "HK Prime" && self.gameObject.name != "Pure Vessel")
                return;

            if (self.FsmName != "Control" && self.FsmName != "HK Prime Control")
                return;

            Apply(self);
        }

        private static void Apply(PlayMakerFSM fsm)
        {
            Log("Pure Vessel attack logic overhaul applied");

            DesyncAttackTiming(fsm);
            FakeOutAttacks(fsm);
            AdaptiveComboShifts(fsm);
            ParryPunishBehavior(fsm);
        }

        private static void DesyncAttackTiming(PlayMakerFSM fsm)
        {
            string[] states =
            {
                "Slash Antic",
                "Dash Slash Antic",
                "Focus Antic",
                "Uppercut Antic",
                "Lunge Antic"
            };

            foreach (var stateName in states)
            {
                var state = fsm.Fsm.GetState(stateName);
                if (state == null) continue;

                foreach (var a in state.Actions)
                {
                    if (a is Wait w)
                        w.time.Value *= UnityEngine.Random.Range(0.35f, 0.9f);

                    if (a is WaitRandom wr)
                    {
                        wr.timeMin.Value *= 0.4f;
                        wr.timeMax.Value *= 0.85f;
                    }
                }
            }
        }

        private static void FakeOutAttacks(PlayMakerFSM fsm)
        {
            string[] attackStates =
            {
                "Slash",
                "Dash Slash",
                "Uppercut",
                "Lunge"
            };

            foreach (var stateName in attackStates)
            {
                var state = fsm.Fsm.GetState(stateName);
                if (state == null) continue;

                state.InsertMethod(0, () =>
                {
                    if (UnityEngine.Random.value < 0.4f)
                    {
                        fsm.SendEvent("FINISHED");
                        return;
                    }

                    if (UnityEngine.Random.value < 0.3f)
                    {
                        fsm.SendEvent("ATTACK FAST");
                    }
                });
            }
        }

        private static void AdaptiveComboShifts(PlayMakerFSM fsm)
        {
            var comboVar = fsm.FsmVariables.FindFsmInt("Combo Count");

            if (comboVar != null)
            {
                comboVar.Value = Mathf.Clamp(comboVar.Value + 1, 0, 10);
            }

            var randState = fsm.FsmVariables.FindFsmInt("Random Attack");

            if (randState != null)
            {
                randState.Value = UnityEngine.Random.Range(0, 3);
            }
        }

        // =====================================================
        // 4. PUNISH PLAYER BEHAVIOR
        // (реакция на агрессию игрока — без новых механик)
        // =====================================================
        private static void ParryPunishBehavior(PlayMakerFSM fsm)
        {
            string[] punishStates =
            {
                "Focus",
                "Heal",
                "Recover"
            };

            foreach (var stateName in punishStates)
            {
                var state = fsm.Fsm.GetState(stateName);
                if (state == null) continue;

                state.InsertMethod(0, () =>
                {

                    if (PlayerClose(fsm, 4f))
                    {
                        fsm.SendEvent("CANCEL");
                    }
                });
            }
        }

        private static bool PlayerClose(PlayMakerFSM fsm, float dist)
        {
            var hero = HeroController.instance;
            if (hero == null) return false;

            return Vector2.Distance(hero.transform.position, fsm.transform.position) < dist;
        }

        // LOG
        private static void Log(string msg)
        {
            Modding.Logger.Log("[Hallowest][PV] " + msg);
        }
    }
}