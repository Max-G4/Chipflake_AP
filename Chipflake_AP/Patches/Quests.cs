using System;
using Archipelago.MultiClient.Net.Helpers;
using Chipflake_AP.Archipelago;
using HarmonyLib;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;


namespace Chipflake_AP.Patches;

public class QuestPatcher
{
    public static ArchipelagoClient Client;
    public static ILocationCheckHelper Lch;
    //public static QuestSwitchNode DialogueInstance;
    
    public static void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(QuestPatcher));
    }

    [HarmonyPatch(typeof(QuestManager), "MarkQuestDone")]
    [HarmonyPrefix]
    static bool QuestIntercept(Quest quest,QuestManager __instance)
    {
        
        int num = Array.IndexOf<Quest>(__instance.allQuests, quest);
        if (__instance.questStates[num].questDone)
        {
            return false;
        }
        
        Client.SendLocation("Quest - " + quest.name);
        if (quest.name == "GetTheSchnitzel")
        {
            Client.GoalWorld();
        }
        
        Debug.Log($"Location {quest.name} is done!");
        
        __instance.questStates[num].questActive = true;
        __instance.questStates[num].somethingNew = true;
        
        __instance.ShowQuestMessage(Plugin.APSticker, "Location Checked!", QuestManager.QuestMessageType.CompletedQuest);
        return false;
    }

    [HarmonyPatch(typeof(QuestSwitchNode), "OnExecute")]
    [HarmonyPrefix]
    static bool GetInstance(Component agent, IBlackboard bb, QuestSwitchNode __instance)
    {
        //DialogueInstance = __instance;
        return true;
    }
    
    
    //For dialogues.
    [HarmonyPatch(typeof(QuestManager), "isQuestDone")]
    [HarmonyPostfix]
    public static void IsQuestDone(Quest quest, ref bool __result)
    {
        __result = Lch.AllLocationsChecked.Contains(Lch.GetLocationIdFromName("Super Chipflake Ü",
            "Quest - " + quest.name)) ;
    }
    
    
}