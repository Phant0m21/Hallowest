using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Hallowest
{
    public static class FalseKnightMod
    {
        public static void HookFalseKnight()
        {
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
        }

        private static void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if ((self.gameObject.name == "False Knight" || self.gameObject.name == "False Knight New")
                && self.FsmName == "FalseyControl")
            {
                // ==========================================
                // 1. БАЗОВОЕ УСКОРЕНИЕ И ТАЙМЕРЫ
                // ==========================================
                ChangeWaitTime(self, "First Idle", 0.05f);
                ChangeWaitRandomTime(self, "Idle", 0.03f, 0.08f); // Сильно зажали паузы босса
                ChangeWaitTime(self, "Idle Pause", 0.03f);

                ChangeWaitTime(self, "S Attack Antic", 0.15f); // Почти моментальный замах перед выпадами
                ChangeWaitTime(self, "Slam Antic", 0.15f);     // Почти моментальный замах перед ударом об землю

                // Ускорение физики прыжков
                var jumpX = self.FsmVariables.FindFsmFloat("Jump Velocity X");
                if (jumpX != null) jumpX.Value = 26f; // Быстрее летит вперед на игрока

                var jumpY = self.FsmVariables.FindFsmFloat("Jump Velocity Y");
                if (jumpY != null) jumpY.Value = 38f; // Быстрее взмывает вверх

                // Больше падающих камней с потолка
                ChangeFsmIntValue(self, "Jump Barrel Min", 4);
                ChangeFsmIntValue(self, "Jump Barrel Max", 6);
                ChangeFsmIntValue(self, "Slam Barrel Min", 5);
                ChangeFsmIntValue(self, "Slam Barrel Max", 8);

                // Урезание времени стана (босс очень быстро встает)
                ChangeWaitTime(self, "Stun Start", 0.4f);

                // ==========================================
                // 2. ХАРДКОР: УСКОРЕНИЕ АНИМАЦИЙ БОССА В 2.5 РАЗА
                // ==========================================
                ModifyAnimationSpeed(self, "S Attack Antic", 2.5f);
                ModifyAnimationSpeed(self, "Slam Antic", 2.5f);
                ModifyAnimationSpeed(self, "Slam Strike", 2.0f);
                ModifyAnimationSpeed(self, "Attack Recover", 2.0f); // Быстрее возвращается в боевую стойку

                // ==========================================
                // 3. ХАРДКОР: ОПАСНАЯ И БЫСТРАЯ УДАРНАЯ ВОЛНА
                // ==========================================
                ModifyShockwavePrefab(self, "Slam Strike");
            }
        }

        // --- ВСЕ ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ДЛЯ СТАБИЛЬНОЙ РАБОТЫ ---

        // Метод для изменения обычного экшена Wait
        private static void ChangeWaitTime(PlayMakerFSM fsm, string stateName, float newTime)
        {
            FsmState state = fsm.Fsm.GetState(stateName);
            if (state == null) return;
            foreach (var action in state.Actions)
            {
                if (action is Wait waitAction)
                {
                    waitAction.time.Value = newTime;
                    break;
                }
            }
        }

        // Метод для изменения экшена WaitRandom
        private static void ChangeWaitRandomTime(PlayMakerFSM fsm, string stateName, float min, float max)
        {
            FsmState state = fsm.Fsm.GetState(stateName);
            if (state == null) return;
            foreach (var action in state.Actions)
            {
                if (action is WaitRandom waitRandomAction)
                {
                    waitRandomAction.timeMin.Value = min;
                    waitRandomAction.timeMax.Value = max;
                    break;
                }
            }
        }

        // Метод для изменения FsmInt переменных
        private static void ChangeFsmIntValue(PlayMakerFSM fsm, string varName, int value)
        {
            var fsmInt = fsm.FsmVariables.FindFsmInt(varName);
            if (fsmInt != null) fsmInt.Value = value;
        }

        // Разгоняем скорость анимаций 2D спрайтов (tk2d) через безопасную рефлексию
        private static void ModifyAnimationSpeed(PlayMakerFSM fsm, string stateName, float speedMultiplier)
        {
            FsmState state = fsm.Fsm.GetState(stateName);
            if (state == null) return;

            foreach (var action in state.Actions)
            {
                if (action.GetType().Name == "Tk2dPlayAnimation" || action.GetType().Name == "Tk2dPlayAnimationWithEvents")
                {
                    var animSpeedField = action.GetType().GetField("animSpeed");
                    if (animSpeedField != null)
                    {
                        FsmFloat fsmFloat = (FsmFloat)animSpeedField.GetValue(action);
                        if (fsmFloat != null) fsmFloat.Value = speedMultiplier;
                    }
                }
            }
        }

        // ИСПРАВЛЕНО: Теперь этот метод полностью безопасен и не вызывает ошибок компиляции типов
        private static void ModifyShockwavePrefab(PlayMakerFSM fsm, string stateName)
        {
            FsmState state = fsm.Fsm.GetState(stateName);
            if (state == null) return;

            foreach (var action in state.Actions)
            {
                // Проверяем имя экшена строкой ("SpawnObject" или "SpawnObject2"), убирая ошибку компиляции!
                if (action.GetType().Name == "SpawnObject" || action.GetType().Name == "SpawnObject2")
                {
                    var gameObjectField = action.GetType().GetField("gameObject");
                    if (gameObjectField != null)
                    {
                        FsmOwnerDefault ownerDefault = (FsmOwnerDefault)gameObjectField.GetValue(action);
                        if (ownerDefault != null)
                        {
                            // Достаем префаб спавнящейся волны
                            GameObject prefab = ownerDefault.GameObject.Value;
                            if (prefab == null)
                            {
                                // Если в поле gameObject пусто, проверяем альтернативное поле префабов у PlayMaker
                                var prefabField = action.GetType().GetField("spawnPrefab");
                                if (prefabField != null)
                                {
                                    FsmGameObject fsmGo = (FsmGameObject)prefabField.GetValue(action);
                                    if (fsmGo != null) prefab = fsmGo.Value;
                                }
                            }

                            if (prefab != null)
                            {
                                // Находим FSM контроля над самой летящей волной
                                PlayMakerFSM waveFsm = prefab.GetComponent<PlayMakerFSM>();
                                if (waveFsm != null)
                                {
                                    // Разгоняем скорость горизонтального полета волны до 35 (в оригинале ~15)
                                    var speedVar = waveFsm.FsmVariables.FindFsmFloat("Speed");
                                    if (speedVar != null) speedVar.Value = 35f;

                                    // Делаем волну шире и выше в два раза, заставляя прыгать выше/использовать теневой рывок
                                    var scaleX = waveFsm.FsmVariables.FindFsmFloat("Scale X");
                                    if (scaleX != null) scaleX.Value = 1.6f;

                                    var scaleY = waveFsm.FsmVariables.FindFsmFloat("Scale Y");
                                    if (scaleY != null) scaleY.Value = 2.0f;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}