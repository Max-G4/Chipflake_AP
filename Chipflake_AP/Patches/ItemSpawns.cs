using System;
using System.Linq;
using Archipelago.MultiClient.Net.Helpers;
using Chipflake_AP.Archipelago;
using HarmonyLib;
using UnityEngine;

namespace Chipflake_AP.Patches;

public class ItemSpawnPatcher
{
    public static ArchipelagoClient Client;
    public static void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(ItemSpawnPatcher));
    }

    [HarmonyPatch(typeof(TypicalItem), "OnEnable")]
    [HarmonyPrefix]
    public static bool spawnQuestItemsOnlyAfterSticker(TypicalItem __instance)
    {
        
        if (__instance.name.Contains("Lemon"))
        {
            Plugin.BepinLogger.LogInfo("Checking if Minigolf quest done");
            if (GetQuestDone("Minigolf"))
            {
                Plugin.BepinLogger.LogInfo("Minigolf quest done");
                __instance.transform.parent.gameObject.SetActive(true);
                return true;
            }
            Plugin.BepinLogger.LogInfo("Minigolf quest not done");

        }
        else if (__instance.name.Contains("MeatBase"))
        {
            Plugin.BepinLogger.LogInfo("Checking if Heck quest done");

            if (GetQuestDone("Heck"))
            {
                __instance.transform.parent.gameObject.SetActive(true);
                return true;
            }
        }
        else if (__instance.name.Contains("23_Batter"))
        {
            Plugin.BepinLogger.LogInfo("Checking if Gamer quest done");

            if (GetQuestDone("Gamer"))
            {
                __instance.transform.parent.gameObject.SetActive(true);
                return true;
            }
        }
        else if (__instance.name.Contains("Oil"))
        {
            Plugin.BepinLogger.LogInfo("Checking if Pizza quest done");
            if (GetQuestDone("Pizza"))
            {
                __instance.transform.parent.gameObject.SetActive(true);
                return true;
            }
        }
        else if (__instance.name.Contains("Love"))
        {
            Plugin.BepinLogger.LogInfo("Checking if Love quest done");

            if (GetQuestDone("Love"))
            {
                __instance.transform.parent.gameObject.SetActive(true);
                return true;
            }
        }
        else
        {
            return true;
        }
        __instance.gameObject.SetActive(false);
        return false;
    }

    [HarmonyPatch(typeof(TypicalItem))]
    public static bool GetQuestDone(string questName)
    {
        Quest itemQuest = null;
        foreach (var quest in QuestManager.Instance.allQuests)
        {
            if (quest.name == questName)
            {
                itemQuest = quest;
            }
        }
        Plugin.BepinLogger.LogInfo("quest " + itemQuest.name + " found");
        int num = Array.IndexOf<Quest>(QuestManager.Instance.allQuests, itemQuest);
        if (QuestManager.Instance.questStates[num].questDone)
        {
            Plugin.BepinLogger.LogInfo("quest " + questName + " done");
            try
            {
                ILocationCheckHelper lch = Client.session.Locations;
                lch.AllLocationsChecked.Contains(lch.GetLocationIdFromName("Super Chipflake Ü",
                    "Quest - " + questName));
                return true;
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }
        Plugin.BepinLogger.LogError("quest " + questName + " not done");
        return false;
    }

    [HarmonyPatch(typeof(NPC), "OnInteract")]
    [HarmonyPostfix]
    public static void activateSpawners(GameManager __instance)
    {
        Plugin.BepinLogger.LogInfo("activating spawners");
        string[] names = new []{"Lemon_Spawn", "OilSpawner", "LoveSpawner", "MeatBaseSpawner", "BatterSpawner"};
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (names.Contains(objs[i].name))
                {
                    Plugin.BepinLogger.LogInfo("spawner " + objs[i].name + " found");
                    objs[i].gameObject.active = true;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(NPC), "OnInteract")]
    [HarmonyPrefix]
    public static void deactivateSpawnersForReset()
    {
        Plugin.BepinLogger.LogInfo("deactivating spawners");
        string[] names = new []{"Lemon_Spawn", "OilSpawner", "LoveSpawner", "MeatBaseSpawner", "BatterSpawner"};
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (names.Contains(objs[i].name))
                {
                    Plugin.BepinLogger.LogInfo("spawner " + objs[i].name + " found");
                    objs[i].gameObject.active = false;
                }
            }
        }
    }
    
}