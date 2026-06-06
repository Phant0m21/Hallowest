using System;
using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Vasi;
using Object = UnityEngine.Object;
using Modding;

namespace Hallowest
{
    public class PureVesselOverhaul
    {
        private static System.Random rand = new System.Random();
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            ModHooks.OnEnableEnemyHook += OnEnemySpawned;
        }

        private static bool OnEnemySpawned(GameObject enemy, bool isAlreadyDead)
        {
            if (enemy == null) return isAlreadyDead;

            if (!enemy.name.Contains("Pure Vessel"))
                return isAlreadyDead;

            Apply(enemy);

            ModHooks.OnEnableEnemyHook -= OnEnemySpawned;

            return isAlreadyDead;
        }

        private static void Apply(GameObject boss)
        {
            PlayMakerFSM control = boss.LocateMyFSM("Control");
            if (control == null) return;

            AddAttacks(boss, control);
            AddHKPrimeStyleLogic(boss, control); // 💀 ДОБАВЛЕНА СЛОЖНАЯ FSM ЛОГИКА

            Log("Pure Vessel patched (lazy init + HK Prime logic)");
        }

        // =========================
        // ТВОИ АТАКИ (НЕ ТРОГАЛ)
        // =========================
        private static void AddAttacks(GameObject boss, PlayMakerFSM control)
        {
            AddVoidRain(boss, control);
            AddSpinBurst(boss, control);
            AddTeleportSlam(boss, control);

            Log("Pure Vessel patched (Hallowest style)");
        }

        private static void AddVoidRain(GameObject boss, PlayMakerFSM control)
        {
            IEnumerator Attack()
            {
                var anim = boss.GetComponent<tk2dSpriteAnimator>();
                anim.Play("Focus Charge");

                yield return new WaitForSeconds(0.6f);

                for (int i = 0; i < 20; i++)
                {
                    float x = HeroController.instance.transform.position.x +
                              UnityEngine.Random.Range(-9f, 9f);

                    float y = HeroController.instance.transform.position.y + 12f;

                    GameObject proj = Object.Instantiate(
                        GetBlackShot(control),
                        new Vector3(x, y, 0),
                        Quaternion.identity
                    );

                    proj.GetComponent<Rigidbody2D>().velocity = Vector2.down * 18f;

                    yield return new WaitForSeconds(0.05f);
                }
            }

            Inject(control, "Void Rain", Attack);
        }

        private static void AddSpinBurst(GameObject boss, PlayMakerFSM control)
        {
            IEnumerator Attack()
            {
                var anim = boss.GetComponent<tk2dSpriteAnimator>();
                anim.Play("Slash1 Antic");

                yield return new WaitForSeconds(0.25f);

                int count = 16;

                for (int i = 0; i < count; i++)
                {
                    float angle = i * (360f / count);

                    Vector2 dir = new Vector2(
                        Mathf.Cos(angle * Mathf.Deg2Rad),
                        Mathf.Sin(angle * Mathf.Deg2Rad)
                    );

                    GameObject proj = Object.Instantiate(
                        GetSilentShot(control),
                        boss.transform.position,
                        Quaternion.identity
                    );

                    proj.GetComponent<Rigidbody2D>().velocity = dir * 24f;
                }

                yield return new WaitForSeconds(0.3f);
            }

            Inject(control, "Shadow Spin", Attack);
        }

        private static void AddTeleportSlam(GameObject boss, PlayMakerFSM control)
        {
            IEnumerator Attack()
            {
                var anim = boss.GetComponent<tk2dSpriteAnimator>();

                anim.Play("Tele Out");
                yield return new WaitForSeconds(0.2f);

                Vector3 hero = HeroController.instance.transform.position;

                boss.transform.position = hero + new Vector3(0, 8f, 0);

                yield return new WaitForSeconds(0.15f);

                GameObject slam = Object.Instantiate(
                    GetBlackShot(control),
                    boss.transform.position,
                    Quaternion.identity
                );

                slam.GetComponent<Rigidbody2D>().velocity = Vector2.down * 35f;

                anim.Play("Fall");

                yield return new WaitForSeconds(0.5f);
            }

            Inject(control, "Teleport Slam", Attack);
        }

        // =========================
        // 💀 HK PRIME STYLE FSM LOGIC
        // =========================
        private static void AddHKPrimeStyleLogic(GameObject boss, PlayMakerFSM control)
        {
            // =========================
            // 1. REMOVE IDLE TIME (как HK Prime)
            // =========================
            control.FsmVariables.FindFsmFloat("Idle Time").Value = 0;
            control.FsmVariables.FindFsmFloat("Idle Time Min").Value = 0;
            control.FsmVariables.FindFsmFloat("Idle Time Max").Value = 0;

            // =========================
            // 2. УСИЛЕНИЕ CHOICE P3 (не равный random)
            // =========================
            var choice = control.GetState("Choice P3");
            var rnd = choice.GetAction<SendRandomEventV3>();

            rnd.eventMax = new FsmInt[3] { 3, 2, 1 };
            rnd.missedMax = new FsmInt[3] { 5, 5, 5 };

            // =========================
            // 3. VOID RAIN CONDITIONAL SPEED (InsertMethod)
            // =========================
            control.GetState("Void Rain").InsertMethod(0, () =>
            {
                float dist = Vector2.Distance(
                    HeroController.instance.transform.position,
                    boss.transform.position
                );

                if (dist > 8f)
                {
                    // дальний бой → быстрее давление
                    Time.timeScale = 1.05f;
                }
            });

            // =========================
            // 4. AFTER ATTACK BRANCH (как After Slash2)
            // =========================
            control.CreateState("After Spin Burst");

            control.GetState("Shadow Spin").AddTransition(FsmEvent.Finished, "After Spin Burst");

            control.GetState("After Spin Burst").AddMethod(() =>
            {
                float r = UnityEngine.Random.value;

                if (r < 0.4f)
                    control.SetState("Void Rain");
                else if (r < 0.7f)
                    control.SetState("Teleport Slam");
                else
                    control.SetState("Idle Stance");
            });

            // =========================
            // 5. TELEPORT SLAM IMPROVEMENT (HK Prime style DSTAB logic)
            // =========================
            control.GetState("Teleport Slam").InsertMethod(0, () =>
            {
                Vector3 hero = HeroController.instance.transform.position;

                // более агрессивный snap-teleport
                boss.transform.position = hero + new Vector3(
                    UnityEngine.Random.Range(-1.5f, 1.5f),
                    8f,
                    0
                );
            });
        }

        // =========================
        // FSM HELPERS
        // =========================
        private static void Inject(PlayMakerFSM control, string name, Func<IEnumerator> logic)
        {
            FsmState state = control.CreateState(name);

            state.InsertCoroutine(0, logic);
            state.AddTransition(FsmEvent.Finished, "Idle Stance");

            control.GetAction<SendRandomEventV3>("Choice P3")
                .AddToSendRandomEventV3(name, 0.25f, 1, 5);
        }

        private static GameObject GetBlackShot(PlayMakerFSM control)
        {
            return control.GetAction<FlingObjectsFromGlobalPoolTime>("SmallShot LowHigh")
                .gameObject.Value;
        }

        private static GameObject GetSilentShot(PlayMakerFSM control)
        {
            return control.GetAction<FlingObjectsFromGlobalPoolTime>("SmallShot LowHigh")
                .gameObject.Value;
        }

        private static void Log(string msg)
        {
            Debug.Log("[Hallowest] " + msg);
        }
    }
}