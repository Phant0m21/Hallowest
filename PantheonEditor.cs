using System.Collections.Generic;
using Modding;
using UnityEngine;

namespace Hallowest
{
    public class PantheonEditor : MonoBehaviour
    {
        private static readonly List<string> VanillaP5Order = new List<string>()
        {
            "GG_Vengefly_V",
            "GG_Gruz_Mother_V",
            "GG_False_Knight",
            "GG_Mega_Moss_Charger",
            "GG_Hornet_1",
            "GG_Ghost_Gorb_V",
            "GG_Dung_Defender",
            "GG_Mage_Knight_V",
            "GG_Brooding_Mawlek_V",
            "GG_Nailmasters",
            "GG_Ghost_Xero_V",
            "GG_Crystal_Guardian",
            "GG_Soul_Master",
            "GG_Oblobbles",
            "GG_Mantis_Lords_V",
            "GG_Ghost_Marmu_V",
            "GG_Flukemarm",
            "GG_Broken_Vessel",
            "GG_Ghost_Galien",
            "GG_Painter",
            "GG_Hive_Knight",
            "GG_Ghost_Hu",
            "GG_Collector_V",
            "GG_God_Tamer",
            "GG_Grimm",
            "GG_Watcher_Knights",
            "GG_Uumuu_V",
            "GG_Nosk_Hornet",
            "GG_Sly",
            "GG_Hornet_2",
            "GG_Crystal_Guardian_2",
            "GG_Lost_Kin",
            "GG_Ghost_No_Eyes_V",
            "GG_Traitor_Lord",
            "GG_White_Defender",
            "GG_Soul_Tyrant",
            "GG_Ghost_Markoth_V",
            "GG_Grey_Prince_Zote",
            "GG_Failed_Champion",
            "GG_Grimm_Nightmare",
            "GG_Hollow_Knight",
            "GG_Radiance"
        };

        private static readonly List<string> MyBossList = new List<string>()
        {
            "GG_Mantis_Lords_V",
            "GG_Hive_Knight",
            "GG_God_Tamer",
            "GG_False_Knight",
            "GG_Traitor_Lord",
            "GG_Grimm",
            "GG_Ghost_Marmu_V",
            "GG_Mega_Moss_Charger",
            "GG_Failed_Champion",
            "GG_Mage_Knight_V",
            "GG_Ghost_Hu",
            "GG_Hornet_1",
            "GG_Crystal_Guardian",
            "GG_Painter",
            "GG_Ghost_No_Eyes_V",
            "GG_Brooding_Mawlek_V",
            "GG_Ghost_Galien",
            "GG_White_Defender",
            "GG_Nailmasters",
            "GG_Vengefly_V",
            "GG_Oblobbles",
            "GG_Broken_Vessel",
            "GG_Nosk_Hornet",
            "GG_Soul_Master",
            "GG_Hornet_2",
            "GG_Sly",
            "GG_Watcher_Knights",
            "GG_Gruz_Mother_V",
            "GG_Flukemarm",
            "GG_Crystal_Guardian_2",
            "GG_Soul_Tyrant",
            "GG_Ghost_Gorb_V",
            "GG_Collector_V",
            "GG_Uumuu_V",
            "GG_Lost_Kin",
            "GG_Ghost_Markoth_V",
            "GG_Grey_Prince_Zote",
            "GG_Ghost_Xero_V",
            "GG_Dung_Defender",
            "GG_Grimm_Nightmare",
            "GG_Hollow_Knight",
            "GG_Radiance"
        };

        public static void Initialize()
        {
            ModHooks.BeforeSceneLoadHook += OnBeforeSceneLoad;
            Modding.Logger.Log("[Hallowest] PantheonEditor initialized");
        }

        private static string OnBeforeSceneLoad(string sceneName)
        {
            Modding.Logger.Log("[Hallowest] Scene load: " + sceneName);

            int index = VanillaP5Order.IndexOf(sceneName);

            if (index >= 0 && index < MyBossList.Count)
            {
                string replacement = MyBossList[index];

                Modding.Logger.Log(
                    "[Hallowest] P5 Replace: " +
                    sceneName +
                    " -> " +
                    replacement
                );

                return replacement;
            }

            return sceneName;
        }
    }
}